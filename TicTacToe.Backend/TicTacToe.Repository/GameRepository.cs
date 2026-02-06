using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Data.Entity;
using TicTacToe.Data;
using TicTacToe.Model;
using AutoMapper.QueryableExtensions;

namespace TicTacToe.Repository
{
    public interface IGameRepository
    {
        Task<GamesEntity?> GetByIdWithIncludesAsync(long gameId, bool includeMoves = false);
        Task<GamesEntity?> GetOngoingGameByRoomCodeAsync(string roomCode);
        Task<GamesEntity> CreateNewGameAsync(long playerXId, bool isNetwork, string? playerOName = null, string? roomCode = null);
        Task JoinGameAsync(long gameId, long playerOId);
        Task MakeMoveAsync(long gameId, long? playerId, short position, short moveNumber, string newBoardState);
        Task FinishGameAsync(long gameId, long? winnerId, bool isDraw);
        Task<List<GameHistoryDto>> GetRecentGamesAsync(int limit = 20);
        Task<GameStatusDto?> GetCurrentGameStatusAsync(long gameId);
        Task<bool> IsPositionFreeAsync(long gameId, short position);
        Task<int> GetMoveCountAsync(long gameId);
        Task<List<RankingEntryDto>> GetTop10();
    }

    public class GameRepository : BaseRepository<GamesEntity>, IGameRepository
    {
        private readonly IMapper _mapper;

        public GameRepository(TicTacToeContext context, ILoggerFactory loggerFactory, IMapper mapper)
            : base(context, loggerFactory, mapper)
        {
            _mapper = mapper;
        }

        public async Task<GamesEntity?> GetByIdWithIncludesAsync(long gameId, bool includeMoves = false)
        {
            var query = _context.Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Winner);

            if (includeMoves)
                query = query.Include(g => g.Moves).ThenInclude(m => m.Player);

            return await query.FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task<GamesEntity?> GetOngoingGameByRoomCodeAsync(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode)) return null;

            return await _context.Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .FirstOrDefaultAsync(g => g.RoomCode == roomCode && g.Status == "ongoing");
        }

        public async Task<GamesEntity> CreateNewGameAsync(long playerXId, bool isNetwork, string? playerOName = null, string? roomCode = null)
        {
            var game = new GamesEntity
            {
                PlayerXId = playerXId,
                Status = isNetwork ? "awaiting_opponent" : "ongoing",
                BoardState = "[[null,null,null],[null,null,null],[null,null,null]]", // ou "{}" se usar dicionário
                RoomCode = roomCode?.ToUpperInvariant(),
                LastMoveAt = DateTime.UtcNow,
                ConnectedPlayers = 0,
                CurrentPlayerId = playerXId
            };

            if (!string.IsNullOrWhiteSpace(playerOName))
            {
                // Se já tem nome do O, pode criar um player temporário ou esperar join
                // Aqui assumimos que player O entra depois
            }

            await _context.AddAsync(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task JoinGameAsync(long gameId, long playerOId)
        {
            var game = await GetByIdAsync(gameId);
            if (game == null || game.PlayerOId != null || game.Status != "awaiting_opponent")
                throw new InvalidOperationException("Partida não disponível para join.");

            game.PlayerOId = playerOId;
            game.LastMoveAt = DateTime.UtcNow;
            game.ConnectedPlayers = 2;
            game.Status = "ongoing";

            await UpdateAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task MakeMoveAsync(long gameId, long? playerId, short position, short moveNumber, string newBoardState)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null || game.Status != "ongoing")
                throw new InvalidOperationException("Partida não encontrada ou finalizad'1a.");

            var move = new MovesEntity
            {
                GameId = gameId,
                PlayerId = playerId,
                Position = position,
                MoveNumber = moveNumber,
                PlayedAt = DateTime.UtcNow
            };

            game.BoardState = newBoardState;
            game.LastMoveAt = DateTime.UtcNow;

            _context.Moves.Add(move);

            // Garante que o game está tracked e atualizado
            _context.Entry(game).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task FinishGameAsync(long gameId, long? winnerId, bool isDraw)
        {
            var game = await GetByIdAsync(gameId);
            if (game == null || game.Status != "ongoing")
                return;

            game.Status = "finished";
            game.WinnerId = winnerId;
            game.IsDraw = isDraw;
            game.FinishedAt = DateTime.UtcNow;

            await UpdateAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task<List<GameHistoryDto>> GetRecentGamesAsync(int limit = 20)
        {
            return await _context.Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Winner)
                .Where(g => g.Status == "finished" || g.Status == "abandoned")
                .OrderByDescending(g => g.FinishedAt ?? g.StartedAt)
                .Take(limit)
                .ProjectTo<GameHistoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<GameStatusDto?> GetCurrentGameStatusAsync(long gameId)
        {
            var game = await GetByIdWithIncludesAsync(gameId, includeMoves: true);
            if (game == null) return null;

            var dto = _mapper.Map<GameStatusDto>(game);
            
            if (game.Status == "ongoing")
            {
                int moveCount = game.Moves?.Count ?? 0;
                dto.CurrentPlayerSymbol = moveCount % 2 == 0 ? "X" : "O";
            }

            return dto;
        }

        public async Task<bool> IsPositionFreeAsync(long gameId, short position)
        {
            return !await _context.Moves.AnyAsync(m => m.GameId == gameId && m.Position == position);
        }

        public async Task<int> GetMoveCountAsync(long gameId)
        {
            return await _context.Moves.CountAsync(m => m.GameId == gameId);
        }

        public async Task<List<RankingEntryDto>> GetTop10()
        {            
            var topPlayers = await _context.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Wins)
            .Take(10)
            .Select(p => new
            {
                p.Name,
                p.Wins,
                p.GamesPlayed,
                p.Draws
            })
            .ToListAsync();   // ← traz só os 10 primeiros

                    
            var ranking = topPlayers.Select(p => new RankingEntryDto
            {
                PlayerName = p.Name,
                Wins = p.Wins,
                GamesPlayed = p.GamesPlayed,
                Draws = p.Draws,
                WinRate = p.GamesPlayed > 0
                    ? Math.Round((double)p.Wins / p.GamesPlayed * 100.0, 2)
                    : 0.0
            })
            .OrderByDescending(r => r.Wins)           // reordena se necessário (opcional)
            .ThenByDescending(r => r.WinRate)
            .ToList();

            return ranking;
        }
    }
}
