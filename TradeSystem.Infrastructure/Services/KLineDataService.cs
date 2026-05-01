using Microsoft.EntityFrameworkCore;

namespace TradeSystem.Infrastructure.Services
{

    public class KLineDataService : BaseService
    {

        public KLineDataService(DbContext context): base(context)
        {
        }
    }
}