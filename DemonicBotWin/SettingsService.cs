using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DemonicBotWin.WinForms.Services
{
    public interface ISettingsService
    {
        string GetSetting(string key);
        void SaveSetting(string key, string value);
        bool HasSetting(string key);
        void ClearSetting(string key);
    }

    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly byte[] _entropy = { 9, 8, 7, 6, 5 }; // For encryption

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DemonicBot");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "settings.dat");
        }

        public string GetSetting(string key)
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return string.Empty;

                var lines = File.ReadAllLines(_settingsFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2 && parts[0] == key)
                    {
                        var encryptedData = Convert.FromBase64String(parts[1]);
                        var decryptedData = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
                        return Encoding.UTF8.GetString(decryptedData);
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading setting '{key}': {ex.Message}");
                return string.Empty;
            }
        }

        public void SaveSetting(string key, string value)
        {
            try
            {
                var encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(value),
                    _entropy,
                    DataProtectionScope.CurrentUser);
                var encryptedString = Convert.ToBase64String(encryptedData);

                var lines = File.Exists(_settingsFilePath) ? File.ReadAllLines(_settingsFilePath).ToList() : new List<string>();
                var existingIndex = lines.FindIndex(l => l.StartsWith(key + "="));

                if (existingIndex >= 0)
                {
                    lines[existingIndex] = $"{key}={encryptedString}";
                }
                else
                {
                    lines.Add($"{key}={encryptedString}");
                }

                File.WriteAllLines(_settingsFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving setting '{key}': {ex.Message}");
            }
        }

        public bool HasSetting(string key)
        {
            return !string.IsNullOrEmpty(GetSetting(key));
        }

        public void ClearSetting(string key)
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return;

                var lines = File.ReadAllLines(_settingsFilePath).ToList();
                lines.RemoveAll(l => l.StartsWith(key + "="));
                File.WriteAllLines(_settingsFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing setting '{key}': {ex.Message}");
            }
        }
    }

    public static class SettingsKeys
    {
        public const string API_URL_KEY = "ApiUrl";
        public const string API_SECRET_KEY = "ApiSecret";
        public const string USER_NAME_KEY = "UserName";
    }
}
