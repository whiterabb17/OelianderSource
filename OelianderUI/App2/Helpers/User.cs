using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OelianderUI.Helpers
{
    public class CollectionListing
    {
        public int Num
        {
            get; set;
        }
        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public string IPAddress
        {
            get; set;
        }
        public string Status
        {
            get; set;
        }
        // public string Threat { get; set; }
    }
    public class User
    {
        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }

        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
    public class ListedUser
    {
        public string Username
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public string ipAddress
        {
            get; set;
        }

        public ListedUser(string username, string password, string address)
        {
            this.Username = username;
            this.Password = password;
            this.ipAddress = address;
        }
    }
}
