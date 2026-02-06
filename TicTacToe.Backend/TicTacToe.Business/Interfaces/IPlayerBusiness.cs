using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Data.Entity;
using TicTacToe.Model;

namespace TicTacToe.Business.Interfaces
{
    public interface IPlayerBusiness
    {
        Task<PlayersEntity> GetOrCreatePlayerAsync(string name);
        Task<List<RankingEntryDto>> GetTopPlayersAsync(int limit = 10);
        Task UpdatePlayerStatsAsync(long playerId, bool won, bool draw);
        Task<PlayersEntity?> GetPlayerByIdAsync(long id);
    }
}
