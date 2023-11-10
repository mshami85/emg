using Emergency.Models;
using X.PagedList;

namespace Emergency.Dtos
{
    public class MessageIndexViewModel
    {
        public IPagedList<MobileMessage> AllMessages { get; set; }
        public IEnumerable<MobileMessage> LatestStatues { get; set; } = Enumerable.Empty<MobileMessage>();
    }
}