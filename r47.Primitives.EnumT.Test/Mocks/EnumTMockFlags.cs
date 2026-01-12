namespace r47.Primitives.EnumT.Test.Mocks
{
    public class EnumTMockFlags : EnumT<EnumTMockFlags>
    {
        private EnumTMockFlags(string name, int value) : base(name, value)
        {
        }

        static EnumTMockFlags()
        {
            Default = FlagA;
        }

        public static readonly EnumTMockFlags FlagA = new EnumTMockFlags("FlagA", 1); // 0b0001
        public static readonly EnumTMockFlags FlagB = new EnumTMockFlags("FlagB", 2); // 0b0010
        public static readonly EnumTMockFlags FlagC = new EnumTMockFlags("FlagC", 4); // 0b0100
        public static readonly EnumTMockFlags ComboAB = new EnumTMockFlags("ComboAB", FlagA.Value | FlagB.Value); // 0b0011
    }
}
