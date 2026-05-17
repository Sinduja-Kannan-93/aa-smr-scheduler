using AaSmr.Api.Data;
using AaSmr.Api.Features.Appointments;
using AaSmr.Api.Features.Branches;
using AaSmr.Api.Features.ServiceTypes;
using AaSmr.Api.Features.Slots;
using AaSmr.Api.Features.Users;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("Health")
   .WithOpenApi();

app.MapUsers();
app.MapBranches();
app.MapServiceTypes();
app.MapSlots();
app.MapAppointments();

app.Run();

public partial class Program { }
