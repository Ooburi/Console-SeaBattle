using Newtonsoft.Json;
using System;

namespace CommonClasses
{
    public class Strings
    {
        public const string Address = "http://localhost:5000/";
    }
    public class Move
    {
        public int I { get; set; }
        public int J { get; set; }
    }
    public class Message
    {
        [JsonProperty]
        public Actions Action { get; set; }
        [JsonProperty]
        public string? Data { get; set; }
        [JsonProperty]
        public string PlayerName { get; set; }
        [JsonProperty]
        public Guid? GameId { get; set; }
        [JsonProperty]
        public PlayerStatus? PlayerStatus { get; set; }

        public override string ToString()
        {
            return $"Action: {Action.ToString()} Data:{Data}";
        }
    }

    public enum Actions
    {
        EmptyAnswer,
        Authorize,
        OK,
        ERROR,
        NameSending,
        CheckForUpdate,
        GameRegistered,
        SendingShips,
        AskName,
        Move,
        Hit,
        Miss,
        ShipsUpdate
    }
    public enum GameStatus
    {
        PlayerOneTurn,
        PlayerTwoTurn,
        Wait
    }
    public enum PlayerStatus
    {
        Offline,
        Online,
        NeedName,
        FoundPartner,
        ShipsPlacing,
        GameStarted,
        MyTurn,
        Wait,
        Win,
        Lose,
        Checking,
        MyTurnAfter,
        WaitAfter
    }

    public class Methods
    {
        public static int[][] getRandomSheeps()
        {
            int[][][] ShipVariants = new int[4][][];

            ShipVariants[0] = new int[][]
            {
             new int[] { 1, 0, 0, 1, 0, 0, 0, 0, 1, 0 },
             new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 1, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
             new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 },
             new int[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 1, 0, 0, 1, 0, 1, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }
            };

            ShipVariants[1] = new int[][]
            {
             new int[] { 0, 1, 0, 1, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 1, 0, 1, 1, 1, 0, 0, 0, 1 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 1, 0, 0, 1, 0, 0, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }
            };
            ShipVariants[2] = new int[][]
            {
             new int[] { 0, 0, 0, 1, 0, 0, 1, 0, 0, 0 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 1, 1, 1, 1, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 0, 0, 0, 1, 0, 1, 0, 0, 0 },
             new int[] { 0, 1, 1, 0, 1, 0, 0, 0, 0, 1 }
            };
            ShipVariants[3] = new int[][]
            {
             new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 0, 0 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
             new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
             new int[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
             new int[] { 1, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
             new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0 },
             new int[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 },
             new int[] { 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 }
            };

            return ShipVariants[new Random().Next(0, 4)];
        }
    }
}
