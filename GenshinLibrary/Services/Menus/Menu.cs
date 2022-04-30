namespace GenshinLibrary.Services.Menus
{
    public class Menu
    {
        public int MenuId { get; }

        public Menu(int menuId)
        {
            MenuId = menuId;
        }
    }

    public class Menu<T> : Menu
    {
        public T Content { get; }

        public Menu(int menuId, T value) : base(menuId)
        {
            Content = value;
        }
    }
}
