using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Models;
using GenshinLibrary.Modules;
using GenshinLibrary.TypeReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenshinLibrary
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
        {
            _client = client;
            _handler = handler;
            _services = services;
        }

        public async Task InitAsync()
        {
            _handler.AddModalInfo<ResinTracker.ResinModal>();
            _handler.AddModalInfo<Wishes.AddWishBulkModal>();
            _handler.AddTypeConverter<WishItem>(new WishItemTypeConverter());
            _handler.AddTypeConverter<Character>(new CharacterTypeReader());
            _handler.AddTypeConverter<Weapon>(new WeaponTypeReader());

            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;
            _handler.Log += Log;
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            await _handler.AddModulesToGuildAsync(_client.GetGuild(Globals.GetConfig().MainGuildId), false, _handler.GetModuleInfo<Changelogs>());
            await _handler.RegisterCommandsGloballyAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
                var context = new SocketInteractionContext(_client, interaction);

                // Execute the incoming command.
                var result = await _handler.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                        case InteractionCommandError.ConvertFailed:
                            {
                                var embed = new EmbedBuilder()
                                    .WithTitle("Could not execute the command")
                                    .WithDescription(result.ErrorReason)
                                    .WithColor(Color.Red)
                                    .Build();

                                if (context.Interaction.HasResponded)
                                    await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                                else
                                    await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
                                break;
                            }
                        case InteractionCommandError.Exception when result is ExecuteResult er:
                            await LogException(er, context);
                            break;
                    }
                }
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task LogException(ExecuteResult executeResult, SocketInteractionContext context)
        {
            var embed = new EmbedBuilder();

            // Footer with the user
            var footer = new EmbedFooterBuilder()
            {
                Text = context.User.ToString(),
                IconUrl = context.User.GetAvatarUrl()
            };

            // Exception stack trace
            StackTrace st = new(executeResult.Exception, true);

            // The log with all non-empty frames
            System.Text.StringBuilder log = new();

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
                .AddField(executeResult.Exception.GetType().Name, executeResult.Exception.Message[..Math.Min(executeResult.Exception.Message.Length, 1024)]);

            // Add all inner exceptions
            var currentException = executeResult.Exception.InnerException;
            while (currentException != null)
            {
                embed.AddField(currentException.GetType().Name, currentException.Message[..Math.Min(currentException.Message.Length, 1024)]);
                currentException = currentException.InnerException;
            }

            List<string> splitExceptionData = new();

            // Add exception data (and split it into groups to fit it into multiple fields
            List<string> temp = new();
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

            string action = context.Interaction switch
            {
                SocketSlashCommand ssc => $"/{ssc.CommandName} {string.Join(' ', ssc.Data.Options.Select(x => $"{x.Name}:{x.Value}"))}",
                SocketMessageComponent smc => $"Button {smc.Data.CustomId}",
                _ => $"{context.Interaction.GetType()}"
            };

            // Message info
            embed.AddField("Command/action", action, true)
                .AddField("Environment",
                $"Guild: {(context.Guild is null ? "DM" : context.Guild.Id.ToString())}\n" +
                $"User: {context.User.Id}\n" +
                $"Channel: {context.Channel.Id}\n");

            List<string> splitStacktrace = new();
            for (int index = 0; index < executeResult.Exception.StackTrace.Length; index += 1994)
                splitStacktrace.Add(executeResult.Exception.StackTrace.Substring(index, Math.Min(1994, executeResult.Exception.StackTrace.Length - index)));

            // Send the logs to the channel
            if (_client.GetChannel(Globals.GetConfig().LogChannelId) is ITextChannel logChannel)
            {
                foreach (string stacktrace in splitStacktrace)
                    await logChannel.SendMessageAsync($"```{stacktrace}```");
                await logChannel.SendMessageAsync(embed: embed.Build());
            }
            else
                Logger.Log("Exception", "No log channel is found to log the exception.");
        }
    }
}