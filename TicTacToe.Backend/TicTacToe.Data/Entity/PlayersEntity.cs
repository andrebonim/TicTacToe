using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.Data.Entity
{
    // Tabela: players
    [Table("players")]
    public class PlayersEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int GamesPlayed { get; set; } = 0;

        public int Wins { get; set; } = 0;

        public int Draws { get; set; } = 0;

        // Navigation properties (opcional, mas muito útil)
        //public virtual ICollection<GamesEntity> GamesAsX { get; set; } = new List<GamesEntity>();
        //public virtual ICollection<GamesEntity> GamesAsO { get; set; } = new List<GamesEntity>();
        //public virtual ICollection<GamesEntity> WinsAsPlayer { get; set; } = new List<GamesEntity>();
        public virtual ICollection<MovesEntity> Moves { get; set; } = new List<MovesEntity>();
    }
}
