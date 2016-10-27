namespace MiniORM.Core
{
    using System;
    using System.Collections.Generic;

    interface IDbContext
    {
        // Method declarations

        /// <summary>
        /// It will insert or update entity depending if it is attached to the context.
        /// </summary>
        bool Persist(Object entity);

        /// <summary>
        /// Return entity object of type T by given id.
        /// </summary>
        T FindById<T>(int id);

        /// <summary>
        /// Returns collection of all entity objects of type T.
        /// </summary>
        IEnumerable<T> FindAll<T>();

        /// <summary>
        /// Returns collection of all entity objects of type T.
        /// </summary>
        IEnumerable<T> FindAll<T>(string where);

        /// <summary>
        /// Returns the first entity object of type T.
        /// </summary>
        T FindFirst<T>();

        /// <summary>
        /// Returns the first entity object of type T matching the criteria given in "where".
        /// </summary>
        T FindFirst<T>(string where);

        /// <summary>
        /// Delete given object from database.
        /// </summary>
        void Delete<T>(Object entity);

        /// <summary>
        /// Delete object of type T with given Id from database.
        /// </summary>
        void DeleteById<T>(int id);
    }
}