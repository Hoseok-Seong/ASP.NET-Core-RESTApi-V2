namespace TodoApi
{
    public class Todo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsComplete { get; set; }
        public Category Category { get; set; }
    }

    public enum Category
    {
        Personal,
        Work,
        Study
    }
}
