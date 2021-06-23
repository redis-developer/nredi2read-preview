using System.Collections.Generic;

namespace NRedi2Read.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Books { get; set; }
    }
}