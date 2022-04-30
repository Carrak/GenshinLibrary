using System;

namespace GenshinLibrary.Pagers
{
    public enum PagerDirection
    {
        Backward,
        Forward
    }

    public abstract class Pager
    {
        public int Page { get; private set; }
        public int TotalPages { get; }

        protected Pager(int totalPages)
        {
            Page = 0;
            TotalPages = totalPages;
        }

        public void FlipPage(PagerDirection pd)
        {
            Page = pd switch
            {
                PagerDirection.Backward => Page == 0 ? TotalPages - 1 : Page - 1,
                PagerDirection.Forward => Page == TotalPages - 1 ? 0 : Page + 1,
                _ => throw new NotImplementedException($"Unknown {nameof(PagerDirection)}")
            };
        }
    }


}
