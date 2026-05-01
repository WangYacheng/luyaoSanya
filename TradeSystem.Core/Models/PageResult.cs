namespace TradeSystem.Core.Models
{
    public class PageResult<T>
    {
        public string? SearchString { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<T>? DataList { get; set; }
    }
}
