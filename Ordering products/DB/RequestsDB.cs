using Ordering_products.Methods;
using Ordering_products.Telegram;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Ordering_products.DB
{

    internal class RequestsDB
    { 
        //Обьект для блока доступа в многопотоке
        static Object lockList = new Object();

        /// <summary>
        /// INSERT Добавление строки в базу данных
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="values">Масив значений для записи</param>
        public static void SetDataDB(Update update, string tableName, params string[] values)
        {
            //Открываем подключение
            ConectionDB.ConectDB();
            //Экземпляр класса SqlCommand
            SqlCommand command;

            switch (tableName)
            {
                case "RegisteredUsers":
                    var IDDateRegistrationsChat = values[5].Replace(", ","/");//Форматирование даты в формат (месяц/день/год)

                    //Строка добавления данных в DB таблица RegisteredUsers
                    command = new SqlCommand(
                    "INSERT INTO RegisteredUsers(IDChat, Name, NameOrganization, AdresDostavki, Telefon, DateRegistrations)" +
                    $" VALUES ('{values[0]}',N'{values[1]}', N'{values[2]}', N'{values[3]}', '{values[4]}', '{IDDateRegistrationsChat}')", ConectionDB.Connection);
                    
                    //Выполнения запроса на добавление и удаление временных данных
                    if (command.ExecuteNonQuery() == 1)
                    {
                        Console.WriteLine("Данные успешно добавлены в таблицу DB RegisteredUsers");

                        lock (lockList)//Блокировка для многопоточного доступа к листу
                        {
                            for (int i = 0; i < StartTgBot.UsersData.Count; i++)
                            {
                                if (StartTgBot.UsersData[i].Split('|')[0] == update.Message.Chat.Id.ToString())
                                {
                                    StartTgBot.UsersData.RemoveAt(i);//Удаление из временной таблицы строки с данными регистрации
                                    StatusID.StatusDelete(update.Message.Chat.Id.ToString());//Удаление из словаря данных по ID чата
                                    Console.WriteLine("Данные успешно удалены из временной таблицы");
                                }
                            }
                        }
                    } 
                    else Console.WriteLine("Ошибка добавления данных в таблицу DB RegisteredUsers");
                    break;

                case "OrderHistory":

                    //Строка добавления данных в DB таблица OrderHistory
                    command = new SqlCommand(
                    "INSERT INTO OrderHistory(IdZakaza, TelegramID, NameOrganization, NameUser, Products, DateZakaza, AdresDostavki)" +
                    $" VALUES (N'{values[0]}',N'{values[1]}', N'{values[2]}', N'{values[3]}', N'{values[4]}', N'{values[5]}', N'{values[6]}')", ConectionDB.Connection);

                    //Выполнения запроса на добавление 
                    if (command.ExecuteNonQuery() == 1) Console.WriteLine("Данные успешно добавлены в таблицу DB OrderHistory");
                    else Console.WriteLine("Ошибка добавления данных в таблицу DB OrderHistory");
                    break;

                case "OrderHistoryUpdate":

                    //Строка добавления данных в DB таблица OrderHistory
                    command = new SqlCommand(
                    $"UPDATE OrderHistory SET DateDostavki = N'{values[0]}' WHERE TelegramID = '{update.Message.Chat.Id.ToString()}' AND DateDostavki = '-1'", ConectionDB.Connection);

                    //Выполнения запроса на добавление 
                    if (command.ExecuteNonQuery() == 1) Console.WriteLine("Данные успешно добавлены в таблицу DB OrderHistory");
                    else Console.WriteLine("Ошибка добавления данных в таблицу DB OrderHistory");
                    break;

                case "ProductSave":
                    //Строка добавления данных в DB таблица ProductSave
                    if (values.Length == 2)//Добавление IdTelegram и Название продукта
                    {
                        command = new SqlCommand($"INSERT INTO ProductSave (IdTelegram, Product) VALUES ('{values[0]}', N'{values[1]}')", ConectionDB.Connection);

                        if (command.ExecuteNonQuery() == 1) Console.WriteLine($"Сохранил продукт - {values[1]}");
                    }
                    else //Добавление количества продуктов в кг или штук
                    {
                        command = new SqlCommand($"UPDATE ProductSave SET Count = N'{values[0]}' WHERE IdTelegram = '{update.Message.Chat.Id.ToString()}' AND Count = '-1'", ConectionDB.Connection);

                        if (command.ExecuteNonQuery() == 1) Console.WriteLine($"Сохранил количество - {values[0]}");
                    }
                    break;

                case "TableProducts":
                    break;
            }

            //Закрываем подключение
            ConectionDB.DisconnectDB();
        }

        /// <summary>
        /// Проверка ID в базе и в словаре 
        /// </summary>
        /// <param name="id">ID чата</param>
        /// <returns>Возвращает null или ok</returns>
        public static string CheckIdDataDB(string id)
        {
            string result = string.Empty;
            //Открываем подключение
            ConectionDB.ConectDB();
            //Проверка ID в базе данных 
            SqlCommand command = new SqlCommand(
            $"SELECT * FROM RegisteredUsers WHERE IDChat = '{id}'", ConectionDB.Connection);

            if (command.ExecuteScalar() == null)
            {
                //Проверка статуса в словаре StatusID
                if (StatusID.CheckStatusId(id) == null)
                {
                    StatusID.StatusAdd(id, "reg-0");
                    Console.WriteLine("Запущена регистрация");
                    result = StatusID.CheckStatusId(id);
                }
                else
                { 
                    result = StatusID.CheckStatusId(id);

                    //Увеличиваем статус регистрации на 1
                    var newStatus = Convert.ToInt32(result.Split('-')[1]);
                    newStatus++;
                    StatusID.StatusUpdate(id, $"reg-{newStatus}");
                    result = StatusID.CheckStatusId(id);
                }
            }
            else
            {
                result = "ok";
                Console.WriteLine("Пользователь зарегистрирован");
            }
                
            //Закрываем подключение
            ConectionDB.DisconnectDB();

            return result;
        }
        
        /// <summary>
        /// Метод получает все продукты из выбранной категории
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Возвращает список продуктов</returns>
        public static List<string> GetDataDB(string category)
        {
            List<string> selectDb = new List<string>(); 
            string result = string.Empty;
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader = null;
            SqlCommand command = new SqlCommand($"SELECT NameProduct FROM TableProducts WHERE Catigory = N'{category}'", ConectionDB.Connection);
            dataReader = command.ExecuteReader();
            
            while (dataReader.Read()) 
            {
                selectDb.Add(Convert.ToString(dataReader[0]));
            }

            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();

            return selectDb;
        }

        /// <summary>
        /// Проверка есть ли продукт со значением -1 по заданному idTelegram
        /// </summary>
        /// <param name="idTelegram"></param>
        /// <returns></returns>
        public static string CheckSaveProduct(string idTelegram)
        {
            // Открываем подключение
            ConectionDB.ConectDB();
            //Проверка есть ли продукт со значением -1 по заданному idTelegram
            SqlCommand command = new SqlCommand($"SELECT Count FROM ProductSave WHERE IdTelegram = N'{idTelegram}' AND Count = N'-1'", ConectionDB.Connection);
            string rez = null;
            try
            {
                rez = command.ExecuteScalar().ToString();
            }
            catch (Exception)
            {
            }
                        
            ConectionDB.DisconnectDB();
            return rez;
        }

        /// <summary>
        /// Проверка есть ли строка без даты доставки по заданному idTelegram
        /// </summary>
        /// <param name="idTelegram"></param>
        /// <returns></returns>
        public static string CheckArrangeDelivery(string idTelegram)
        {
            // Открываем подключение
            ConectionDB.ConectDB();
            //Проверка есть ли строка со значением -1 по заданному idTelegram
            SqlCommand command = new SqlCommand($"SELECT DateDostavki FROM OrderHistory WHERE TelegramID = N'{idTelegram}' AND DateDostavki = N'-1'", ConectionDB.Connection);
            string rez = null;
            try
            {
                rez = command.ExecuteScalar().ToString();
            }
            catch (Exception)
            {
            }

            ConectionDB.DisconnectDB();
            return rez;
        }
        /// <summary>
        /// Удаление всех выбранных продуктов из базы по Id чата
        /// </summary>
        /// <param name="idTelegram"></param>
        public static void DeleteProduct(string idTelegram)
        {
            // Открываем подключение
            ConectionDB.ConectDB();
            //Проверка есть ли продукт со значением -1 по заданному idTelegram
            SqlCommand command = new SqlCommand($"DELETE FROM ProductSave WHERE IdTelegram = N'{idTelegram}'", ConectionDB.Connection);
            command.ExecuteNonQuery();

            ConectionDB.DisconnectDB();
        }

        /// <summary>
        /// Метод выводит собранный заказ для подтверждения
        /// </summary>
        /// <param name="update"></param>
        /// <param name="botClient"></param>
        internal static void OrderPreview(Update update, ITelegramBotClient botClient)
        {
            string spisok = null;
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader = null;
            SqlCommand command = new SqlCommand($"SELECT Products FROM OrderHistory WHERE TelegramID = N'{update.Message.Chat.Id.ToString()}' AND Status = '-1'", ConectionDB.Connection);
            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                spisok = Convert.ToString(dataReader[0]);
            }

            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();

            //Выводим список и кнопку подтвердить
            SelectionProducts.DeleteMessageOldCallback(update, botClient);
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Подтвердить оформление заказа", callbackData: "finish")
                }
            });
            botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: $"Ваш заказ:\n\n{spisok}", replyMarkup: replyKeyboardMarkup);
        }

        internal static void OrderFinish(Update update, ITelegramBotClient botClient)
        {
            string IdZakaza = null;
            string NameOrganization = null;
            string NameUser = null;
            string Products = null;
            string DateZakaza = null;
            string AdresDostavki = null;
            string DateDostavki = null;

            string tgId = update.CallbackQuery.Message.Chat.Id.ToString();
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader = null;
            SqlCommand command = new SqlCommand($"SELECT IdZakaza, NameOrganization, NameUser, Products, DateZakaza, AdresDostavki, DateDostavki FROM OrderHistory WHERE TelegramID = N'{tgId}' AND Status = '-1'", ConectionDB.Connection);
            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                IdZakaza = Convert.ToString(dataReader[0]);
                NameOrganization = Convert.ToString(dataReader[1]);
                NameUser = Convert.ToString(dataReader[2]);
                Products = Convert.ToString(dataReader[3]);
                DateZakaza = Convert.ToString(dataReader[4]);
                AdresDostavki = Convert.ToString(dataReader[5]);
                DateDostavki = Convert.ToString(dataReader[6]);
            }

            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();

            //Выводим финальный список
            SelectionProducts.DeleteMessageOldCallback(update, botClient);
            RequestsDB.DeleteProduct(tgId);

            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Начать выбор продуктов", callbackData: "category")
                }
            });
            botClient.SendTextMessageAsync(tgId, text:
                $"Заказ №{IdZakaza}\n" +
                $"От {DateZakaza}\n\n"+
                $"Заказчик:\n" +
                $"{NameUser}\n" +
                $"{NameOrganization}\n\n" +
                $"Адрес доставки:\n" +
                $"{AdresDostavki}\n" +
                $"Дата доставки:\n" +
                $"{DateDostavki}\n\n" +
                $"Список продуктов:\n" +
                $"{Products}", replyMarkup: replyKeyboardMarkup);
        }
    }
}
