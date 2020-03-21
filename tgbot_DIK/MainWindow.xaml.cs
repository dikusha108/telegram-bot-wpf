using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using System.Drawing;


namespace tgbot_DIK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TelegramBotClient bot;
        public static string pics = @"pics.txt";
        public static string wisdoms = @"wisdoms.txt";

        ObservableCollection<User> Users = new ObservableCollection<User>();
        public MainWindow()
        {
            InitializeComponent();
            UserList.ItemsSource = Users;


            string token = "977020581:AAH3u6-4Z4U1Ogz0kDZmadgY9i5EhnMG56M";
            bot = new TelegramBotClient(token);

            string json = "";

            bot.OnMessage += delegate (object sender, MessageEventArgs messageEventArgs)
            {
                BotMessage(sender, messageEventArgs);
                

                //добавление нового пользователя в Users
                this.Dispatcher.Invoke(() =>
                {
                    var newuser = new User(messageEventArgs.Message.Chat.FirstName, messageEventArgs.Message.Chat.Id);
                    if (!Users.Contains(newuser)) Users.Add(newuser);
                    Users[Users.IndexOf(newuser)].AddMsg($"{ newuser.Nickname}: {messageEventArgs.Message.Text}");

                    //добавление информации о пользователе в json-файл
                    json += JsonConvert.SerializeObject(newuser);
                    System.IO.File.AppendAllText("data.json", json);
                });
            };

            Send.Click += delegate { SendMessage(); };
            bot.StartReceiving();
        }

        /// <summary>
        /// Метод отправки сообщения пользователю
        /// </summary>
        public void SendMessage()
        {
            var user = Users[Users.IndexOf(UserList.SelectedItem as User)];
            //формирование ответа
            string message = "Bot: "+ MsgToSend.Text;
            //отправка ответа пользователю
            bot.SendTextMessageAsync(user.Id, MsgToSend.Text);
            //добавление ответа в коллекцию сообщений данного пользователя
            user.Messages.Add(message);
            //добавление изменений в json-файл
            string json = JsonConvert.SerializeObject(user);
            System.IO.File.AppendAllText("data.json", json);

            MsgToSend.Text = String.Empty;

        }



        //методы работы телеграм-бота
        public static void BotMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;


            if (message.Text == "/start")
            {
                bot.SendTextMessageAsync(message.Chat.Id, System.IO.File.ReadAllText("start.txt"));
            }

            if (message.Text == "/wisdom")
            {
                bot.SendTextMessageAsync(message.Chat.Id, RandString(wisdoms));
            }

            if (message.Text == "/pic")
            {
                bot.SendPhotoAsync(message.Chat.Id, RandString(pics));
            }

            if (messageEventArgs.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, отправьте фото как документ");
            }

            if (messageEventArgs.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                DownLoad(messageEventArgs.Message.Document.FileId, messageEventArgs.Message.Document.FileName);
                string downloaded = "_" + messageEventArgs.Message.Document.FileName;
                System.Threading.Thread.Sleep(10000);
                bot.SendTextMessageAsync(message.Chat.Id, "Файл получен.");


                string extension = System.IO.Path.GetExtension(downloaded);
                if ((extension == ".jpg") || (extension == ".png") || (extension == ".jpeg") || (extension == ".JPEG")
                    || (extension == ".JPG") || (extension == ".PNG"))
                {

                    TextToPhoto(downloaded, RandString(wisdoms));
                    SendFile(messageEventArgs, "new" + downloaded);
                }
            }

            if (message.Text == "/downloadsmth")
            {
                FilesFromDir("downloadsmth", messageEventArgs);

            }


        }


        /// <summary>
        /// Метод, возвращающий случайную строку из файла
        /// </summary>
        /// <param name="path"> Имя файла </param>
        /// <returns></returns>
        public static string RandString(string path)
        {

            //чтение списка строк из файла
            List<string> mytext = new List<string>();
            using (StreamReader reader = System.IO.File.OpenText(path))
            {
                string line = null;
                do
                {
                    line = reader.ReadLine();
                    mytext.Add(line);
                }
                while (line != null);
            }
            //генерируем рандомную строку
            Random rand = new Random();
            int index = rand.Next(0, mytext.Count - 1);
            return mytext[index];
        }


        /// <summary>
        /// Метод, накладывающий текст на картинку
        /// </summary>
        /// <param name="path">Имя файла-изображения</param>
        /// <param name="text">Текст, который необходимо наложить</param>
        public static void TextToPhoto(string path, string text)
        {
            using (var image = System.Drawing.Image.FromFile(path))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    var textBounds = graphics.VisibleClipBounds;
                    textBounds.Inflate(-100, -300);

                    graphics.DrawString(
                        text,
                        new Font("Verdana", (float)35),
                        System.Drawing.Brushes.White,
                        textBounds
                    );
                }

                image.Save("new" + path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// Метод, скачивающий отпраленный пользователем файл
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="path">Имя файла</param>
        public static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream("_" + path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();

            fs.Dispose();
        }

        /// <summary>
        /// Метод, отправляющий пользователю выбранный файл
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="messageEventArgs"></param>
        public static void FilesFromDir(string directory, MessageEventArgs messageEventArgs)
        {
            //Получение массива имен файлов из директории
            string[] files = Directory.GetFiles(directory);
            int i = 0; string answer = null;
            var message = messageEventArgs.Message;

            //Создание текста формата "/номер ИмяФйла.расширение и отправка его пользователю
            foreach (string e in files)
            {
                answer += "/" + i.ToString() + " " + e.Substring(13) + "\n";
                i++;
            }
            bot.SendTextMessageAsync(message.Chat.Id, answer);

            //Получение ответа от пользвателя с номером выбранного файла и отправка этого файла
            bot.OnMessage += delegate (object sender, MessageEventArgs messageEventArgs1)
            {
                for (int j = 0; j < i; j++)
                {
                    if (messageEventArgs1.Message.Text == "/" + j.ToString())
                    {
                        SendFile(messageEventArgs1, files[j]);
                    }
                }

            };
        }

        /// <summary>
        /// Метод отправки файла пользователю
        /// </summary>
        /// <param name="e"></param>
        /// <param name="path"></param>
        private static async void SendFile(MessageEventArgs e, string path)
        {
            Message message;
            using (Stream stream = System.IO.File.OpenRead(path))
            {
                message = await bot.SendDocumentAsync(
                    e.Message.Chat.Id,
                    new InputOnlineFile(stream, path),
                    "Ваш файл"
                );

            }
        }

        
    }
}

