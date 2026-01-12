using FluentAssertions;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;

namespace r47.Primitives.EnumT.Test
{
    [Collection("parallel-execution")]
    public class EnumTFlagsTests
    {
        [Fact]
        public void Ampersand_NullSemantics()
        {
            (EnumTMockFlags.FlagA & (EnumTMockFlags)null).Should().BeFalse();
            (((EnumTMockFlags)null) & EnumTMockFlags.FlagA).Should().BeFalse();
            (((EnumTMockFlags)null) & (EnumTMockFlags)null).Should().BeFalse();
        }

        [Fact]
        public void Ampersand_SharesCommonBit()
        {
            (EnumTMockFlags.FlagA & EnumTMockFlags.FlagB).Should().BeFalse();
            (EnumTMockFlags.ComboAB & EnumTMockFlags.FlagB).Should().BeTrue();
        }

        [Fact]
        public void OrOperator_CombinesToMask()
        {
            int mask = EnumTMockFlags.FlagA | EnumTMockFlags.FlagB;
            mask.Should().Be(3);

            int maskWithNull = ((EnumTMockFlags)null) | EnumTMockFlags.FlagC;
            maskWithNull.Should().Be(4);
        }

        [Fact]
        public void NotOperator_ComplementsMask()
        {
            int notA = ~EnumTMockFlags.FlagA;
            (notA & EnumTMockFlags.FlagA.Value).Should().Be(0);
            (notA & EnumTMockFlags.FlagB.Value).Should().Be(EnumTMockFlags.FlagB.Value);
        }

        [Fact]
        public void HasFlag_SubsetSemantics()
        {
            EnumTMockFlags.ComboAB.HasFlag(EnumTMockFlags.FlagA).Should().BeTrue();
            EnumTMockFlags.ComboAB.HasFlag(EnumTMockFlags.FlagC).Should().BeFalse();
            EnumTMockFlags.FlagA.HasFlag(EnumTMockFlags.ComboAB).Should().BeFalse();
            EnumTMockFlags.FlagA.HasFlag(null).Should().BeFalse();
        }
    }
}
