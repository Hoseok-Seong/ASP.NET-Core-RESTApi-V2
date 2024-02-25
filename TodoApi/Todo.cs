namespace TodoApi
{
    public class Todo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int days { get; set; }
        public string DueDate { get; set; }
    }

    public enum Category
    {
        Personal,
        Work,
        Study
    }
}
