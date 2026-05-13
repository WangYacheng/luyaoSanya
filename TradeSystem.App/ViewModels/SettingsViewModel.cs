using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.Models;
using TradeSystem.App.Views.Pages;
using TradeSystem.Core.Abstractions;
using TradeSystem.Core.Models;

namespace TradeSystem.App.ViewModels;

public enum MenuDropPlacement
{
    Before,
    After,
    Child
}

public partial class SettingsViewModel : ViewModel
{
    private readonly INavigationMenuItemService _navigationMenuItemService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private bool _isInitialized;
    private bool _isLoading;
    private int _temporaryId = -1;

    [ObservableProperty]
    private bool _hasPendingChanges;

    [ObservableProperty]
    private ObservableCollection<string> _iconOptions =
    [
        "Home24",
        "DataHistogram24",
        "Settings24",
        "Person24",
        "People24",
        "Cart24",
        "Box24",
        "Document24",
        "Add24",
        "Edit24",
        "Delete24",
        "Folder24",
        "List24",
        "Shield24"
    ];

    [ObservableProperty]
    private ObservableCollection<string> _pageOptions = [];

    [ObservableProperty]
    private ObservableCollection<NavigationMenuDraftItem> _rootMenus = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private NavigationMenuDraftItem? _selectedMenu;

    [ObservableProperty]
    private string _statusMessage = "正在加载菜单配置...";

    public SettingsViewModel(
        INavigationMenuItemService navigationMenuItemService,
        MainWindowViewModel mainWindowViewModel)
    {
        _navigationMenuItemService = navigationMenuItemService;
        _mainWindowViewModel = mainWindowViewModel;
        PageOptions = new ObservableCollection<string>(GetPageNames());
    }

