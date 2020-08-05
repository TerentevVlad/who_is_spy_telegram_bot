using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoIsSpy.Data;
using System.Timers;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace WhoIsSpy
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TelegramBotClient Bot;
        private HashSet<long> hashSet;
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            textBlockKey.Text = Preferences.GetString("key", "Введите токен бота");
            textBlockId.Text = Preferences.GetString("id", "0");
            hashSet = new HashSet<long>();
            
        }

        private string TextHelp(Message message)
        {
            string namesAndId = message.From.FirstName + " id:" + message.From.Id;

            return "/help - получить помощь по командам бота\n" +
                   "/start - начать игру\n" +
                   "/stop - закончить игру\n" +
                   "/locations - получить список локаций\n" +
                   "/run:5 - запуск игры, распределение ролей. Через : указывается время игры в минутах. Если ничего не указать по умолчанию будет 10 минут\n" + 
                   namesAndId;
        }
        

        private void SaveId(int id)
        {
            hashSet.Add(id);
            ShowIdsInList();
        }

        private void RemoveId(long userId)
        {
            hashSet.Remove(userId);
            ShowIdsInList();
        }

        private void ShowIdsInList()
        {
            ListId.Items.Clear();
            foreach (var item in hashSet)
            {
                var textBlock = new TextBlock();
                textBlock.Text = "id: " + item;
                ListId.Items.Add(textBlock);
            }
        }

        private string GetLocations()
        {
            string listLocations = "Нет никаких локаций";
            

            var list = ListLocation.Instance.locations;
            if (list.Count > 0)
            {
                listLocations = "";
                foreach (var item in list)
                {
                    listLocations += item + "\n";
                }
            }

            return listLocations;
        }

        private async void ReadMessage(Message message)
        {
            if (message.Type == MessageType.Text)
            {
                if (message.Text == "/help")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, TextHelp(message),
                        replyToMessageId: message.MessageId);
                }
                else if (message.Text == "/start")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ожидайте других игроков и напомните одмену, что пора бы начать\n /help - для получения списка доступных команд");
                    SaveId(message.From.Id);
                }
                else if(message.Text == "/stop")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Вы завершили игру");
                    RemoveId(message.Chat.Id);
                }
                else if (message.Text == "/locations")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, GetLocations());
                }
                else if(message.Text.StartsWith("/run"))
                {
                    if (message.Chat.Id.ToString() == textBlockId.Text)
                    {
                        var messageWithOptions = message.Text.Split(':');
                        int minutes = 10;
                        if (messageWithOptions.Length == 2)
                        {
                            try
                            {
                                minutes = Convert.ToInt32(messageWithOptions[1]);
                                minutes = Math.Abs(minutes);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        if (timer != null) timer.Stop();
                        timer = new DispatcherTimer() 
                        { 
                            Interval = new TimeSpan(0, minutes, 0)
                        };
                        timer.Tick += Timer_Tick;
                        timer.Start();
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Игра началась",
                            replyToMessageId: message.MessageId);
                        SendMessageAboutRole();
                    }
                }
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            if (Bot != null)
            {
                foreach (var id in hashSet)
                {
                    try
                    {
                        string message = "Время вышло";
                        await Bot.SendTextMessageAsync(id, message);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }

                }
            }
            timer.Stop();
        }

        private async void BwDoWork(string key)
        {
            try
            {
                Bot = new TelegramBotClient(key);
                await Bot.SetWebhookAsync("");
                int offsetMessage = 0;

                while (true)
                {
                    var upadtes = await Bot.GetUpdatesAsync(offsetMessage);

                    foreach (var update in upadtes)
                    {
                        ReadMessage(update.Message);
                        offsetMessage = update.Id + 1;
                    }
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ConnectToTelegram_OnClick(object sender, RoutedEventArgs e)
        {

            var text = textBlockKey.Text;
            var id = textBlockId.Text;
            Preferences.SaveString("key", text);
            Preferences.SaveString("id", id);
            
            if (Bot == null)
            {
                BwDoWork(text);
            }
        }

        public async void SendMessageAboutRole()
        {
            if (Bot != null)
            {
                Random random = new Random();
                int indexSpy = random.Next(0, hashSet.Count);
                int imdexStartStep = random.Next(0, hashSet.Count);
                int i = 0;
                string location = ListLocation.Instance.GetRandomLocation();
                foreach (var id in hashSet)
                {
                    string message = "";
                    try
                    {
                        message += "********************";
                        if (i == indexSpy)
                            message += "\nТы Шпион";
                        else
                            message += "\nЛокация : " + location;
                        if (i == imdexStartStep)
                        {
                            message += "\nТвой ход первый";
                        }
                        await Bot.SendTextMessageAsync(id, message);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    i++;
                }
            }

        }

        private async void StartGame_OnClick(object sender, RoutedEventArgs e)
        {
            SendMessageAboutRole();
        }

        private void AddFile_OnClick(object sender, RoutedEventArgs e)
        {
            ListLocation.Instance.OpenFileFromPicker();
        }
    }
}
