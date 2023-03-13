using ThreeplyWebApi.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using ThreeplyWebApi.Controllers.AuthenticationScheme;
using Serilog.Configuration;
using Serilog;
using ThreeplyWebApi.Services.ScheduleParser;
using MongoDB.Driver.Core.Operations;
using Microsoft.Extensions.Logging.Console;
using ThreeplyWebApi.Services.ServicesOptions;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.Configure<GroupsOptions>(builder.Configuration.GetSection("Groups"));
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("ThreeplyDatabase"));
builder.Services.Configure<ScheduleParserOptions>(builder.Configuration.GetSection("Parsing"));
builder.Services.Configure<ScheduleUpdateOptions>(builder.Configuration.GetSection("Parsing"));
builder.Logging.AddConsole();
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext());
builder.Services.AddAuthentication();
builder.Services.AddControllers();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<ScheduleParserService>();
builder.Services.AddSingleton<GroupsService>();
builder.Services.AddHostedService<ScheduleUpdateWorker>();

builder.Services.AddAuthentication(GeneralUserAuthenticationSchemeOptions.Name).
    AddScheme<GeneralUserAuthenticationSchemeOptions, GeneralUserAuthenticationSchemeHandler>(GeneralUserAuthenticationSchemeOptions.Name, opt => { });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