    public override async Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
        {
            await LoadDraftAsync();
            _isInitialized = true;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private void AddTopLevelMenu()
    {
        var item = CreateNewMenu(null);
        RootMenus.Add(item);
        RecalculateOrders();
        SelectedMenu = item;
        MarkPending("已添加顶级菜单，等待发布。");
    }

    [RelayCommand]
    private void AddChildMenu(NavigationMenuDraftItem? parent)
    {
        if (parent is null)
        {
            return;
        }

        var child = CreateNewMenu(parent);
        parent.Children.Add(child);
        parent.IsExpanded = true;
        RecalculateOrders();
        SelectedMenu = child;
        MarkPending("已添加子菜单，等待发布。");
    }

    [RelayCommand]
    private void DeleteMenu(NavigationMenuDraftItem? item)
    {
        if (item is null)
        {
            return;
        }

        var removedItems = Flatten([item]).ToHashSet();
        var siblings = item.Parent?.Children ?? RootMenus;
        siblings.Remove(item);
        if (SelectedMenu is not null && removedItems.Contains(SelectedMenu))
        {
            SelectedMenu = RootMenus.FirstOrDefault();
        }

        RecalculateOrders();
        MarkPending("已删除菜单及其子菜单，等待发布。");
    }

    [RelayCommand]
    private void SaveDraft()
    {
        StatusMessage = HasPendingChanges
            ? "草稿已保存在当前页面，发布后才会写入数据库。"
            : "当前没有未发布的修改。";
    }

    [RelayCommand]
    private async Task PublishAsync()
    {
        var allItems = Flatten(RootMenus).ToList();
        var validationError = Validate(allItems);
        if (!string.IsNullOrWhiteSpace(validationError))
        {
            StatusMessage = validationError;
            return;
        }

        RecalculateOrders();
        await _navigationMenuItemService.PublishAsync(allItems.Select(item => item.ToEntity()).ToList());
        await _mainWindowViewModel.RefreshNavigationAsync();
        await LoadDraftAsync();
        HasPendingChanges = false;
        StatusMessage = "菜单已发布，并已刷新左侧导航。";
    }

    public bool MoveMenuItem(
        NavigationMenuDraftItem draggedItem,
        NavigationMenuDraftItem targetItem,
        MenuDropPlacement placement)
    {
        if (draggedItem == targetItem || IsDescendantOf(targetItem, draggedItem))
        {
            return false;
        }

        var oldSiblings = draggedItem.Parent?.Children ?? RootMenus;
        oldSiblings.Remove(draggedItem);

        if (placement == MenuDropPlacement.Child)
        {
            draggedItem.Parent = targetItem;
            targetItem.Children.Add(draggedItem);
            targetItem.IsExpanded = true;
        }
        else
        {
            var newSiblings = targetItem.Parent?.Children ?? RootMenus;
            draggedItem.Parent = targetItem.Parent;
            var targetIndex = newSiblings.IndexOf(targetItem);
            var insertIndex = placement == MenuDropPlacement.After ? targetIndex + 1 : targetIndex;
            newSiblings.Insert(Math.Clamp(insertIndex, 0, newSiblings.Count), draggedItem);
        }

        RecalculateOrders();
        SelectedMenu = draggedItem;
        MarkPending("菜单层级或排序已调整，等待发布。");
        return true;
    }

    private async Task LoadDraftAsync()
    {
        _isLoading = true;
        RootMenus = [];
        RootMenus.CollectionChanged += OnRootMenusCollectionChanged;

        var items = (await _navigationMenuItemService.GetAllAsync()).ToList();
        if (items.Count == 0)
        {
            items = CreateDefaultMenus();
            StatusMessage = "数据库暂无菜单，已加载默认草稿。";
        }
        else
        {
            StatusMessage = "菜单配置已加载。";
        }

        _temporaryId = Math.Min(-1, items.Min(item => item.Id) - 1);
        BuildTree(items);
        RecalculateOrders();
        ApplyFilter();
        SelectedMenu = RootMenus.FirstOrDefault();
        HasPendingChanges = false;
        _isLoading = false;
    }

    private void BuildTree(IReadOnlyList<NavigationMenuItem> items)
    {
        var draftById = items.ToDictionary(item => item.Id, NavigationMenuDraftItem.FromEntity);

        foreach (var draftItem in draftById.Values)
        {
            SubscribeItem(draftItem);
        }

        foreach (var source in items.OrderBy(item => item.ParentId).ThenBy(item => item.Order))
        {
            var draftItem = draftById[source.Id];
            if (source.ParentId is not null && draftById.TryGetValue(source.ParentId.Value, out var parent))
            {
                draftItem.Parent = parent;
                parent.Children.Add(draftItem);
            }
            else
            {
                RootMenus.Add(draftItem);
            }
        }
    }

    private NavigationMenuDraftItem CreateNewMenu(NavigationMenuDraftItem? parent)
    {
        var isGroup = parent is null;
        var item = new NavigationMenuDraftItem
        {
            Id = _temporaryId--,
            Parent = parent,
            Title = "新菜单",
            Icon = "Document24",
            IsGroup = isGroup,
            TargetPageTag = isGroup ? string.Empty : PageOptions.FirstOrDefault() ?? string.Empty,
            IsVisible = true
        };

        SubscribeItem(item);
        return item;
    }

    private void SubscribeItem(NavigationMenuDraftItem item)
    {
        item.PropertyChanged += OnDraftItemPropertyChanged;
        item.Children.CollectionChanged += OnChildCollectionChanged;
    }

    private void OnRootMenusCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MarkPending("菜单列表已修改，等待发布。");
    }

