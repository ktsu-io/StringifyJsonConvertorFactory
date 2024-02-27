#pragma warning disable CS1591

namespace ktsu.io.StringifyJsonConvertorFactory.Test;

using System.Text.Json;

[TestClass]
public class Test
{
	public static Test FromString(string value) => new(value);

	public override string ToString() => hiddenString;

	private readonly string hiddenString;

	public Test(string value) => hiddenString = value;
	public Test() => hiddenString = "default";

	private static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.General)
	{
		Converters =
		{
			new StringifyJsonConvertorFactory(),
		}
	};

	[TestMethod]
	public void TestRoundTrip()
	{
		Test test = new("test");
		string jsonString = JsonSerializer.Serialize(test, JsonSerializerOptions);
		var result = JsonSerializer.Deserialize<Test>(jsonString, JsonSerializerOptions);
		Assert.AreEqual(test.hiddenString, result?.hiddenString);
	}

	[TestMethod]
	public void TestDictionary()
	{
		Dictionary<Test, int> test = new()
		{
			{
				new("test1"), 1
			},
			{
				new("test2"), 2
			},
		};
		string jsonString = JsonSerializer.Serialize(test, JsonSerializerOptions);
		var result = JsonSerializer.Deserialize<Dictionary<Test, int>>(jsonString, JsonSerializerOptions) ?? new();
		Assert.IsTrue(test.Keys.Select(x => x.hiddenString).SequenceEqual(result.Keys.Select(x => x.hiddenString)));
		Assert.IsTrue(test.Values.SequenceEqual(result.Values));
	}
}
