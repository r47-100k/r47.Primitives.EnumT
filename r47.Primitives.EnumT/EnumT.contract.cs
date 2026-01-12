using System;

namespace r47.Primitives.EnumT
{
    public interface IEnumT
    {
        string Text { get; }

        int Index { get; }
        Guid Oid { get; }
        bool IsVisible { get; }
    }
}