using System;

namespace r47.Primitives.EnumT
{
    /// <summary>
    /// Public contract for an <c>EnumT</c> entry.
    /// </summary>
    public interface IEnumEntry
    {
        /// <summary>
        /// Human-readable display text of the entry.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Sort key used for UI ordering, independent of the numeric value.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Stable unique identifier of the entry.
        /// </summary>
        Guid Oid { get; }

        /// <summary>
        /// Indicates whether the entry should appear in lists of visible entries.
        /// </summary>
        bool IsVisible { get; }
    }
}