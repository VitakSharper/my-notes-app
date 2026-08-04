using Common;
using Microsoft.EntityFrameworkCore;
using Overflow.ServiceDefaults;
using ProfileService.Data;
using ProfileService.Endpoints;
using ProfileService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddKeyClockAuthentication();

// Turns the DataAnnotations on the request DTOs into a 400 with the field errors, instead of every
// handler checking lengths by hand. Opt-in, and new in .NET 10 for minimal APIs.
builder.Services.AddValidation();

builder.AddNpgsqlDbContext<ProfileDbContext>("profileDb");

// Conventional routing: the only configuration needed is where the handlers live.
await builder.UseWolverineWithRabbitMqAsync(options =>
{
    options.ApplicationAssembly = typeof(Program).Assembly;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();

// After authentication, so the claims are there to create a profile from, and before the endpoints,
// so /profiles/me finds the row the first time a user shows up.
app.UseMiddleware<UserProfileCreationMiddleware>();

app.MapProfileEndpoints();

app.MapDefaultEndpoints();

// Same as the question service: the schema is applied at startup rather than by hand.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
