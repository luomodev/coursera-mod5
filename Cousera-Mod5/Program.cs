using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory user store
var users = new List<User>();

app.UseMiddleware<RequestCountingMiddleware>();

// 1) Get all users
app.MapGet("/users", () => Results.Ok(users));

// 2) Get a single user by username
app.MapGet("/users/{username}", (string username) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    return user is not null 
        ? Results.Ok(user) 
        : Results.NotFound();
});

// 3) Create a new user
app.MapPost("/users", (User newUser) =>
{
    if (users.Any(u => u.Username == newUser.Username))
        return Results.Conflict($"User '{newUser.Username}' already exists.");

    users.Add(newUser);
    return Results.Created($"/users/{newUser.Username}", newUser);
});

// 4) Update an existing user’s age
app.MapPut("/users/{username}", (string username, User updated) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    if (user is null) 
        return Results.NotFound();

    user.UserAge = updated.UserAge;
    return Results.Ok(user);
});

// 5) Delete a user
app.MapDelete("/users/{username}", (string username) =>
{
    var user = users.FirstOrDefault(u => u.Username == username);
    if (user is null) 
        return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.MapGet("/metrics", () =>
    Results.Ok(RequestCountingMiddleware.Counts)
);
app.Run();

// Simple user model
public class User
{
    required public string Username { get; set; }
    public int UserAge { get; set; }
}


