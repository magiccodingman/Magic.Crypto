using Crypto.MauiBlazor.Helpers;
using Crypto.MauiBlazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Crypto.MauiBlazor.Service
{
    public class SettingsService
    {
        private readonly string settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
        private readonly string hashFilePath = Path.Combine(FileSystem.AppDataDirectory, "password.hash");

        public async Task SaveSettingsAsync(AppSettings settings, string password)
        {
            var json = JsonSerializer.Serialize(settings);
            var encryptedJson = EncryptionUtils.Encrypt(json, password);
            await File.WriteAllTextAsync(settingsFilePath, encryptedJson);
        }

        public async Task<AppSettings> LoadSettingsAsync(string password)
        {
            if (!File.Exists(settingsFilePath))
                return new AppSettings();

            var encryptedJson = await File.ReadAllTextAsync(settingsFilePath);
            try
            {
                var json = EncryptionUtils.Decrypt(encryptedJson, password);

                if (string.IsNullOrWhiteSpace(json))
                    return new AppSettings();

                return JsonSerializer.Deserialize<AppSettings>(json);
            }
            catch
            {
                return new AppSettings();
            }
        }

        public async Task SavePasswordHashAsync(string password)
        {
            var hashedPassword = HashUtils.HashPassword(password);
            await File.WriteAllTextAsync(hashFilePath, hashedPassword);
        }

        public async Task<bool> VerifyPasswordAsync(string password)
        {
            var storedHash = await GetStoredHash();
            return HashUtils.VerifyPassword(password, storedHash);
        }

        public async Task<string?> GetStoredHash()
        {
            if (!File.Exists(hashFilePath))
                return null;

            var storedHash = await File.ReadAllTextAsync(hashFilePath);
            return storedHash;
        }

        public async Task DeleteSettingsAsync()
        {
            if (File.Exists(settingsFilePath))
                File.Delete(settingsFilePath);
            if (File.Exists(hashFilePath))
                File.Delete(hashFilePath);
        }
    }
}
