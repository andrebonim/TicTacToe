using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicTacToe.Business.Interfaces;
using TicTacToe.Model;
using TicTacToe.Repository;

namespace TicTacToe.Business
{
    public class GameBusiness : IGameBusiness
    {
        private readonly IGameRepository _gameRepository;
        private readonly IPlayerBusiness _playerBusiness;
        private readonly IMapper _mapper;

        public GameBusiness(
            IGameRepository gameRepository,
            IPlayerBusiness playerBusiness,
            IMapper mapper)
        {
            _gameRepository = gameRepository;
            _playerBusiness = playerBusiness;
            _mapper = mapper;
        }

        public async Task<GamesModel> CreateNewGameAsync(string playerXName, bool isNetwork, string? roomCode = null)
        {
            var playerX = await _playerBusiness.GetOrCreatePlayerAsync(playerXName);

            roomCode ??= Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            var game = await _gameRepository.CreateNewGameAsync(
                playerX.Id,
                isNetwork,
                roomCode: roomCode
            );

            var dto = _mapper.Map<GamesModel>(game);
            dto.PlayerX = new PlayerSummaryDto { Id = playerX.Id, Name = playerX.Name };
            dto.CurrentPlayerSymbol = "X";

            return dto;
        }

        public async Task<GamesModel> JoinGameAsync(long gameId, string playerOName)
        {
            var playerO = await _playerBusiness.GetOrCreatePlayerAsync(playerOName);

            await _gameRepository.JoinGameAsync(gameId, playerO.Id);

            var game = await _gameRepository.GetByIdWithIncludesAsync(gameId)
                ?? throw new InvalidOperationException("Partida não encontrada após join.");

            return _mapper.Map<GamesModel>(game);
        }

        public async Task<MakeMoveResponseDto> MakeMoveAsync(MakeMoveDto dto)
        {
            var gameId = dto.GameId;
            var playerId = dto.PlayerId;
            var position = dto.Position;

            var game = await _gameRepository.GetByIdWithIncludesAsync(gameId, includeMoves: true);
            if (game == null || game.Status != "ongoing")
                throw new InvalidOperationException("Partida não encontrada ou já finalizada.");

            if (game.Status != "ongoing")
                throw new InvalidOperationException("Partida não iniciada, finalizada ou abandonada.");

            // Validação de turno baseada em CurrentPlayerId
            //if (game.CurrentPlayerId != dto.PlayerId)
            //    throw new InvalidOperationException("Não é sua vez de jogar.");

            int moveCount = await _gameRepository.GetMoveCountAsync(gameId);
            string currentSymbol = moveCount % 2 == 0 ? "X" : "O";

            // Validação de turno
            long? expectedPlayerId = currentSymbol == "X" ? game.PlayerXId : game.PlayerOId;

            // Se playerId foi enviado, valida
            if (playerId.HasValue)
            {
                if (expectedPlayerId != playerId.Value)
                    throw new InvalidOperationException("Não é a sua vez de jogar.");
            }
            else
            {
                // Modo local ou sem ID: assume que o front está correto
                // (opcional: logar warning)
            }

            if (!await _gameRepository.IsPositionFreeAsync(gameId, (short)position))
                throw new InvalidOperationException("Esta posição já está ocupada.");

            // Atualiza board
            var board = JsonSerializer.Deserialize<string?[][]>(game.BoardState)
                ?? throw new InvalidOperationException("Estado do tabuleiro inválido.");

            int row = position / 3;
            int col = position % 3;
            board[row][col] = currentSymbol;

            string newBoardJson = JsonSerializer.Serialize(board);

            // Salva a jogada e atualiza o board_state
            await _gameRepository.MakeMoveAsync(
                gameId,
                playerId,
                (short)position,
                (short)(moveCount + 1),
                newBoardJson  
            );

            // Verifica fim de jogo
            bool hasWinner = HasWinner(board, currentSymbol);
            bool isDraw = !hasWinner && (moveCount + 1 == 9);

            long? winnerId = null;
            if (hasWinner)
            {
                winnerId = currentSymbol == "X" ? game.PlayerXId : game.PlayerOId;
            }

            if (hasWinner || isDraw)
            {
                await _gameRepository.FinishGameAsync(gameId, winnerId, isDraw);

                if (winnerId.HasValue)
                    await _playerBusiness.UpdatePlayerStatsAsync(winnerId.Value, true, false);

                if (game.PlayerXId.HasValue)
                    await _playerBusiness.UpdatePlayerStatsAsync(game.PlayerXId.Value, false, isDraw);

                if (game.PlayerOId.HasValue)
                    await _playerBusiness.UpdatePlayerStatsAsync(game.PlayerOId.Value, false, isDraw);
            }

            var updatedStatus = await _gameRepository.GetCurrentGameStatusAsync(gameId);

            return new MakeMoveResponseDto
            {
                Success = true,
                UpdatedGame = updatedStatus,
                Winner = hasWinner ? currentSymbol : null,
                IsDraw = isDraw
            };
        }   

        public async Task<GameStatusDto?> GetGameStatusAsync(long gameId)
        {
            return await _gameRepository.GetCurrentGameStatusAsync(gameId);
        }

        public async Task<List<GameHistoryDto>> GetRecentGamesAsync(int limit = 20)
        {
            return await _gameRepository.GetRecentGamesAsync(limit);
        }

        public async Task<GamesModel?> GetGameByRoomCodeAsync(string roomCode)
        {
            var game = await _gameRepository.GetOngoingGameByRoomCodeAsync(roomCode);
            if (game == null) return null;

            return _mapper.Map<GamesModel>(game);
        }

        private static bool HasWinner(string?[][] board, string symbol)
        {
            // Linhas
            for (int i = 0; i < 3; i++)
                if (board[i][0] == symbol && board[i][1] == symbol && board[i][2] == symbol)
                    return true;

            // Colunas
            for (int i = 0; i < 3; i++)
                if (board[0][i] == symbol && board[1][i] == symbol && board[2][i] == symbol)
                    return true;

            // Diagonais
            if (board[0][0] == symbol && board[1][1] == symbol && board[2][2] == symbol) return true;
            if (board[0][2] == symbol && board[1][1] == symbol && board[2][0] == symbol) return true;

            return false;
        }

        public async Task FinishGameAsync(long gameId, string? winnerSymbol, bool isDraw)
        {
            var game = await _gameRepository.GetByIdWithIncludesAsync(gameId)
                ?? throw new InvalidOperationException("Partida não encontrada.");

            //if (game.Status != "ongoing")
            //    throw new InvalidOperationException("Partida já finalizada ou abandonada.");

            long? winnerId = null;
            if (winnerSymbol == "X") winnerId = game.PlayerXId;
            else if (winnerSymbol == "O") winnerId = game.PlayerOId;

            await _gameRepository.FinishGameAsync(gameId, winnerId, isDraw);

            // Atualiza estatísticas
            if (winnerId.HasValue)
                await _playerBusiness.UpdatePlayerStatsAsync(winnerId.Value, true, false);

            if (game.PlayerXId.HasValue)
                await _playerBusiness.UpdatePlayerStatsAsync(game.PlayerXId.Value, false, isDraw);

            if (game.PlayerOId.HasValue)
                await _playerBusiness.UpdatePlayerStatsAsync(game.PlayerOId.Value, false, isDraw);
        }

        public async Task<List<RankingEntryDto>> Top10()
        {
            return await _gameRepository.GetTop10();
        }
    }
}
