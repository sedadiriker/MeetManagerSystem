namespace EntitiesLayer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? ProfilePicture { get; set; }

        public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();


    }
}
