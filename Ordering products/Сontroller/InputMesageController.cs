using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Threading.Tasks;

namespace Ordering_products.Сontroller
{
    internal class InputMesageController
    {
        //Проверка всех входящих сообщений с сервера
        async public Task InputMesageAsunc(ITelegramBotClient botClient, Update update)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)//Любой текст
            {
                // 1 - Проверка ID в базе RegisteredUsers и его статуса.
               

                

            }
            else if (update.Type == UpdateType.CallbackQuery)//Ответ на кнопки коллбэк
            {
               
            }
        }
    }
}
