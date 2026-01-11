using System;

namespace MAN.Simulutions.CommonCore.EnumTCore.Contract
{
    public interface IEnumT
    {
        string Text { get; }

        int Index { get; }
        Guid Oid { get; }
        bool IsVisible { get; }
    }
}