    private void OnChildCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (NavigationMenuDraftItem item in e.NewItems)
            {
                SubscribeItem(item);
            }
        }

        MarkPending("菜单列表已修改，等待发布。");
    }

    private void OnDraftItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NavigationMenuDraftItem.IsGroup)
            && sender is NavigationMenuDraftItem item)
        {
            if (item.IsGroup)
            {
                item.TargetPageTag = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(item.TargetPageTag))
            {
                item.TargetPageTag = PageOptions.FirstOrDefault() ?? string.Empty;
            }
        }

        if (e.PropertyName is nameof(NavigationMenuDraftItem.IsExpanded)
            or nameof(NavigationMenuDraftItem.IsVisibleInFilter)
            or nameof(NavigationMenuDraftItem.IconSymbol))
        {
            return;
        }

        MarkPending("菜单详情已修改，等待发布。");
    }

    private void MarkPending(string message)
    {
        if (_isLoading)
        {
            return;
        }

        HasPendingChanges = true;
        StatusMessage = message;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var keyword = SearchText.Trim();
        foreach (var item in RootMenus)
        {
            ApplyFilter(item, keyword);
        }
    }

    private static bool ApplyFilter(NavigationMenuDraftItem item, string keyword)
    {
        var ownMatch = string.IsNullOrWhiteSpace(keyword)
            || item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || item.TargetPageTag.Contains(keyword, StringComparison.OrdinalIgnoreCase);

        var childMatch = false;
        foreach (var child in item.Children)
        {
            childMatch |= ApplyFilter(child, keyword);
        }

        item.IsVisibleInFilter = ownMatch || childMatch;
        if (childMatch && !string.IsNullOrWhiteSpace(keyword))
        {
            item.IsExpanded = true;
        }

        return item.IsVisibleInFilter;
    }

    private void RecalculateOrders()
    {
        RecalculateOrders(RootMenus);
    }

    private static void RecalculateOrders(IReadOnlyList<NavigationMenuDraftItem> siblings)
    {
        for (var index = 0; index < siblings.Count; index++)
        {
            siblings[index].Order = index + 1;
            RecalculateOrders(siblings[index].Children);
        }
    }

    private static IEnumerable<NavigationMenuDraftItem> Flatten(IEnumerable<NavigationMenuDraftItem> items)
    {
        foreach (var item in items)
        {
            yield return item;

            foreach (var child in Flatten(item.Children))
            {
                yield return child;
            }
        }
    }

    private static bool IsDescendantOf(NavigationMenuDraftItem possibleChild, NavigationMenuDraftItem possibleParent)
    {
        var current = possibleChild.Parent;
        while (current is not null)
        {
            if (current == possibleParent)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static string? Validate(IReadOnlyList<NavigationMenuDraftItem> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                return "菜单名称不能为空。";
            }

            if (!item.IsGroup && string.IsNullOrWhiteSpace(item.TargetPageTag))
            {
                return $"菜单“{item.Title}”必须选择页面。";
            }
        }

        return null;
    }

    private static List<string> GetPageNames()
    {
        return typeof(SettingsPage).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "TradeSystem.App.Views.Pages")
            .Where(type => type.Name.EndsWith("Page", StringComparison.Ordinal))
            .Select(type => type.Name)
            .OrderBy(name => name)
            .ToList();
    }

    private static List<NavigationMenuItem> CreateDefaultMenus()
    {
        return
        [
            new NavigationMenuItem
            {
                Id = -1,
                Title = "仪表盘",
                Icon = "Home24",
                TargetPageTag = nameof(DashboardPage),
                Order = 1,
                IsGroup = false,
                IsVisible = true
            },
            new NavigationMenuItem
            {
                Id = -2,
                Title = "数据",
                Icon = "DataHistogram24",
                TargetPageTag = nameof(DataPage),
                Order = 2,
                IsGroup = false,
                IsVisible = true
            },
            new NavigationMenuItem
            {
                Id = -4,
                Title = "数据入库",
                Icon = "DataDownload24",
                TargetPageTag = nameof(DataDownloadPage),
                Order = 3,
                IsGroup = false,
                IsVisible = true
            },
            new NavigationMenuItem
            {
                Id = -5,
                Title = "排行榜",
                Icon = "DataTrend24",
                TargetPageTag = nameof(RankingPage),
                Order = 4,
                IsGroup = false,
                IsVisible = true
            },
            new NavigationMenuItem
            {
                Id = -3,
                Title = "菜单配置",
                Icon = "Settings24",
                TargetPageTag = nameof(SettingsPage),
                Order = 5,
                IsGroup = false,
                IsVisible = true
            }
        ];
    }
}
