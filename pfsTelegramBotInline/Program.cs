using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace pfsTelegramBotInline
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("203067277:AAGzj4MXygP0FIWjoC80Oog0nTOQJGKXbEI");

        static void Main(string[] args)
        {

            Bot.MessageReceived += async (sender, e) =>
            {
                var keyboard = new InlineKeyboardMarkup(
               new InlineKeyboardButton[][]
               {
                        // First row
                        new [] {
                            // First column
                            new InlineKeyboardButton("1.1"),

                            // Second column
                            new InlineKeyboardButton("1.2")
                        },
                        // Second row
                        new [] {
                            // First column
                            new InlineKeyboardButton("2.1"),

                            // Second column
                            new InlineKeyboardButton("2.2")
                        } }
               );
                await Bot.SendTextMessageAsync(e.Message.Chat.Id, "tap on the keyboard", replyMarkup: keyboard);
            };

            Bot.CallbackQueryReceived += async (sender, e) =>
            {
                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"You have chosen the following: {e.CallbackQuery.Data}");
            };

            Bot.StartReceiving();

            Console.ReadLine();

            Bot.StopReceiving();
        }
    }
}
