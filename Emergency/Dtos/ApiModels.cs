
using Emergency.Models;

namespace Emergency.Dtos
{
    public class ApiLoginModel
    {
        public string ClientVersion { get; set; }
        public string AndroidId { get; set; }
        public string SecureCode { get; set; }
    }

    public class MessageModel
    {
        public DateTime SendDate { get; set; }
        public string? Message { get; set; }
        public GeoLocation Location { get; set; } = new GeoLocation();
        public MobileStatus Status { get; set; }

        public bool IsValid()
        {
            var case1 = Status == MobileStatus.IAM_OK;
            var case2 = Status == MobileStatus.NEED_HELP && Location.HasValue() && !string.IsNullOrWhiteSpace(Message);
            var case3 = Status == MobileStatus.EMERGENCY && Location.HasValue();

            return case1 || case2 || case3;
        }
    }
}