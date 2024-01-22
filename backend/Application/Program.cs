using Application.Service;
using Data.Context;
using Data.Repository;
using Domain.Entities;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Repository;
using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(p => p.AddPolicy("teste", p => p.AllowAnyMethod()
                                                         .AllowAnyHeader()
                                                         .WithOrigins("http://localhost:3000", "https://3a32-189-84-136-194.ngrok-free.app")
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

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDispositivosService, DispositivosService>();
builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddSingleton<AuthenticatorOptions>(new AuthenticatorOptions{});
builder.Services.AddScoped<IFido2Repository, Data.Repository.Fido2Repository>();
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
