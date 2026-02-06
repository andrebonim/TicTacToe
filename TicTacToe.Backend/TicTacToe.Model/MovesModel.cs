using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Model
{
    public class MovesModel
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public string PlayerName { get; set; } = string.Empty; // ou só symbol "X"/"O"
        public int Position { get; set; } // 0 a 8
        public int MoveNumber { get; set; } // 1 a 9
        public DateTime PlayedAt { get; set; }
    }

    public class MakeMoveDto
    {
        public long GameId { get; set; }
        public long? PlayerId { get; set; }
        public int Position { get; set; } // 0 a 8
        // O backend deve validar se é a vez do jogador e se a posição está livre
    }

    public class MakeMoveResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public GameStatusDto? UpdatedGame { get; set; }
        public string? Winner { get; set; }
        public bool IsDraw { get; set; }
    }
}
