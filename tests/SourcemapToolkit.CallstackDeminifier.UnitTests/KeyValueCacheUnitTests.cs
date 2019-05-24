using System;
using Xunit;
using NSubstitute;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{

	public class KeyValueCacheUnitTests
	{
		[Fact]
		public void GetValue_KeyNotInCache_CallValueGetter()
		{
			// Arrange
			Func<string, string> valueGetter = Substitute.For<Func<string, string>>();
			valueGetter("bar").Returns("foo");
			KeyValueCache<string, string> keyValueCache = new KeyValueCache<string, string>(valueGetter);

			// Act
			string result = keyValueCache.GetValue("bar");

			// Assert
			Assert.Equal("foo", result);

		}

		[Fact]
		public void GetValue_CallGetTwice_OnlyCallValueGetterOnce()
		{
			// Arrange
			Func<string, string> valueGetter = Substitute.For<Func<string, string>>();
			valueGetter("bar").Returns("foo");
			KeyValueCache<string, string> keyValueCache = new KeyValueCache<string, string>(valueGetter);
			keyValueCache.GetValue("bar"); // Place the value in the cache

			// Act
			string result = keyValueCache.GetValue("bar");

			// Assert
			Assert.Equal("foo", result);
			valueGetter.Received(1)("bar");
		}

		[Fact]
		public void GetValue_CallGetTwiceValueGetterReturnsNull_CallGetterTwice()
		{
			// Arrange
			Func<string, string> valueGetter = Substitute.For<Func<string, string>>();
			valueGetter("bar").Returns((string) null);
			KeyValueCache<string, string> keyValueCache = new KeyValueCache<string, string>(valueGetter);
			keyValueCache.GetValue("bar"); // Place null in the cache

			// Act
			string result = keyValueCache.GetValue("bar");

			// Assert
			Assert.Null(result);
			valueGetter.Received(2)("bar");
		}

		[Fact]
		public void GetValue_CallGetMultipleTimesFirstGetterReturnsNull_CacheFirstNonNullValue()
		{
			// Arrange
			Func<string, string> valueGetter = Substitute.For<Func<string, string>>();
			valueGetter("bar").Returns((string) null);
			KeyValueCache<string, string> keyValueCache = new KeyValueCache<string, string>(valueGetter);
			keyValueCache.GetValue("bar"); // Place null in the cache
			valueGetter("bar").Returns("foo");
			keyValueCache.GetValue("bar"); // Place a non null value in the cahce

			// Act
			string result = keyValueCache.GetValue("bar");

			// Assert
			Assert.Equal("foo", result);
			valueGetter.Received(2);

		}
	}
}
