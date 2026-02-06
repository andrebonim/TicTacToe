using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Business;
using TicTacToe.Business.Interfaces;
using TicTacToe.Model;
using TicTacToe.Model.Extensions;

namespace TicTacToe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameBusiness _gameBusiness;
        private readonly IPlayerBusiness _playerBusiness;

        public GamesController(IGameBusiness gameBusiness, IPlayerBusiness playerBusiness)
        {
            _gameBusiness = gameBusiness;
            _playerBusiness = playerBusiness;
        }

        /// <summary>
        /// Cria uma nova partida (jogador X entra imediatamente)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GamesModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateGame([FromBody] GameCreateDto dto)
        {
                if (string.IsNullOrWhiteSpace(dto.PlayerXName))
                return BadRequest("Nome do Jogador X é obrigatório.");

            try
            {
                var gameDto = await _gameBusiness.CreateNewGameAsync(dto.PlayerXName.Trim(), dto.IsNetwork,  dto.RoomCode);
                return CreatedAtAction(nameof(GetGameStatus), new { id = gameDto.Id }, gameDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Jogador O entra na partida (join)
        /// </summary>
        [HttpPost("{id}/join")]
        [ProducesResponseType(typeof(GamesModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> JoinGame(long id, [FromBody] PlayerCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Nome do Jogador O é obrigatório.");

            try
            {
                var updatedGame = await _gameBusiness.JoinGameAsync(id, dto.Name.Trim());
                return Ok(updatedGame);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return NotFound("Partida não encontrada.");
            }
        }

        /// <summary>
        /// Faz uma jogada na posição indicada (0-8)
        /// </summary>
        [HttpPost("{id}/moves")]
        [ProducesResponseType(typeof(MakeMoveResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MakeMove(long id, [FromBody] MakeMoveDto dto)
        {
            if (dto.Position < 0 || dto.Position > 8)
                return BadRequest("Posição inválida (0 a 8).");

            try
            {
                // Chama o Business com o DTO inteiro
                var result = await _gameBusiness.MakeMoveAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Logar
                return StatusCode(500, "Erro interno.");
            }
        }

        /// <summary>
        /// Consulta status atual da partida (polling para front-end)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GameStatusDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetGameStatus(long id)
        {
            var status = await _gameBusiness.GetGameStatusAsync(id);
            if (status == null)
                return NotFound();

            return Ok(status);
        }

        /// <summary>
        /// Últimas partidas finalizadas (histórico)
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<GameHistoryDto>), 200)]
        public async Task<IActionResult> GetHistory([FromQuery] int limit = 20)
        {
            limit = Math.Clamp(limit, 5, 100);
            var history = await _gameBusiness.GetRecentGamesAsync(limit);
            return Ok(history);
        }

        // Opcional: endpoint para buscar partida por room code
        [HttpGet("room/{roomCode}")]
        [ProducesResponseType(typeof(GamesModel), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByRoomCode(string roomCode)
        {
            var game = await _gameBusiness.GetGameByRoomCodeAsync(roomCode);
            if (game == null)
                return NotFound("Partida não encontrada ou já finalizada.");

            return Ok(game);
        }

        /// <summary>
        /// Finaliza uma partida (local ou rede) informando vencedor ou empate
        /// </summary>
        [HttpPost("{id}/finish")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> FinishGame(long id, [FromBody] FinishGameDto dto)
        {
            try
            {
                await _gameBusiness.FinishGameAsync(id, dto.WinnerSymbol, dto.IsDraw);
                return Ok(new { message = "Partida finalizada e salva com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Logar erro real
                return StatusCode(500, "Erro interno ao finalizar partida.");
            }
        }

        /// <summary>
        /// Exibe um modal com os 10 maiores vencedores
        /// </summary>
        [HttpGet("top10")]
        [ProducesResponseType(typeof(List<RankingEntryDto>), 200)]
        public async Task<IActionResult> GetTop10Winners()
        {
            var top10 = await _gameBusiness.Top10();

            if (top10 == null)
                return NotFound("Ranking não localizado.");

            return Ok(top10);
        }

        public class FinishGameDto
        {
            public string? WinnerSymbol { get; set; }
            public bool IsDraw { get; set; }
        }
    }
}