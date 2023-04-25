using CommonClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string MyName { get; set; }

        static string EnemyName { get; set; } = null;
        static PlayerStatus playerStatus = PlayerStatus.Offline;
        static Guid gameId = Guid.Empty;
        private static int[][] Ships = new int[10][];
        private static int[][] EnemyShips = new int[10][];
        static async Task Main(string[] args)
        {
            SetName();
            Login();

            while (true)
            {
                switch (playerStatus)
                {
                   
                    case PlayerStatus.Online:
                        Console.Clear();
                        WriteYellow("Ожидаем второго игрока");
                        CheckForUpdate();
                        break;
                    case PlayerStatus.GameStarted:
                        WriteRed("Game started");
                        CreateShips();
                        SendShips();
                        GetEnemyName();
                        break;
                    case PlayerStatus.Wait:
                        RefreshScreen();
                        WriteYellow("\tОжидаем хода противника");
                        CheckForUpdate();
                        break;
                    case PlayerStatus.MyTurn:
                        RefreshScreen();
                        WriteBlue("\tВаш ход");
                        SendMove(PrepareMove());
                        break;
                    case PlayerStatus.Win:
                        Console.Clear();
                        WriteRed("\n\tПОЗДРАВЛЯЕМ!!!\nВЫ ВЫИГРАЛИ!!!");
                        Console.ReadLine();
                        break;
                    case PlayerStatus.Lose:
                        Console.Clear();
                        WriteRed("\n\tВЫ ПРОИГРАЛИ");
                        Console.ReadLine();
                        break;
                }
            }
        }
        private static void SendMove(Move move)
        {
           
            string data = JsonConvert.SerializeObject(move);
            Message mData = new Message() { Action = Actions.Move, Data = data, PlayerName = MyName, PlayerStatus = playerStatus, GameId = gameId };
            string answer = MakePostRequest(mData).Result;
            Message mAnswer = JsonConvert.DeserializeObject<Message>(answer);

            if (mAnswer.Action == Actions.Hit)
            {
                EnemyShips[move.I][move.J] = 2;
                playerStatus = (PlayerStatus)mAnswer.PlayerStatus;

            }
            if (mAnswer.Action == Actions.Miss)
            {
                EnemyShips[move.I][move.J] = 3;
                playerStatus = (PlayerStatus)mAnswer.PlayerStatus;
            }
        }
        private static Move PrepareMove()
        {
            string letter;
            string number;

            List<string> letters = new List<string>() { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            List<string> numbers = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

            Console.WriteLine("Введите букву, куда будете бить, например \"A\"");
            letter = Console.ReadLine();

            if (!letters.Contains(letter.ToLower()))
            {
                WriteRed("Неверный ввод");
                return PrepareMove();
            }

            Console.WriteLine("Введите цифру, куда будете бить, например \"4\"");
            number = Console.ReadLine();

            if (!numbers.Contains(number))
            {
                WriteRed("Неверный ввод");
                return PrepareMove();
            }

            int J = Convert.ToInt32(number);
            J--;
            int I = 0;
            switch (letter.ToLower())
            {
                case "a":
                    I = 0;
                    break;
                case "b":
                    I = 1;
                    break;
                case "c":
                    I = 2;
                    break;
                case "d":
                    I = 3;
                    break;
                case "e":
                    I = 4;
                    break;
                case "f":
                    I = 5;
                    break;
                case "g":
                    I = 6;
                    break;
                case "h":
                    I = 7;
                    break;
                case "i":
                    I = 8;
                    break;
                case "j":
                    I = 9;
                    break;
            }

            return new Move() { I = J, J = I };
        }
        private static void RefreshScreen()
        {

            Console.Clear();

            Console.WriteLine(String.Format("    My Ships          |    |{0}`s Ships      ", EnemyName));
            Console.WriteLine("    A B C D E F G H I J    A B C D E F G H I J");
            for (int i = 0; i < 10; i++)
            {
                if (i < 9)
                    Console.Write(i + 1 + "  |");
                else
                    Console.Write(i + 1 + " |");
                for (int j = 0; j < 10; j++)
                {
                    if (Ships[i][j] == 0) Console.Write(" |"); // Clear Spot
                    if (Ships[i][j] == 1) Console.Write("O|"); // Ship
                    if (Ships[i][j] == 2) Console.Write("X|"); // Ship Destroyed
                    if (Ships[i][j] == 3) Console.Write(".|"); // Miss
                }
                Console.Write("##|");
                for (int j = 0; j < 10; j++)
                {
                    if (EnemyShips[i][j] == 0) Console.Write(" |");
                    if (EnemyShips[i][j] == 1) Console.Write("O|");
                    if (EnemyShips[i][j] == 2) Console.Write("X|");
                    if (EnemyShips[i][j] == 3) Console.Write(".|");
                }
                Console.Write(i + 1 + "\n");
            }
        }
        private static async void Login()
        {
            string result = await MakeRequest("login", MyName);

            if (result == "success")
            {
                playerStatus = PlayerStatus.Online;
                return;
            }
            Console.WriteLine("Ошибка: Игрок с таким именем уже существует");
            SetName();
            Login();
        }
        static  void CheckForUpdate()
        {
            Thread.Sleep(1000);
            Message mData = new Message() { Action = Actions.CheckForUpdate, Data = "null", PlayerName = MyName, PlayerStatus=playerStatus, GameId=gameId};
            string answer = MakePostRequest(mData).Result;

            Message mAnswer = JsonConvert.DeserializeObject<Message>(answer);

            if (mAnswer.Action == Actions.GameRegistered)
            {
                playerStatus = (PlayerStatus)mAnswer.PlayerStatus;
                gameId = (Guid)mAnswer.GameId;
            }
            if (mAnswer.Action == Actions.ShipsUpdate)
            {
                playerStatus = (PlayerStatus)mAnswer.PlayerStatus;
                Ships = JsonConvert.DeserializeObject<int[][]>(mAnswer.Data);
            }
        }
        static void SendShips()
        {
            string data = JsonConvert.SerializeObject(Ships);
            Message mData = new Message() { Action = Actions.SendingShips, Data = data, PlayerName = MyName, PlayerStatus = playerStatus, GameId = gameId };
            string answer = MakePostRequest(mData).Result;
            Message mAnswer = JsonConvert.DeserializeObject<Message>(answer);
            playerStatus = (PlayerStatus)mAnswer.PlayerStatus;
        }
        static void GetEnemyName()
        {
            Message mData = new Message() { Action = Actions.AskName, Data = MyName, PlayerName =MyName, PlayerStatus = playerStatus, GameId = gameId };
            string answer = MakePostRequest(mData).Result;
            Message mAnswer = JsonConvert.DeserializeObject<Message>(answer);
            EnemyName = mAnswer.Data;
        }
        private static void SetName()
        {
            Console.WriteLine("Введите ваше имя");
            string name = Console.ReadLine();
            if (string.IsNullOrEmpty(name)) SetName(); else MyName = name;
        }
        private static async Task<string> MakePostRequest(Message data)
        {
            string json = JsonConvert.SerializeObject(data);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Strings.Address}data"),
                Content = content
            };

            var res =  client.SendAsync(request).Result;
            return await res.Content.ReadAsStringAsync();
        }
        private static async Task<string> MakeRequest(string action, string data)
        {
            var stringTask = client.GetStringAsync(Strings.Address + $"{action}/{data}/");
            var msg = await stringTask;

            return msg;
        }
        static void WriteBlue(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void WriteRed(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void WriteYellow(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void CreateShips()
        {
            for (int i = 0; i < 10; i++)
            {
                EnemyShips[i] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            Ships = Methods.getRandomSheeps();
        }
    }
}
