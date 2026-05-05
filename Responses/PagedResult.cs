namespace QuilvianSystemBackend.Responses
{
    public class PagedResult<T>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<T> Items { get; set; } = new();
    }
}
