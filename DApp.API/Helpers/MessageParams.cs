namespace DApp.API.Helpers
{
    public class MessageParams
    {
        private const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;

        public int PageSize {
            get {return pageSize;}
            set {
                if (pageSize != value) {
                    pageSize = (value > maxPageSize ? maxPageSize : value);
                }
            }
        }

        public int UserId { get; set; }

        public string MessageContainer { get; set; } = "unread";
    }
}