using Ordering_products.DB;
using Ordering_products.Telegram;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Ordering_products
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Запуск прослушки сервера
            StartTgBot.Start();
    
        }
    }
}
