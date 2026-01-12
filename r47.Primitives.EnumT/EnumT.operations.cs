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
        /// Returns a cloned list of all entries as plain data objects.
        /// Useful when you need a detached, serializable view.
        /// </summary>
        /// <returns>A list of cloned entries as <see cref="IEnumT"/>.</returns>
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
        /// Returns all entries sorted by <see cref="EnumT{T}.Index"/>.
        /// </summary>
        /// <returns>Sorted entries.</returns>
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
        /// Returns only visible entries sorted by <see cref="EnumT{T}.Index"/>.
        /// </summary>
        /// <returns>Sorted visible entries.</returns>
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
        /// Tries to find an entry by its <see cref="EnumT{T}.Oid"/> without throwing exceptions.
        /// </summary>
        /// <param name="oid">The OID to look up.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if an entry with the given OID exists; otherwise <c>false</c>.</returns>
        public static bool TryFind(Guid oid, out T result)
        {
            lock (ItemsLock)
            {
                result = Items.FirstOrDefault(n => n._oid == oid);
                return result != null;
            }
        }

        /// <summary>
        /// Returns the entry with the specified numeric <c>Value</c> or throws if missing.
        /// </summary>
        /// <param name="value">The numeric value to look up.</param>
        /// <returns>The found entry.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no entry with the given value exists.</exception>
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
        /// Tries to find an entry by its numeric <c>Value</c>.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if an entry is found; otherwise <c>false</c>.</returns>
        public static bool TryFromValue(int value, out T result)
        {
            lock (ItemsLock)
            {
                result = Items.FirstOrDefault(n => n._value == value);
                return result != null;
            }
        }

        /// <summary>
        /// Returns the entry with the specified <see cref="EnumT{T}.Text"/>.
        /// Uses <see cref="StringComparison.Ordinal"/> by default.
        /// </summary>
        /// <param name="text">The display text to match.</param>
        /// <param name="comparison">The string comparison option.</param>
        /// <returns>The found entry.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="text"/> is empty or whitespace.</exception>
        /// <exception cref="KeyNotFoundException">If no entry with this text exists.</exception>
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
        /// Tries to find an entry by its <see cref="EnumT{T}.Text"/>.
        /// Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="text">The display text.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if an entry is found; otherwise <c>false</c>.</returns>
        public static bool TryFromText(string text, out T result)
            => TryFromText(text, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Tries to find an entry by its <see cref="EnumT{T}.Text"/> using the specified comparison.
        /// </summary>
        /// <param name="text">The display text.</param>
        /// <param name="comparison">The string comparison option.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if an entry is found; otherwise <c>false</c>.</returns>
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
        /// Tries to parse an entry from a string.
        /// Order: GUID (OID) → int (Value) → Text (case-insensitive default).
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
        public static bool TryParse(string input, out T result)
            => TryParse(input, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Tries to parse an entry from a string using the specified text comparison.
        /// Order: GUID (OID) → int (Value) → Text.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="comparison">The string comparison for the text step.</param>
        /// <param name="result">The found entry or <c>null</c>.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
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