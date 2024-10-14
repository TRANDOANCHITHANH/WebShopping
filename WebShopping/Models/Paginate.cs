namespace WebShopping.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public Paginate()
        {

        }
        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            //Lam tron tong item/10 items tren 1 trang VD:16 item/10 = tron 3 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);
            int currentPage = page;
            int startPage = currentPage -5 ;
            int endPage = currentPage + 4 ;
            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }
            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }
            TotalPages = totalPages;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
