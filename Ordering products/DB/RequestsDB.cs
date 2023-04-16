using Ordering_products.Methods;
using Ordering_products.Telegram;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Telegram.Bot.Types;

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
    }
}
