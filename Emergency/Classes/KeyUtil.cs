namespace Emergency.Classes
{
    public static class KeyUtil
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!?@#$%&";

        public static string GenerateKey(int length = 8)
        {
            return new string(Enumerable.Range(1, length)
                                        .Select(n => chars[Random.Shared.Next(chars.Length)])
                                        .ToArray());
        }
    }
}