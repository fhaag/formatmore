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
using System.Collections.Generic;
using System.Linq;

namespace FormatMoreUtilities
{
	partial class FormatMore
	{
		private static readonly Dictionary<string, IReadOnlyList<ListFormatInfo>> _listFormatPresets = new Dictionary<string, IReadOnlyList<ListFormatInfo>>();

		/// <summary>
		/// Stores a list format specification with a given key as a reusable preset.
		/// </summary>
		/// <param name="key">The preset key.</param>
		/// <param name="format">The list format specification.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>, or <paramref name="key"/> is an empty or whitespace-only string.</exception>
		public static void RegisterListFormatPreset(string key, string format)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentNullException(nameof(key));
			}
			if (format == null)
			{
				throw new ArgumentNullException(nameof(format));
			}

			var matches = ListFormatPattern.Matches(format);
			var settings = matches.Select(m =>
			{
				var structureMatch = ListFormatStructurePattern.Match(m.Value);
				return new ListFormatInfo(structureMatch);
			}).ToArray();

			_listFormatPresets[key] = settings;
		}

		/// <summary>
		/// Removes all stored reusable presets.
		/// </summary>
		public static void ClearPresets()
		{
			_listFormatPresets.Clear();
		}
	}
}
