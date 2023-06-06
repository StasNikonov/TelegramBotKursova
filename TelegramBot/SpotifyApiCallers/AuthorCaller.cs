using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.SpotifyApiCallers
{
    public class AuthorCaller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5236/api";

        public AuthorCaller()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAuthor(string authorId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Author/{authorId}");
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при отриманні інформації про автора: {ex.Message}");
                throw;
            }
        }
    }
}
