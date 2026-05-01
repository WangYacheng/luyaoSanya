namespace TradeSystem.Core.Models
{
    public class NavigationMenuItem
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string TargetPageTag { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsGroup { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
