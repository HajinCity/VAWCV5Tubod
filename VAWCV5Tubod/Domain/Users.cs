using System;
using System.Collections.Generic;
using System.Text;

namespace VAWCV5Tubod.Domain
{
    public class Users
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
