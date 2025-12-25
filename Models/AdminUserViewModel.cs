namespace StokDepo.Models
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = "";
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
