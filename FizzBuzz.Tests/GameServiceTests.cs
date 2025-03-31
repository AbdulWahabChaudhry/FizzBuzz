using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzBuzz.Data;
using FizzBuzz.Models;
using FizzBuzz.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FizzBuzz.Tests
{
    public class GameServiceTests
    {
        private readonly FizzBuzzDbContext _context;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            // Use in-memory database for testing
            var options = new DbContextOptionsBuilder<FizzBuzzDbContext>()
                .UseInMemoryDatabase(databaseName: "FizzBuzzTestDb_GameService")
                .Options;

            _context = new FizzBuzzDbContext(options);
            _context.Database.EnsureCreated();

            _context.Rules.RemoveRange(_context.Rules);
            _context.SaveChanges();

            _context.Rules.AddRange(
                new Rule { Id = 1, Divider = 3, Text = "Fizz", IsActive = true },
                new Rule { Id = 2, Divider = 5, Text = "Buzz", IsActive = true }
            );
            _context.SaveChanges();

            _gameService = new GameService(_context);
        }

        [Fact]
        public void CalculateFizzBuzzValue_NumberDivisibleBy3_ReturnsFizz()
        {
            var rules = new List<Rule>
            {
                new Rule { Divider = 3, Text = "Fizz", IsActive = true },
                new Rule { Divider = 5, Text = "Buzz", IsActive = true },
            };

            var result = _gameService.CalculateFizzBuzzValue(9, rules);

            Assert.Equal("Fizz", result);
        }

        [Fact]
        public void CalculateFizzBuzzValue_NumberDivisibleBy3And5_ReturnsFizzBuzz()
        {
            var rules = new List<Rule>
            {
                new Rule { Divider = 3, Text = "Fizz", IsActive = true },
                new Rule { Divider = 5, Text = "Buzz", IsActive = true },
            };

            var result = _gameService.CalculateFizzBuzzValue(15, rules);

            Assert.Equal("FizzBuzz", result);
        }

        [Fact]
        public void CalculateFizzBuzzValue_NumberNotDivisibleByAnyDivider_ReturnsNumberItself()
        {
            var rules = new List<Rule>
            {
                new Rule { Divider = 3, Text = "Fizz", IsActive = true },
                new Rule { Divider = 5, Text = "Buzz", IsActive = true },
            };

            var result = _gameService.CalculateFizzBuzzValue(7, rules);

            Assert.Equal("7", result);
        }

        [Fact]
        public async Task GetAllRules_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveRules()
        {
            var inactiveRule = new Rule { Divider = 10, Text = "Foo", IsActive = false };
            _context.Rules.Add(inactiveRule);
            await _context.SaveChangesAsync();

            var activeRules = await _gameService.GetAllRules(includeInactive: false);

            Assert.True(activeRules.All(r => r.IsActive));
        }

        [Fact]
        public async Task GetAllRules_WhenIncludeInactiveIsTrue_ReturnsAllRules()
        {
            var allRules = await _gameService.GetAllRules(includeInactive: true);

            Assert.Equal(2, allRules.Count);
        }

        [Fact]
        public async Task NewRule_AddsRuleToDatabase()
        {
            var newRule = new Rule { Divider = 7, Text = "Pop", IsActive = true };

            var createdRule = await _gameService.NewRule(newRule);
            var allRules = await _gameService.GetAllRules(true);

            Assert.NotNull(createdRule);
            Assert.Equal(newRule.Divider, createdRule.Divider);
            Assert.Contains(allRules, r => r.Divider == 7 && r.Text == "Pop");
        }

        [Fact]
        public async Task UpdateRule_UpdatesExistingRule()
        {
            var existingRule = await _gameService.GetRule(1); // Divider=3, Text="Fizz"
            existingRule.Text = "Fizzle";

            var updatedRule = await _gameService.UpdateRule(existingRule);

            Assert.Equal("Fizzle", updatedRule.Text);

            var ruleInDb = await _gameService.GetRule(1);
            Assert.Equal("Fizzle", ruleInDb.Text);
        }

        [Fact]
        public async Task DeleteRule_RemovesRuleFromDatabase()
        {
            var ruleToDelete = await _gameService.GetRule(2); // Divider=5, Text="Buzz"
            Assert.NotNull(ruleToDelete);

            var success = await _gameService.DeleteRule(ruleToDelete);
            var ruleAfterDelete = await _gameService.GetRule(2);

            Assert.True(success);
            Assert.Null(ruleAfterDelete);
        }
    }
}
