using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Business.Interfaces;
using TicTacToe.Data.Entity;
using TicTacToe.Model;
using TicTacToe.Repository;

namespace TicTacToe.Business
{
    public class PlayerBusiness : IPlayerBusiness
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IMapper _mapper;

        public PlayerBusiness(IPlayerRepository playerRepository, IMapper mapper)
        {
            _playerRepository = playerRepository;
            _mapper = mapper;
        }

        public async Task<PlayersEntity> GetOrCreatePlayerAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
                throw new ArgumentException("Nome do jogador deve ter pelo menos 2 caracteres.");

            return await _playerRepository.CreateOrGetAsync(name.Trim());
        }

        public async Task<List<RankingEntryDto>> GetTopPlayersAsync(int limit = 10)
        {
            limit = Math.Clamp(limit, 3, 50);
            return await _playerRepository.GetRankingAsync(limit);
        }

        public async Task UpdatePlayerStatsAsync(long playerId, bool won, bool draw)
        {
            await _playerRepository.UpdateStatsAsync(playerId, won, draw);
        }

        public async Task<PlayersEntity?> GetPlayerByIdAsync(long id)
        {
            return await _playerRepository.GetByIdAsync(id);
        }
    }
}
