using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.App.Views.Pages;
using TradeSystem.Core.Abstractions;
using TradeSystem.Core.Models;
using Wpf.Ui.Controls;

namespace TradeSystem.App.ViewModels;

public partial class MainWindowViewModel : ViewModel
{
    private readonly INavigationMenuItemService _navigationMenuItemService;

    [ObservableProperty]
    private string _applicationTitle = "Trade System";

    [ObservableProperty]
    private ObservableCollection<object> _navigationItems = [];

    [ObservableProperty]
    private ObservableCollection<object> _navigationFooter = [];

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = [];

    public MainWindowViewModel(INavigationMenuItemService navigationMenuItemService)
    {
        _navigationMenuItemService = navigationMenuItemService;
        TrayMenuItems = [new MenuItem { Header = "Home", Tag = "tray_home" }];
        _ = RefreshNavigationAsync();
    }

    public async Task RefreshNavigationAsync()
    {
        var menuItems = (await _navigationMenuItemService.GetAllAsync())
            .Where(item => item.IsVisible)
            .OrderBy(item => item.ParentId)
            .ThenBy(item => item.Order)
            .ThenBy(item => item.Id)
            .ToList();

        if (menuItems.Count == 0)
        {
            LoadDefaultNavigation();
            return;
        }

        var roots = menuItems.Where(item => item.ParentId is null).ToList();
        var childrenByParentId = menuItems
            .Where(item => item.ParentId is not null)
            .GroupBy(item => item.ParentId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.Order).ThenBy(item => item.Id).ToList());

        var mainItems = new ObservableCollection<object>();
        var footerItems = new ObservableCollection<object>();

        foreach (var root in roots)
        {
            var navigationItem = BuildNavigationItem(root, childrenByParentId);
            if (navigationItem is null)
            {
                continue;
            }

            if (root.TargetPageTag == nameof(SettingsPage))
            {
                footerItems.Add(navigationItem);
            }
            else
            {
                mainItems.Add(navigationItem);
            }
        }

        if (footerItems.Count == 0)
        {
            footerItems.Add(CreateNavigationItem("菜单配置", "Settings24", typeof(SettingsPage)));
        }

        NavigationItems = mainItems;
        NavigationFooter = footerItems;
    }

    private static NavigationViewItem? BuildNavigationItem(
        NavigationMenuItem source,
        IReadOnlyDictionary<int, List<NavigationMenuItem>> childrenByParentId)
    {
        var childItems = childrenByParentId.TryGetValue(source.Id, out var children)
            ? children.Select(child => BuildNavigationItem(child, childrenByParentId)).Where(item => item is not null).ToList()
            : [];

        var pageType = source.IsGroup ? null : ResolvePageType(source.TargetPageTag);
        if (pageType is null && childItems.Count == 0)
        {
            return null;
        }

        var item = CreateNavigationItem(source.Title, source.Icon, pageType);
        foreach (var childItem in childItems)
        {
            item.MenuItems.Add(childItem!);
        }

        return item;
    }

    private static NavigationViewItem CreateNavigationItem(string title, string icon, Type? pageType)
    {
        var item = new NavigationViewItem
        {
            Content = title,
            Icon = new SymbolIcon { Symbol = ResolveSymbol(icon) }
        };

        if (pageType is not null)
        {
            item.TargetPageType = pageType;
        }

        return item;
    }

    private static Type? ResolvePageType(string pageName)
    {
        return typeof(SettingsPage).Assembly
            .GetTypes()
            .FirstOrDefault(type =>
                type.Namespace == "TradeSystem.App.Views.Pages"
                && type.Name.Equals(pageName, StringComparison.Ordinal));
    }

    private static SymbolRegular ResolveSymbol(string icon)
    {
        return Enum.TryParse<SymbolRegular>(icon, true, out var symbol) ? symbol : SymbolRegular.Document24;
    }

    private void LoadDefaultNavigation()
    {
        var workspace = CreateNavigationItem("Workspace", "Folder24", null);
        workspace.MenuItems.Add(CreateNavigationItem("Home", "Home24", typeof(DashboardPage)));
        workspace.MenuItems.Add(CreateNavigationItem("Data", "DataHistogram24", typeof(DataPage)));
        workspace.MenuItems.Add(CreateNavigationItem("数据入库", "DataDownload24", typeof(DataDownloadPage)));
        workspace.MenuItems.Add(CreateNavigationItem("排行榜", "DataTrend24", typeof(RankingPage)));

        NavigationItems =
        [
            workspace
        ];

        NavigationFooter =
        [
            CreateNavigationItem("Settings", "Settings24", typeof(SettingsPage))
        ];
    }
}
