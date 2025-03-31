using FizzBuzz.Data;
using FizzBuzz.Models;
using FizzBuzz.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

using Rule = FizzBuzz.Models.Rule;

var builder = WebApplication.CreateBuilder(args);

//builder.Environment.WebRootPath = Path.Combine(
//    builder.Environment.ContentRootPath,
//    "fizzbuzz-web",
//    "build"
//);
//builder.Services.AddSpaStaticFiles(options =>
//{
//    options.RootPath = "fizzbuzz-web/build";
//});
// Add EF Core in-memory
builder.Services.AddDbContext<FizzBuzzDbContext>(options =>
    options.UseInMemoryDatabase("FizzBuzzDb"));

builder.Services.AddScoped<IGameService, GameService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger (optional for ease of testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("MyCorsPolicy", builder =>
//    {
//        builder.WithOrigins("http://localhost:3000", "http://localhost:3001")
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//    });
//});

// Enable static files & spa fallback
//builder.Services.AddSpaStaticFiles(configuration =>
//{
//    configuration.RootPath = "fizzbuzz-web/build";
//});

var app = builder.Build();

// Migrate or seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FizzBuzzDbContext>();
    // Optionally seed with default rules: Fizz(3), Buzz(5)
    if (!db.Rules.Any())
    {
        db.Rules.Add(new Rule { Divider = 3, Text = "Fizz", IsActive = true });
        db.Rules.Add(new Rule { Divider = 5, Text = "Buzz", IsActive = true });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Serve the files from the React build folder
app.UseStaticFiles();          // For the root
//app.UseSpaStaticFiles();
app.MapFallbackToFile("index.html");
//app.UseCors("MyCorsPolicy");
//app.UseRouting();
app.MapControllers();

app.Run();
