namespace MiniORM.Core
{
    using System.Data.SqlClient;

    class ConnectionStringBuilder
    {
        // Fields
        private SqlConnectionStringBuilder builder;

        private string connectionString;

        // Constructor
        public ConnectionStringBuilder(string databaseName)
        {
            this.builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = "(local)";
            builder["Integrated Security"] = true;
            builder["Connect Timeout"] = 1000;
            builder["Trusted_Connection"] = true;
            builder["Initial Catalog"] = databaseName;
            this.connectionString = builder.ToString();
        }

        // Properties
        public string ConnectionString { get { return this.connectionString; } }
    }
}