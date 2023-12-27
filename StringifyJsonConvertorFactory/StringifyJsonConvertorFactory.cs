namespace ktsu.io.StringifyJsonConvertorFactory;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public class StringifyJsonConvertorFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));
		return typeToConvert.IsClass && typeToConvert.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public) is not null;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));

		var converterType = typeof(StringifyJsonConvertor<>).MakeGenericType(new Type[] { typeToConvert });
		return (JsonConverter)Activator.CreateInstance(converterType, BindingFlags.Instance | BindingFlags.Public, binder: null, args: null, culture: null)!;
	}

	private class StringifyJsonConvertor<T> : JsonConverter<T>
	{
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));

			if (reader.TokenType != JsonTokenType.String)
			{
				throw new JsonException();
			}

			dynamic t = reader.GetString()!;
			return (T?)t;
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			ArgumentNullException.ThrowIfNull(writer, nameof(writer));

			writer.WriteStringValue(value.ToString());
		}
	}
}
