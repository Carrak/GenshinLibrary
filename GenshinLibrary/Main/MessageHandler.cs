using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Preconditions;
using GenshinLibrary.TypeReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenshinLibrary
{
    public class MessageHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly CommandSupportService _support;

        public void BlockReceivingMessages() => _client.MessageReceived -= HandleMessagesAsync;
        public void EnableReceivingMessages() => _client.MessageReceived += HandleMessagesAsync;

        public MessageHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client, CommandSupportService support)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _support = support;

            _client.MessageReceived += HandleMessagesAsync;
            _commands.CommandExecuted += HandleCommandExecuted;
        }

        /// <summary>
        ///     Installs command modules and commands themselves.
        /// </summary>
        public async Task InstallCommandsAsync()
        {
            _commands.AddTypeReader<Color>(new ColorTypeReader());
            _commands.AddTypeReader<Banner>(new BannerTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _commands.CreateModuleAsync("", x =>
            {
                var helpCommand = _commands.Modules.FirstOrDefault(x => x.Name == "Support").Commands.FirstOrDefault(x => x.Name == "help" && x.Parameters.Count == 1);
                x.AddAttributes(new HelpIgnoreAttribute());

                foreach (var module in _commands.Modules.Where(x => !x.Attributes.Any(attr => attr is HelpIgnoreAttribute)))
                    x.AddCommand(module.Name, async (ctx, _, provider, _1) =>
                    {
                        var parseResult = ParseResult.FromSuccess(new List<TypeReaderResult>() { TypeReaderResult.FromSuccess(module.Name) }, new List<TypeReaderResult>());
                        await helpCommand.ExecuteAsync(ctx, parseResult, _services);
                    }, command => { });
            });

            foreach (var cmd in _commands.Commands.Where(x => !x.Module.Attributes.Any(atr => atr is HelpIgnoreAttribute)))
            {
                if (!cmd.Preconditions.Any(x => x is RatelimitAttribute))
                    Console.WriteLine($"{cmd.Name} has no cooldown set.");

                if (string.IsNullOrEmpty(cmd.Summary))
                    Console.WriteLine($"{cmd.Name} has no summary.");
            }
        }

        /// <summary>
        ///     Determines the behaviour when a message is received.
        /// </summary>
        /// <param name="arg">The received message.</param>
        private async Task HandleMessagesAsync(SocketMessage arg)
        {
            // Return if the message is from a bot
            if (!(arg is SocketUserMessage message) || message.Author.IsBot)
                return;

            var channel = message.Channel as SocketGuildChannel;
            var guild = channel?.Guild;

            // Return if bot can't reply
            if (guild != null && (!guild.CurrentUser.GuildPermissions.SendMessages || !guild.CurrentUser.GetPermissions(channel).SendMessages))
                return;

            var context = new GLCommandContext(_client, message);
            var prefix = Globals.DefaultPrefix;

            int argPosition = 0;
            if (message.HasStringPrefix(prefix + " ", ref argPosition, StringComparison.OrdinalIgnoreCase) || message.HasStringPrefix(prefix, ref argPosition, StringComparison.OrdinalIgnoreCase))
            {
                if (Globals.Maintenance)
                {
                    var app = await context.Client.GetApplicationInfoAsync();
                    if (context.User.Id != app.Owner.Id)
                        return;
                }

                var result = await _commands.ExecuteAsync(context, argPosition, _services, MultiMatchHandling.Best);

                if (!result.IsSuccess)
                    await HandleErrorsAsync(context, result, argPosition);
            }
        }

        private Task HandleCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            if (commandInfo.IsSpecified)
                Console.WriteLine(new LogMessage(LogSeverity.Info, "Command", $"{context.User} executed the {commandInfo.Value.Name} command in " +
                    $"{(context.Guild is null ? "DM" : $"{context.Guild.Name} in channel #{context.Channel.Name}")} ({result.IsSuccess})\n" +
                    $"{(result.IsSuccess ? "" : $"\nCommand execution error: {result.ErrorReason}")}"));
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Determines the behaviour when a command fails to execute.
        /// </summary>
        private async Task HandleErrorsAsync(SocketCommandContext context, IResult result, int argPosition)
        {
            switch (result.Error)
            {
                case CommandError.BadArgCount:
                case CommandError.ParseFailed:
                case CommandError.Exception when result is ParseResult pr:
                    var command = _commands.Search(context, argPosition).Commands[0].Command;
                    string description = "";

                    description = $"Usage: `{Globals.DefaultPrefix}{_support.GetCommandHeader(command)}`\nFor more information refer to `{Globals.DefaultPrefix}help {_support.GetFullCommandName(command)}`";

                    var helpEmbed = new EmbedBuilder();

                    helpEmbed.WithTitle(result.ErrorReason)
                        .WithDescription(description)
                        .WithColor(Color.Red);

                    await context.Channel.SendMessageAsync(embed: helpEmbed.Build());
                    break;

                case CommandError.Exception when result is ExecuteResult er:
                    if (er.Exception is HttpException)
                    {
                        await context.Channel.SendMessageAsync("An HttpException has occured. This is likely due to missing permissions. Check the bot's permissions and try again.");
                        return;
                    }

                    // Construct the embed
                    var embed = new EmbedBuilder();
                    embed.WithColor(Color.Red)
                        .WithDescription("**An exception occured while executing this command.**")
                        .WithFooter("Contact Carrak#8088 if this keeps happening.");
                    // Send the message without awaiting it
                    _ = Task.Run(async () => await context.Channel.SendMessageAsync(embed: embed.Build()));
                    // Log the exception
                    await LogException(er, context);
                    break;

                case CommandError.UnknownCommand:
                    return;

                default:
                    if (!string.IsNullOrEmpty(result.ErrorReason))
                        await context.Channel.SendMessageAsync($"{result.ErrorReason}");
                    break;
            }
        }

        /// <summary>
        ///     Logs the exception in a convenient 
        /// </summary>
        /// <param name="executeResult">The result of the failed command.</param>
        /// <param name="context">The context of the command.</param>
        private async Task LogException(ExecuteResult executeResult, SocketCommandContext context)
        {
            var embed = new EmbedBuilder();

            // Footer with the user
            var footer = new EmbedFooterBuilder()
            {
                Text = context.User.ToString(),
                IconUrl = context.User.GetAvatarUrl()
            };

            // Exception stack trace
            StackTrace st = new StackTrace(executeResult.Exception, true);

            // The log with all non-empty frames
            System.Text.StringBuilder log = new System.Text.StringBuilder();

            // Get frames where the line is specified
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                int line = sf.GetFileLineNumber();

                if (line == 0)
                    continue;

                log.Append($"In {sf.GetFileName()}\nAt line {line}\n");
            }

            embed.WithColor(Color.Red)
                .WithTitle($"Command exception")
                .WithAuthor(_client.CurrentUser)
                .WithDescription(log.ToString())
                .WithFooter(footer)
                .AddField(executeResult.Exception.GetType().Name, executeResult.Exception.Message.Substring(0, Math.Min(executeResult.Exception.Message.Length, 1024)));

            // Add all inner exceptions
            var currentException = executeResult.Exception.InnerException;
            while (currentException != null)
            {
                embed.AddField(currentException.GetType().Name, currentException.Message.Substring(0, Math.Min(currentException.Message.Length, 1024)));
                currentException = currentException.InnerException;
            }

            List<string> splitExceptionData = new List<string>();

            // Add exception data (and split it into groups to fit it into multiple fields
            List<string> temp = new List<string>();
            string splitter = "\n";
            foreach (var data in executeResult.Exception.Data.Cast<System.Collections.DictionaryEntry>())
            {
                string toAdd = $"**{data.Key}**\n{data.Value}";

                if (toAdd.Length > 1024)
                    continue;

                if (temp.Sum(x => x.Length) + toAdd.Length + (temp.Count - 1) * splitter.Length < 1024)
                    temp.Add(toAdd);
                else
                {
                    splitExceptionData.Add(string.Join(splitter, temp));
                    temp.Clear();
                    temp.Add(toAdd);
                }
            }

            // Add the remains (if any are present)
            if (temp.Count > 0)
                splitExceptionData.Add(string.Join(splitter, temp));

            // Add a field for each exception data fraction
            for (int i = 0; i < splitExceptionData.Count; i++)
                embed.AddField($"Exception data {i + 1}", splitExceptionData[i]);

            // Message info
            embed.AddField("Message content", context.Message.Content, true)
                .AddField("Environment",
                $"Guild: {(context.Guild is null ? "DM" : context.Guild.Id.ToString())}\n" +
                $"User: {context.User.Id}\n" +
                $"Channel: {context.Channel.Id}\n" +
                $"Message: {context.Message.Id}");

            List<string> splitStacktrace = new List<string>();
            for (int index = 0; index < executeResult.Exception.StackTrace.Length; index += 1994)
                splitStacktrace.Add(executeResult.Exception.StackTrace.Substring(index, Math.Min(1994, executeResult.Exception.StackTrace.Length - index)));

            // Send the logs to the channel
            if (_client.GetChannel(830891691862655056) is ITextChannel logChannel)
            {
                try
                {
                    foreach (string stacktrace in splitStacktrace)
                        await logChannel.SendMessageAsync($"```{stacktrace}```");
                    await logChannel.SendMessageAsync(embed: embed.Build());
                }
                catch (HttpException) { }
            }
        }
    }
}
