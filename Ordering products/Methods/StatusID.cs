using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering_products.Methods
{
    public class StatusID
    {
        //Словарь для временного хранения статуса пользователя (Регистрация, Сбор заказа)
        static Dictionary<string, string> StatusId = new Dictionary<string, string>();

        /// <summary>
        /// Добавление данных в словарь
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <param name="status">Значение</param>
        public static void StatusAdd(string id, string status)
        {
            var rezaltAdd = StatusId.TryAdd(id, status);
        }

        /// <summary>
        /// Удаление данных по ключу
        /// </summary>
        /// <param name="id">Ключ</param>
        public static void StatusDelete(string id)
        {
            var rezaltDelete = StatusId.Remove(id);
        }

        /// <summary>
        /// Обновление статуса
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <param name="status">Новое значение</param>
        public static void StatusUpdate(string id, string status)
        {
            StatusId[id] = status;
        }

        /// <summary>
        /// Метот проверки статуса Id
        /// </summary>
        /// <param name="id">Id чата</param>
        /// <returns>Если есть Id в словаре возвращает статус, если нет вернёт null</returns>
        public static string CheckStatusId(string id)
        {
            var rezaltContains = StatusId.ContainsKey(id);
            if (rezaltContains) 
            {
                var status = StatusId[id];
                return status;
            }

            return null;
        }
    }
} 
