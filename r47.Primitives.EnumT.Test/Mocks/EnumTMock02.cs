namespace r47.Primitives.EnumT.Test.Mocks
{

    public class EnumTMock02 : EnumT<EnumTMock02>
    {
        public EnumTMock02(string name, bool isVisible = true)
            : base(name, isVisible)
        {
        }

        public EnumTMock02(string name, int? value, int? index, bool isVisible = true)
            : base(name, value, index, isVisible)
        {
        }

        static EnumTMock02()
        {
            Default = Entry1;
        }

        public static readonly EnumTMock02 Entry1 = new EnumTMock02("Entry1", null, 3, false);
        public static readonly EnumTMock02 Entry2 = new EnumTMock02("Entry2", null, 4);
        public static readonly EnumTMock02 Entry3 = new EnumTMock02("Entry3", null, 2, false);
        public static readonly EnumTMock02 Entry4 = new EnumTMock02("Entry4");
    }
}