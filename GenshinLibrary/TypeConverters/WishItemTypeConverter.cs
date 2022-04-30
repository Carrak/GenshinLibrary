using Discord;
using Discord.Interactions;
using GenshinLibrary.Models;
using GenshinLibrary.Services.Wishes;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.TypeReaders
{
    public class WishItemTypeConverter : TypeConverter<WishItem>
    {
        public override ApplicationCommandOptionType GetDiscordType()
        {
            return ApplicationCommandOptionType.String;
        }

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var _wishes = (WishService)services.GetService(typeof(WishService));
            return _wishes.GetBestSuggestion(option.Value as string) is WishItem c ?
                Task.FromResult(TypeConverterResult.FromSuccess(c)) :
                Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"No such wish item has been found."));
        }
    }

    public class CharacterTypeReader : TypeConverter<Character>
    {
        public override ApplicationCommandOptionType GetDiscordType()
        {
            return ApplicationCommandOptionType.String;
        }

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var _wishes = (WishService)services.GetService(typeof(WishService));
            return _wishes.GetBestSuggestion(option.Value as string) is Character c ?
                Task.FromResult(TypeConverterResult.FromSuccess(c)) :
                Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"No such character has been found."));
        }
    }

    public class WeaponTypeReader : TypeConverter<Weapon>
    {
        public override ApplicationCommandOptionType GetDiscordType()
        {
            return ApplicationCommandOptionType.String;
        }

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var _wishes = (WishService)services.GetService(typeof(WishService));
            return _wishes.GetBestSuggestion(option.Value as string) is Weapon w ?
                Task.FromResult(TypeConverterResult.FromSuccess(w)) :
                Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"No such weapon has been found."));
        }
    }
}
