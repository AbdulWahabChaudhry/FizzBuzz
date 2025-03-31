using FizzBuzz.Data;
using FizzBuzz.DTOs;
using FizzBuzz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FizzBuzz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private static Random _random = new Random();

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        // GET: api/game/new
        [HttpGet("new")]
        public ActionResult<int> GetNewRandomNumber()
        {
            // Generate random positive integer. You can choose an upper bound, e.g., 1 - 100
            int randomNumber = _random.Next(1, 101);
            return Ok(randomNumber);
        }

        // POST: api/game/check
        [HttpPost("check")]
        public async Task<ActionResult<bool>> CheckAnswer([FromBody] CheckAnswerDto dto)
        {
            var rules = await _gameService.GetAllRules();

            string correctResult = _gameService.CalculateFizzBuzzValue(dto.Number, rules);
            bool isCorrect = (dto.PlayerAnswer.Trim().Equals(correctResult, StringComparison.OrdinalIgnoreCase));

            return Ok(isCorrect);
        }
    }
}
