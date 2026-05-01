using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.Core.Models;
using Wpf.Ui.Controls;

namespace TradeSystem.App.Models
{
    public partial class NavigationMenuDraftItem : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconSymbol))]
        private string _icon = "Document24";

        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private bool _isExpanded = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPageMenu))]
        private bool _isGroup;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private bool _isVisibleInFilter = true;

        [ObservableProperty]
        private int _order;

        [ObservableProperty]
        private string _targetPageTag = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        public ObservableCollection<NavigationMenuDraftItem> Children { get; } = [];

        public NavigationMenuDraftItem? Parent { get; set; }

        public bool IsPageMenu => !IsGroup;

        public SymbolRegular IconSymbol =>
            Enum.TryParse<SymbolRegular>(Icon, true, out var symbol) ? symbol : SymbolRegular.Document24;

        public NavigationMenuItem ToEntity()
        {
            return new NavigationMenuItem
            {
                Id = Id,
                ParentId = Parent?.Id,
                Title = Title,
                Icon = Icon,
                TargetPageTag = TargetPageTag,
                Order = Order,
                IsGroup = IsGroup,
                IsVisible = IsVisible
            };
        }

        public static NavigationMenuDraftItem FromEntity(NavigationMenuItem item)
        {
            return new NavigationMenuDraftItem
            {
                Id = item.Id,
                Icon = string.IsNullOrWhiteSpace(item.Icon) ? "Document24" : item.Icon,
                IsGroup = item.IsGroup,
                IsVisible = item.IsVisible,
                Order = item.Order,
                TargetPageTag = item.TargetPageTag,
                Title = item.Title
            };
        }
    }
}
