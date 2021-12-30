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
using FluentAssertions;
using Xunit;

namespace FormatMoreUtilities.Test
{
	public sealed class BackwardCompatibilityTest
	{
		#region infrastructure

		private void TestInput(string format, params object[] args) => TestInput(null, format, args);

		private void TestInput(IFormatProvider provider, string format, params object[] args)
		{
			var formatResult = String.Format(provider, format, args);
			var formatMoreResult = FormatMore.Format(provider, format, args);

			formatMoreResult.Should().Be(formatResult);
		}

		#endregion

		[Fact]
		public void NoArgs() => TestInput("123 test");

		[Fact]
		public void SingleArg() => TestInput("xy{0}z", 42);

		[Fact]
		public void SingleArgStart() => TestInput("{0}abc", true);

		[Fact]
		public void SingleArgEnd() => TestInput("qwertz{0}", "test");
		
		[Fact]
		public void SingleArgMultiOccurrence() => TestInput("{0}xy{0}z", 1000);

		[Fact]
		public void MultiArg() => TestInput("ab{0}c{1}xyz", 50, "X");
		
		[Fact]
		public void MultiArgUnordered() => TestInput("ab{1}{2}abc{0}", 10, 20, 30);

		[Fact]
		public void SingleArgFmt() => TestInput("def{0:X3}DEF", 42);

		[Fact]
		public void MultiArgAdjacent() => TestInput("{0}{1}", "a", "x");

		[Fact]
		public void MultiArgOneBetween() => TestInput("{0}m{1}", "a", "x");

		[Fact]
		public void MultiArgTwoBetween() => TestInput("{0}Mm{1}", "a", "x");

		[Fact]
		public void MultiArgThreeBetween() => TestInput("{0}MmR{1}", "a", "X");

		[Fact]
		public void SingleArgOneBefore() => TestInput("a{0}", "R");

		[Fact]
		public void SingleArgTwoBefore() => TestInput("Aa{0}", "R");

		[Fact]
		public void SingleArgOneAfter() => TestInput("{0}a", "Z");

		[Fact]
		public void SingleArgTwoAfter() => TestInput("{0}Aa", "Z");
	}
}