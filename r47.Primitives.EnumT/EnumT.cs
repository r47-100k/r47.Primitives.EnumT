using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace r47.Primitives.EnumT
{
    /// <summary>
    /// A type-safe, extensible alternative to classic C# enums.
    /// Provides:
    /// - A display <see cref="Text"/> string independent of the identifier.
    /// - A sortable <see cref="Index"/> for UI ordering.
    /// - A stable <see cref="Oid"/> to correlate with persisted data (e.g., database).
    /// - Optional visibility control via <see cref="IsVisible"/> and a per-type <see cref="Default"/> entry.
    /// Values are implemented as singletons defined by derived types as static fields.
    /// </summary>
    /// <typeparam name="T">The concrete derived enumeration class.</typeparam>
    public abstract partial class EnumT<T> : IEnumT where T : EnumT<T>
    {
        protected static readonly List<T> Items = new List<T>();
        private static readonly object ItemsLock = new object();
        private static volatile T _default;
        // Track used values and indices to enforce uniqueness and prevent duplicates
        private static readonly HashSet<int> UsedValues = new HashSet<int>();
        private static readonly HashSet<int> UsedIndices = new HashSet<int>();

        /// <summary>
        /// Static constructor: ensures the concrete type <typeparamref name="T"/> is initialized
        /// so its static fields (the enum entries) are constructed deterministically.
        /// </summary>
        static EnumT()
        {
            // Deterministically run T's static constructor without reflection tricks
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        protected EnumT(string name, bool isVisible = true)
            : this(null, name, null, null, isVisible)
        {
        }

        protected EnumT(string name, int? value, bool isVisible = true)
            : this(null, name, value, null, isVisible)
        {
        }


        protected EnumT(string name, int? value, int? index, bool isVisible = true)
            : this(null, name, value, index, isVisible)
        {
        }

        /// <summary>
        /// Used by derived classes to initialize entries.
        /// </summary>
        /// <param name="oid">Optional stable identifier; if not provided, a new GUID is generated.</param>
        /// <param name="text">Display text of the entry.</param>
        /// <param name="value">
        /// Optional numeric value. Must be unique within the derived enum type. If not provided, it is auto-numbered.
        /// </param>
        /// <param name="index">Optional sort key for UI ordering. If not provided, it is auto-numbered.</param>
        /// <param name="isVisible">Whether this entry should be included in visible (UI) lists.</param>
        protected EnumT(Guid? oid, string text, int? value, int? index, bool isVisible=true)
        {
            _text = text;
            // Ensure atomic numbering and registration under concurrency
            lock (ItemsLock)
            {
                // Determine next value/index
                var newValue = value ?? GetNextValue();
                var newIndex = index ?? GetNextIndex();

                // Enforce uniqueness for explicitly provided values/indices
                if (value.HasValue && UsedValues.Contains(value.Value)) throw new InvalidOperationException($"Duplicate enum Value detected for type {typeof(T).Name}: {value.Value}");

                if (index.HasValue && UsedIndices.Contains(index.Value)) throw new InvalidOperationException($"Duplicate enum Index detected for type {typeof(T).Name}: {index.Value}");

                // Protect against accidental collisions for auto-numbering
                while (UsedValues.Contains(newValue))
                {
                    if (newValue == int.MaxValue) throw new InvalidOperationException($"No available Value left for type {typeof(T).Name}");
                    newValue++;
                }
                while (UsedIndices.Contains(newIndex))
                {
                    if (newIndex == int.MaxValue) throw new InvalidOperationException($"No available Index left for type {typeof(T).Name}");
                    newIndex++;
                }

                _value = newValue;
                _index = newIndex;
                _oid = oid ?? Guid.NewGuid();
                _isVisible = isVisible;

                UsedValues.Add(_value);
                UsedIndices.Add(_index);
                Items.Add((T)this);
            }
        }

        /// <summary>
        /// Computes the next available numeric <c>Value</c> for auto-numbering.
        /// Scans existing entries to find the highest value and returns the next free value.
        /// </summary>
        /// <returns>The next available value.</returns>
        private static int GetNextValue()
        {
            int v = int.MinValue;
            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (n._value > v)
                    {
                        v = n._value;
                    }
                }
                // Next candidate greater than current max
                long candidate = (long)v + 1;
                if (candidate > int.MaxValue) throw new InvalidOperationException($"No available Value left for type {typeof(T).Name}");

                int c = (int)candidate;
                while (UsedValues.Contains(c))
                {
                    if (c == int.MaxValue) throw new InvalidOperationException($"No available Value left for type {typeof(T).Name}");
                    c++;
                }
                return c;
            }
        }

        /// <summary>
        /// Computes the next available <see cref="Index"/> for auto-numbering.
        /// Scans existing entries to find the highest index and returns the next free index.
        /// </summary>
        /// <returns>The next available index.</returns>
        private static int GetNextIndex()
        {
            int v = int.MinValue;
            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (n._index > v)
                    {
                        v = n._index;
                    }
                }
                long candidate = (long)v + 1;
                if (candidate > int.MaxValue)
                    throw new InvalidOperationException($"No available Index left for type {typeof(T).Name}");

                int c = (int)candidate;
                while (UsedIndices.Contains(c))
                {
                    if (c == int.MaxValue)
                        throw new InvalidOperationException($"No available Index left for type {typeof(T).Name}");
                    c++;
                }
                return c;
            }
        }

        #region <-- properties -->
        /// <summary>
        /// Human-readable display text of the entry (useful for UI lists).
        /// </summary>
        public string Text
        {
            get { return _text; }
        }
        private readonly string _text;

        /// <summary>
        /// The numeric value of the entry.
        /// </summary>
        public int Value
        {
            get { return _value; }
        }
        private readonly int _value;

        /// <summary>
        /// Sort key used for ordering entries independently from <see cref="Value"/>.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }
        private readonly int _index;

        /// <summary>
        /// Stable unique identifier (OID) of the entry.
        /// </summary>
        public Guid Oid
        {
            get { return _oid; }
        }
        private readonly Guid _oid;

        /// <summary>
        /// Indicates whether this entry should appear in <see cref="EnumT{T}.SortVisibleEntries()"/> results.
        /// Useful to hide technical values used only for calculations.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
        }
        private readonly bool _isVisible;

        /// <summary>
        /// Gets a value indicating whether this entry is the default entry for its type.
        /// </summary>
        public bool IsDefault
        {
            get { return this == Default; }
        }

        /// <summary>
        /// Gets or sets the default entry for this type.
        /// </summary>
        public static T Default
        {
            get { return _default; }
            protected set { _default = value; }
        }
        #endregion

        /// <summary>
        /// Implicitly converts an <see cref="EnumT{T}"/> instance to its numeric <see cref="Value"/>.
        /// </summary>
        /// <param name="m">The enum instance.</param>
        /// <returns>The numeric value.</returns>
        public static implicit operator int(EnumT<T> m) => m._value;

        /// <summary>
        /// Flag-Check: returns true if a and b share any common flag bit. Null operands are treated as 0 (no flags).
        /// </summary>
        public static bool operator &(EnumT<T> a, EnumT<T> b)
        {
            var aNull = ReferenceEquals(a, null);
            var bNull = ReferenceEquals(b, null);
            if (aNull || bNull) return false;

            return (a._value & b._value) != 0;
        }

        /// <summary>
        /// Bitwise OR of two enum values. Null operands are treated as 0 (no flags). Returns the combined integer mask.
        /// </summary>
        public static int operator |(EnumT<T> a, EnumT<T> b)
        {
            var av = ReferenceEquals(a, null) ? 0 : a._value;
            var bv = ReferenceEquals(b, null) ? 0 : b._value;
            return av | bv;
        }

        /// <summary>
        /// Bitwise NOT of the enum value. Null is treated as 0. Returns the complemented integer mask.
        /// </summary>
        public static int operator ~(EnumT<T> a)
        {
            var av = ReferenceEquals(a, null) ? 0 : a._value;
            return ~av;
        }

        /// <summary>
        /// Checks whether this value contains all bits of the specified flag (subset check).
        /// Mirrors the semantics of Enum.HasFlag.
        /// </summary>
        public bool HasFlag(EnumT<T> flag)
        {
            if (ReferenceEquals(flag, null)) return false;
            return (this._value & flag._value) == flag._value;
        }

        /// <summary>
        /// Equality operator comparing two entries by their numeric <see cref="Value"/>.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns><c>true</c> if both are null or both have the same value; otherwise <c>false</c>.</returns>
        public static bool operator ==(EnumT<T> a, EnumT<T> b)
        {
            var aNull = ReferenceEquals(a, null);
            var bNull = ReferenceEquals(b, null);
            if (aNull && bNull) return true;

            if (aNull ^ bNull) return false;

            return a._value == b._value;
        }

        /// <summary>
        /// Inequality operator comparing two entries by their numeric <see cref="Value"/>.
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns><c>true</c> if values differ; otherwise <c>false</c>.</returns>
        public static bool operator !=(EnumT<T> a, EnumT<T> b) => !(a == b);

        /// <summary>
        /// Returns the <see cref="Text"/> of the entry.
        /// </summary>
        public override string ToString() => Text;

        /// <summary>
        /// Determines equality based on the numeric <see cref="Value"/>.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><c>true</c> if values are equal; otherwise <c>false</c>.</returns>
        protected bool Equals(EnumT<T> other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// Two entries are equal if they are of the same runtime type and have the same <see cref="Value"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EnumT<T>) obj);
        }

        /// <summary>
        /// Returns a hash code for the entry based on its numeric <see cref="Value"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => _value;
    }
}