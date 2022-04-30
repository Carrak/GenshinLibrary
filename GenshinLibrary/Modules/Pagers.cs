using Discord.Interactions;
using GenshinLibrary.Pagers;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.Menus;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    public class Pagers : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MenuService _menus;

        public Pagers(MenuService menus)
        {
            _menus = menus;
        }

        [ComponentInteraction("banner_pager:*,*,*")]
        [VerifyUserAndMenu]
        public async Task Pager(ulong userId, int menuId, PagerDirection pd)
        {
            var pager = _menus.GetMenuContent<BannerSelectionPager>(userId);
            pager.FlipPage(pd);
            await pager.UpdateAsync(Context);
        }
    }
}
