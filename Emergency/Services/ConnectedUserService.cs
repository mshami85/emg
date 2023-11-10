namespace Emergency.Services
{
    public interface IConnectedUserService
    {
        void Add(string connectionId, int id, string name, string role);
        void Remove(string connectionId);
        (int Id, string Name, string Role) Get(string connectionId);
        IEnumerable<(int Id, string Name, string Role)> GetConnected();
    }

    public class ConnectedUserService : IConnectedUserService
    {
        private readonly Dictionary<string, (int Id, string Name, string Role)> _users;

        public ConnectedUserService()
        {
            _users = new();
        }

        public void Add(string connectionId, int id, string name, string role)
        {
            lock (this)
            {
                _users.Add(connectionId, (id, name, role));
            }
        }

        public void Remove(string connectionId)
        {
            lock (this)
            {
                _users.Remove(connectionId);
            }
        }

        public IEnumerable<(int Id, string Name, string Role)> GetConnected()
        {
            lock (this)
            {
                return _users.Select(u => u.Value).ToList();
            }
        }

        public (int Id, string Name, string Role) Get(string connectionId)
        {
            lock (this)
            {
                if (_users.ContainsKey(connectionId))
                    return _users[connectionId];
                return default;
            }
        }
    }
}
