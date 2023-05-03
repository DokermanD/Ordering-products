using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Ordering_products.DB
{
    public class ConectionDB
    {       
        public static SqlConnection Connection { get; private set; }

        /// <summary>
        /// Открываем подключение к базе данных
        /// </summary>
        public static void ConectDB()
        {
            try
            {
                //Создаём подключение к базе данных
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ProjectDB"].ConnectionString);
                Connection = connection;
                Console.WriteLine("Подключение к базе успешно создано.");

                //Открываем подключение к базе данных
                Connection.Open();
                

                //Проверка открылось ли подключение
                if (Connection.State == ConnectionState.Open)
                {
                    Console.WriteLine("Подключение к базе успешно открыто.");
                }
                else
                {
                    Console.WriteLine("Не открылось подключение базы данных!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при открытии подключения к базе данных!\n" + e.Message);
            }
        }

        /// <summary>
        /// Закрываем подключение к базе данных
        /// </summary>
        public static void DisconnectDB()
        {
            Connection.Close();

            //Проверка закрытия подключение
            if (Connection.State == ConnectionState.Closed)
            {
                Console.WriteLine("Подключение к базе успешно закрыто.");
            }
            else
            {
                Console.WriteLine("Не закрылось подключение к базе данных!");
            }
        }

    }
}
