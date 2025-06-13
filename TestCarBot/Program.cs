using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TestCarBot.Bot;
using TestCarBot.DbService;
using TestCarBot.Models;
using TestCarBot.Services;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables();

var configuration = builder.Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));

services.AddSingleton(new TelegramBotClient(configuration.GetSection("BotConfiguration:Token").Value!));

services.Configure<BotConfiguration>(configuration.GetSection("BotConfiguration"));
services.AddSingleton<BotService>();
services.AddSingleton<UpdateHandlers>();
services.AddScoped<BotCommands>();
services.AddSingleton<PdfGeneratorService>();
services.AddSingleton<OpenAiService>();
services.AddScoped<OcrService>();

services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

var serviceProvider = services.BuildServiceProvider();



var bot = serviceProvider.GetRequiredService<BotService>();
await bot.StartAsync();