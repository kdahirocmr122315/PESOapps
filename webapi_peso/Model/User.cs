namespace webapi_peso.Model
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int UserType { get; set; }
        public int InActive { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
