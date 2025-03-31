using FizzBuzz.Data;
using FizzBuzz.Models;
using Microsoft.EntityFrameworkCore;

namespace FizzBuzz.Services
{
    public interface IGameService
    {
        string CalculateFizzBuzzValue(int number, List<Models.Rule> rules);
        Task<List<Rule>> GetAllRules(bool includeInactive = false);
        Task<Rule> GetRule(int id);
        Task<Rule> NewRule(Rule newRule);
        Task<Rule> UpdateRule(Rule existingRule);
        Task<bool> DeleteRule(Rule rule);
    }
    public class GameService : IGameService
    {
        private readonly FizzBuzzDbContext _context;

        public GameService(FizzBuzzDbContext context)
        {
            _context = context;
        }

        public string CalculateFizzBuzzValue(int number, List<Models.Rule> rules)
        {
            // Apply all active rules in ascending order, or an approach that concatenates all matches.
            // Classic FizzBuzz logic: if multiple rules apply, we typically combine their text (like “FizzBuzz”).
            // We can also adopt a "first match" approach – but the example suggests combination for 3 & 5 => "FizzBuzz".

            var matchingTexts = new List<string>();
            foreach (var rule in rules.OrderBy(r => r.Divider))
            {
                if (number % rule.Divider == 0)
                {
                    matchingTexts.Add(rule.Text);
                }
            }

            if (matchingTexts.Count == 0)
                return number.ToString();

            return string.Join("", matchingTexts);
        }

        public async Task<List<Rule>> GetAllRules(bool includeInactive = false)
        {
            return includeInactive ? await _context.Rules.AsNoTracking().ToListAsync() : await _context.Rules.AsNoTracking().Where(r => r.IsActive).ToListAsync();
        }

        public async Task<Rule> GetRule(int id) => await _context.Rules.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<Rule> NewRule(Rule newRule)
        {
            _context.Rules.Add(newRule);
            await _context.SaveChangesAsync();

            return newRule;
        }

        public async Task<Rule> UpdateRule(Rule existingRule)
        {
            _context.Rules.Update(existingRule);
            await _context.SaveChangesAsync();

            return existingRule;
        }

        public async Task<bool> DeleteRule(Rule rule)
        {
            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
