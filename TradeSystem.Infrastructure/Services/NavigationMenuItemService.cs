using Microsoft.EntityFrameworkCore;

namespace TradeSystem.Infrastructure.Services
{
    
    public class NavigationMenuItemService : BaseService
    {
        
        public NavigationMenuItemService(DbContext context): base(context)
        {
        }
    }
}