using AutoMapper;
using TicTacToe.Data.Entity;
using TicTacToe.Data;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Model;

namespace TicTacToe.Repository
{
    public interface IPlayerRepository : IBaseRepository<PlayersEntity>
    {
        Task<PlayersEntity?> GetByNameAsync(string name, bool caseInsensitive = true);
        Task<PlayersEntity> CreateOrGetAsync(string name);
        Task<List<RankingEntryDto>> GetRankingAsync(int top = 10);
        Task UpdateStatsAsync(long playerId, bool isWin, bool isDraw);
    }

    public class PlayerRepository : BaseRepository<PlayersEntity>, IPlayerRepository
    {
        public PlayerRepository(TicTacToeContext context, ILoggerFactory loggerFactory, IMapper mapper)
            : base(context, loggerFactory, mapper)
        {
        }

        public async Task<PlayersEntity?> GetByNameAsync(string name, bool caseInsensitive = true)
        {
            if (caseInsensitive)
                return await _context.Players
                    .FirstOrDefaultAsync(p => EF.Functions.ILike(p.Name, name));

            return await _context.Players
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<PlayersEntity> CreateOrGetAsync(string name)
        {
            var player = await GetByNameAsync(name);
            if (player != null) return player;

            player = new PlayersEntity { Name = name.Trim() };
            await _context.AddAsync(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<List<RankingEntryDto>> GetRankingAsync(int top = 10)
        {
            return await _context.Players
                .Where(p => p.GamesPlayed > 0)
                .OrderByDescending(p => p.Wins)
                .ThenByDescending(p => p.GamesPlayed > 0 ? (double)p.Wins / p.GamesPlayed : 0)
                .Take(top)
                .ProjectTo<RankingEntryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task UpdateStatsAsync(long playerId, bool isWin, bool isDraw)
        {
            var player = await base.GetByIdAsync(playerId);
            if (player == null) return;

            player.GamesPlayed++;
            if (isWin) player.Wins++;
            if (isDraw) player.Draws++;

            await UpdateAsync(player);
            await _context.SaveChangesAsync();
        }

    }
}

