using System;
using System.Collections.Generic;
using System.Linq;

namespace r47.Primitives.EnumT
{
    public abstract partial class EnumT<T>
    {
        /// <summary>
        /// liefert eine geclonte Liste aller Einträge des enums
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IEnumT> ClonedEntries()
        {
            return Items.Select(n => new EnumTCloneEntry(n.Text, n.Value, n.Index, n.Oid, n.IsVisible)).Cast<IEnumT>().ToList();
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
            foreach (var n in Items)
            {
                if (n._oid == oid) return n;
            }

            throw new ArgumentException($"{oid} is not a member of this enum");
        }
    }
}