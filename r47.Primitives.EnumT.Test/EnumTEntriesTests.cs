using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;

namespace r47.Primitives.EnumT.Test
{
    [Collection("parallel-execution")]
    public class EnumTEntriesTests
    {
        [Fact]
        public void Entries_ReturnsSnapshot_InRegistrationOrder()
        {
            // arrange - force static init
            var _ = EnumTMock01.Entry1;

            // act
            var entries = EnumTMock01.Entries();

            // assert
            entries.Should().NotBeNull();
            entries.Should().HaveCount(3);
            entries[0].Should().BeSameAs(EnumTMock01.Entry1);
            entries[1].Should().BeSameAs(EnumTMock01.Entry2);
            entries[2].Should().BeSameAs(EnumTMock01.Entry3);
        }

        [Fact]
        public void Entries_ReturnsReadOnly_Copy_EachTime()
        {
            // arrange
            var first = EnumTMock02.Entries();
            var second = EnumTMock02.Entries();

            // The two snapshots should not be the same instance
            ReferenceEquals(first, second).Should().BeFalse();

            // Returned list should be read-only
            var asCollection = (ICollection<EnumTMock02>)first;
            asCollection.IsReadOnly.Should().BeTrue();
            Action addAct = () => asCollection.Add(EnumTMock02.Entry1);
            addAct.Should().Throw<NotSupportedException>();

            // Ensure contents are as expected
            first.Should().Contain(new[] { EnumTMock02.Entry1, EnumTMock02.Entry2, EnumTMock02.Entry3, EnumTMock02.Entry4 });
        }
    }
}
