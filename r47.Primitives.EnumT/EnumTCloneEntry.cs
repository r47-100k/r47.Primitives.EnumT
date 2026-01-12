using System;

namespace r47.Primitives.EnumT
{
    /// <summary>
    /// wird nur verwendet um einträge aus dem enum clonen zu können
    /// </summary>
    public class EnumTCloneEntry : IEnumT
    {
        public EnumTCloneEntry(string name, int value, int index, Guid oid, bool isVisible)
        {
            _name = name;
            _value = value;
            _index = index;
            _oid = oid;
            _isVisible = isVisible;
        }

        public string Text => _name;
        private readonly string _name;

        public int Value => _value;
        private readonly int _value;

        public int Index => _index;
        private readonly int _index;

        public Guid Oid => _oid;
        private readonly Guid _oid;

        public bool IsVisible => _isVisible;
        private readonly bool _isVisible;
    }
}