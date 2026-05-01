using Microsoft.EntityFrameworkCore;
using TradeSystem.Core.Abstractions;
using TradeSystem.Core.Models;
using TradeSystem.Infrastructure.Data;

namespace TradeSystem.Infrastructure.Services
{
    public class NavigationMenuItemService : INavigationMenuItemService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public NavigationMenuItemService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<NavigationMenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await context.NavigationMenus
                .AsNoTracking()
                .OrderBy(item => item.ParentId)
                .ThenBy(item => item.Order)
                .ThenBy(item => item.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task PublishAsync(IReadOnlyList<NavigationMenuItem> menuItems, CancellationToken cancellationToken = default)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var existingItems = await context.NavigationMenus
                .OrderBy(item => item.ParentId)
                .ThenBy(item => item.Order)
                .ToListAsync(cancellationToken);

            var incomingPositiveIds = menuItems
                .Where(item => item.Id > 0)
                .Select(item => item.Id)
                .ToHashSet();

            var itemsToDelete = existingItems
                .Where(item => !incomingPositiveIds.Contains(item.Id))
                .OrderByDescending(item => GetDepth(item, existingItems))
                .ToList();

            context.NavigationMenus.RemoveRange(itemsToDelete);
            await context.SaveChangesAsync(cancellationToken);

            var existingById = await context.NavigationMenus
                .ToDictionaryAsync(item => item.Id, cancellationToken);

            var newIdByTemporaryId = new Dictionary<int, int>();
            foreach (var draftItem in menuItems.OrderBy(item => GetDepth(item, menuItems)).ThenBy(item => item.Order))
            {
                var parentId = ResolveParentId(draftItem.ParentId, newIdByTemporaryId);

                if (draftItem.Id > 0 && existingById.TryGetValue(draftItem.Id, out var existingItem))
                {
                    CopyValues(draftItem, existingItem, parentId);
                    continue;
                }

                var newItem = new NavigationMenuItem();
                CopyValues(draftItem, newItem, parentId);
                context.NavigationMenus.Add(newItem);
                await context.SaveChangesAsync(cancellationToken);

                if (draftItem.Id < 0)
                {
                    newIdByTemporaryId[draftItem.Id] = newItem.Id;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private static void CopyValues(NavigationMenuItem source, NavigationMenuItem target, int? parentId)
        {
            target.ParentId = parentId;
            target.Title = source.Title.Trim();
            target.Icon = source.Icon.Trim();
            target.TargetPageTag = source.TargetPageTag.Trim();
            target.Order = source.Order;
            target.IsGroup = source.IsGroup;
            target.IsVisible = source.IsVisible;
        }

        private static int? ResolveParentId(int? parentId, IReadOnlyDictionary<int, int> newIdByTemporaryId)
        {
            if (parentId is null)
            {
                return null;
            }

            if (parentId.Value > 0)
            {
                return parentId.Value;
            }

            return newIdByTemporaryId.TryGetValue(parentId.Value, out var resolvedId) ? resolvedId : null;
        }

        private static int GetDepth(NavigationMenuItem item, IReadOnlyCollection<NavigationMenuItem> items)
        {
            var depth = 0;
            var current = item;
            var guard = 0;

            while (current.ParentId is not null && guard++ < items.Count)
            {
                var parent = items.FirstOrDefault(candidate => candidate.Id == current.ParentId.Value);
                if (parent is null)
                {
                    break;
                }

                depth++;
                current = parent;
            }

            return depth;
        }
    }
}
