using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Data.Entity
{
    // Tabela: moves
    [Table("moves")]
    public class MovesEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long GameId { get; set; }

        public long? PlayerId { get; set; }

        [Range(0, 8)]
        public short Position { get; set; }     // 0 a 8

        [Range(1, 9)]
        public short MoveNumber { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(GameId))]
        public virtual GamesEntity Game { get; set; } = null!;

        [ForeignKey(nameof(PlayerId))]
        public virtual PlayersEntity? Player { get; set; }
    }
}
