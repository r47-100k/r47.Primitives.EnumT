using System;
using MAN.Simulutions.CommonCore.EnumTCore.Contract;

namespace MAN.Simulutions.CommonCore.EnumTCore.Impl
{
    /// <summary>
    /// wird nur verwendet um einträge aus dem enum clonen zu können
    /// </summary>
    public class EnumTEntry : IEnumT
    {
        #region <-- ctor -->
        public EnumTEntry(string name, int value, int index, Guid oid, bool isVisible)
        {
            _name = name;
            _value = value;
            _index = index;
            _oid = oid;
            _isVisible = isVisible;
        }

        #endregion

        #region <-- properties -->
        public string Text
        {
            get { return _name; }
        }
        private readonly string _name;

        public int Value
        {
            get { return _value; }
        }
        private readonly int _value;

        public int Index
        {
            get { return _index; }
        }
        private readonly int _index;

        public Guid Oid
        {
            get { return _oid; }
        }
        private readonly Guid _oid;

        public bool IsVisible
        {
            get { return _isVisible; }
        }
        private readonly bool _isVisible;
        #endregion
    }
}