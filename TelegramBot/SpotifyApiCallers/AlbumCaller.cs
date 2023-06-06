using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.SpotifyApiCallers
{
    public class AlbumCaller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5236/api";

        public AlbumCaller()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAlbum(string albumId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Album/{albumId}");
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при отриманні інформації про альбом: {ex.Message}");
                throw;
            }
        }
    }
}
