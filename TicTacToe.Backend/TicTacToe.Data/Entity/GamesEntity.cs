using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Data.Entity
{
    // Tabela: games
    [Table("games")]
    public class GamesEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? PlayerXId { get; set; }
        public long? PlayerOId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "ongoing";  // ongoing, finished, abandoned

        public long? WinnerId { get; set; }

        public bool IsDraw { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string BoardState { get; set; } = "[[null,null,null],[null,null,null],[null,null,null]]";

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? FinishedAt { get; set; }

        public string? RoomCode { get; set; }          // para multiplayer local/online simples

        public DateTime? LastMoveAt { get; set; }

        public int ConnectedPlayers { get; set; } = 1; // 1 = criador, 2 = liberado para jogar
        public long? CurrentPlayerId { get; set; }     // ID do jogador que deve jogar agora

        // Navigation properties
        [ForeignKey(nameof(PlayerXId))]
        public virtual PlayersEntity? PlayerX { get; set; }

        [ForeignKey(nameof(PlayerOId))]
        public virtual PlayersEntity? PlayerO { get; set; }

        [ForeignKey(nameof(WinnerId))]
        public virtual PlayersEntity? Winner { get; set; }

        public virtual ICollection<MovesEntity> Moves { get; set; } = new List<MovesEntity>();
    }
}
