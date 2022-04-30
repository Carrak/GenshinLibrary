using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotsList.Api;
using GenshinLibrary.Services;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Menus;
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
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private InteractionService _interactionService;
        private AuthDiscordBotListApi _dbl;

        private async Task RunBotAsync()
        {
            // Instantiate the essentials
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged
                & ~GatewayIntents.GuildScheduledEvents
                & ~GatewayIntents.GuildInvites
                | GatewayIntents.GuildMembers
            });

            _commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
            });

            _interactionService = new InteractionService(_client.Rest);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_interactionService)
                .AddSingleton<InteractiveService>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<ResinTrackerService>()
                .AddSingleton<WishService>()
                .AddSingleton<GachaSimulatorService>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton<MenuService>()
                .BuildServiceProvider();

            // Register events
            _client.Log += Log;
            _client.JoinedGuild += OnJoin;
            _client.LeftGuild += OnLeave;
            _client.Ready += Ready;

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
            _services.GetRequiredService<MessageHandler>().Init();
            await _services.GetRequiredService<WishService>().InitAsync();
            await _services.GetRequiredService<ResinTrackerService>().InitAsync();
            await _services.GetRequiredService<InteractionHandler>().InitAsync();

            // Login and start
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Make sure it doesn't die
            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            await UpdateStatus();
        }

        private async Task OnJoin(SocketGuild guild)
        {
            if (guild.SystemChannel != null)
            {
                var embed = Globals.HelpEmbed.ToEmbedBuilder();
                embed.WithTitle("Thank you for inviting the bot!");
                await guild.SystemChannel.SendMessageAsync(embed: embed.Build());
            }

            _ = Task.Run(async () => await guild.DownloadUsersAsync());

            await _dbl.UpdateStats(_client.Guilds.Count);
            await UpdateStatus();
        }

        private async Task OnLeave(SocketGuild guild)
        {
            await _dbl.UpdateStats(_client.Guilds.Count);
            await UpdateStatus();
        }

        private async Task UpdateStatus()
        {
            await _client.SetGameAsync($"{_client.Guilds.Count} guilds");
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
