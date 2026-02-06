using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Model
{
    public class PlayersModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public double WinRate => GamesPlayed > 0 ? Math.Round((double)Wins / GamesPlayed * 100, 1) : 0;
    }

    public class PlayerCreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class PlayerSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
