using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Threading.Tasks;
using Ordering_products.DB;
using Ordering_products.Methods;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Ordering_products.Сontroller
{
    internal class InputMesageController
    {
        /// <summary>
        /// Метод распределяет пользователей на регистрацию или на сбор продуктов
        /// </summary>
        /// <param name="update"></param>
        public static Task MesageController(ITelegramBotClient botClient, Update update)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)//Любой текст
            {
                // 1 - Проверка ID в базе RegisteredUsers и его статуса.
                var result = RequestsDB.CheckIdDataDB(update.Message.Chat.Id.ToString());

                // 2 - Работаем с результатом проверки
                switch (result)
                {
                    case "ok":
                        //Запуск подбора продуктов
                        SelectionProducts.SelectProd(botClient, update, result);
                        break;

                    default:
                        //Процес регистрации
                        Registration.RegistrationNewUser(botClient, update, result);
                        break;
                }
                
            }
            else if (update.Type == UpdateType.CallbackQuery)//Ответ на кнопки коллбэк
            {
                // 1 - Проверка ID в базе RegisteredUsers и его статуса.
                var result = RequestsDB.CheckIdDataDB(update.CallbackQuery.Message.Chat.Id.ToString());
                var callbackData = update.CallbackQuery.Data.ToString();

                //Работаем с результатом
                SelectionProducts.SelectProd(botClient, update, callbackData);
            }

            return Task.CompletedTask;
        }


    }
}
