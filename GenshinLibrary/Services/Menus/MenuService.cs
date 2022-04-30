using Microsoft.Extensions.Caching.Memory;

namespace GenshinLibrary.Services.Menus
{
    public class MenuService
    {
        private readonly MemoryCache _cache;

        public MenuService()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public int CreateMenu(ulong userId, MemoryCacheEntryOptions options = default)
        {
            return _cache.Set(userId, new Menu(GetNextMenuId(userId)), options).MenuId;
        }

        public int CreateMenu<T>(ulong userId, T content, MemoryCacheEntryOptions options = default)
        {
            return _cache.Set(userId, new Menu<T>(GetNextMenuId(userId), content), options).MenuId;
        }

        public void SetMenuContent<T>(ulong userId, T content)
        {
            var currentMenu = _cache.Get<Menu>(userId);
            _cache.Set(userId, new Menu<T>(currentMenu.MenuId, content));
        }

        public T GetMenuContent<T>(ulong userId)
        {
            return _cache.Get<Menu<T>>(userId).Content;
        }

        public bool TryGetMenuId(ulong userId, out int menuId)
        {
            if(_cache.TryGetValue<Menu>(userId, out var menu))
            {
                menuId = menu.MenuId;
                return true;
            }
            menuId = default;
            return false;
        }

        private int GetNextMenuId(ulong userId) => _cache.TryGetValue<Menu>(userId, out var menu) ? menu.MenuId + 1 : 0;
    }
}
