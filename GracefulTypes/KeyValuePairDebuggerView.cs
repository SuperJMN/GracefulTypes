using System.Diagnostics;

namespace GracefulTypes
{
    /// <summary>
    /// Debugger view for hash tree entries
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [DebuggerDisplay("{" + nameof(DebuggerDisplayString) + ",nq}", Name = "{DebuggerNameDisplayString,nq}")]
    internal class KeyValuePairDebuggerView<TKey, TValue>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public KeyValuePairDebuggerView(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Key for entry
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Value for entry
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Debugger display string
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string DebuggerDisplayString => Value?.ToString();

        /// <summary>
        /// Debugger value string
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string DebuggerNameDisplayString => Key?.ToString();
    }
}