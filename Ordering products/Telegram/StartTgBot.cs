using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Ordering_products.Сontroller;
using Telegram.Bot.Types.ReplyMarkups;
using Ordering_products.DB;
using Ordering_products.Methods;
using Telegram.Bot.Types.Enums;
using System.Linq;

namespace Ordering_products.Telegram
{
    public class StartTgBot
    {
        //Создаём подключение с токеном
        static TelegramBotClient client = new TelegramBotClient("5829168895:AAGTVjwNR_30142qDTvLq29wJNS3w5yXMxQ");

        //Временное хранение данных по регистрации новых клиентов
        public static List<string> UsersData = new List<string>();
        
        /// <summary>
        /// Запуск прослушки сервера Телеграм
        /// </summary>
        public static void Start()
        {
            //Запуск прослушки сервера
            client.StartReceiving(Update, Error);
            Console.ReadKey();
        }

        //Основной метод получения сообщений от пользователя
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
           await InputMesageController.MesageController(botClient, update);
        }

        //Метоб обработки ошибок
        static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }   
}
