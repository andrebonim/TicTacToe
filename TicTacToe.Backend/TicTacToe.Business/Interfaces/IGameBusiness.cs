using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Model;

namespace TicTacToe.Business.Interfaces
{
    public interface IGameBusiness
    {
        Task<GamesModel> CreateNewGameAsync(string playerXName, bool isNetwork, string? roomCode = null);
        Task<GamesModel> JoinGameAsync(long gameId, string playerOName);
        Task<MakeMoveResponseDto> MakeMoveAsync(MakeMoveDto dto);
        Task<GameStatusDto?> GetGameStatusAsync(long gameId);
        Task<List<GameHistoryDto>> GetRecentGamesAsync(int limit = 20);
        Task<GamesModel?> GetGameByRoomCodeAsync(string roomCode);
        Task FinishGameAsync(long gameId, string? winnerSymbol, bool isDraw);
        Task<List<RankingEntryDto>> Top10();
    }
}
