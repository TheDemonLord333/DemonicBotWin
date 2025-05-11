using DemonicBotWin.WinForms.Models;
using DemonicBotWin.WinForms.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DemonicBotWin.WinForms.Services
{
    public interface IApiService
    {
        Task<bool> InitializeAsync();
        Task<List<DiscordServer>> GetServersAsync();
        Task<List<DiscordChannel>> GetChannelsAsync(string serverId);
        Task<bool> SendEmbedAsync(string channelId, EmbedMessage embed);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;

        private string _apiUrl;
        private string _apiSecret;

        public ApiService(ISettingsService settingsService)
        {
            _httpClient = new HttpClient();
            _settingsService = settingsService;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _apiUrl = _settingsService.GetSetting(SettingsKeys.API_URL_KEY);
                _apiSecret = _settingsService.GetSetting(SettingsKeys.API_SECRET_KEY);

                if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiSecret))
                {
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Secret", _apiSecret);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApiService initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<DiscordServer>> GetServersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/api/servers");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ServerResponse>(json);
                    return result?.Guilds ?? new List<DiscordServer>();
                }

                return new List<DiscordServer>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching servers: {ex.Message}");
                return new List<DiscordServer>();
            }
        }

        public async Task<List<DiscordChannel>> GetChannelsAsync(string serverId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/api/channels/{serverId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ChannelResponse>(json);
                    return result?.Channels ?? new List<DiscordChannel>();
                }

                return new List<DiscordChannel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching channels: {ex.Message}");
                return new List<DiscordChannel>();
            }
        }

        public async Task<bool> SendEmbedAsync(string channelId, EmbedMessage embed)
        {
            try
            {
                var payload = new SendEmbedRequest
                {
                    ChannelId = channelId,
                    EmbedData = embed
                };

                var jsonContent = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiUrl}/api/send-embed", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending embed: {ex.Message}");
                return false;
            }
        }

        private class ServerResponse
        {
            public List<DiscordServer> Guilds { get; set; }
        }

        private class ChannelResponse
        {
            public List<DiscordChannel> Channels { get; set; }
        }

        private class SendEmbedRequest
        {
            public string ChannelId { get; set; }
            public EmbedMessage EmbedData { get; set; }
        }
    }
}