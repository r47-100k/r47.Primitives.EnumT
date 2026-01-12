using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace r47.Primitives.EnumT
{
    /// <summary>
    /// erweitert den klassischen enum um ein paar feinheiten
    /// 1.  enumBaseT ist sortierbar
    ///     dies ist nützlich, um zB die werte des enums unabhängig von ihrem enum wert oder 
    ///     dem namen in einer listbox anzuzeigen
    /// 2.  enumBaseT hat einen namen der als string definiert ist
    ///     damit ist man frei in der verwendung von eigenen namen ohne c# konventionen
    /// 3.  jeder enumBaseT wert hat eine oid
    ///     das ermöglicht die zuordnung von persistierten Werten zB aus einer DB
    /// </summary>
    /// <typeparam text="T"></typeparam>
    public abstract partial class EnumT<T> : IEnumT where T : EnumT<T>
    {
        protected static readonly List<T> Items = new List<T>();
        private static readonly object ItemsLock = new object();
        private static volatile T _default;
        // Track used values and indices to enforce uniqueness and prevent duplicates
        private static readonly HashSet<int> UsedValues = new HashSet<int>();
        private static readonly HashSet<int> UsedIndices = new HashSet<int>();

        /// <summary>
        /// Statischer Konstruktor: stellt sicher, dass der konkrete Typ T initialisiert wird,
        /// damit dessen statische Felder (die Enum-Einträge) konstruiert werden.
        /// </summary>
        static EnumT()
        {
            // Erzwingt deterministisch die Ausführung des Type Initializers von T, ohne Reflection-Tricks
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
        /// wird von abgeleiteten klassen verwendet um einträge zu initialisieren
        /// </summary>
        /// <param text="oid">oid, die zB aus einer datenbank kommt</param>
        /// <param text="text">anzeige text</param>
        /// <param text="value">
        /// eigentlicher enum wert. es gelten die gleiche regeln wie bei einem normalen enum. 
        /// also jeder value muss innerhalb des enums eindeutig sein
        /// </param>
        /// <param text="index">sortierschlüssel</param>
        /// <param text="isVisible">bestimmt die sichtbarkeit eines Eintrages in einer sortierten Liste</param>
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
        /// funktionalität zur autonummerierung des wertes.
        /// durchsucht alle werte nach dem größtem value und liefert den nächsten "freien" value = highestValue +1
        /// 
        /// VORSICHT:
        /// wird bei der konkreten klasse später der wert highestValue +1 gesetzt, kommt der value 2 * im enum vor
        /// </summary>
        /// <returns></returns>
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
        /// funktionalität zur autonummerierung de indexes.
        /// durchsucht alle werte nach dem größtem index und liefert den nächsten "freien" index = highestValue +1
        /// </summary>
        /// <returns></returns>
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
        /// der Text des enums kann zB direkt zum Befüllen von Listboxen verwendet werden
        /// </summary>
        public string Text
        {
            get { return _text; }
        }
        private readonly string _text;

        /// <summary>
        /// der eigentliche enum-wert
        /// </summary>
        public int Value
        {
            get { return _value; }
        }
        private readonly int _value;

        /// <summary>
        /// index des eintrags innerhalb der liste (nützlich zB zum sortieren von einträgen in listboxen unabhängig vom value)
        /// </summary>
        public int Index
        {
            get { return _index; }
        }
        private readonly int _index;

        /// <summary>
        /// eindeutige id des eintrags
        /// </summary>
        public Guid Oid
        {
            get { return _oid; }
        }
        private readonly Guid _oid;

        /// <summary>
        /// standard ist isVisible = true. damit wird der Wert bei einer Sortierung (SortVisibleEntries()) mit ausgegeben.
        /// einen Wert nicht mit auszugeben ist zb sinnvoll, um einen wert der nur zu Berechnungen deklariert wird (zB veroderte werte)
        /// von der Befüllung einer Listbox auszuschließen.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
        }
        private readonly bool _isVisible;

        /// <summary>
        /// liefert true, wenn dieser enum als default deklariert ist
        /// </summary>
        public bool IsDefault
        {
            get { return this == Default; }
        }

        /// <summary>
        /// liefert den default enum
        /// </summary>
        public static T Default
        {
            get { return _default; }
            protected set { _default = value; }
        }
        #endregion

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

        public static bool operator ==(EnumT<T> a, EnumT<T> b)
        {
            var aNull = ReferenceEquals(a, null);
            var bNull = ReferenceEquals(b, null);
            if (aNull && bNull) return true;

            if (aNull ^ bNull) return false;

            return a._value == b._value;
        }

        public static bool operator !=(EnumT<T> a, EnumT<T> b) => !(a == b);

        public override string ToString() => Text;

        /// <summary>
        /// der EnumT gilt dann als gleich, wenn der value gleich ist. das ermöglicht den einfachen Vergleich mit einem int
        /// </summary>
        /// <param text="other"></param>
        /// <returns></returns>
        protected bool Equals(EnumT<T> other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EnumT<T>) obj);
        }

        public override int GetHashCode() => _value;
    }
}