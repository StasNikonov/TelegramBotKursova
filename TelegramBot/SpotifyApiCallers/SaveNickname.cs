using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.SpotifyApiCallers
{
    public class SaveNicknames
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseUrl = "http://localhost:5236/api";

        public SaveNicknames()
        {
            httpClient = new HttpClient();
        }

        public async Task SaveNicknameToApi(string nickname)
        {
            var apiUrl = $"{ApiBaseUrl}/nickname/setusers";

            var content = new StringContent(JsonConvert.SerializeObject(new { NewName = nickname }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Никнейм успешно сохранен в базе данных.");
            }
            else
            {
                Console.WriteLine("Произошла ошибка при сохранении никнейма.");
            }
        }
        public async Task UpdateNicknameInApi(string newName)
        {
            using (var httpClient = new HttpClient())
            {
                var apiUrl = $"{ApiBaseUrl}/nickname/updateusers";
                var requestData = new Dictionary<string, string>
        {
            { "newName", newName }
        };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(apiUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Update nickname request failed with status code {response.StatusCode}");
                }
            }
        }

    }

}

