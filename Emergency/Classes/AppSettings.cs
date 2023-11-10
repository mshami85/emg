namespace Emergency.Classes
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public JWTSection JWT { get; set; }
        public Version MinVersion { get; set; }
        public bool NotifyForChat { get; set; }
        public bool NotifyForMessage { get; set; }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }

    public class JWTSection
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Secret { get; set; }
        public int ValidityDays { get; set; }

    }
}
