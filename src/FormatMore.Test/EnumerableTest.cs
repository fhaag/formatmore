/*
Copyright (c) 2021 Florian Haag

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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Xunit;

#nullable enable

namespace FormatMoreUtilities.Test
{
    public sealed class EnumerableTest
    {
        #region infrastructure

        private void TestInput(string expected, string format, params object[] args) => TestInput(expected, null, format, args);

		private void TestInput(string expected, IFormatProvider? provider, string format, params object[] args)
		{
			var formatMoreResult = FormatMore.Format(provider, format, args);

			formatMoreResult.Should().Be(expected);
		}

        #endregion

        #region data

        private static readonly char[] fiveLetters = new[] { 'f', 'B', 'e', 'a', 'V' };

        private static readonly float[] sixFloats = new[] { 3f, 2.5f, 6.71855f, 2332.0059221f, -93.1782f, 0.0035583f };

        #endregion

        [Fact]
        public void TestEmpty() => TestInput("abc", "a{0[]}bc", new object[] { new string[0] });

        [Fact]
        public void TestEmptyPlaceholder()
        {
            var format = "{0[e(none)]}";
            TestInput("(none)", format, new object[] { new string[0] });
            TestInput("abcxyz", format, new object[] { new[] { "abc", "xyz" } });
        }

        [Fact]
        public void TestSingleItem() => TestInput("abcdmnf", "abcd{0[]}f", new object[] { new[] { 'm', 'n' } });

        [Fact]
        public void TestMultiItems() => TestInput("fBeaVxyz", "{0[]}xyz", new object[] { fiveLetters });

        [Fact]
        public void TestMulti() => TestInput("def;GHI;JK-x,ab", "{1[d;]}-{0[d,]}", new object[] { new[] { "x", "ab" }, new[] { "def", "GHI", "JK" } });

        [Fact]
        public void TestDelimitedMultiItems() => TestInput("ABCf + B + e + a + VDEF", "ABC{0[d + ]}DEF", new object[] { fiveLetters });

        [Fact]
        public void TetsDelimiterPrecedence() => TestInput("f+B+e+a+V", "{0[d+|d-]}", new object[] { fiveLetters });

        [Fact]
        public void TestSpecialDelimiters() => TestInput("Af-B-e+a..VBC", "A{0[d-|D-2=+|D-1=..]}BC", new object[] { fiveLetters });

        [Theory]
        [InlineData("")]
        [InlineData("=")]
        public void TestEqualConditionalDelimiters(string conditionPrefix)
        {
            var format = "{0[d+|D-1/" + conditionPrefix + "3=_|D-1=-]}";
            TestInput("a-b", format, new object[] { new[] { 'a', 'b' } });
            TestInput("a+b_c", format, new object[] { new[] { 'a', 'b', 'c' } });
            TestInput("a+b+c-d", format, new object[] { new[] { 'a', 'b', 'c', 'd' } });
            TestInput("a+b+c+d-e", format, new object[] { new[] { 'a', 'b', 'c', 'd', 'e' } });
        }

        [Fact]
        public void TestLessThanConditionalDelimiters()
        {
            var format = "{0[d+|D-1/<4=_|D-1=-]}";
            TestInput("a_b", format, new object[] { new[] { 'a', 'b' } });
            TestInput("a+b_c", format, new object[] { new[] { 'a', 'b', 'c' } });
            TestInput("a+b+c-d", format, new object[] { new[] { 'a', 'b', 'c', 'd' } });
            TestInput("a+b+c+d-e", format, new object[] { new[] { 'a', 'b', 'c', 'd', 'e' } });
        }

        [Fact]
        public void TestGreaterThanConditionalDelimiters()
        {
            var format = "{0[d+|D-1/>3=_|D-1=-]}";
            TestInput("a-b", format, new object[] { new[] { 'a', 'b' } });
            TestInput("a+b-c", format, new object[] { new[] { 'a', 'b', 'c' } });
            TestInput("a+b+c_d", format, new object[] { new[] { 'a', 'b', 'c', 'd' } });
            TestInput("a+b+c+d_e", format, new object[] { new[] { 'a', 'b', 'c', 'd', 'e' } });
        }

        private sealed class FormattingCultures : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new CultureInfo("en-US") };
                yield return new object[] { new CultureInfo("de-DE") };
                yield return new object[] { new CultureInfo("zh-Hans") };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(FormattingCultures))]
        public void TestDefaultFormatFloats(CultureInfo culture) => TestInput("_" + String.Join(";", sixFloats.Select(f => f.ToString(culture))),
            culture, "_{0[d;]}", new object[] { sixFloats });

        [Theory]
        [ClassData(typeof(FormattingCultures))]
        public void TestCustomFormatFloats(CultureInfo culture) => TestInput("_" + String.Join("/", sixFloats.Select(f => f.ToString("0.##", culture))),
            culture, "_{0[d/]:0.##}", new object[] { sixFloats });

        [Fact]
        public void TestCount()
        {
            TestInput("rrfB", "rr{0[2]}", new object[] { fiveLetters });
            TestInput("rrfBe", "rr{0[3]}", new object[] { fiveLetters });
            TestInput("rrfBeaV", "rr{0[5]}", new object[] { fiveLetters });
            TestInput("rrfBeaV", "rr{0[6]}", new object[] { fiveLetters });
        }

        [Fact]
        public void TestMaxCount()
        {
            TestInput("rrfB", "rr{0[2..4]}", new object[] { fiveLetters });
            TestInput("rrfBeaV", "rr{0[2..6]}", new object[] { fiveLetters });
        }

        [Fact]
        public void TestMoreItemsMarker()
        {
            TestInput("f+B...", "{0[2|d+|m...]}", new object[] { fiveLetters });
            TestInput("f+B+e+a+V", "{0[8|d+|m...]}", new object[] { fiveLetters });
        }

        [Fact]
        public void TestRemainderItem()
        {
            TestInput("f+B+...", "{0[2|d+|r...]}", new object[] { fiveLetters });
            TestInput("f+B+e+a+V", "{0[8|d+|r...]}", new object[] { fiveLetters });
        }
    }
}
