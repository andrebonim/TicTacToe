using Microsoft.AspNetCore.Mvc;
using TicTacToe.Model;
using TicTacToe.Business;
using TicTacToe.Business.Interfaces;
using TicTacToe.Model.Extensions;

namespace TicTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerBusiness _playerBusiness;

        public PlayersController(IPlayerBusiness playerBusiness)
        {
            _playerBusiness = playerBusiness;
        }

        /// <summary>
        /// Cria ou retorna jogador existente pelo nome (case insensitive)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PlayersModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrGet([FromBody] PlayerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length < 2)
                return BadRequest("Nome deve ter pelo menos 2 caracteres.");

            var player = await _playerBusiness.GetOrCreatePlayerAsync(dto.Name.Trim());
            var result = player.SecureMap<PlayersModel>();

            return Ok(result);
        }

        /// <summary>
        /// Ranking dos melhores jogadores (por vitórias, depois taxa de vitória)
        /// </summary>
        [HttpGet("ranking")]
        [ProducesResponseType(typeof(List<RankingEntryDto>), 200)]
        public async Task<IActionResult> GetRanking([FromQuery] int top = 10)
        {
            top = Math.Clamp(top, 5, 50);
            var ranking = await _playerBusiness.GetTopPlayersAsync(top);
            return Ok(ranking);
        }
    }
}