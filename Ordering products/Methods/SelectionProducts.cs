using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Ordering_products.DB;
using Newtonsoft.Json.Linq;

namespace Ordering_products.Methods
{
    internal class SelectionProducts
    {
        public static void SelectProd(ITelegramBotClient botClient, Update update, string callbackData)
        {
            switch (callbackData)
            {
                case "category":
                case "ok":
                    //Вывод всех категорий для выбора
                    SelectCategory(botClient, update);
                    break;

                case "Фрукты":
                case "Овощи":
                case "Рыба":
                    SelectProduct(botClient, update, callbackData);
                    break;

                    //TODO - обработка нажатия на тпродукт
            }
           
        }

        /// <summary>
        /// Метод выводит все категории для выбора
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        public static void SelectCategory(ITelegramBotClient botClient, Update update)
        {
            DeleteMessageOldCallback(update, botClient);
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Фрукты", callbackData: "Фрукты"),
                    InlineKeyboardButton.WithCallbackData(text: "Овощи", callbackData: "Овощи"),
                    InlineKeyboardButton.WithCallbackData(text: "Рыба", callbackData: "Рыба")
                }
            });

            //Отправка сообщения в бота
            if (update.Type == UpdateType.CallbackQuery)
            {
                botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(), text: "Выберите пожалуйста категорию продукта.", replyMarkup: replyKeyboardMarkup);
            }
            else
            {
                botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id.ToString(), text: "Выберите пожалуйста категорию продукта.", replyMarkup: replyKeyboardMarkup);
            }
            
        }

        /// <summary>
        /// Метод выводит все продукты в выбранной категории
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="product"></param>
        public static void SelectProduct(ITelegramBotClient botClient, Update update, string product)
        {
            DeleteMessageOldCallback(update, botClient);
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(GetProductDb(product));

            //Отправка сообщения в бота
            if (update.Type == UpdateType.CallbackQuery)
            {
                botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(), text: "Выберите пожалуйста продукт.", replyMarkup: replyKeyboardMarkup);
            }
            else
            {
                botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id.ToString(), text: "Выберите пожалуйста продукт.", replyMarkup: replyKeyboardMarkup);
            }
            
        }

        /// <summary>
        /// Выборка из базы всех продуктов по выбранной категории и сборка в масив
        /// </summary>
        /// <param name="product">Название категории продукта</param>
        /// <returns>Возвращает масив с названием продуктов</returns>
        public static InlineKeyboardButton[][] GetProductDb(string product)
        {
            //Получаем список продуктов из базы данных
            var listSelect = RequestsDB.GetDataDB(product);

            InlineKeyboardButton[][] inlineKeyboardButtons = new InlineKeyboardButton[(int)Math.Ceiling((double)listSelect.Count / 3) + 1][];

            int a;
            int schet = 0;
            int countListProducts = listSelect.Count;

            for (int i = 0; i < inlineKeyboardButtons.Length - 1; i++)
            {
                var reset = 0;
                var b = 0;

                //Расчитываем размер масива
                countListProducts = countListProducts - 3;
                var arrycoll = countListProducts < 0 ? countListProducts + 3 : 3;

                InlineKeyboardButton[] array = new InlineKeyboardButton[arrycoll];
                for (a = schet; a < listSelect.Count; a++)
                {
                    reset++;
                    array[b] = InlineKeyboardButton.WithCallbackData(text: $"{listSelect[a]}", callbackData: $"{listSelect[a]}");
                    schet++;
                    b++;
                    if (reset == 3) break;
                }
                inlineKeyboardButtons[i] = array;

            }

            //Вывод кнопки возврата в категории
            InlineKeyboardButton[] arrayBackCategory = new InlineKeyboardButton[1];
            arrayBackCategory[0] = InlineKeyboardButton.WithCallbackData(text: $"<< Вернуться в категории", callbackData: $"category");
            inlineKeyboardButtons[inlineKeyboardButtons.Length - 1] = arrayBackCategory;

            return inlineKeyboardButtons;
        }



        /// <summary>
        /// Метод удаляет 1 последних сообщения
        /// </summary>
        /// <param name="update"></param>
        /// <param name="botClient"></param>
        public static void DeleteMessageOldCallback(Update update, ITelegramBotClient botClient)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId-1);
            }
            else
            {
                botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId - 1);
            }
            
        }
    }
}
