namespace MiniORM.Entities
{
    using MiniORM.Attributes;
    using System;

    [Entity(TableName = "Users")]
    class User
    {
        // Fields
        [Id]
        private int id;

        [Column(Name = "Username")]
        private string username;

        [Column(Name = "Password")]
        private string password;

        [Column(Name = "Age")]
        private int age;

        [Column(Name = "RegistrationDate")]
        private DateTime registrationDate;

        // Constructor
        public User(string username, string password, int age, DateTime registrationDate)
        {
            this.Username = username;
            this.Password = password;
            this.Age = age;
            this.RegistrationDate = registrationDate;
        }

        // Properties
        public string Username { get; set; }

        public string Password { get; set; }

        public int Age { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}