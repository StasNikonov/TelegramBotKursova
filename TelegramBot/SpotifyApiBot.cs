using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using TelegramBot;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using TelegramBot.SpotifyApiCallers;

namespace TelegramBot
{
    public class SpotifyApiBot
    {

        TelegramBotClient botClient = new TelegramBotClient("6057729269:AAHSkiqRRoU5H19TSVkvr_8SpB_W2hnGhRM");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        private readonly SaveNicknames saveNicknames;

        public SpotifyApiBot()
        {
            saveNicknames = new SaveNicknames();
        }
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Bot {botMe.Username} get started");
            Console.ReadKey();
        }
        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;

        }
        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
        }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "/authorization", "/track" }, new KeyboardButton[] { "/author", "/album" } }) { ResizeKeyboard = true };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Привіт! Це телеграм бот в якому ви можете отримати цікаву інформацію від Спотіфаю про виконавця, альбом, трек. Для початку пройдіть авторизацію.", replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "/authorization")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Уведіть свій Нікнейм у вигляді \n'/username ім'я': ");
            }
            if(message.Text == "/author")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Уведіть посилання на Спотіфай аккаунт автора про якого ви хочете дізнатися щось більше: ");
            }
            if(message.Text.StartsWith("https://open.spotify.com/artist/"))
            {
                await GiveInformationAboutAuthor(botClient, message);
            }
            if (message.Text == "/track")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Уведіть посилання на трек у Спотіфаю про який ви хочете дізнатися щось більше: ");
            }
            if (message.Text.StartsWith("https://open.spotify.com/track/"))
            {
                await GiveInformationAboutTrack(botClient, message);
            }
            if (message.Text == "/album")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Уведіть посилання на альбом у Спотіфаю про який ви хочете дізнатися щось більше: ");
            }
            if (message.Text.StartsWith("https://open.spotify.com/album/"))
            {
                await GiveInformationAboutAlbum(botClient, message);
            }
            if (message.Text.StartsWith("/username"))
            {
                await HandleSetNicknameMessage(botClient, message);
            }
            if (message.Text.StartsWith("/update"))
            {
                await HandleUpdateNicknameMessage(botClient, message);
            }
        }
        private async Task GiveInformationAboutTrack(ITelegramBotClient botClient, Message message)
        {
            string spotifyLink = message.Text;
            string[] parts = spotifyLink.Split('/');
            string trackId = parts[^1];
            try
            {
                var getTrack = new TrackCaller();
                var authorInfo = await getTrack.GetTrack(trackId);
                var jsonObject = JObject.Parse(authorInfo);
                JArray artistsArray = (JArray)jsonObject["artists"];
                string artistName = (string)artistsArray[0]["name"].ToString();
                string output = $"Ім'я: " + jsonObject["name"].ToString() + "\n";
                output += $"Ім'я виконавця: {artistName}\n";
                output += $"Довжина треку в мілісекундах: " + jsonObject["duration_ms"].ToString() + "ms\n";
                output += $"Популярність треку: " + jsonObject["popularity"].ToString() + "\n";
                output += $"Тип треку: " + jsonObject["type"].ToString() + "\n";
                output += $"Зовнішнє посилання: " + jsonObject["external_urls"]["href"].ToString() + "\n";


                await botClient.SendTextMessageAsync(message.Chat.Id, output);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при отриманні даних про автора: " + ex.Message);
            }
        }
        private async Task GiveInformationAboutAuthor(ITelegramBotClient botClient, Message message)
        {
            string spotifyLink = message.Text;
            string[] parts = spotifyLink.Split('/');
            string authorId = parts[^1];

            try
            {
                var getAuthor = new AuthorCaller();
                var authorInfo = await getAuthor.GetAuthor(authorId);
                var jsonObject = JObject.Parse(authorInfo);
                JArray genresArray = (JArray)jsonObject["genres"];
                string genres = string.Join(", ", genresArray.Select(g => g.ToString()));
                string output = $"Ім'я: " + jsonObject["name"].ToString() + "\n";
                output += $"Загальна кількість фоловерів: " + (int)jsonObject["followers"]["total"] + "\n";
                output += $"Жанри: {genres}\n";
                output += $"Зовнішнє посилання: " + jsonObject["external_urls"]["href"].ToString() + "\n";

                await botClient.SendTextMessageAsync(message.Chat.Id, output);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при отриманні даних про автора: " + ex.Message);
            }
        }
        private async Task GiveInformationAboutAlbum(ITelegramBotClient botClient, Message message)
        {
            string spotifyLink = message.Text;
            string[] parts = spotifyLink.Split('/');
            string trackId = parts[^1];
            try
            {
                var getAlbum = new AlbumCaller();
                var albumInfo = await getAlbum.GetAlbum(trackId);
                var jsonObject = JObject.Parse(albumInfo);
                JArray artistsArray = (JArray)jsonObject["artists"];
                string artistName = (string)artistsArray[0]["name"].ToString();
                string output = $"Ім'я: " + jsonObject["name"].ToString() + "\n";
                output += $"Ім'я виконавця: {artistName}\n";
                output += $"Дата релізу: " + jsonObject["release_date"].ToString() + "\n";
                output += $"Кількість треків в альбомі: " + jsonObject["total_tracks"].ToString() + "\n";
                output += $"Тип альбому: " + jsonObject["album_type"].ToString() + "\n";
                output += $"Зовнішнє посилання: " + jsonObject["external_urls"]["href"].ToString() + "\n";


                await botClient.SendTextMessageAsync(message.Chat.Id, output);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при отриманні даних про автора: " + ex.Message);
            }
        }
        private async Task HandleUpdateNicknameMessage(ITelegramBotClient botClient, Message message)
        {
            string[] parts = message.Text.Split(' ');
            string name = parts[^1];
            await saveNicknames.UpdateNicknameInApi(name);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Твій нік {name} успішно оновлений");
        }
        private async Task HandleSetNicknameMessage(ITelegramBotClient botClient, Message message)
        {
            string[] parts = message.Text.Split(' ');
            string name = parts[^1];
            await saveNicknames.SaveNicknameToApi(name);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Твій нік {name} успішно збережений");
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Вітаю, {name}, тепер ти можеш використовувати увесь функціонал цього боту, успіхів!");
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Також ви завжди зможете оновити свій нік, для цього просто напишіть \n'/update нік'");
        }
    }
}
