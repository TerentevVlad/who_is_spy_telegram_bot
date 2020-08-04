using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace WhoIsSpy
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TelegramBotClient Bot;
        private string Key = "Введите токен бота";
        private HashSet<long> hashSet;

        public MainPage()
        {
            this.InitializeComponent();
            textBlockKey.Text = Key;
            hashSet = new HashSet<long>();
            
        }

        private string TextHelp(Message message)
        {
            string namesAndId = message.From.FirstName + " id:" + message.From.Id;

            return "/start - начать игру\n" +
                   "/stop - закончить игру\n" +
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

        private async void ReadMessage(Message message)
        {
            if (message.Type == MessageType.Text)
            {
               
                var userId = message.Chat.Id;
                if (message.Text == "/help")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, TextHelp(message),
                        replyToMessageId: message.MessageId);

                }
                else if (message.Text == "/start")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ожидайте других игроков и напомните одмену, что пора бы начать\n /help - для получения помощи");
                    SaveId(message.From.Id);
                }
                else if(message.Text == "/stop")
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Вы завершили игру");
                    RemoveId(message.Chat.Id);
                }
            }
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
            if (Bot == null)
            {
                BwDoWork(text);
            }
        }

        private async void StartGame_OnClick(object sender, RoutedEventArgs e)
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
                    try
                    {
                        if (i == indexSpy)
                            await Bot.SendTextMessageAsync(id, "Ты Шпион");
                        else
                            await Bot.SendTextMessageAsync(id, "Локация : " + location);
                        if (i == imdexStartStep)
                        {
                            await Bot.SendTextMessageAsync(id, "Твой ход первый");
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    i++;
                }
            }
        }

        private void AddFile_OnClick(object sender, RoutedEventArgs e)
        {
            ListLocation.Instance.OpenFileFromPicker();
        }
    }
}
