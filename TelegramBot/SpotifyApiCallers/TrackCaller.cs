using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.SpotifyApiCallers
{
    public class TrackCaller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5236/api";

        public TrackCaller()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetTrack(string trackId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Track/{trackId}");
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при отриманні інформації про трек: {ex.Message}");
                throw;
            }
        }
    }
}
