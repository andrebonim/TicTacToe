using Microsoft.AspNetCore.SignalR;
using TicTacToe.Business;
using TicTacToe.Business.Interfaces;
using TicTacToe.Model;

namespace TicTacToe.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameBusiness _gameBusiness;

        public GameHub(IGameBusiness gameBusiness)
        {
            _gameBusiness = gameBusiness;
        }

        // Cliente chama: connection.invoke("JoinRoom", gameId ou roomCode);
        public async Task JoinRoom(string roomIdentifier)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomIdentifier);

            // Opcional: notificar que alguém entrou
            await Clients.Group(roomIdentifier).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        public async Task JoinGame(long gameId, string quemEntrou)
        {
            try
            {
                string group = $"game_{gameId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);

                Console.WriteLine($"Connection {Context.ConnectionId} joined game {gameId}");
                

                // Busca o estado atual da partida no banco
                var game = await _gameBusiness.GetGameStatusAsync(gameId);
                if (game == null)
                {
                    await Clients.Caller.SendAsync("MoveError", "Partida não encontrada.");
                    return;
                }

                // Se já tem 2 jogadores conectados, não faz nada extra (ou rejeita, dependendo da regra)
                //if (game.ConnectedPlayers >= 2)
                //{
                //    await Clients.Caller.SendAsync("MoveError", "Sala cheia.");
                //    return;
                //}

                
                

                // Se agora tem 2 jogadores, inicia o jogo e notifica TODOS
                if (quemEntrou == "convidado")
                {
                    game.Status = "ongoing";
                    game.CurrentPlayerId = game.PlayerXId; // X começa
                    game.ConnectedPlayers = 2;

                    // Salva a mudança no banco (use o método do business ou repository)
                    var newGame =await _gameBusiness.JoinGameAsync(gameId, Context.ConnectionId); // ou chame o repositório diretamente se preferir

                    // Envia para o novo jogador (convidado)
                    //await Clients.Group(group).SendAsync("meuteste", "Sou o convidado da Sala");                    
                    await Clients.Caller.SendAsync("PlayerJoined", "O");
                    await Clients.Caller.SendAsync("SetPlayerId", newGame.PlayerO.Id);

                    // Envia para todos os outros no grupo (o criador)
                    await Clients.OthersInGroup(group).SendAsync("PlayerJoined", "X");
                    await Clients.OthersInGroup(group).SendAsync("SetPlayerId", game.PlayerXId);
                }
                else
                {
                    // Apenas o criador entrou → só loga, sem notificar ninguém
                    //await Clients.Caller.SendAsync("StatusUpdate", "Você criou a sala. Aguarde o segundo jogador.");
                    //await Clients.Group(group).SendAsync("meuteste", "Sou o criador da Sala");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no JoinGame para gameId {gameId}: {ex.Message}\nStack: {ex.StackTrace}");
                await Clients.Caller.SendAsync("MoveError", "Erro interno ao entrar na sala.");
            }
        }

        // Cliente chama: connection.invoke("SendMove", gameId, playerId, position);
        // gameId e playerId devem ser enviados como string ou number do React
        public async Task SendMove(long gameId, long playerId, int position)
        {
            
            if (position < 0 || position > 8)
            {
                await Clients.Caller.SendAsync("MoveError", "Posição inválida (0-8)");
                return;
            }

            try
            {
                // Chamada para validar e realizar o movimento
                MakeMoveDto dtoMove = new MakeMoveDto() { GameId = gameId, PlayerId = playerId, Position = position };
                var result = await _gameBusiness.MakeMoveAsync(dtoMove);

                // Se deu certo, envia o estado atualizado para TODOS no grupo
                await Clients.Group($"game_{gameId}").SendAsync("ReceiveMove", result.UpdatedGame);

                // Opcional: notificar apenas o chamador com confirmação
                await Clients.Caller.SendAsync("MoveAccepted", result);

                // Se o jogo terminou, pode enviar evento específico
                if (result.IsDraw || !string.IsNullOrEmpty(result.Winner))
                {
                    await Clients.Group(gameId.ToString()).SendAsync("GameEnded", result);
                }
            }
            catch (ArgumentException ex)
            {
                await Clients.Caller.SendAsync("MoveError", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Clients.Caller.SendAsync("MoveError", ex.Message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("MoveError", "Erro interno no servidor");
                // Logar o erro real no seu logger
            }
        }
    }
}