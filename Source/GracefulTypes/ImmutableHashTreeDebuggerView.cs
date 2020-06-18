using System;
using System.Collections.Generic;
using System.Linq;

namespace GracefulTypes
{
    /// <summary>
    /// Class for debugger
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class ImmutableHashTreeDebuggerView<TKey, TValue>
    {
        private readonly ImmutableHashTree<TKey, TValue> hashTree;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="hashTree"></param>
        public ImmutableHashTreeDebuggerView(ImmutableHashTree<TKey, TValue> hashTree)
        {
            this.hashTree = hashTree;
        }

        /// <summary>
        /// Keys for hash tree
        /// </summary>
        public IEnumerable<TKey> Keys => hashTree.Keys.ToList();

        /// <summary>
        /// Values
        /// </summary>
        public IEnumerable<TValue> Values => hashTree.Values.ToList();

        /// <summary>
        /// Items
        /// </summary>
        public IEnumerable<KeyValuePairDebuggerView<TKey, TValue>> Items
        {
            get
            {
                var list =
                    hashTree.Select(kvp => new KeyValuePairDebuggerView<TKey, TValue>(kvp.Key, kvp.Value)).ToList();

                list.Sort((x, y) => string.Compare(x.Key?.ToString(), y.Key?.ToString(), StringComparison.CurrentCultureIgnoreCase));

                return list;
            }
        }
    }
}
