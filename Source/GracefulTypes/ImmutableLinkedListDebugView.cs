using System.Diagnostics;
using System.Linq;

namespace GracefulTypes
{
    /// <summary>
    /// Class for debugger view
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ImmutableLinkedListDebugView<T>
    {
        private readonly ImmutableLinkedList<T> immutableLinkedList;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="immutableLinkedList"></param>
        public ImmutableLinkedListDebugView(ImmutableLinkedList<T> immutableLinkedList)
        {
            this.immutableLinkedList = immutableLinkedList;
        }

        /// <summary>
        /// Items in list
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => immutableLinkedList.ToArray();
    }
}
