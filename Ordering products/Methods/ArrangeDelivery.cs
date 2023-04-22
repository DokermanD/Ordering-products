using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot;
using Ordering_products.DB;
using System.Data.SqlClient;
using System.Linq;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Data.Common;

namespace Ordering_products.Methods
{
    internal class ArrangeDelivery
    {
        static int IdZakaza = 0;
        static string TelegramID = "";
        static string NameOrganization = "";
        static string NameUser = "";
        static string AdresDostavki = "";
        static string Products = "";

        public static void SaveOrdersFinish(ITelegramBotClient botClient, Update update, string callbackData)
        {
            
            TelegramID = update.CallbackQuery.Message.Chat.Id.ToString();


            //Формируем Id заказа
            var listId = GetIdOrder();
            if (listId.Count == 0) IdZakaza = 1;
            else
            {
                var max = 0;    
                foreach (var id in listId) if (id > max) max = id;
                IdZakaza = max + 1;
            }

            //Получаем данные клиента
            GetRegestryData(TelegramID);

            //Формируем в строку выбранные продукты
            GetProductsStringData(TelegramID);

            //Добавляем строку с данными в базу без даты доставки
            string dateTimeNow = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            RequestsDB.SetDataDB(update, "OrderHistory", Convert.ToString(IdZakaza), TelegramID, NameOrganization, NameUser, Products, dateTimeNow, AdresDostavki);

            //Запрашиваем дату доставки
            SelectionProducts.DeleteMessageOldCallback(update, botClient);
            botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id.ToString(),
                text: "Укажите пожалуйста дату доставки продуктов.\nВ формате (дд.мм.гггг  чч:мм)\nНапример: 21.10.2023 12:30");

            //Чистим временный список продуктов по Id
            //
        }

        

        /// <summary>
        /// Получаем название товара и количество в переменную из базы
        /// </summary>
        /// <param name="TelegramID"></param>
        public static void GetProductsStringData(string TelegramID)
        {
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader;
            SqlCommand command = new SqlCommand($"SELECT Product, Count FROM ProductSave WHERE IdTelegram = '{TelegramID}'", ConectionDB.Connection);
            dataReader = command.ExecuteReader();

            StringBuilder stringBuilder= new StringBuilder();   
            //Перебор полученых данных
            while (dataReader.Read()) // построчно считываем данные
            {
                var product = dataReader.GetValue(0).ToString();
                var count = dataReader.GetValue(1).ToString();

                stringBuilder.AppendLine($"{product} - {count}");
            }


            Products = stringBuilder.ToString();
            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();
            stringBuilder.Clear();
        }

        /// <summary>
        /// Получаем данные по регистрации пользователя из базы
        /// </summary>
        /// <param name="TelegramID"></param>
        public static void GetRegestryData(string TelegramID)
        {            
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader;
            SqlCommand command = new SqlCommand($"SELECT NameOrganization, Name, AdresDostavki FROM RegisteredUsers WHERE IDChat = '{TelegramID}'", ConectionDB.Connection);
            dataReader = command.ExecuteReader();

            //Перебор полученых данных
            while (dataReader.Read()) // построчно считываем данные
            {
                NameOrganization = dataReader.GetValue(0).ToString();
                NameUser = dataReader.GetValue(1).ToString();
                AdresDostavki = dataReader.GetValue(2).ToString();
            }

            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();
        }

        /// <summary>
        /// Получаем последний индекс и увеличиваем на 1
        /// </summary>
        /// <returns></returns>
        public static List<int> GetIdOrder()
        {
            List<int> selectOrder = new List<int>();
            string result = string.Empty;
            //Открываем подключение
            ConectionDB.ConectDB();

            //Вытаскиваем все продукты по категории
            SqlDataReader dataReader = null;
            SqlCommand command = new SqlCommand($"SELECT IdZakaza FROM OrderHistory", ConectionDB.Connection);
            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                selectOrder.Add(Convert.ToInt32(dataReader[0]));
            }

            //Закрываем датаридер
            dataReader.Close();
            //Закрываем подключение
            ConectionDB.DisconnectDB();

            return selectOrder;
        }
    }
}
