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
        public static void ConectDB()
        {
            try
            {
                //Создаём подключение к базе данных
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ProjectDB"].ConnectionString);
                //Открываем подключение к базе данных
                connection.Open();
                Connection = connection;

                //Проверка открылось ли подключение
                if (connection.State == ConnectionState.Open)
                {
                    Console.WriteLine("Подключение к базе успешно создано и открыто.");
                }
                else
                {
                    Console.WriteLine("Не открылось подключение базы данных!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при подключении базы данных!\n" + e.Message);
            }
           
            
        }
    }
}
