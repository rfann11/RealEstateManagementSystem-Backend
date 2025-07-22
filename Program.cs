using REMS.Backend.Data; // DbContext sýnýfýnýzýn namespace'i
using Microsoft.EntityFrameworkCore; // DbContextOptionsBuilder için gerekli
using Npgsql.EntityFrameworkCore.PostgreSQL; // PostgreSQL saðlayýcýsý için gerekli
using REMS.Backend.Interfaces; // IIlService için
using REMS.Backend.Services;   // IlService için

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// DbContext'i servislere ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
// Servis Baðýmlýlýklarýný Enjekte Etme (Dependency Injection) 
builder.Services.AddScoped<IIlService, IlService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
