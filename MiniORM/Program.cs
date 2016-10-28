namespace MiniORM
{
    using Core;
    using Entities;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            User dani = new User("Dani", "Password", 22, DateTime.Now);
            ConnectionStringBuilder builder = new ConnectionStringBuilder("MiniORM");
            EntityManager manager = new EntityManager(builder.ConnectionString, true);
            manager.Persist(dani);

            dani.Username = "Daniel";
            manager.Persist(dani);
        }
    }
}
