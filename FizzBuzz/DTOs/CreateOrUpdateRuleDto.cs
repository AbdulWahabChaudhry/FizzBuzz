namespace FizzBuzz.DTOs
{
    public class CreateOrUpdateRuleDto
    {
        public int Divider { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
