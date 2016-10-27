namespace MiniORM
{
    using Core;
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            User dani = new User("Dani", "Pass", 22, DateTime.Now);
            ConnectionStringBuilder builder = new ConnectionStringBuilder("MiniORM");
            EntityManager manager = new EntityManager(builder.ConnectionString, true);
            manager.Persist(dani);
        }
    }
}
