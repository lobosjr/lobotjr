﻿using CefSharp.Wpf;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Client;
using LobotJR.Shared.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace LobotJR.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _cancelError = "error=access_denied";

        private static readonly IEnumerable<string> _chatScopes = new List<string>(new string[] { "chat:read", "chat:edit", "whispers:read", "whispers:edit", "channel:moderate" });
        private static readonly IEnumerable<string> _broadcastScopes = new List<string>(new string[] { "channel_subscriptions" });

        private ClientData _clientData;
        private string _state = Guid.NewGuid().ToString();

        private TokenData _tokenData;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _clientData = LoadClientData();
            _tokenData = LoadTokenData();
            if (_tokenData.ChatToken == null)
            {
                LoadTwitchAuthPage(Browser, _chatScopes, "Bot Account");
            }
            else if (_tokenData.BroadcastToken == null)
            {
                LoadTwitchAuthPage(Browser, _broadcastScopes, "Streamer Account");
            }
            else
            {
                LaunchBot();
            }
        }

        private void SetBrowserEmulation()
        {
            var browserEmulationKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            var exeName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var keyName = $"{exeName}.exe";
            uint browserVersion = 11001;
            using (var registryEntry = Registry.CurrentUser.OpenSubKey(browserEmulationKey, true))
            {
                var registryValue = registryEntry.GetValue(keyName);
                if (registryValue == null || (int)registryValue != browserVersion)
                {
                    registryEntry.SetValue(keyName, (uint)11001, RegistryValueKind.DWord);
                }
            }
        }

        private void LaunchBot()
        {
            FileUtils.WriteTokenData(_tokenData);
            Dispatcher.Invoke(() =>
            {
                Hide();
                using (var lobot = new Process())
                {
                    lobot.StartInfo.FileName = "LobotJR.exe";
                    lobot.StartInfo.UseShellExecute = true;
                    lobot.Start();
                }
                Close();
            });
        }

        private TokenResponse HandleAuthResponse(Uri uri)
        {
            // If you're not familiar with Linq, this is going to look like complete nonsense,
            // but it's basically just a way to turn the url we get back into something we can actually use
            var returnValues = uri.Query.Substring(1).Split('&')
                .Select(x => x.Split('=')).ToDictionary(key => key[0], value => value[1]);

            if (returnValues["state"] == _state)
            {
                _state = Guid.NewGuid().ToString();

                var tokenData = AuthToken.Fetch(_clientData.ClientId, _clientData.ClientSecret, returnValues["code"], _clientData.RedirectUri);
                return tokenData;
            }
            else
            {
                MessageBox.Show(this, "CSRF attack detected, exiting application", "Security Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
                return null;
            }
        }

        private void AddQuery(UriBuilder builder, string rawKey, string rawValue)
        {
            var key = Uri.EscapeUriString(rawKey);
            var value = Uri.EscapeUriString(rawValue);
            if (!string.IsNullOrWhiteSpace(builder.Query))
            {
                builder.Query = builder.Query.Substring(1) + "&" + key + "=" + value;
            }
            else
            {
                builder.Query = key + "=" + value;
            }
        }

        private ClientData LoadClientData()
        {
            ClientData clientData;
            if (FileUtils.HasClientData())
            {
                clientData = FileUtils.ReadClientData();
                if (string.IsNullOrWhiteSpace(clientData.ClientId)
                    || string.IsNullOrWhiteSpace(clientData.ClientSecret)
                    || string.IsNullOrWhiteSpace(clientData.RedirectUri))
                {
                    LaunchClientDataUpdater(clientData);
                }
            }
            else
            {
                clientData = new ClientData()
                {
                    ClientSecret = FileUtils.ReadLegacySecret()
                };
                LaunchClientDataUpdater(clientData);
            }
            return clientData;
        }

        private TokenData LoadTokenData()
        {
            if (FileUtils.HasTokenData())
            {
                var tokenData = FileUtils.ReadTokenData();
                if (tokenData != null)
                {
                    if (tokenData.ChatToken != null)
                    {
                        var validationResponse = AuthToken.Validate(tokenData.ChatToken.AccessToken);
                        if (validationResponse == null)
                        {
                            tokenData.ChatToken = AuthToken.Refresh(_clientData.ClientId, _clientData.ClientSecret, tokenData.ChatToken.RefreshToken);
                        }
                        else if (!validationResponse.Login.Equals(tokenData.ChatUser) || _chatScopes.Any(x => !validationResponse.Scopes.Contains(x)))
                        {
                            tokenData.ChatToken = null;
                        }
                    }
                    if (tokenData.BroadcastToken != null)
                    {
                        var validationResponse = AuthToken.Validate(tokenData.BroadcastToken.AccessToken);
                        if (validationResponse == null)
                        {
                            tokenData.BroadcastToken = AuthToken.Refresh(_clientData.ClientId, _clientData.ClientSecret, tokenData.BroadcastToken.RefreshToken);
                        }
                        else if (!validationResponse.Login.Equals(tokenData.BroadcastUser) || _broadcastScopes.Any(x => !validationResponse.Scopes.Contains(x)))
                        {
                            tokenData.BroadcastToken = null;
                        }
                    }
                    return tokenData;
                }
            }
            return new TokenData();
        }

        private void LaunchClientDataUpdater(ClientData clientData)
        {
            var updateModal = new UpdateConfig();
            updateModal.ClientIdValue = clientData.ClientId;
            updateModal.ClientSecretValue = clientData.ClientSecret;
            updateModal.RedirectUriValue = clientData.RedirectUri;
            var result = updateModal.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                MessageBox.Show(this, "Unable to launch lobot without proper client config. Closing.", "Missing Client Config", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            else
            {
                clientData.ClientId = updateModal.ClientIdValue;
                clientData.ClientSecret = updateModal.ClientSecretValue;
                clientData.RedirectUri = updateModal.RedirectUriValue;
                FileUtils.WriteClientData(clientData);
            }
        }

        private void LoadTwitchAuthPage(ChromiumWebBrowser control, IEnumerable<string> scopes, string title)
        {
            SetBrowserEmulation();
            Dispatcher.Invoke((Action)(() =>
            {
                LoginLabel.Content = title;

                var builder = new UriBuilder("https", "id.twitch.tv");
                builder.Path = "oauth2/authorize";
                AddQuery(builder, "client_id", _clientData.ClientId);
                AddQuery(builder, "redirect_uri", _clientData.RedirectUri);
                AddQuery(builder, "response_type", "code");
                AddQuery(builder, "scope", string.Join(" ", scopes));
                AddQuery(builder, "force_verify", "true");
                AddQuery(builder, "state", _state);
                control.Load(builder.Uri.ToString());
            }));
        }

        private void UpdateClientData_Click(object sender, RoutedEventArgs e)
        {
            LaunchClientDataUpdater(_clientData);
            LoadTwitchAuthPage(Browser, _chatScopes, "Chat Account");
        }

        private void Browser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (e.Url.StartsWith(_clientData.RedirectUri))
            {
                if (new Uri(e.Url).Query.Contains(_cancelError))
                {
                    Dispatcher.Invoke(() =>
                    {
                        Close();
                    });
                }
                else if (_tokenData.ChatToken == null)
                {
                    _tokenData.ChatToken = HandleAuthResponse(new Uri(e.Url));
                    var validationResponse = AuthToken.Validate(_tokenData.ChatToken.AccessToken);
                    _tokenData.ChatUser = validationResponse.Login;
                    if (_tokenData.ChatToken == null)
                    {
                        MessageBox.Show("Failed to re-obtain chat token, trying again.");
                        LoadTwitchAuthPage(Browser, _chatScopes, "Bot Account");
                    }
                    else if (_tokenData.BroadcastToken == null)
                    {
                        LoadTwitchAuthPage(Browser, _broadcastScopes, "Streamer Account");
                    }
                    else
                    {
                        LaunchBot();
                    }
                }
                else
                {
                    _tokenData.BroadcastToken = HandleAuthResponse(new Uri(e.Url));
                    var validationResponse = AuthToken.Validate(_tokenData.BroadcastToken.AccessToken);
                    _tokenData.BroadcastUser = validationResponse.Login;
                    if (_tokenData.BroadcastToken == null)
                    {
                        MessageBox.Show("Failed to obtain broadcast token, trying again.");
                        LoadTwitchAuthPage(Browser, _broadcastScopes, "Streamer Account");
                    }
                    else
                    {
                        LaunchBot();
                    }
                }
            }
        }
    }
}