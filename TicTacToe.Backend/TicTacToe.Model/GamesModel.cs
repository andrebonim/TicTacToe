using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Model
{
    public class GamesModel
    {
        public long Id { get; set; }
        public string Status { get; set; } = "ongoing"; // ongoing, finished, abandoned
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public bool IsDraw { get; set; }
        public PlayerSummaryDto? PlayerX { get; set; }
        public PlayerSummaryDto? PlayerO { get; set; }
        public PlayerSummaryDto? Winner { get; set; }
        public string? RoomCode { get; set; }
        public List<string> Board { get; set; } = new List<string>(9) { "", "", "", "", "", "", "", "", "" }; // flat 0-8
        public string? CurrentPlayerSymbol { get; set; } // "X" ou "O"
        public int MoveCount { get; set; }
        public int ConnectedPlayers { get; set; } = 1; // 1 = criador, 2 = liberado para jogar
        public long? CurrentPlayerId { get; set; }     // ID do jogador que deve jogar agora
    }

    public class GameCreateDto
    {
        public string PlayerXName { get; set; } = string.Empty;
        public string? PlayerOName { get; set; } // opcional (pode entrar depois)
        public string? RoomCode { get; set; }     
        public bool IsNetwork { get; set; }     
    }

    public class GameJoinDto
    {
        public long GameId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
    }

    public class GameStatusDto
    {
        public long GameId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? WinnerName { get; set; }
        public bool IsDraw { get; set; }
        public string CurrentPlayerSymbol { get; set; } = string.Empty;
        public List<string> Board { get; set; } = new();
        public DateTime? FinishedAt { get; set; }
        public int ConnectedPlayers { get; set; } = 1; // 1 = criador, 2 = liberado para jogar
        public long? CurrentPlayerId { get; set; }     // ID do jogador que deve jogar agora
        public long PlayerXId { get; set; }
        public long PlayerOId { get; set; }
    }   
}
