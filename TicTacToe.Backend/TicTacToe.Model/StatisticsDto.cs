using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Model
{
    public class RankingEntryDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int GamesPlayed { get; set; }
        public double WinRate { get; set; }
        public int Draws { get; set; }
    }

    public class GameHistoryDto
    {
        public long Id { get; set; }
        public string PlayerXName { get; set; } = string.Empty;
        public string PlayerOName { get; set; } = string.Empty;
        public string? WinnerName { get; set; }
        public bool IsDraw { get; set; }
        public DateTime PlayedAt { get; set; }
        public string Result => IsDraw ? "Empate" : (WinnerName ?? "Em andamento");
    }

    public class GameResultSummaryDto
    {
        public int TotalGames { get; set; }
        public int XWins { get; set; }
        public int OWins { get; set; }
        public int Draws { get; set; }
        public DateTime? MostRecentGame { get; set; }
    }
}
