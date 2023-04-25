using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using CommonClasses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Server
{
    public class Startup
    {
        static List<Game> Games = new List<Game>();
        static List<Player> Players = new List<Player>();

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Program> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/login/{name:alpha}", async context =>
                {
                    var name = context.Request.RouteValues["name"];
                    await Login(name.ToString(), context);
                    logger.LogInformation(context.Request.Path);
                });
                endpoints.MapPost("/data", async context =>
                {
                    if (!context.Request.HasJsonContentType())
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                        return;
                    }

                    var msg = await context.Request.ReadFromJsonAsync<Message>();
                    await ManageRequest(msg, context);
                    logger.LogInformation(msg.ToString());
                });
            });
        }
        private static Guid CreateGame(Player p1, Player p2)
        {
            Game g = new Game() { Player1 = p1, Player2 = p2, GameStatus = GameStatus.Wait, GameId = Guid.NewGuid() };
            Games.Add(g);
            return g.GameId;
        }
        private static Message AnswerUpdate(Message req)
        {
            var player = Players.Find(p => p.Name == req.PlayerName);
            Message ans = req;

            switch (req.PlayerStatus)
            {
                case PlayerStatus.Online:
                    bool inGame = false;
                    Guid gameId = Guid.Empty;

                    foreach (Game g in Games)
                    {
                        if (g.IsMember(req.PlayerName))
                        {
                            inGame = true;
                            gameId = g.GameId;
                            break;
                        }
                    }
                    if (!inGame)
                    {
                        var player2 = Players.Find(p => p.Name != req.PlayerName && p.PlayerStatus == PlayerStatus.Online);
                        if (player2 != null)
                        {
                            gameId = CreateGame(player, player2);
                        }
                    }
                    if (gameId != Guid.Empty)
                    {
                        ans.PlayerStatus = PlayerStatus.GameStarted;
                        ans.GameId = gameId;
                        ans.Action = Actions.GameRegistered;
                    }
                    else
                    {
                        ans.PlayerStatus = PlayerStatus.Online;
                        ans.Action = Actions.OK;
                    }
                    break;
                case PlayerStatus.Wait:
                    var game = Games.Find(g => g.GameId == req.GameId);

                    if (game.IfLost(player.Name))
                    {
                        ans.Action = Actions.ShipsUpdate;
                        ans.Data = JsonConvert.SerializeObject(player.Ships);
                        ans.PlayerStatus = PlayerStatus.Lose;
                        break;
                    }

                    if (game.IfMyTurn(player.Name))
                    {
                        ans.Action = Actions.ShipsUpdate;
                        ans.Data = JsonConvert.SerializeObject(player.Ships);
                        ans.PlayerStatus = PlayerStatus.MyTurn;
                    }
                    else
                    {
                        ans.Action = Actions.ShipsUpdate;
                        ans.Data = JsonConvert.SerializeObject(player.Ships);
                        ans.PlayerStatus = PlayerStatus.Wait;
                    }
                    break;
            }

            return ans;
        }
        private async Task ManageRequest(Message data, HttpContext context)
        {
            Message request = data;
            Message answer = new Message() { Action = Actions.EmptyAnswer };

            switch (request.Action)
            {
                case Actions.CheckForUpdate:
                    answer = AnswerUpdate(request);
                    break;
                case Actions.SendingShips:
                    var g = Games.Find(g => g.GameId == request.GameId);
                    var p = g.GetPlayer(request.PlayerName);

                    int[][] ships = JsonConvert.DeserializeObject<int[][]>(request.Data);
                    p.Ships = ships;
                    p.ShipsPlaced = true;

                    if (g.Ready())
                    {
                        g.GameStatus = (GameStatus)new Random().Next(0, 2);
                        if (g.IfMyTurn(request.PlayerName))
                        {
                            answer.Action = Actions.OK;
                            answer.PlayerStatus = PlayerStatus.MyTurn;
                        } else
                        {
                            answer.Action = Actions.OK;
                            answer.PlayerStatus = PlayerStatus.Wait;
                        }
                    }
                    else
                    {
                        answer.Action = Actions.OK;
                        answer.PlayerStatus = PlayerStatus.Wait;
                    }
                    break;
                case Actions.AskName:
                    var game = Games.Find(g => g.GameId == request.GameId);
                    if (game.Player1.Name == request.PlayerName) answer.Data = game.Player2.Name;
                    else answer.Data = game.Player1.Name;
                    answer.Action = Actions.OK;
                    break;
                case Actions.Move:
                    var game1 = Games.Find(g => g.GameId == request.GameId);
                    Player opp;
                    if (request.PlayerName == game1.Player1.Name) opp = game1.Player2; else opp = game1.Player1;
                    if (game1.ProcessMove(request.PlayerName, request.Data))
                    {
                        if (game1.IfWin(request.PlayerName))
                        {
                            answer.Action = Actions.OK;
                            answer.Data = JsonConvert.SerializeObject(opp.Ships);
                            answer.PlayerStatus = PlayerStatus.Win;
                            break;
                        }
                        answer.Action = Actions.Hit;
                        answer.PlayerStatus = PlayerStatus.MyTurn;
                        answer.Data = JsonConvert.SerializeObject(opp.Ships);
                    }
                    else
                    {
                        answer.Action = Actions.Miss;
                        answer.PlayerStatus = PlayerStatus.Wait;
                        answer.Data = JsonConvert.SerializeObject(opp.Ships);
                    }
                    break;
            }

            await context.Response.WriteAsJsonAsync(answer);
        }
        private async Task Login(string name, HttpContext context)
        {
            if (Players.Find(p => p.Name == name) != null)
            {
                await context.Response.WriteAsync("error");
                return;
            }

            Player player = new Player(name) { PlayerStatus = PlayerStatus.Online };
            Players.Add(player);
            await context.Response.WriteAsync("success");
        }
        public class Player
        {
            
            public string Name { get; set; } = "";
            public PlayerStatus PlayerStatus { get; set; }
            public int[][] Ships { get; set; }
            public bool ShipsPlaced { get; set; } = false;
            public Player(string name)
            {
                Name = name;
            }
        }
        public class Game
        {
            public Player Player1 { get; set; }
            public Player Player2 { get; set; }
            public GameStatus GameStatus { get; set; }
            public Guid GameId { get; set; }

            public bool IfWin(string name)
            {
                Player p;
                if (Player1.Name == name) p = Player2; else p = Player1;
                bool win = true;

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (p.Ships[i][j] == 1) win = false;
                    }
                }
                return win;
            }
            public bool IfLost(string name)
            {
                Player p;
                if (Player1.Name == name) p = Player1; else p = Player2;
                bool lost = true;

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (p.Ships[i][j] == 1) lost = false;
                    }
                }
                return lost;
            }
            public bool ProcessMove(string name, string data)
            {
                Move move = JsonConvert.DeserializeObject<Move>(data);
                Player opp;
                if (name == Player1.Name) opp = Player2; else opp = Player1;

                if (opp.Ships[move.I][move.J] == 1 || opp.Ships[move.I][move.J] == 2)
                {
                    opp.Ships[move.I][move.J] = 2;
                    return true;
                }
                else
                {
                    opp.Ships[move.I][move.J] = 3;
                    if (GameStatus == GameStatus.PlayerOneTurn) GameStatus = GameStatus.PlayerTwoTurn; else GameStatus = GameStatus.PlayerOneTurn;
                    return false;
                }
            }
            public Player GetPlayer(string name)
            {
                if (Player1.Name == name) return Player1;
                if (Player2.Name == name) return Player2;
                return null;
            }
            public bool IsMember(string name)
            {
                return (Player1.Name == name || Player2.Name == name);
            }

            public bool Ready()
            {
                return (Player1.ShipsPlaced && Player2.ShipsPlaced);
            }
            public bool IfMyTurn(string name)
            {
                return (GameStatus == GameStatus.PlayerOneTurn && (Player1.Name == name)) || (GameStatus == GameStatus.PlayerTwoTurn && (Player2.Name == name));
            }
        }
    }
}
