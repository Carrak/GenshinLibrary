﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Commands
{
    public class GLInteractiveBase : InteractiveBase<GLCommandContext>
    {
        public async Task<SocketMessage> NextMessageWithConditionAsync(GLCommandContext context, Func<SocketMessage, bool> func)
        {
            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            List<RestUserMessage> errorMessages = new List<RestUserMessage>();
            TimeSpan timeout = TimeSpan.FromSeconds(60);
            int cancelLimit = 3;

            int count = 0;

            // Temporary message handler
            Task Handler(SocketMessage message)
            {
                // Ensure the message is in the same channel and by the same user
                if (message.Author.Id != context.User.Id || message.Channel.Id != context.Channel.Id)
                    return Task.CompletedTask;

                // If message matches the condition, set it as the result
                if (func(message))
                {
                    eventTrigger.SetResult(message);
                    return Task.CompletedTask;
                }

                // Check if the process should be cancelled, and if it is, set the result to null
                if (++count == cancelLimit || message.Content.StartsWith(Globals.DefaultPrefix, StringComparison.OrdinalIgnoreCase))
                    eventTrigger.SetResult(null);

                return Task.CompletedTask;
            }

            Context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var delay = Task.Delay(timeout);
            var task = await Task.WhenAny(trigger, delay);

            Context.Client.MessageReceived -= Handler;

            if (task == trigger)
                return await trigger;
            else
                return null;
        }

        public async Task<bool> NextReactionAsync(IEmote emote, IUserMessage message, IUser user, TimeSpan? timeout = null)
        {
            var eventTrigger = new TaskCompletionSource<bool>();
            timeout ??= TimeSpan.FromSeconds(60);

            // Temporary message handler
            Task Handler(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
            {
                // Ensure the message is in the same channel and by the same user
                if (reaction.User.Value.Id != user.Id || cachedMessage.Id != message.Id)
                    return Task.CompletedTask;

                // If message matches the condition, set it as the result
                if (reaction.Emote.ToString() == emote.Name.ToString())
                    eventTrigger.SetResult(true);

                return Task.CompletedTask;
            }

            Context.Client.ReactionAdded += Handler;

            var trigger = eventTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay);

            Context.Client.ReactionAdded -= Handler;

            if (task == trigger)
                return await trigger;
            else
                return false;
        }
    }
}
