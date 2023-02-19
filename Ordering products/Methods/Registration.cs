using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using Ordering_products.Telegram;
using System.Linq;
using Ordering_products.DB;

namespace Ordering_products.Methods
{
    public class Registration
    {
        // Блокировка листа
        static Object lockList = new Object();

        /// <summary>
        /// Метод проводит опрос нового клиента
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="status">Текущий статус</param>
        public static void RegistrationNewUser(ITelegramBotClient botClient, Update update, string status)
        {
            switch (status)
            {
                case "reg-0":
                    botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Здравствуйте!\nДавайте пройдём быструю регистрацию.\nКак я могу к вам обращатся?", parseMode: ParseMode.Markdown);
                    AddMesageTextAsync(update, status);
                    break;

                case "reg-1":
                    botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Как называется ваша организация?", parseMode: ParseMode.Markdown);
                    AddMesageTextAsync(update, status);
                    break;

                case "reg-2":
                    botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Адрес куда доставлять?", parseMode: ParseMode.Markdown);
                    AddMesageTextAsync(update, status);
                    break;

                case "reg-3":
                    botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Укажите пожалуйста телефон для связи.", parseMode: ParseMode.Markdown);
                    AddMesageTextAsync(update, status);
                    break;

                case "reg-4":
                    botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, text: "Отлично Вы прошли регистрацию!", parseMode: ParseMode.Markdown);
                    AddMesageTextAsync(update, status);
                    break;
            }
        }

        /// <summary>
        /// Метод добавляет данные введённые пользователям в список перезаписывая строки с нужным ID
        /// </summary>
        /// <param name="update"></param>
        /// <param name="resultInputUser"></param>
        public static void AddMesageTextAsync(Update update, string status)
        {
            //await Task.Run(() =>
            //{
                var check = true;
            //Проверка есть ли idChat в списке
            for (int i = 0; i < StartTgBot.UsersData.Count; i++)
            {
                lock (lockList)
                {
                    if (StartTgBot.UsersData[i].Contains(update.Message.Chat.Id.ToString()))
                    {
                        var oldStr = StartTgBot.UsersData[i];
                        StartTgBot.UsersData.RemoveAt(i);
                        var newStr = $"{oldStr}|{update.Message.Text}";
                        StartTgBot.UsersData.Add(newStr);
                        check = false;
                        break;
                    }
                }
            }

            //Проверка на конец регистрации и запись в базу данных
            if (status == "reg-4")
            {
                //Находим строку
                var regStrLinq = StartTgBot.UsersData.Where(x => x.Contains(update.Message.Chat.Id.ToString()));
                var regStr = "";
                foreach (var str in regStrLinq)
                {
                    regStr = str;
                }
                //Пишем строку в базу данных
                RequestsDB.SetDataDB(update, "RegisteredUsers", regStr.Split('|')[0], regStr.Split('|')[1], regStr.Split('|')[2], regStr.Split('|')[3], regStr.Split('|')[4], DateTime.Now.ToString("MM/dd/yyyy").ToString());
            }
                
            if (check)
            {
                lock (lockList)
                {
                    StartTgBot.UsersData.Add(update.Message.Chat.Id.ToString());
                }
            }
            //});
        }
    }
}
