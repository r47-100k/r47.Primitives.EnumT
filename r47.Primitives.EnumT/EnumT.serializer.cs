using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace r47.Primitives.EnumT
{
    public abstract partial class EnumT<T>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the current <see cref="EnumT{T}"/> entries to a JSON stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public static void ToJson(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            
            var entries = ClonedEntries().Cast<EnumEntry>().ToList();
            JsonSerializer.Serialize(stream, entries, SerializerOptions);
        }

        /// <summary>
        /// Serializes the current <see cref="EnumT{T}"/> entries to a JSON file.
        /// </summary>
        /// <param name="fileName">The path to the destination file.</param>
        public static void ToJson(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            using (var fs = File.Create(fileName))
            {
                ToJson(fs);
            }
        }

        /// <summary>
        /// Deserializes <see cref="EnumEntry"/> items from a JSON stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <returns>A list of deserialized entries.</returns>
        public static List<EnumEntry> FromJson(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            return JsonSerializer.Deserialize<List<EnumEntry>>(stream, SerializerOptions);
        }

        /// <summary>
        /// Deserializes <see cref="EnumEntry"/> items from a JSON file.
        /// </summary>
        /// <param name="fileName">The path to the JSON file.</param>
        /// <returns>A list of deserialized entries.</returns>
        public static List<EnumEntry> FromJson(string fileName)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException("JSON file not found.", fileName);

            using (var fs = File.OpenRead(fileName))
            {
                return FromJson(fs);
            }
        }
    }
}