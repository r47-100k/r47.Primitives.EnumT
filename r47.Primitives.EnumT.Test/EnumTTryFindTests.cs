using System;
using System.Linq;
using FluentAssertions;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;

namespace r47.Primitives.EnumT.Test
{
    [Collection("parallel-execution")]
    public class EnumTTryFindTests
    {
        [Fact]
        public void TryFind_ReturnsTrue_AndResult_WhenOidExists()
        {
            // arrange
            var all = EnumTMock01.SortEntries().ToArray();

            // act & assert
            foreach (var n in all)
            {
                var ok = EnumTMock01.TryFind(n.Oid, out var found);
                ok.Should().BeTrue();
                found.Should().Be(n);
            }
        }

        [Fact]
        public void TryFind_ReturnsFalse_AndNull_WhenOidMissing()
        {
            // arrange
            var missing = Guid.NewGuid();

            // act
            var ok = EnumTMock01.TryFind(missing, out var notFound);

            // assert
            ok.Should().BeFalse();
            notFound.Should().BeNull();
        }
    }
}
