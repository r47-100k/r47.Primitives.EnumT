using System;
using System.Collections.Generic;
using System.Linq;

namespace r47.Primitives.EnumT
{
    public abstract partial class EnumT<T>
    {
        /// <summary>
        /// Provides a read-only snapshot of all registered entries.
        /// The returned collection is a copy created under lock and cannot be modified by the caller.
        /// Order corresponds to registration order.
        /// </summary>
        public static IReadOnlyList<T> Entries()
        {
            lock (ItemsLock)
            {
                return new List<T>(Items).AsReadOnly();
            }
        }

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
                result = Items.FirstOrDefault(n => n._oid == oid);
                return result != null;
            }
        }

        /// <summary>
        /// Liefert den Eintrag mit dem angegebenen numerischen Wert. Wirft eine Ausnahme, wenn nicht gefunden.
        /// </summary>
        /// <param name="value">Der numerische <c>Value</c> des gesuchten Eintrags.</param>
        /// <returns>Der gefundene Eintrag.</returns>
        /// <exception cref="KeyNotFoundException">Wenn kein Eintrag mit diesem Wert existiert.</exception>
        public static T FromValue(int value)
        {
            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (n._value == value)
                        return n;
                }
            }
            throw new KeyNotFoundException($"No {typeof(T).Name} value with Value={value} was found.");
        }

        /// <summary>
        /// Versucht, einen Eintrag anhand seines numerischen Wertes zu finden.
        /// </summary>
        /// <param name="value">Der numerische <c>Value</c>.</param>
        /// <param name="result">Der gefundene Eintrag oder <c>null</c>.</param>
        /// <returns><c>true</c>, wenn ein Eintrag gefunden wurde; andernfalls <c>false</c>.</returns>
        public static bool TryFromValue(int value, out T result)
        {
            lock (ItemsLock)
            {
                result = Items.FirstOrDefault(n => n._value == value);
                return result != null;
            }
        }

        /// <summary>
        /// Liefert den Eintrag mit dem angegebenen Text.
        /// Standardmäßig wird <see cref="StringComparison.Ordinal"/> verwendet.
        /// </summary>
        /// <param name="text">Der anzuzeigende Text des Eintrags.</param>
        /// <param name="comparison">Die String-Vergleichsoption.</param>
        /// <returns>Der gefundene Eintrag.</returns>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="text"/> <c>null</c> ist.</exception>
        /// <exception cref="ArgumentException">Wenn <paramref name="text"/> leer oder nur aus Leerzeichen besteht.</exception>
        /// <exception cref="KeyNotFoundException">Wenn kein Eintrag mit diesem Text existiert.</exception>
        public static T FromText(string text, StringComparison comparison = StringComparison.Ordinal)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text must not be empty or whitespace.", nameof(text));

            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (string.Equals(n._text, text, comparison))
                        return n;
                }
            }
            throw new KeyNotFoundException($"No {typeof(T).Name} value with Text='{text}' was found (comparison: {comparison}).");
        }

        /// <summary>
        /// Versucht, einen Eintrag anhand seines Textes zu finden. Standardmäßig wird <see cref="StringComparison.OrdinalIgnoreCase"/> verwendet.
        /// </summary>
        /// <param name="text">Der Text.</param>
        /// <param name="result">Der gefundene Eintrag oder <c>null</c>.</param>
        /// <returns><c>true</c>, wenn ein Eintrag gefunden wurde; andernfalls <c>false</c>.</returns>
        public static bool TryFromText(string text, out T result)
            => TryFromText(text, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Versucht, einen Eintrag anhand seines Textes zu finden, mit frei wählbarer Vergleichsoption.
        /// </summary>
        /// <param name="text">Der Text.</param>
        /// <param name="comparison">String-Vergleichsoption.</param>
        /// <param name="result">Der gefundene Eintrag oder <c>null</c>.</param>
        /// <returns><c>true</c>, wenn ein Eintrag gefunden wurde; andernfalls <c>false</c>.</returns>
        public static bool TryFromText(string text, StringComparison comparison, out T result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(text)) return false;

            lock (ItemsLock)
            {
                foreach (var n in Items)
                {
                    if (string.Equals(n._text, text, comparison))
                    {
                        result = n;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Versucht, einen Eintrag aus einer String-Repräsentation zu lesen.
        /// Reihenfolge: GUID (OID) -> int (Value) -> Text (case-insensitive standard).
        /// </summary>
        /// <param name="input">Die Eingabe.</param>
        /// <param name="result">Der gefundene Eintrag oder <c>null</c>.</param>
        /// <returns><c>true</c>, wenn das Parsen erfolgreich war; andernfalls <c>false</c>.</returns>
        public static bool TryParse(string input, out T result)
            => TryParse(input, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Versucht, einen Eintrag aus einer String-Repräsentation zu lesen.
        /// Reihenfolge: GUID (OID) -> int (Value) -> Text (mit angegebener Vergleichsoption).
        /// </summary>
        /// <param name="input">Die Eingabe.</param>
        /// <param name="comparison">String-Vergleichsoption für Textvergleich.</param>
        /// <param name="result">Der gefundene Eintrag oder <c>null</c>.</param>
        /// <returns><c>true</c>, wenn das Parsen erfolgreich war; andernfalls <c>false</c>.</returns>
        public static bool TryParse(string input, StringComparison comparison, out T result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // GUID (OID)
            if (Guid.TryParse(input, out var oid))
            {
                if (TryFind(oid, out result)) return true;
            }

            // Integer Value
            if (int.TryParse(input, out var intVal))
            {
                if (TryFromValue(intVal, out result)) return true;
            }

            // Text
            return TryFromText(input, comparison, out result);
        }
    }
}