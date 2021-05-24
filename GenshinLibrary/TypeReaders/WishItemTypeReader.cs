using Discord.Commands;
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
            if (_wishes.WishItems.TryGetValue(input, out var wishItem))
                return Task.FromResult(TypeReaderResult.FromSuccess(wishItem));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"No such item has been found. Did you mean `\"{_wishes.GetBestSuggestion(input).Name}\"`?"));
        }
    }
}
