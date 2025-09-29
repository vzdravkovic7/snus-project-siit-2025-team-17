namespace SnusProject.Models
{
    public class ClientInfo
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public ClientPermission Permission { get; set; }
        public int Priority { get; set; }

        public ClientInfo(int clientId, string name, ClientPermission permission, int priority)
        {
            ClientId = clientId;
            Name = name;
            Permission = permission;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"ID: {ClientId}, Name: {Name}, Permission: {Permission}, Priority: {Priority}";
        }
    }
}