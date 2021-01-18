using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command
{
    /// <summary>
    /// Wraps a collection of items to be used as a compact response.
    /// </summary>
    /// <typeparam name="T">The type of the collection to wrap.</typeparam>
    public class CompactCollection<T> : ICompactResponse
    {
        private readonly Func<T, string> SelectFunction;

        /// <summary>
        /// The items in the collection to be used as a compact response.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Creates an object that contains a collection and can prepare it as a compact response.
        /// </summary>
        /// <param name="items">The collection of items to send in the response.</param>
        /// <param name="select">A lambda to use with the linq select method that converts the object into a string.</param>
        public CompactCollection(IEnumerable<T> items, Func<T, string> select)
        {
            Items = items;
            SelectFunction = select;
        }

        public IEnumerable<string> ToCompact()
        {
            return Items.Select(SelectFunction);
        }
    }
}
