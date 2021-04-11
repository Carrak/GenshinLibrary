using Discord.Addons.Interactive;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.ReactionCallback.Base
{
    /// <summary>
    ///     Abstract class for paging a collection where each element of the collection is a page.
    /// </summary>
    /// <inheritdoc/> 
    abstract class SingleItemPagedMessage<T, U> : PagedMessageBase<T, U> where U : T
    {
        protected SingleItemPagedMessage(InteractiveService interactive,
            SocketCommandContext context,
            IEnumerable<T> collection) : base(interactive, context, collection, collection.Count())
        {
        }

        protected override U CurrentPage()
        {
            return (U)_collection.ElementAt(Page);
        }
    }

    /// <inheritdoc/>
    abstract class SingleItemPagedMessage<T> : SingleItemPagedMessage<T, T>
    {
        protected SingleItemPagedMessage(InteractiveService interactive,
            SocketCommandContext context,
            IEnumerable<T> collection) : base(interactive, context, collection)
        {
        }
    }
}
