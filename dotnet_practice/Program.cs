using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;
using System.ComponentModel;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.Urls.Add("http://localhost:5144");

app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "todos/$1"));
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context); //?
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path}  {DateTime.UtcNow}] Finished.");
});

var todos = new List<Todo>();

app.MapGet("/todos", () => todos); //minimal APIs understands  w/o strongly typed results

app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) =>
{
    var targetTodo = todos.SingleOrDefault(t => id == t.Id);
    return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);

});
  

app.MapPost("/todos", (Todo task) => {
    todos.Add(task);
    return TypedResults.Created("/todos{id}", task);

}).AddEndpointFilter(async (context, next) =>
{

    var taskArgument = context.GetArgument<Todo>(0); //return item from list at index 0, return value is Todo type
    var errors = new Dictionary<string, string[]>();
    if (taskArgument.dueDate < DateTime.UtcNow)
    {
        errors.Add(nameof(Todo.dueDate), ["Cannot have due date in the past."]);
    }
    if (taskArgument.isCompleted)
    {
        errors.Add(nameof(Todo.isCompleted), ["Cannot add completed todo."]);
    }
    if (errors.Count > 0)
    {               //ValidationPoblem is another format for strongly typed results
                    //it contains 400 status code which is associated with a bad request
        return Results.ValidationProblem(errors); //we have a validation problem -> here's all of the errors associeted with it
        /* this is formatted according to the problem specification 
           it's a specification  outlines how all web apis, ones written
           in .NET and otherwise should validation errors to their users this is*/
    }
    return await next(context);
}); ;

app.MapDelete("/todos/{id}", (int id) =>   
{
    todos.RemoveAll(t => t.Id == id);
    return TypedResults.NoContent();
});

app.Run();


public record Todo(int Id, string Name, DateTime dueDate, bool isCompleted);