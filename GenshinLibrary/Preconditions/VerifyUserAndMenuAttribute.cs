using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Services.Menus;

namespace GenshinLibrary.Preconditions
{
    public class VerifyUserAndMenuAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.Interaction is not SocketMessageComponent componentContext)
                return Task.FromResult(PreconditionResult.FromError("Context unrecognized as component context."));

            // Split the custom ID and ensure it's of correct length
            var customIdSplit = componentContext.Data.CustomId.Split(':');
            if (customIdSplit.Length < 1)
                return Task.FromResult(PreconditionResult.FromError(""));

            // Parse parameters into appropriate types
            var paramsSplit = customIdSplit[1].Split(',');
            if (!(paramsSplit.Length >= 2 && ulong.TryParse(paramsSplit[0], out ulong userId) && int.TryParse(paramsSplit[1], out int componentMenuId)))
                return Task.FromResult(PreconditionResult.FromError("Invalid parameters in custom ID."));

            // Verify user is the same
            if (componentContext.User.Id != userId)
                return Task.FromResult(PreconditionResult.FromError("User ID does not match that of the component"));

            // Verify menu is the same
            var menuService = services.GetService(typeof(MenuService)) as MenuService;
            if (!(menuService.TryGetMenuId(userId, out var existingMenuId) && existingMenuId == componentMenuId))
                return Task.FromResult(PreconditionResult.FromError("Menu ID does not match that of the component"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
