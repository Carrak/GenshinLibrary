using Discord;
using Discord.Interactions;
using GenshinLibrary.Models;
using GenshinLibrary.Services.Wishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.AutocompleteHandlers
{
    public class WishItemAutocomplete : AutocompleteHandler
    {
        private const StringComparison sc = StringComparison.InvariantCultureIgnoreCase;
        protected Type[] types { get; set; } = Array.Empty<Type>();

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var wishes = (WishService)services.GetService(typeof(WishService));
            var subject = autocompleteInteraction.Data.Current.Value as string;

            List<AutocompleteResult> suggestions = new();
            foreach (var wishItem in wishes.WishItemsByWID.Values)
            {
                if (!types.Contains(wishItem.GetType()))
                    continue;

                if (wishItem.Name.StartsWith(subject, sc) || wishItem.Aliases.Any(x => x.StartsWith(subject, sc)))
                    suggestions.Insert(0, new AutocompleteResult(wishItem.Name, wishItem.Name));
                else if (wishItem.Name.Contains(subject, sc) || wishItem.Aliases.Any(x => x.Contains(subject, sc)))
                    suggestions.Add(new AutocompleteResult(wishItem.Name, wishItem.Name));

                if (suggestions.Count == 5)
                    break;
            }

            return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
        }
    }

    public class WishItemAutocomplete<T1> : WishItemAutocomplete 
        where T1 : WishItem
    {
        public WishItemAutocomplete() : base() 
        {
            types = new[] { typeof(T1) };
        }
    }

    public class WishItemAutocomplete<T1,T2> : WishItemAutocomplete 
        where T1 : WishItem
        where T2 : WishItem
    {
        public WishItemAutocomplete() : base()
        {
            types = new[] { typeof(T1), typeof(T2) };
        }
    }
}
