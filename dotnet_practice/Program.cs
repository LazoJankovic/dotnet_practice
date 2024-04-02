using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.Urls.Add("http://localhost:5144");

// app.MapGet("/", () => "Hello World!");
var todos = new List<Todo>();

app.MapGet("/todos", () => todos); //minimal APIs understands  w/o strongly typed results

app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todos.SingleOrDefault(t => id == t.Id);
    return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);
});

app.MapPost("/todos", (Todo task) => {
    todos.Add(task);
    return TypedResults.Created("/todos{id}", task);
});

app.MapDelete("/todos/{id}", (int id) =>
{
    todos.RemoveAll(t => t.Id == id);
    return TypedResults.NoContent();
});

app.Run();


public record Todo(int Id, string Name, DateTime DueData, bool isCompleted);