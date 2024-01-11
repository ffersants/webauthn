using Data.Context;
using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddFido2Context<AppDbContext>();
builder.Services.AddCors(p => p.AddPolicy("teste", p => p.AllowAnyMethod()
                                                         .AllowAnyHeader()
                                                         .WithOrigins("http://localhost:3000")
                                                         .AllowCredentials()));
//builder.Services.Configure<Fido2Configuration>(builder.Configuration.GetSection("fido2"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnectionString")));
builder.Services.AddFido2(builder.Configuration.GetSection("Fido2"));

var app = builder.Build();

app.UseCors("teste");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

//app.UseSession();

app.MapControllers();

app.Run();
