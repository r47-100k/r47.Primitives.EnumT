using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;

namespace r47.Primitives.EnumT.Test
{
    [Collection("parallel-execution")]
    public class EnumTLookupTests
    {
        [Fact]
        public void FromValue_ReturnsInstance_And_ThrowsWhenMissing()
        {
            // arrange
            var all = EnumTMock01.SortEntries().ToArray();

            // act/assert: existing
            foreach (var n in all)
            {
                var found = EnumTMock01.FromValue(n.Value);
                found.Should().BeSameAs(n);
            }

            // act/assert: missing -> throws
            int missing = all.Max(e => e.Value) + 12345;
            Action act = () => EnumTMock01.FromValue(missing);
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void TryFromValue_ReturnsTrueForExisting_AndFalseForMissing()
        {
            // arrange
            var n = EnumTMock01.Entry2;
            int missing = n.Value + 9999999;

            // act/assert
            EnumTMock01.TryFromValue(n.Value, out var okResult).Should().BeTrue();
            okResult.Should().BeSameAs(n);

            EnumTMock01.TryFromValue(missing, out var notFound).Should().BeFalse();
            notFound.Should().BeNull();
        }

        [Fact]
        public void FromText_IsCaseSensitive_ByDefault_TryFromText_DefaultsToIgnoreCase()
        {
            // arrange
            var n = EnumTMock02.Entry2; // "Entry2"

            // exact case works for FromText (Ordinal)
            EnumTMock02.FromText(n.Text).Should().BeSameAs(n);

            // different case: FromText should throw
            var upper = n.Text.ToUpperInvariant();
            if (upper != n.Text)
            {
                Action act = () => EnumTMock02.FromText(upper);
                act.Should().Throw<KeyNotFoundException>();
            }

            // TryFromText with default (IgnoreCase) should succeed
            EnumTMock02.TryFromText(upper, out var ciFound).Should().BeTrue();
            ciFound.Should().BeSameAs(n);

            // explicit comparison can be passed as well
            EnumTMock02.TryFromText(n.Text, StringComparison.Ordinal, out var csFound).Should().BeTrue();
            csFound.Should().BeSameAs(n);
        }

        [Fact]
        public void TryParse_Guid_Int_Text_And_FailureCases()
        {
            // arrange
            var n = EnumTMock01.Entry3;

            // GUID (OID)
            EnumTMock01.TryParse(n.Oid.ToString(), out var byGuid).Should().BeTrue();
            byGuid.Should().BeSameAs(n);

            // int Value
            EnumTMock01.TryParse(n.Value.ToString(), out var byValue).Should().BeTrue();
            byValue.Should().BeSameAs(n);

            // Text (case-insensitive default)
            EnumTMock01.TryParse(n.Text.ToUpperInvariant(), out var byText).Should().BeTrue();
            byText.Should().BeSameAs(n);

            // whitespace / empty -> false
            EnumTMock01.TryParse("   ", out var ws).Should().BeFalse();
            ws.Should().BeNull();

            // unknown -> false
            EnumTMock01.TryParse("this-does-not-exist", out var unk).Should().BeFalse();
            unk.Should().BeNull();
        }
    }
}
