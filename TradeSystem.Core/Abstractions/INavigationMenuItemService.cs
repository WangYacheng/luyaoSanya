using TradeSystem.Core.Models;

namespace TradeSystem.Core.Abstractions
{
    public interface INavigationMenuItemService
    {
        Task<IReadOnlyList<NavigationMenuItem>> GetAllAsync(CancellationToken cancellationToken = default);

        Task PublishAsync(IReadOnlyList<NavigationMenuItem> menuItems, CancellationToken cancellationToken = default);
    }
}
