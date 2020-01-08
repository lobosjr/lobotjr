using LobotJR.Shared.Authentication;
using LobotJR.Shared.Client;
using LobotJR.Shared.Utility;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LobotJR.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _cancelAuthUri = "https://id.twitch.tv/oauth2/authorize";
        private const string _chatScope = "chat:read chat:edit whispers:read whispers:edit";
        private const string _broadcastScope = "channel:read:subscriptions";

        private ClientData _clientData;
        private bool _isNavigating = false;
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
                LoadTwitchAuthPage(Browser, _chatScope, "Bot Account");
            }
            else if (_tokenData.BroadcastToken == null)
            {
                LoadTwitchAuthPage(Browser, _broadcastScope, "Streamer Account");
            }
            else
            {
                LaunchBot();
            }
        }

        private void Browser_Navigated(object sender, NavigationEventArgs e)
        {
            if (_isNavigating)
            {
                // The wpf browser control doesn't have an event for redirects or load failures
                // if we get to the navigated event twice without a load complete, it's probably
                // a redirect, which means our twitch client data is wrong.
                MessageBox.Show(this, "Unable to load twitch authentication page, please confirm your client data and try again.", "Twitch Authentication Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LaunchClientDataUpdater(_clientData);

            }
            _isNavigating = true;

            if (e.Uri.ToString().Equals(_cancelAuthUri))
            {
                Close();
            }
            else if (e.Uri.ToString().StartsWith(_clientData.RedirectUri))
            {
                if (_tokenData.ChatToken == null)
                {
                    _tokenData.ChatToken = HandleAuthResponse(e.Uri);
                    if (_tokenData.BroadcastToken == null)
                    {
                        LoadTwitchAuthPage(Browser, _broadcastScope, "Streamer Account");
                    }
                    else
                    {
                        LaunchBot();
                    }
                }
                else
                {
                    _tokenData.BroadcastToken = HandleAuthResponse(e.Uri);
                    LaunchBot();
                }
            }
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _isNavigating = false;
        }

        private void LaunchBot()
        {
            FileUtils.WriteTokenData(_tokenData);
            Hide();
            using (var lobot = new Process())
            {
                lobot.StartInfo.FileName = "LobotJR.exe";
                lobot.StartInfo.UseShellExecute = true;
                lobot.Start();
            }
            Close();
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
                    }
                    if (tokenData.BroadcastToken != null)
                    {
                        var validationResponse = AuthToken.Validate(tokenData.BroadcastToken.AccessToken);
                        if (validationResponse == null)
                        {
                            tokenData.BroadcastToken = AuthToken.Refresh(_clientData.ClientId, _clientData.ClientSecret, tokenData.BroadcastToken.RefreshToken);
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

        private void LoadTwitchAuthPage(WebBrowser control, string scope, string title)
        {
            LoginLabel.Content = title;

            var builder = new UriBuilder("https", "id.twitch.tv");
            builder.Path = "oauth2/authorize";
            AddQuery(builder, "client_id", _clientData.ClientId);
            AddQuery(builder, "redirect_uri", _clientData.RedirectUri);
            AddQuery(builder, "response_type", "code");
            AddQuery(builder, "scope", scope);
            AddQuery(builder, "force_verify", "true");
            AddQuery(builder, "state", _state);
            control.Navigate(builder.Uri);
        }

        private void UpdateClientData_Click(object sender, RoutedEventArgs e)
        {
            LaunchClientDataUpdater(_clientData);
            LoadTwitchAuthPage(Browser, _chatScope, "Chat Account");
        }
    }
}