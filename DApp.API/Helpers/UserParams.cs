namespace DApp.API.Helpers
{
    public class UserParams
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

        public int MaxAge { get; set; }
        public int MinAge { get; set; }
        public string Gender { get; set; }
        public int UserId { get; set; }
        public string OrderBy { get; set; }
        public bool Likers { get; set; }
        public bool Likees { get; set; }
    }
}