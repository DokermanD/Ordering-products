using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering_products.Methods
{
    public class StatusID
    {
        //Словарь для временного хранения статуса пользователя (Регистрация, Сбор заказа)
        Dictionary<string, string> StatusId = new Dictionary<string, string>();

        /// <summary>
        /// Добавление данных в словарь
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <param name="status">Значение</param>
        public void StatusAdd(string id, string status)
        {
            var rezaltAdd = StatusId.TryAdd(id, status);
        }

        /// <summary>
        /// Удаление данных по ключу
        /// </summary>
        /// <param name="id">Ключ</param>
        public void StatusDelete(string id)
        {
            var rezaltDelete = StatusId.Remove(id);
        }

        /// <summary>
        /// Обновление статуса
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <param name="status">Значение</param>
        public void StatusUpdate(string id, string status)
        {
            var rezaltDelete = StatusId.Remove(id);
            var rezaltAdd = StatusId.TryAdd(id, status);
        }
    }
}
