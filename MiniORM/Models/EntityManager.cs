namespace MiniORM.Core
{
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    class EntityManager : IDbContext
    {
        // Fields
        private SqlConnection connection;

        private string connectionString;

        private bool isCodeFirst;

        // Constructor
        public EntityManager(string connectionString, bool isCodeFirst)
        {
            this.connectionString = connectionString;
            this.isCodeFirst = isCodeFirst;
        }

        /// <summary>
        /// Entry method to create table if doesent exist,
        /// insert and update entity data in database.
        /// </summary>
        public bool Persist(Object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot Persist NULL entity.");
            }

            if (isCodeFirst && !CheckIfTableExists(entity.GetType()))
            {
                this.CreateTable(entity.GetType());
            }

            Type entityType = entity.GetType();
            FieldInfo idInfo = GetId(entityType);
            int id = (int)idInfo.GetValue(entity);

            if (id <= 0)
            {
                return this.Insert(entity, idInfo);
            }

            return this.Update(entity, idInfo);
        }
        
        private bool Update(Object entity, FieldInfo idInfo)
        {
            int numberOfAffectedRows = 0;
            string updateString = PrepareTableUpdateString(entity, idInfo);

            using (connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand(updateString, this.connection);
                numberOfAffectedRows = command.ExecuteNonQuery();
            }

            return numberOfAffectedRows > 0;
        }
        
        private bool Insert(Object entity, FieldInfo idInfo)
        {
            int numberOfAffectedRows = 0;
            string insertIntoString = PrepareTableInsertionString(entity);

            using (connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand(insertIntoString, this.connection);
                numberOfAffectedRows = command.ExecuteNonQuery();

                // Check and set Id
                string selectMaxIdString = $"SELECT MAX([Id]) " +
                                           $"FROM {this.GetTableName(entity.GetType())} ";
                command = new SqlCommand(selectMaxIdString, connection);
                int id = (int)command.ExecuteScalar();
                idInfo.SetValue(entity, id);
            }

            return numberOfAffectedRows > 0;
        }

        private bool CheckIfTableExists(Type type)
        {
            int numberOfTables;

            string query = $"SELECT COUNT(Name) " +
                           $"FROM sys.sysobjects " +
                           $"WHERE [Name] = '{this.GetTableName(type)}' " +
                           $"AND [xtype] = 'U'; ";

            using (connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand(query, this.connection);
                numberOfTables = (int)command.ExecuteScalar();
            }

            if (numberOfTables > 0)
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Get the field with Id attribute of the given entity. 
        /// If there is no field with Id attribute throw exception.
        /// </summary>
        private FieldInfo GetId(Type entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot get Id for NULL type.");
            }

            FieldInfo id = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(x => x.IsDefined(typeof(IdAttribute)));

            if (id == null)
            {
                throw new ArgumentNullException("No Id field was found in the current class.");
            }

            return id;
        }

        /// <summary>
        /// Returns the value of the TableName property of Entity attribute 
        /// or if it’s not set returns the name of the entity.
        /// </summary>
        private string GetTableName(Type entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot get TableName for NULL value.");
            }

            if (!entity.IsDefined(typeof(EntityAttribute)))
            {
                throw new ArgumentException("Cannot get TableName.");
            }

            string tableName = entity.GetCustomAttribute<EntityAttribute>().TableName;

            if (tableName == null)
            {
                throw new ArgumentNullException("TableName cannot be NULL.");
            }

            return tableName;
        }

        /// <summary>
        /// Returns the value of the Name property of Column attribute 
        /// or if it’s not set returns the name of the field.
        /// </summary>
        private string GetFieldName(FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Cannot get Field for NULL type.");
            }

            if (!field.IsDefined(typeof(ColumnAttribute)))
            {
                return field.Name;
            }

            string fieldName = field.GetCustomAttribute<ColumnAttribute>().Name;

            if (fieldName == null)
            {
                throw new ArgumentNullException("FieldName cannot be NULL.");
            }

            return fieldName;
        }

        private void CreateTable(Type entity)
        {
            string creationString = PrepareTableCreationString(entity);

            using (connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand(creationString, connection);
                command.ExecuteNonQuery();
            }
        }

        private string PrepareTableCreationString(Type entity)
        {
            StringBuilder createSQLTable = new StringBuilder();
            createSQLTable.Append($"CREATE TABLE {this.GetTableName(entity)} (");
            createSQLTable.Append($"Id INT IDENTITY(1, 1) PRIMARY KEY, ");

            FieldInfo[] columnInfos = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(ColumnAttribute))).ToArray();

            foreach (FieldInfo columnInfo in columnInfos)
            {
                createSQLTable.Append($"{this.GetFieldName(columnInfo)} {this.GetTypeToDB(columnInfo)}, ");
            }

            createSQLTable.Remove(createSQLTable.Length - 2, 2);
            createSQLTable.Append(" )");

            return createSQLTable.ToString();
        }

        private string PrepareTableUpdateString(Object entity, FieldInfo idInfo)
        {
            StringBuilder updateSQLTable = new StringBuilder();
            StringBuilder columnNameValueBuilder = new StringBuilder();

            FieldInfo[] columnNames = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(ColumnAttribute))).ToArray();

            foreach (FieldInfo columnName in columnNames)
            {
                if (this.GetTypeToDB(columnName) != "DATETIME")
                {
                    columnNameValueBuilder.Append($"[{this.GetFieldName(columnName)}] = '{columnName.GetValue(entity)}', ");
                }
                else // If the Type IS DATETIME add to string by converting
                {
                    DateTime datetime = (DateTime)columnName.GetValue(entity);
                    columnNameValueBuilder.Append($"[{this.GetFieldName(columnName)}] = '{datetime.ToString("yyyy-MM-dd HH:mm:ss")}', ");
                }
            }

            updateSQLTable.Append($"UPDATE {this.GetTableName(entity.GetType())} SET ");
            columnNameValueBuilder.Remove(columnNameValueBuilder.Length - 2, 2);
            updateSQLTable.Append(columnNameValueBuilder.ToString());
            updateSQLTable.Append($" WHERE [Id] = '{idInfo.GetValue(entity)}' ;");

            return updateSQLTable.ToString();
        }

        private string PrepareTableInsertionString(Object entity)
        {
            StringBuilder insertIntoSQLTable = new StringBuilder();
            StringBuilder columnNamesBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();

            FieldInfo[] columnNames = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.IsDefined(typeof(ColumnAttribute))).ToArray();

            foreach (FieldInfo columnName in columnNames)
            {
                columnNamesBuilder.Append($"{this.GetFieldName(columnName)}, ");
                if (this.GetTypeToDB(columnName) != "DATETIME")
                {
                    valuesBuilder.Append($"'{columnName.GetValue(entity).ToString()}', ");
                }
                else // If the Type IS DATETIME add to string by converting
                {
                    DateTime datetime = (DateTime)columnName.GetValue(entity);
                    valuesBuilder.Append($"'{datetime.ToString("yyyy-MM-dd HH:mm:ss")}', ");
                }
            }

            columnNamesBuilder.Remove(columnNamesBuilder.Length - 2, 2);
            columnNamesBuilder.Append(" ) ");
            valuesBuilder.Remove(valuesBuilder.Length - 2, 2);
            valuesBuilder.Append(" )");

            insertIntoSQLTable.Append($"INSERT INTO {this.GetTableName(entity.GetType())} ( ");
            insertIntoSQLTable.Append(columnNamesBuilder.ToString());
            insertIntoSQLTable.Append($"VALUES ( ");
            insertIntoSQLTable.Append(valuesBuilder.ToString());

            return insertIntoSQLTable.ToString();
        }

        private string GetTypeToDB(FieldInfo field)
        {
            switch (field.FieldType.Name)
            {
                case "Int32": return "INT";
                case "String": return "VARCHAR(max)";
                case "DateTime": return "DATETIME";
                case "Boolean": return "BIT";
                default: throw new ArgumentException("No present field type.");
            }
        }

        // IDbContext Methods
        public void Delete<T>(object entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteById<T>(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> FindAll<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> FindAll<T>(string where)
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(int id)
        {
            throw new NotImplementedException();
        }

        public T FindFirst<T>()
        {
            throw new NotImplementedException();
        }

        public T FindFirst<T>(string where)
        {
            throw new NotImplementedException();
        }
    }
}