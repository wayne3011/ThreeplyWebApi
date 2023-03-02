using ThreeplyWebApi.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using ThreeplyWebApi.Controllers.AuthenticationScheme;
using ThreeplyWebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.Configure<GroupsOptions>(builder.Configuration.GetSection("Groups"));
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("ThreeplyDatabase"));
builder.Logging.AddMongoLogger(opt => builder.Configuration.GetSection("Logging").GetSection("MongoLogger").Bind(opt));
builder.Services.AddAuthentication();

builder.Services.AddControllers();
builder.Services.AddSingleton<MongoDbService>();
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
