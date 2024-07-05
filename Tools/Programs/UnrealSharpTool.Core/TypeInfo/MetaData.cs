using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnrealSharp.Utils.Extensions;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace UnrealSharpTool.Core.TypeInfo;

/// <summary>
/// Class MetaDefinition.
/// </summary>
public class MetaDefinition : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// Gets or sets the metas.
    /// </summary>
    /// <value>The metas.</value>
    public SortedList<string, string> Metas { get; set; } = new();

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        Metas.Clear();
    }

    /// <summary>
    /// Determines whether the specified key has meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if the specified key has meta; otherwise, <c>false</c>.</returns>
    public bool HasMeta(string key)
    {
        return Metas.ContainsKey(key);
    }

    /// <summary>
    /// Gets the meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public string? GetMeta(string key)
    {
        Metas.TryGetValue(key, out var value);

        return value;
    }

    /// <summary>
    /// Tries the get meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> success, <c>false</c> otherwise.</returns>
    public bool TryGetMeta(string key, out string? value)
    {
        return Metas.TryGetValue(key, out value);
    }

    /// <summary>
    /// Tries the get meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <returns><c>true</c> get success, <c>false</c> otherwise.</returns>
    public bool TryGetMeta(string key, out bool value)
    {
        value = false;
        if(!Metas.TryGetValue(key, out var temp))
        {
            return false;
        }

        value = temp.iEquals("True") || temp.iEquals("1");
        return true;
    }

    /// <summary>
    /// Tries the get meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> get success, <c>false</c> otherwise.</returns>
    public bool TryGetMeta(string key, out int value)
    {
        value = 0;
        return Metas.TryGetValue(key, out var temp) && int.TryParse(temp, out value);
    }

    /// <summary>
    /// Tries the get meta.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public bool TryGetMeta<T>(string key, out T? value) where T : Enum
    {
        value = default;
        if(!TryGetMeta(key, out int iv))
        {
            return false;
        }

        value = (T)Enum.ToObject(typeof(T), iv);
        return true;
    }

    /// <summary>
    /// Sets the meta.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void SetMeta(string key, string value)
    {
        Metas[key] = value;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return Metas.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return Metas.GetEnumerator();
    }
}

/// <summary>
/// Class MetaDefinitionConverter.
/// Implements the <see cref="JsonConverter" />
/// </summary>
/// <seealso cref="JsonConverter" />
public class MetaDefinitionConverter : JsonConverter
{
    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(MetaDefinition);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var metaDefinition = new MetaDefinition();
        var jsonArray = JArray.Load(reader);

        foreach (var jToken in jsonArray)
        {
            var keyValueObject = (JObject)jToken;
            var key = keyValueObject["Name"]!.Value<string>()!;
            var value = keyValueObject["Value"]!.Value<string>()!;
            metaDefinition.SetMeta(key, value);
        }

        return metaDefinition;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var metaDefinition = (MetaDefinition)value!;

        var jsonArray = new JArray();

        foreach (var keyValueObject in metaDefinition.Metas.Select(keyValuePair => new JObject
                 {
                     { "Name", keyValuePair.Key },
                     { "Value", keyValuePair.Value }
                 }))
        {
            jsonArray.Add(keyValueObject);
        }

        jsonArray.WriteTo(writer);
    }
}