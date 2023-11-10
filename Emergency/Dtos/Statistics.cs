namespace Emergency.Dtos
{
    public class MessageStatistics
    {
        public int TotalCount { get; set; }
        public int DeliveredCount { get; set; }
    }

    public class StatusStatistics
    {
        public int TotalCount { get; set; }
        public int OkCount { get; set; }
        public int HelpCount { get; set; }
        public int EmergencyCount { get; set; }
    }
}
