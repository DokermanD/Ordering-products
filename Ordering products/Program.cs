using Ordering_products.DB;
using Ordering_products.Telegram;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Ordering_products
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Подключение к базе данных
            ConectionDB.ConectDB();

            //Запуск прослушки сервера
            StartTgBot.Start();

            RequestsDB.SetDataDB("RegisteredUsers", "Лена","Орифлейм", "Ленинский 51 кв. 859", "+79214048399", DateTime.Now.ToString("G"));
    
        }
    }
}
