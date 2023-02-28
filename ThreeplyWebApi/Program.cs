using ThreeplyWebApi.Services;
using ThreeplyWebApi.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using ThreeplyWebApi.Controllers.AuthenticationScheme;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<GroupsDatabaseSettings>(builder.Configuration.GetSection("ThreeplyDatabase"));
builder.Services.AddAuthentication();

builder.Services.AddControllers();
builder.Services.AddSingleton<GroupsService>();

builder.Services.AddAuthentication(GeneralUserAuthenticationSchemeOptions.Name).
    AddScheme<GeneralUserAuthenticationSchemeOptions, GeneralUserAuthenticationSchemeHandler>(GeneralUserAuthenticationSchemeOptions.Name, opt => { });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
