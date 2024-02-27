#pragma warning disable CS1591

namespace ktsu.io.StringifyJsonConvertorFactory;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReflectionExtensions;

public class StringifyJsonConvertorFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));
		return typeToConvert.TryFindMethod("FromString", BindingFlags.Static | BindingFlags.Public, out var method) &&
			method is not null && method.GetParameters().Length != 0 &&
			method.GetParameters().First().ParameterType == typeof(string);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));

		var converterType = typeof(StringifyJsonConvertor<>).MakeGenericType(new Type[] { typeToConvert });
		return (JsonConverter)Activator.CreateInstance(converterType, BindingFlags.Instance | BindingFlags.Public, binder: null, args: null, culture: null)!;
	}

	private class StringifyJsonConvertor<T> : JsonConverter<T>
	{
		private static readonly MethodInfo? FromStringMethod;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "<Pending>")]
		static StringifyJsonConvertor()
		{
			typeof(T).TryFindMethod("FromString", BindingFlags.Static | BindingFlags.Public, out FromStringMethod);
			if (FromStringMethod?.ContainsGenericParameters ?? false)
			{
				FromStringMethod = FromStringMethod?.MakeGenericMethod(typeof(T));
			}
		}

		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));

			return reader.TokenType == JsonTokenType.String
				? (T)FromStringMethod!.Invoke(null, new object[] { reader.GetString()! })!
				: throw new JsonException();
		}

		public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));

			return (T)FromStringMethod!.Invoke(null, new object[] { reader.GetString()! })!;
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			ArgumentNullException.ThrowIfNull(writer, nameof(writer));

			writer.WriteStringValue(value.ToString());
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			ArgumentNullException.ThrowIfNull(writer, nameof(writer));

			writer.WritePropertyName(value.ToString() ?? "");
		}
	}
}
