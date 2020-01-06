using System.Windows;

namespace LobotJR.Launcher
{
    /// <summary>
    /// Interaction logic for UpdateConfig.xaml
    /// </summary>
    public partial class UpdateConfig : Window
    {
        public string ClientIdValue { get; set; }
        public string ClientSecretValue { get; set; }
        public string RedirectUriValue { get; set; }

        public UpdateConfig()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ClientIdValue = ClientId.Text;
            ClientSecretValue = ClientSecret.Text;
            RedirectUriValue = RedirectUri.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ClientConfig_Loaded(object sender, RoutedEventArgs e)
        {
            ClientId.Text = ClientIdValue;
            ClientSecret.Text = ClientSecretValue;
            RedirectUri.Text = RedirectUriValue;
        }
    }
}
