using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
});
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/dueby", GetTodosDueBy);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    var todos = await db.Todos.ToArrayAsync();
    var result = todos.Select(todo => new
    {
        todo.Id,
        todo.Name,
        todo.IsComplete,
        Category = todo.Category.ToString(),
        todo.CreatedAt,
        todo.DueDate
    });

    return TypedResults.Ok(result);
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(Todo inputTodo, TodoDb db)
{
    var todo = new Todo
    {
        Name = inputTodo.Name,
        IsComplete = inputTodo.IsComplete,
        Category = inputTodo.Category,
        days = inputTodo.days,
        DueDate = DateTime.Now.AddDays(inputTodo.days).ToString("yyyy-MM-dd")
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}

static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    todo.Category = inputTodo.Category;
    todo.days = inputTodo.days;
    todo.DueDate = inputTodo.CreatedAt.AddDays(inputTodo.days).ToString("yyyy-MM-dd");

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

static async Task<IResult> GetTodosDueBy([FromQuery] string dueDateString, TodoDb db)
{
    var dueDate = DateTime.ParseExact(dueDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    var todos = await db.Todos
        .Where(todo => DateTime.ParseExact(todo.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= dueDate)
        .ToArrayAsync();

    return TypedResults.Ok(todos);
}