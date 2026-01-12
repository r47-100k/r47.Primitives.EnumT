using System;
using System.Linq;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;
using FluentAssertions;

namespace r47.Primitives.EnumT.Test
{
    [Collection("parallel-execution")]
    public class EnumTTests
    {
        [Fact]
        public void Ctor_WithAutonumerate_ValuesIndexes()
        {
            // arrange
            var e1 = int.MinValue + 1;
            var e2 = e1 + 1;
            var e3 = e2 + 1;

            // act
            var r = EnumTMock01.Entry1;

            // assert
            EnumTMock01.Entry1.Value.Should().Be(e1);
            EnumTMock01.Entry2.Value.Should().Be(e2);
            EnumTMock01.Entry3.Value.Should().Be(e3);

            EnumTMock01.Entry1.Index.Should().Be(e1);
            EnumTMock01.Entry2.Index.Should().Be(e2);
            EnumTMock01.Entry3.Index.Should().Be(e3);

            // indices are ascending
            EnumTMock01.Entry1.Index.Should().BeLessThan(EnumTMock01.Entry2.Index);
            EnumTMock01.Entry2.Index.Should().BeLessThan(EnumTMock01.Entry3.Index);

            // values are ascending
            EnumTMock01.Entry1.Value.Should().BeLessThan(EnumTMock01.Entry2.Value);
            EnumTMock01.Entry2.Value.Should().BeLessThan(EnumTMock01.Entry3.Value);

            // all entries are visible
            EnumTMock01.SortVisibleEntries().Count().Should().Be(EnumTMock01.ClonedEntries().Count());

            // determination default
            EnumTMock01.Entry1.IsDefault.Should().BeTrue();
            EnumTMock01.Default.Should().Be(EnumTMock01.Entry1);
        }

        [Fact]
        public void InitializeTests()
        {
            // arrange
            var sut = EnumTMock01.Entry1;
            var t1 = sut.Text;
            var v1 = sut.Value;

            // act
            var t2 = Guid.NewGuid().ToString();
            var v2 = 987654321;
            sut.Initialize(t2, v2);

            // assert
            sut.Text.Should().Be(t2);
            sut.Value.Should().Be(v2);
            t1.Should().NotBe(t2);
            v1.Should().NotBe(v2);
        }

        [Fact]
        public void SortEntries()
        {
            // arrange
            var m = EnumTMock02.Entry1;

            // act
            var r = EnumTMock02.SortEntries().ToArray();

            // assert
            r[0].Text.Should().Be(EnumTMock02.Entry3.Text);
            r[1].Text.Should().Be(EnumTMock02.Entry1.Text);
            r[2].Text.Should().Be(EnumTMock02.Entry2.Text);
        }

        [Fact]
        public void SortVisibleEntries()
        {
            // arrange
            var m = EnumTMock02.Entry1;

            // act
            var r = EnumTMock02.SortVisibleEntries().ToArray();

            // assert
            r.Should().HaveCount(2);
            r[0].Text.Should().Be(EnumTMock02.Entry2.Text);
            r[1].Text.Should().Be(EnumTMock02.Entry4.Text);
        }

        [Fact]
        public void Find()
        {
            // arrange
            var items = EnumTMock01.SortEntries();

            // act & assert
            foreach (var n in items)
            {
                EnumTMock01.Find(n.Oid).Text.Should().Be(n.Text);
            }
        }
    }
}