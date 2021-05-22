using LobotJR.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LobotJR.Test.Mocks
{
    /// <summary>
    /// To prevent having to constantly mock out respository functions, this
    /// class implements the IRepository interface with a List providing the
    /// backing data.
    /// </summary>
    /// <typeparam name="T">The type of the repository.</typeparam>
    public class ListRepository<T> : IRepository<T>
    {
        private readonly PropertyInfo idProperty;

        /// <summary>
        /// The collection holding the data to use for the test.
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// Creates an IRepository that is backed by an exposed list.
        /// </summary>
        /// <param name="data">The list that holds the repository data.</param>
        /// <param name="idPropertyName">The id property, which must be an int.</param>
        public ListRepository(List<T> data, string idPropertyName = "Id")
        {
            Data = data;
            idProperty = typeof(T).GetProperties().Where(x => x.PropertyType == typeof(int) && x.Name.Equals(idPropertyName)).FirstOrDefault();
        }

        public void Commit()
        {
        }

        public T Create(T entry)
        {
            Data.Add(entry);
            return entry;
        }

        public T Delete(T entry)
        {
            var entryId = (int)idProperty.GetValue(entry);
            return DeleteById(entryId);
        }

        public T DeleteById(int id)
        {
            var toDelete = ReadById(id);
            Data.Remove(toDelete);
            return toDelete;
        }

        public IEnumerable<T> Read()
        {
            return Data;
        }

        public IEnumerable<T> Read(Func<T, bool> filter)
        {
            return Data.Where(filter);
        }

        public T Read(T entry)
        {
            return entry;
        }

        public T ReadById(int id)
        {
            return Data.Where(x => (int)idProperty.GetValue(x) == id).FirstOrDefault();
        }

        public T Update(T entry)
        {
            if (Data.Contains(entry))
            {
                Data.Remove(entry);
                Data.Add(entry);
                return entry;
            }
            return default(T);
        }
    }
}
