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
            List<T> snapshot;
            lock (ItemsLock)
            {
                snapshot = new List<T>(Items);
            }
            return snapshot
                .Select(n => new EnumTCloneEntry(n.Text, n.Value, n.Index, n.Oid, n.IsVisible))
                .Cast<IEnumT>()
                .ToList();
        }

        /// <summary>
        /// sortiert die enum liste nach dem sortierschlüssel = index. Alle Elemente werden aufgenommen
        /// </summary>
        /// <returns>sortierte enum liste</returns>
        public static IEnumerable<T> SortEntries()
        {
            List<T> retval;
            lock (ItemsLock)
            {
                retval = new List<T>(Items);
            }
            retval.Sort((x, y) => x.Index.CompareTo(y.Index));
            return retval;
        }

        /// <summary>
        /// sortiert die enum liste nach dem sortierschlüssel = index. Elemente mit isVisible=false werden nicht mit aufgenommen
        /// </summary>
        /// <returns>sortierte enum liste</returns>
        public static IEnumerable<T> SortVisibleEntries()
        {
            List<T> retval;
            lock (ItemsLock)
            {
                retval = Items.Where(n => n.IsVisible).ToList();
            }
            retval.Sort((x, y) => x.Index.CompareTo(y.Index));
            return retval;
        }

        /// <summary>
        /// Versucht, einen Eintrag mittels OID zu finden, ohne eine Ausnahme zu werfen.
        /// </summary>
        /// <param name="oid">Die OID des gesuchten Eintrags.</param>
        /// <param name="result">Gibt bei Erfolg den gefundenen Eintrag zurück, andernfalls <c>null</c>.</param>
        /// <returns><c>true</c>, wenn ein Eintrag mit der OID gefunden wurde; andernfalls <c>false</c>.</returns>
        public static bool TryFind(Guid oid, out T result)
        {
            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (n._oid == oid)
                    {
                        result = n;
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }
    }
}