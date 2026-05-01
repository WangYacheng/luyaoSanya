using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TradeSystem.App.Models;
using TradeSystem.App.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace TradeSystem.App.Views.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : INavigableView<ViewModels.SettingsViewModel>
{
    public ViewModels.SettingsViewModel ViewModel { get; }
    private Point _dragStartPoint;

    public SettingsPage(ViewModels.SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void MenuTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        ViewModel.SelectedMenu = e.NewValue as NavigationMenuDraftItem;
    }

    private void MenuTreeItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void MenuTreeItem_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(null);
        if (Math.Abs(currentPosition.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(currentPosition.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
        if (treeViewItem?.DataContext is not NavigationMenuDraftItem item)
        {
            return;
        }

        DragDrop.DoDragDrop(treeViewItem, item, DragDropEffects.Move);
    }

    private void MenuTreeItem_OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(NavigationMenuDraftItem))
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void MenuTreeItem_OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(NavigationMenuDraftItem))
            || sender is not TreeViewItem treeViewItem
            || treeViewItem.DataContext is not NavigationMenuDraftItem targetItem
            || e.Data.GetData(typeof(NavigationMenuDraftItem)) is not NavigationMenuDraftItem draggedItem)
        {
            return;
        }

        var position = e.GetPosition(treeViewItem);
        var placement = GetDropPlacement(treeViewItem, position);
        ViewModel.MoveMenuItem(draggedItem, targetItem, placement);
        e.Handled = true;
    }

    private static MenuDropPlacement GetDropPlacement(TreeViewItem treeViewItem, Point position)
    {
        if (position.X > treeViewItem.ActualWidth * 0.62)
        {
            return MenuDropPlacement.Child;
        }

        return position.Y > treeViewItem.ActualHeight / 2
            ? MenuDropPlacement.After
            : MenuDropPlacement.Before;
    }

    private static T? FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T target)
            {
                return target;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
