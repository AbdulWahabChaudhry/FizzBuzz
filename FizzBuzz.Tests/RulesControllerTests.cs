using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzBuzz.Controllers;
using FizzBuzz.Data;
using FizzBuzz.DTOs;
using FizzBuzz.Models;
using FizzBuzz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FizzBuzz.Tests
{
    public class RulesControllerTests
    {
        private readonly FizzBuzzDbContext _context;
        private readonly IGameService _gameService;
        private readonly RulesController _controller;

        public RulesControllerTests()
        {
            var options = new DbContextOptionsBuilder<FizzBuzzDbContext>()
                .UseInMemoryDatabase("FizzBuzzTestDb_RulesController")
                .Options;

            _context = new FizzBuzzDbContext(options);
            _context.Database.EnsureCreated();

            // Clear and seed
            _context.Rules.RemoveRange(_context.Rules);
            _context.SaveChanges();

            _context.Rules.AddRange(
                new Rule { Divider = 3, Text = "Fizz", IsActive = true },
                new Rule { Divider = 5, Text = "Buzz", IsActive = true }
            );
            _context.SaveChanges();

            _gameService = new GameService(_context);
            _controller = new RulesController(_gameService);
        }

        [Fact]
        public async Task GetRules_ReturnsAllRules()
        {
            var result = await _controller.GetRules();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Rule>>>(result);
            var rules = Assert.IsAssignableFrom<IEnumerable<Rule>>(actionResult.Value);
            Assert.Equal(2, rules.Count()); 
        }

        [Fact]
        public async Task CreateRule_ValidRule_ReturnsCreatedAtAction()
        {
            var dto = new CreateOrUpdateRuleDto
            {
                Divider = 7,
                Text = "Pop",
                IsActive = true
            };

            var result = await _controller.CreateRule(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var rule = Assert.IsType<Rule>(createdResult.Value);
            Assert.Equal(7, rule.Divider);
            Assert.Equal("Pop", rule.Text);
        }

        [Fact]
        public async Task CreateRule_InvalidDivider_ReturnsBadRequest()
        {
            var dto = new CreateOrUpdateRuleDto
            {
                Divider = 0,  
                Text = "Zero",
                IsActive = true
            };

            var result = await _controller.CreateRule(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Divider must be a positive integer.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateRule_ExistingId_UpdatesTheRule()
        {
            var existingRule = _context.Rules.FirstOrDefault(r => r.Divider == 3);
            Assert.NotNull(existingRule);

            var dto = new CreateOrUpdateRuleDto
            {
                Divider = 9,
                Text = "FizzNew",
                IsActive = false
            };

            var result = await _controller.UpdateRule(existingRule.Id, dto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedRuleTask = Assert.IsType<Task<Rule>>(okResult.Value);

            var updatedRule = await updatedRuleTask;
            Assert.Equal(9, updatedRule.Divider);
            Assert.Equal("FizzNew", updatedRule.Text);
            Assert.False(updatedRule.IsActive);
        }

        [Fact]
        public async Task UpdateRule_NonExistingId_ReturnsNotFound()
        {
            var dto = new CreateOrUpdateRuleDto
            {
                Divider = 11,
                Text = "Foo",
                IsActive = true
            };

            var result = await _controller.UpdateRule(999, dto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteRule_ExistingId_ReturnsNoContent()
        {
            var existingRule = _context.Rules.FirstOrDefault(r => r.Divider == 3);
            Assert.NotNull(existingRule);

            var result = await _controller.DeleteRule(existingRule.Id);

            Assert.IsType<NoContentResult>(result);

            var ruleInDb = _context.Rules.Find(existingRule.Id);
            Assert.Null(ruleInDb);
        }

        [Fact]
        public async Task DeleteRule_NonExistingId_ReturnsNotFound()
        {
            var result = await _controller.DeleteRule(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
