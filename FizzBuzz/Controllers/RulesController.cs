using FizzBuzz.Data;
using FizzBuzz.DTOs;
using FizzBuzz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rule = FizzBuzz.Models.Rule;

namespace FizzBuzz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public RulesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rule>>> GetRules(bool includeInactive = false)
        {
            return await _gameService.GetAllRules(includeInactive: includeInactive);
        }

        [HttpPost]
        public async Task<ActionResult<Rule>> CreateRule(CreateOrUpdateRuleDto dto)
        {
            if (dto.Divider <= 0)
                return BadRequest("Divider must be a positive integer.");

            var rule = await _gameService.NewRule(new Rule
            {
                Divider = dto.Divider,
                Text = dto.Text,
                IsActive = dto.IsActive
            });

            return CreatedAtAction(nameof(GetRules), new { id = rule.Id }, rule);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Rule>> UpdateRule(int id, CreateOrUpdateRuleDto dto)
        {
            var existingRule = await _gameService.GetRule(id);
            if (existingRule == null)
                return NotFound();

            if (dto.Divider <= 0)
                return BadRequest("Divider must be a positive integer.");

            existingRule.Divider = dto.Divider;
            existingRule.Text = dto.Text;
            existingRule.IsActive = dto.IsActive;

            var updatedRule = _gameService.UpdateRule(existingRule);
            return Ok(updatedRule);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var existingRule = await _gameService.GetRule(id);
            if (existingRule == null)
                return NotFound();

            var success = _gameService.DeleteRule(existingRule);
            return NoContent();
        }
    }
}
