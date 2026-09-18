using System.Windows;
using LANMonitor.Services;

namespace LANMonitor
{
    public partial class SettingsWindow : Window
    {
        private readonly DatabaseManager _dbManager;

        public SettingsWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;
            LoadSettings();
        }

        private void LoadSettings()
        {
            TxtWebhookUrl.Text = _dbManager.GetSetting("WebhookUrl");
            TxtSmtpHost.Text = _dbManager.GetSetting("SmtpHost");
            TxtSmtpPort.Text = _dbManager.GetSetting("SmtpPort", "587");
            TxtFromEmail.Text = _dbManager.GetSetting("FromEmail");
            TxtToEmail.Text = _dbManager.GetSetting("ToEmail");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _dbManager.SaveSetting("WebhookUrl", TxtWebhookUrl.Text.Trim());
            _dbManager.SaveSetting("SmtpHost", TxtSmtpHost.Text.Trim());
            _dbManager.SaveSetting("SmtpPort", TxtSmtpPort.Text.Trim());
            _dbManager.SaveSetting("FromEmail", TxtFromEmail.Text.Trim());
            _dbManager.SaveSetting("ToEmail", TxtToEmail.Text.Trim());

            MessageBox.Show("Settings saved successfully.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
    }
}