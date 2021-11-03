using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api;
using GenshinLibrary.Services;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Resin;
using GenshinLibrary.Services.Wishes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary
{
    class Init
    {
        static void Main() => new Init().RunBotAsync().GetAwaiter().GetResult();

        // Discord.NET essentials.
        private static DiscordSocketClient _client;
        private static CommandService _commands;
        private static IServiceProvider _services;

        private CommandSupportService _support;
        private AuthDiscordBotListApi _dbl;

        private async Task RunBotAsync()
        {
            // Instantiate the essentials
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
            });

            _commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<InteractiveService>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<CommandSupportService>()
                .AddSingleton<ResinTrackerService>()
                .AddSingleton<WishService>()
                .AddSingleton<GachaSimulatorService>()
                .BuildServiceProvider();

            // Set services
            _support = _services.GetRequiredService<CommandSupportService>();

            // Register events
            _client.Log += Log;
            _client.JoinedGuild += OnJoin;
            _client.LeftGuild += OnLeave;

            // Retrieve the config
            var config = Globals.GetConfig();

            // Init DBL auth
            _dbl = new AuthDiscordBotListApi(830870729390030960, config.TopGGToken);

            // Retrieve connection string and init db connection
            Logger.Log("Database", "Connecting to database");
            await _services.GetRequiredService<DatabaseService>().InitAsync(config.Connection);

            // Retrieve token
            string token = config.Token;

            // Init services
            await _services.GetRequiredService<MessageHandler>().InstallCommandsAsync();
            await _services.GetRequiredService<WishService>().InitAsync();
            await _services.GetRequiredService<ResinTrackerService>().InitAsync();

            // Login and start
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Set status
            await _client.SetGameAsync("gl!help");

            // Make sure it doesn't die
            await Task.Delay(-1);
        }

        private async Task OnJoin(SocketGuild guild)
        {
            if (guild.SystemChannel != null)
            {
                var embed = _support.GetInfoEmbed();
                embed.WithTitle("Thank you for inviting the bot!");
                await guild.SystemChannel.SendMessageAsync(embed: embed.Build());
            }

            _ = Task.Run(async () => await guild.DownloadUsersAsync());

            await _dbl.UpdateStats(_client.Guilds.Count);
        }

        private async Task OnLeave(SocketGuild guild)
        {
            await _dbl.UpdateStats(_client.Guilds.Count);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
