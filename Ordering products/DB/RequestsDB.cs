using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Ordering_products.DB
{

    internal class RequestsDB
    {
        /// <summary>
        /// INSERT Добавление строки в базу данных
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="values">Масив значений для записи</param>
        public static void SetDataDB(string tableName, params string[] values)
        {           
            SqlCommand command = new SqlCommand(
                $"INSERT INTO [{tableName}] (FIO, NameOrganization, AdresDostavki, Telefon, DateRegistrations)" +
                $" VALUES (N'{values[0]}',N'{values[1]}', N'{values[2]}', N'{values[3]}', '{values[4]}')", ConectionDB.Connection);

            if (command.ExecuteNonQuery() == 1)
            {
                Console.WriteLine("Данные успешно добавлены в DB");
            }
            
        }


    }
}
