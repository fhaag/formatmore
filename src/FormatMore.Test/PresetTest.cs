/*
Copyright (c) 2022 Florian Haag

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using static System.Globalization.CultureInfo;
using Xunit;
using FluentAssertions;

namespace FormatMoreUtilities.Test
{
	[Collection(nameof(PresetTestCollection))]
	public sealed class PresetTest : FormattingTestBase
	{
		[Fact]
		public void TestSingleListFormatPreset()
		{
			FormatMore.RegisterListFormatPreset("aa", "[d:|b__]");
			FormatMore.RegisterListFormatPreset("bb", "[d+]");

			try
			{
				var args = new object[] { new[] { "abc", "xyz", "ghij" } };
				TestInput("__abc:__xyz:__ghij", "{0[paa]}", args);
				TestInput("abc+xyz+ghij", "{0[pbb]}", args);
			}
			finally
			{
				FormatMore.ClearPresets();
			}
		}

		[Fact]
		public void TestMultiListFormatPreset()
		{
			FormatMore.RegisterListFormatPreset("A", "[d:|b__][dM]");

			try
			{
				TestInput("__1M2M5:__3M23", InvariantCulture, "{0[pA]}", new object[] { new[] { new[] { 1, 2, 5 }, new[] { 3, 23 } } });
			}
			finally
			{
				FormatMore.ClearPresets();
			}
		}

		[Fact]
		public void TestPresetMixed()
		{
			FormatMore.RegisterListFormatPreset("a", "[d-]");
			FormatMore.RegisterListFormatPreset("b", "[d+][d/]?");

			try
			{
				var args = new object[] { new[] { new[] { "x", "yyy", "z" }, new[] { "a", "ab" } } };
				TestInput("x-yyy-z::a-ab", "{0[d::][pa]}", args);
				TestInput("x/yyy/z-a/ab", "{0[pa][d/]}", args);
				TestInput("x+y/y/y+z,a+a/b", "{0[d,][pb]}", args);
				TestInput("x;y+y+y;z,a;a+b", "{0[d,][d;][pb]}", args);
			}
			finally
			{
				FormatMore.ClearPresets();
			}
		}

		[Fact]
		public void TestUnknownListFormatPreset()
		{
			Action action = () => FormatMore.Format("{0[pA]}", 42);
			action.Should().Throw<FormatException>("because no list formatting preset named A has been registered");
		}
	}
}
