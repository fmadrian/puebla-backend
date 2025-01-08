
using Microsoft.EntityFrameworkCore;
using PueblaApi.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region DB Context

var dbSettings = builder.Configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbSettings.ConnectionString));

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
}*/


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
