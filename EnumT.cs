using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MAN.Simulutions.CommonCore.EnumTCore.Contract;

namespace MAN.Simulutions.CommonCore.EnumTCore.Impl
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
    public abstract class EnumT<T> : IEnumT where T : EnumT<T>
    {
        #region <-- types -->
        protected static readonly List<T> Items = new List<T>();
        private static T _default;
        #endregion

        #region <-- construction/destruction -->

        /// <summary>
        /// das ist ein statischer ctor() der VOR irgendeinem anderen ctor() im program aufgerufen wird. 
        /// zu diesem zeitpunkt kann es aber sein, dass die klasse T noch nicht konstruiert ist. 
        /// das führt zu einer exception!
        /// 
        /// daher mussman die klasse t selbst initialisieren. der aufruf eines beliebigen statischen feldes erzwingt die 
        /// initialisierung
        /// </summary>
        static EnumT()
        {
            if (Items.Count == 0)
            {
                //wenn keine statischen felder definiert sind -> exception, ansonsten ...
                FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
                if (fields.Length == 0)
                {
                    throw new Exception("No Public Static Field defined in Enum class");
                }

                //...rufe irgendein statische feld auf
                fields[0].GetValue(null);
            }
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
            _value = value ?? GetNextValue();
            _index = index ?? GetNextIndex();
            _oid = oid ?? Guid.NewGuid();
            _isVisible = isVisible;

            // cast into T does not crash if T is also the type of the concrete generic class :
            // OK     -> public class C : ToEnumBase<C>
            // NOT OK -> public class D : ToEnumBase<C>
            Items.Add((T)this);
        }

        /// <summary>
        /// is used to change text/value during runtime.
        /// Note:
        /// this is dangerous! because clients may rely on a specific value. so use this only for initialization stuff
        /// </summary>
        /// <param name="newText">new value for the text property to be set</param>
        /// <param name="newValue">new value for the value property to be set. if it is null the org value will remain unchanged</param>
        public virtual void Initialize(string newText, int? newValue)
        {
            if (string.IsNullOrEmpty(newText) == false)
            {
                _text = newText;
            }

            if (newValue.HasValue)
            {
                _value = newValue.Value;
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
            foreach (var n in Items)
            {
                if (n._value > v)
                {
                    v = n._value;
                }
            }
            return v + 1;
        }

        /// <summary>
        /// funktionalität zur autonummerierung de indexes.
        /// durchsucht alle werte nach dem größtem index und liefert den nächsten "freien" index = highestValue +1
        /// </summary>
        /// <returns></returns>
        private static int GetNextIndex()
        {
            int v = int.MinValue;
            foreach (var n in Items)
            {
                if (n._index > v)
                {
                    v = n._index;
                }
            }
            return v + 1;
        }

        #endregion


        #region <-- properties -->
        /// <summary>
        /// der Text des enums kann zB direkt zum Befüllen von Listboxen verwendet werden
        /// </summary>
        public string Text
        {
            get { return _text; }
        }
        private string _text;

        /// <summary>
        /// der eigentliche enum-wert
        /// </summary>
        public int Value
        {
            get { return _value; }
        }
        private int _value;

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

        #region <-- methods -->
        /// <summary>
        /// liefert eine geclonte Liste aller Einträge des enums
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IEnumT> ClonedEntries()
        {
            return Items.Select(n => new EnumTEntry(n.Text, n.Value, n.Index, n.Oid, n.IsVisible)).Cast<IEnumT>().ToList();
        }

        /// <summary>
        /// sortiert die enum liste nach dem sortierschlüssel = index. Alle Elemente werden aufgenommen
        /// </summary>
        /// <returns>sortierte enum liste</returns>
        public static IEnumerable<T> SortEntries()
        {
            var retval = new List<T>(Items);
            retval.Sort((x, y) => x.Index.CompareTo(y.Index));
            return retval;
        }

        /// <summary>
        /// sortiert die enum liste nach dem sortierschlüssel = index. Elemente mit isVisible=false werden nicht mit aufgenommen
        /// </summary>
        /// <returns>sortierte enum liste</returns>
        public static IEnumerable<T> SortVisibleEntries()
        {
            var retval = Items.Where(n => n.IsVisible).ToList();
            retval.Sort((x, y) => x.Index.CompareTo(y.Index));
            return retval;
        }

        /// <summary>
        /// sucht einen eintrag mittels oid
        /// </summary>
        /// <param text="oid"></param>
        /// <returns></returns>
        public static T Find(Guid oid)
        {
            foreach (T n in Items)
            {
                if (n._oid == oid)
                    return n;
            }

            throw new ArgumentException(String.Format("{0} is not a member of this enum", oid));
        }

        #endregion

        #region <-- operators -->
        //int conversion
        public static implicit operator int(EnumT<T> m)
        {
            return m._value;
        }

        public static bool operator &(EnumT<T> a, EnumT<T> b)
        {
            bool aNull = ReferenceEquals(a, null);
            bool bNull = ReferenceEquals(b, null);
            if (aNull && bNull)
                return true;

            if (aNull ^ bNull)
                return false;

            return (a._value & b._value) != 0;
        }

        // overload operator ==
        public static bool operator ==(EnumT<T> a, EnumT<T> b)
        {
            bool aNull = ReferenceEquals(a, null);
            bool bNull = ReferenceEquals(b, null);
            if (aNull && bNull)
                return true;

            if (aNull ^ bNull)
                return false;

            return a._value == b._value;
        }

        // overload operator !=
        public static bool operator !=(EnumT<T> a, EnumT<T> b)
        {
            return !(a == b);
        }
        #endregion

        #region <-- ToString() -->
        public override string ToString()
        {
            return Text;
        }
        #endregion

        #region <-- Equals() -->
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

        public override int GetHashCode()
        {
            return _value;
        }
        #endregion
    }
}