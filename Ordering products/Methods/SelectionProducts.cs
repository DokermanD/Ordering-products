using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Ordering_products.DB;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Threading;

namespace Ordering_products.Methods
{
    internal class SelectionProducts
    {
        public static void SelectProd(ITelegramBotClient botClient, Update update, string callbackData)
        {
            if (update.Type == UpdateType.CallbackQuery)//Ответ на кнопки коллбэк
            {
                switch (callbackData)
                {
                    case "category":
                        //Вывод всех категорий для выбора
                        SelectCategory(botClient, update);
                        break;
                    case "categoryFinali":
                        //Вывод всех категорий для выбора
                        SelectCategoryFinali(botClient, update);
                        break;

                    case "Фрукты":
                    case "Овощи":
                    case "Рыба":
                        //Вывод всех продуктов из выбранной категории
                        SelectProduct(botClient, update, callbackData);
                        break;

                    case "null"://Разделитель меню и продуктов
                        break;

                    case "delivery"://Оформление доставки
                        ArrangeDelivery.SaveOrdersFinish(botClient, update, callbackData);
                        break;
                    case "reset"://Обнуление заказа с удалением из базы
                        ResetSelectProduct(botClient, update, callbackData);
                        break;

                    case "finish"://Итоговый вывод заказа со всеми данными
                        RequestsDB.OrderFinish(update, botClient);
                        break;


                    //Сохранение продукта в базу и запрос каличества в кг. или штук. (обработка нажатия на тпродукт)
                    default:
                        var rezSaveProduct = RequestsDB.CheckSaveProduct(update.CallbackQuery.Message.Chat.Id.ToString());
                        if (rezSaveProduct == null) SaveSelectProduct(botClient, update, callbackData);
                                          
                        break;
                }
            }
            else
            {
                //Проверяем есть ли в базе ProductSave продукт с незаполненым полем количества
                var rezSaveProduct = RequestsDB.CheckSaveProduct(update.Message.Chat.Id.ToString());
                //Проверяем есть ли в базе OrderHistory строка с незаполненым полем дата доставки
                var rezArrangeDelivery = RequestsDB.CheckArrangeDelivery(update.Message.Chat.Id.ToString());

                if (rezSaveProduct != null) 
                {
                    //Добавляем колличество к продукту (вводит юзер)
                    RequestsDB.SetDataDB(update, "ProductSave", update.Message.Text);
                    DeleteMessageOldCallback(update, botClient);
                }
                else if (rezArrangeDelivery != null)
                {
                    //Дописываем дату доставки
                    SetDateDelivery(botClient, update, update.Message.Text);
                }
                else
                {
                    DeleteMessageOldCallback(update, botClient);
                    //Вывод всех категорий для выбора
                    SelectCategory(botClient, update);
                }
                
            }
        }

        private static void SetDateDelivery(ITelegramBotClient botClient, Update update, string text)
        {
            //Вкидываем дату доставки в заказ
            RequestsDB.SetDataDB(update, "OrderHistoryUpdate", text);
            DeleteMessageOldCallback(update, botClient);

            // Выводим педпросмотр заказа с кнопкай подтвердить заказ
            RequestsDB.OrderPreview(update, botClient);

        }

        /// <summary>
        /// Удаление всех выбранных продуктов из базы по Id чата
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="callbackData"></param>
        private static void ResetSelectProduct(ITelegramBotClient botClient, Update update, string callbackData)
        {
            var idTelegram = update.CallbackQuery.Message.Chat.Id.ToString();
            //Удаление всех выбранных продуктов из базы по Id чата
            RequestsDB.DeleteProduct(idTelegram);

            DeleteMessageOldCallback(update, botClient);
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Начать выбор продуктов", callbackData: "category")
                }
            });
            //Сообщение о сбросе заказа
            botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(), text: "Заказ был обнулён", replyMarkup: replyKeyboardMarkup);
        }

        /// <summary>
        /// Сохранения в базу названия продукта
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="callbackData"></param>
        private static void SaveSelectProduct(ITelegramBotClient botClient, Update update, string callbackData)
        {
            var idTelegram = update.CallbackQuery.Message.Chat.Id.ToString();
            //Добавляем idTelegram и Название продукта в базу
            RequestsDB.SetDataDB(update, "ProductSave", idTelegram, callbackData);

            //Отправка сообщения в бота
            botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(), text: "Введите пожалуйста нужное количество\r\nНапример:  5 кг.   или   5 шт. ");
           
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
        /// Предложение выбора категорий после финального вывода заказа
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        public static void SelectCategoryFinali(ITelegramBotClient botClient, Update update)
        {
            //Редактируем сообщение после нажатия кнопки Начать выбор продукта, удаляем саму кнопку под сообщением
            botClient.EditMessageReplyMarkupAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(), update.CallbackQuery.Message.MessageId);

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

            InlineKeyboardButton[][] inlineKeyboardButtons = new InlineKeyboardButton[(int)Math.Ceiling((double)listSelect.Count / 3) + 3][];

            int a;
            int schet = 0;
            int countListProducts = listSelect.Count;

            for (int i = 0; i < inlineKeyboardButtons.Length - 3; i++)
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
            InlineKeyboardButton[] arrayzona = new InlineKeyboardButton[1];
            arrayzona[0] = InlineKeyboardButton.WithCallbackData(text: $"--------------------------------", callbackData: $"null");
            inlineKeyboardButtons[inlineKeyboardButtons.Length - 3] = arrayzona;

            //Вывод кнопки возврата в категории
            InlineKeyboardButton[] arrayBackCategory = new InlineKeyboardButton[1];
            arrayBackCategory[0] = InlineKeyboardButton.WithCallbackData(text: $"<< Вернуться в категории", callbackData: $"category");
            inlineKeyboardButtons[inlineKeyboardButtons.Length - 2] = arrayBackCategory;

            //Вывод кнопки возврата в категории
            InlineKeyboardButton[] arrayFinaly = new InlineKeyboardButton[2];
            arrayFinaly[0] = InlineKeyboardButton.WithCallbackData(text: $"✔ Оформить доставку", callbackData: $"delivery");
            arrayFinaly[1] = InlineKeyboardButton.WithCallbackData(text: $"✖ Обнулить заказ", callbackData: $"reset");
            inlineKeyboardButtons[inlineKeyboardButtons.Length - 1] = arrayFinaly;

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
                //botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId-1);
            }
            else
            {
                botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId - 1);
            }
            
        }
    }
}
