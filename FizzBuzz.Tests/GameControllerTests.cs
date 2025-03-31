using System.Threading.Tasks;
using FizzBuzz.Controllers;
using FizzBuzz.Data;
using FizzBuzz.DTOs;
using FizzBuzz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FizzBuzz.Tests
{
    public class GameControllerTests
    {
        private readonly FizzBuzzDbContext _context;
        private readonly IGameService _gameService;
        private readonly GameController _controller;

        public GameControllerTests()
        {
            var options = new DbContextOptionsBuilder<FizzBuzzDbContext>()
                .UseInMemoryDatabase("FizzBuzzTestDb_GameController")
                .Options;

            _context = new FizzBuzzDbContext(options);
            _context.Database.EnsureCreated();

            // Clear and seed
            _context.Rules.RemoveRange(_context.Rules);
            _context.SaveChanges();

            _context.Rules.AddRange(
                new Models.Rule { Divider = 3, Text = "Fizz", IsActive = true },
                new Models.Rule { Divider = 5, Text = "Buzz", IsActive = true }
            );
            _context.SaveChanges();

            _gameService = new GameService(_context);
            _controller = new GameController(_gameService);
        }

        [Fact]
        public void GetNewRandomNumber_ReturnsOkWithInt()
        {
            // Act
            var result = _controller.GetNewRandomNumber();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var number = Assert.IsType<int>(okResult.Value);
            Assert.InRange(number, 1, 100);
        }

        [Theory]
        [InlineData(3, "fizz", true)]
        [InlineData(3, "Fizz", true)]
        [InlineData(3, "buzz", false)]
        [InlineData(4, "Fizz", false)]
        [InlineData(15, "FizzBuzz", true)]
        public async Task CheckAnswer_ReturnsCorrectOrIncorrect(
            int inputNumber,
            string playerAnswer,
            bool expectedResult)
        {
            var dto = new CheckAnswerDto
            {
                Number = inputNumber,
                PlayerAnswer = playerAnswer
            };

            var actionResult = await _controller.CheckAnswer(dto);

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var isCorrect = Assert.IsType<bool>(okResult.Value);
            Assert.Equal(expectedResult, isCorrect);
        }
    }
}
