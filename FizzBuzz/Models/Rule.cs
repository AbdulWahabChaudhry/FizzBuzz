namespace FizzBuzz.Models
{
    public class Rule
    {
        public int Id { get; set; }
        public int Divider { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
