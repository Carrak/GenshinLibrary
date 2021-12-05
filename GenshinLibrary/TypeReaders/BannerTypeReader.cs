using Discord.Commands;
using GenshinLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using GenshinLibrary.Attributes;

namespace GenshinLibrary.TypeReaders
{
    class BannerTypeReader : TypeReader
    {
        IReadOnlyDictionary<string, Banner> Banners { get; }

        public BannerTypeReader()
        {
            var dict = ImmutableDictionary.CreateBuilder<string, Banner>(StringComparer.InvariantCultureIgnoreCase);

            var names = Enum.GetNames(typeof(Banner));
            var ignoredNames = new HashSet<string>(typeof(Banner).GetAttribute<EnumIgnoreAttribute>().IgnoredNames);
            foreach (var name in names)
            {
                if (ignoredNames.Contains(name))
                    continue;

                var value = Enum.Parse<Banner>(name);
                dict[name[0].ToString()] = value;
                dict[name] = value;
            }

            Banners = dict.ToImmutable();
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Banners.TryGetValue(input, out var value))
                return Task.FromResult(TypeReaderResult.FromSuccess(value));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Value is not a banner."));
        }
    }
}
