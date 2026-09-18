using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

namespace LANMonitor.Services
{
    public class NotificationService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // 1. Native Windows Toast Notification
        public void SendToastNotification(string deviceName, string ipAddress, string state)
        {
            new ToastContentBuilder()
                .AddText($"⚠️ Network Alert: {deviceName}")
                .AddText($"Device at {ipAddress} transitioned to state: {state.ToUpper()}")
                .Show();
        }

        // 2. Incoming Webhook Alert (Microsoft Teams / Slack)
        public async Task SendWebhookAlertAsync(string webhookUrl, string deviceName, string ipAddress, string state)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) return;

            try
            {
                var payload = new
                {
                    text = $"🚨 **LAN Monitor Alert** 🚨\n**Device:** {deviceName} (`{ipAddress}`)\n**New Status:** {state}\n**Timestamp:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                };

                string json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application.json");

                await _httpClient.PostAsync(webhookUrl, content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Webhook dispatch failed: {ex.Message}");
            }
        }

        // 3. SMTP Email Notification
        public async Task SendEmailAlertAsync(string smtpHost, int port, string fromEmail, string password, string toEmail, string deviceName, string ipAddress, string state)
        {
            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(toEmail)) return;

            try
            {
                using var client = new SmtpClient(smtpHost, port)
                {
                    Credentials = new System.Net.NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                using var mail = new MailMessage(fromEmail, toEmail)
                {
                    Subject = $"[Alert] LAN Device {deviceName} is {state}",
                    Body = $"Critical Network Alert:\n\nDevice: {deviceName}\nIP Address: {ipAddress}\nNew State: {state}\nTime: {DateTime.UtcNow} UTC\n\nPlease inspect local network hardware."
                };

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email dispatch failed: {ex.Message}");
            }
        }
    }
}