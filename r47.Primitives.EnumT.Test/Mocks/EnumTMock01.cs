namespace r47.Primitives.EnumT.Test.Mocks
{

    public class EnumTMock01 : EnumT<EnumTMock01>
    {
        private EnumTMock01(string name) : base(name) { }

        static EnumTMock01()
        {
            Default = Entry1;
        }

        public static readonly EnumTMock01 Entry1 = new EnumTMock01("Entry1");
        public static readonly EnumTMock01 Entry2 = new EnumTMock01("Entry2");
        public static readonly EnumTMock01 Entry3 = new EnumTMock01("Entry3");
    }
}