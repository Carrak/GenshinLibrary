using Discord.Commands;
using GenshinLibrary.GenshinWishes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace GenshinLibrary.TypeReaders
{
    class BannerTypeReader : TypeReader
    {
        IReadOnlyDictionary<string, Banner> banners { get; }

        public BannerTypeReader()
        {
            var dict = ImmutableDictionary.CreateBuilder<string, Banner>(StringComparer.InvariantCultureIgnoreCase);

            var names = Enum.GetNames(typeof(Banner));
            foreach(var name in names)
            {
                var value = (Banner)Enum.Parse(typeof(Banner), name);
                dict[name[0].ToString()] = value;
                dict[name] = value;
            }

            banners = dict.ToImmutable();
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (banners.TryGetValue(input, out var value))
                return Task.FromResult(TypeReaderResult.FromSuccess(value));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Value is not a banner."));
        }
    }
}
