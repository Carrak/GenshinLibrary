using Discord.Commands;
using GenshinLibrary.Models;
using GenshinLibrary.Services.Wishes;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.TypeReaders
{
    class WishItemTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var _wishes = (WishService)services.GetService(typeof(WishService));
            if (_wishes.WishItems.TryGetValue(input, out var wishItem1))
                return Task.FromResult(TypeReaderResult.FromSuccess(wishItem1));
            else if (_wishes.GetBestSuggestion(input) is WishItem wishItem2)
                return Task.FromResult(TypeReaderResult.FromSuccess(wishItem2));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"No such wish item has been found."));
        }
    }
}
