using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Data.Entity;


namespace TicTacToe.Data
{
    public class TicTacToeContext: DbContext
    {
        public TicTacToeContext(DbContextOptions<TicTacToeContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlayersEntity>(entity =>
            {
                entity.ToTable("players");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.GamesPlayed).HasColumnName("games_played");
                entity.Property(e => e.Wins).HasColumnName("wins");
                entity.Property(e => e.Draws).HasColumnName("draws");
            });

            modelBuilder.Entity<GamesEntity>(entity =>
            {
                entity.ToTable("games");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PlayerXId).HasColumnName("player_x_id");
                entity.Property(e => e.PlayerOId).HasColumnName("player_o_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.WinnerId).HasColumnName("winner_id");
                entity.Property(e => e.IsDraw).HasColumnName("is_draw");
                entity.Property(e => e.BoardState).HasColumnName("board_state");
                entity.Property(e => e.StartedAt).HasColumnName("started_at");
                entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
                entity.Property(e => e.RoomCode).HasColumnName("room_code");
                entity.Property(e => e.LastMoveAt).HasColumnName("last_move_at");
                entity.Property(e => e.ConnectedPlayers).HasColumnName("connected_players");
                entity.Property(e => e.CurrentPlayerId).HasColumnName("current_player_id");
            });

            modelBuilder.Entity<MovesEntity>(entity =>
            {
                entity.ToTable("moves");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.GameId).HasColumnName("game_id");
                entity.Property(e => e.PlayerId).HasColumnName("player_id");
                entity.Property(e => e.Position).HasColumnName("position");
                entity.Property(e => e.MoveNumber).HasColumnName("move_number");
                entity.Property(e => e.PlayedAt).HasColumnName("played_at");
            });

            // Se você tinha índices, configure-os também em minúsculo
            modelBuilder.Entity<GamesEntity>()
                .HasIndex(g => g.Status)
                .HasDatabaseName("idx_games_status");

            // Relação PlayerX
            modelBuilder.Entity<GamesEntity>()
                .HasOne(g => g.PlayerX)
                .WithMany()  // sem WithMany, porque removemos a coleção
                .HasForeignKey(g => g.PlayerXId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relação PlayerO
            modelBuilder.Entity<GamesEntity>()
                .HasOne(g => g.PlayerO)
                .WithMany()  // sem WithMany
                .HasForeignKey(g => g.PlayerOId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relação Winner
            modelBuilder.Entity<GamesEntity>()
                .HasOne(g => g.Winner)
                .WithMany()  // sem WithMany
                .HasForeignKey(g => g.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<PlayersEntity> Players { get; set; } = null!;
        public DbSet<GamesEntity> Games { get; set; } = null!;
        public DbSet<MovesEntity> Moves { get; set; } = null!;
    }
}
