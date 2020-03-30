using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public partial class Form1 : Form
    {


        string Token = "297996480:AAHTDqCSh7mhKSirU8XQkxboX6J4g7UfuvY"; //Токен бота-секундомера
        
        KeyboardButton leftbutton; // левая кнопка
        
        KeyboardButton righttbutton; //правая кнопка
        KeyboardButton[] row; //ряд кнопок
        ReplyKeyboardMarkup keyboard; //клавиатура

        List<StopWatch> list = new List<StopWatch>(); // лист для хранения секундомеров (каждому пользователю свой секундомер)
        private TelegramBotClient Bot; // ссылка на объект телеграмм-бот-клиента
        
        public Form1()
        {
            InitializeComponent();

            Bot = new TelegramBotClient(Token); // создаём бота
         
            #region Задание кнопок
            leftbutton = new KeyboardButton("Старт");// кнопка для пуска или интервала            
            righttbutton = new KeyboardButton("Стоп");// кнопка для остановки
            row = new KeyboardButton[2];
            row[0] = leftbutton;
            row[1] = righttbutton;            
            keyboard = new ReplyKeyboardMarkup(row);
            keyboard.ResizeKeyboard = true; 
            #endregion

            Run();

        }

        public async Task Run()
        {
            var me = await Bot.GetMeAsync();
            

            var offset = 0;
            while (true)
            {
                var updates = await Bot.GetUpdatesAsync(offset); // получаем все сообщения, пришедшие боту
                foreach (var update in updates)
                {
                    var msg = update.Message; // объект, хранящий очередное сообщение
                    long chatId = msg.Chat.Id; // Уникальный номер отправителя 
                    
                    
                    switch (update.Message.Type) // узнаем тип очередного сообщения
                    {
                        case MessageType.TextMessage:// если сообщение текстовое, то
                            StopWatch stopwatch = list.FirstOrDefault(timer => timer.timerId == msg.Chat.Id); // секундомер пользователя ,который берется из списка по уникальному номеру пользователя
                            switch (msg.Text)
                            {
                                case "Старт": // если сообщение было "Старт", то 
                                    if (stopwatch != null) // если данный секундомер существует, то                                    
                                        stopwatch.Restart(); // перезапускаем его
                                    else // иначе
                                        list.Add(new StopWatch(msg.Chat.Id)); // добавляем в список данный секундомер и сразу же запускаем его                                    
                                    Send(chatId, "Отчёт пошел",true); // отправляем пользователю сообщение о запуске его секундомера                                  
                                    break;
                                case "Интервал": // если сообщение было "Интервал", то 
                                    if (stopwatch != null) // если данный секундомер существует, то
                                    {
                                        if (stopwatch.IsRunning == true) // если данный секундомер запущен, то
                                        {
                                            int k = ++stopwatch.IntervalCounter; // увеличиваем счетчик интервалов
                                            Send(chatId, string.Format("Интервал №{0}: {1} ", k, stopwatch.Elapsed), true); // отправляем пользователю сообщение о данном интервале его секундомера
                                        }
                                    }                                   
                                    break;
                                case "Стоп": // если сообщение было "Стоп", то
                                    if (stopwatch!=null) // если данный секундомер существует, то
                                    {
                                        if (stopwatch.IsRunning == true) // если данный секундомер запущен, то
                                        {
                                            stopwatch.Stop(); // остановить секундомер
                                            Send(chatId, "Секундомер остановлен. Результат: " + stopwatch.Elapsed); // отправляем пользователю сообщение об остановке и результате его секундомера
                                            stopwatch.IntervalCounter = 0; // обнулям счетчик интервалов
                                        }
                                        else Send(chatId, "Запустите секундомер"); // отправляем пользователю сообщение о не запуске его секундомера
                                        break;
                                    }
                                     Send(chatId, "Запустите секундомер"); // отправляем пользователю сообщение о не запуске его секундомера
                                    break;
                                
                                case "/start": // если сообщение было "/start", то
                                    Send(chatId, string.Format("Привет, {0}. Тебя приветсвует {1} ", msg.Chat.FirstName, me.FirstName)); // приветствуем и представляемся пользователю 
                                    break;

                                default:
                                    Send(chatId, "Я тебя не понимаю:("); // выводится при вводе другой (неизвестной) команде
                                    break;
                            }
                            break;
                        
                        default:
                            Send(chatId, "Пожалуйста, введите текстовое сообщение"); // при отправке нетекстовых сообщений просим ввести текстовое сообщение
                            break;
                    }

                    offset = update.Id + 1; // увеличиваем смещение на единицу
                }
                await Task.Delay(1);
            }
        }

        private async void Send(long chatId, string message,bool flag = false)
        {
            if (flag) leftbutton.Text = "Интервал"; // если флаг истинный, то левая кнопка для "Интервала"
            else leftbutton.Text = "Старт"; // иначе для запуска
            await Bot.SendTextMessageAsync(chatId, message, true, false, 0, keyboard, ParseMode.Html);            
        }
       
    }
    class StopWatch:Stopwatch // класс секундомер, наследованный от Stopwatch
    {
        public long timerId; // уникальный номер секундомера (равняется уникальному номеру пользователя)
        public int IntervalCounter=0; // счётчик под интервал
        
        public StopWatch(long chatId) // конструктор секундомера, принимающий ID пользователя
        {
            this.timerId = chatId; // присваивание ID пользователя уникальному номеру секундомера 
            Restart(); // запуск секндомера                
        }
    }
}

