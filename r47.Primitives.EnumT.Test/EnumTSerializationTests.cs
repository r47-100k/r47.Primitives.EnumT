using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using r47.Primitives.EnumT.Test.Mocks;
using Xunit;

namespace r47.Primitives.EnumT.Test
{
    public class EnumTSerializationTests
    {
        [Fact]
        public void ToJson_And_FromJson_Stream_ShouldPreserveData()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                // Act: Serialize
                EnumTMock01.ToJson(stream);
                stream.Position = 0;

                // Act: Deserialize
                List<EnumEntry> results = EnumTMock01.FromJson(stream);

                // Assert
                results.Should().NotBeNull();
                results.Count.Should().Be(3);

                var entry1 = results.FirstOrDefault(e => e.Text == "Entry1");
                entry1.Should().NotBeNull();
                entry1.Value.Should().Be(EnumTMock01.Entry1.Value);
                entry1.Index.Should().Be(EnumTMock01.Entry1.Index);
                entry1.Oid.Should().Be(EnumTMock01.Entry1.Oid);
                entry1.IsVisible.Should().BeTrue();
            }
        }

        [Fact]
        public void ToJson_And_FromJson_File_ShouldWorkCorrectly()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                // Act: Serialize to file
                EnumTMock02.ToJson(tempFile);

                // Act: Deserialize from file
                List<EnumEntry> results = EnumTMock02.FromJson(tempFile);

                // Assert
                results.Should().HaveCount(4);
                
                // Check Entry1 (which was initialized with IsVisible = false in Mock02)
                var entry1 = results.First(e => e.Text == "Entry1");
                entry1.IsVisible.Should().BeFalse();
                entry1.Index.Should().Be(3);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void FromJson_WithEmptyStream_ShouldReturnNullOrThrow()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                // Act & Assert
                // System.Text.Json usually throws JsonException on empty input depending on options
                Assert.ThrowsAny<Exception>(() => EnumTMock01.FromJson(stream));
            }
        }

        [Fact]
        public void ToJson_ShouldThrow_WhenStreamIsNull()
        {
            // Act
            Action act = () => EnumTMock01.ToJson((Stream)null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
