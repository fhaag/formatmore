/*
Copyright (c) 2021, 2022 Florian Haag

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
using static System.Globalization.CultureInfo;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FormatMoreUtilities
{
	/// <summary>
	/// Provides utility functions for formatting strings with values.
	/// </summary>
	public static partial class FormatMore
	{
		private const string ListFormatPatternText = @"\[(?:[0-9]+(?:\.\.[0-9]+)?)?(?:(?:\||(?<=\[))[^|\]0-9](?:(?:[^|\]]|\|\||\]\])*))*\]\??";

		private static readonly Regex ListFormatPattern = new Regex(ListFormatPatternText);

		private static readonly Regex ListFormatStructurePattern =
			new Regex(@"^(?<listFormatting>\[(?:(?<count>[0-9]+)(?:\.\.(?<maxCount>[0-9]+))?)?(?:(?:\||(?<=\[))(?<optionKey>[^|\]0-9])(?<optionValue>(?:[^|\]]|\|\||\]\])*))*\](?<listModifier>\?)?)$");

		private static readonly Regex FormatItemPattern =
			new Regex(@"\{(?<index>[0-9]+)(?<listFormatting>" + ListFormatPatternText + @")*(?:,(?<alignment>[+-]?[0-9]+))?(?:\:(?<format>(?:[^}]|\}\})*))?\}");

		private static readonly Regex LengthBasedArgumentValuePattern =
			new Regex(@"^(?<index>[+-]?[0-9]+)(?:/(?<lengthCondition>[=<>])?(?<lengthConditionOperand>[0-9]+))?\=(?<value>.*)$");

		private static readonly Regex OptionReplacementPattern = new Regex(@"([\|\]])\1");

		private sealed class ListFormatInfo
		{
			public ListFormatInfo(Match source)
			{
				var modifierMatch = source.Groups["listModifier"];
				if (modifierMatch.Success)
				{
					IsOptional = true;
				}

				var countMatch = source.Groups["count"];
				Count = countMatch.Success ? (int?)int.Parse(countMatch.Value, InvariantCulture) : null;

				var maxCountMatch = source.Groups["maxCount"];
				MaxCount = maxCountMatch.Success ? int.Parse(maxCountMatch.Value, InvariantCulture) : Count;

				if (MaxCount.HasValue && Count.HasValue && MaxCount.Value < Count)
				{
					throw new FormatException("The maximum number of items to display must not be less than the standard number of items to display.");
				}

				var optionKeys = source.Groups["optionKey"];
				var optionValues = source.Groups["optionValue"];
				if (optionKeys.Captures.Count != optionValues.Captures.Count)
				{
					throw new ArgumentException();
				}

				Options = Enumerable.Range(0, optionKeys.Captures.Count)
					.Select(optionIndex => (Key: optionKeys.Captures[optionIndex].Value, Value: optionValues.Captures[optionIndex].Value))
					.GroupBy(pair => pair.Key).ToDictionary(g => g.Key[0], g => g.Select(p => OptionReplacementPattern.Replace(p.Value, m => m.Groups[1].Value)).ToArray());

				_itemTexts = new Lazy<(string BeforeItemText, string AfterItemText)>(() =>
				{
					Options.TryGetValue('b', out var beforeValues);
					Options.TryGetValue('a', out var afterValues);

					return (beforeValues?[0] ?? "", afterValues?[0] ?? "");
				});
			}

			public int? Count { get; }

			public int? MaxCount { get; }

			public bool IsOptional { get; }

			public IReadOnlyDictionary<char, string[]> Options { get; }

			public string MoreItemsMarker
			{
				get
				{
					if (Options.TryGetValue('m', out var values))
					{
						return values[0];
					}
					return "";
				}
			}

			public string? RemainderItem
			{
				get
				{
					if (Options.TryGetValue('r', out var values))
					{
						return values[0];
					}
					return null;
				}
			}

			public string EmptyPlaceholder
			{
				get
				{
					if (Options.TryGetValue('e', out var values))
					{
						return values[0];
					}
					return "";
				}
			}

			public string? ReplaceWithPreset
			{
				get
				{
					if (Options.TryGetValue('p', out var values))
					{
						return values[0];
					}
					return null;
				}
			}

			private readonly Lazy<(string BeforeItemText, string AfterItemText)> _itemTexts;

			public (string BeforeItemText, string AfterItemText) ItemTexts => _itemTexts.Value;

			private enum LengthCondition : byte
			{
				Equals = 0,
				LessThan = 1,
				GreaterThan = 2
			}

			private static IEnumerable<(int Index, LengthCondition? Condition, int? LengthConditionOperand, string Value)> ParseLengthBasedValues(IEnumerable<string> rawValues)
				=> rawValues.Select<string, (int Index, LengthCondition? Condition, int? LengthConditionOperand, string Value)>(v =>
				{
					var match = LengthBasedArgumentValuePattern.Match(v);
					if (!match.Success)
					{
						throw new FormatException($"Invalid length-based argument: {v}");
					}

					try
					{
						var index = int.Parse(match.Groups["index"].Value, InvariantCulture);
						var val = match.Groups["value"].Value;

						var lengthConditionOperandGroup = match.Groups["lengthConditionOperand"];
						if (lengthConditionOperandGroup.Success)
						{
							var conditionOperand = int.Parse(lengthConditionOperandGroup.Value, InvariantCulture);

							var lengthConditionGroup = match.Groups["lengthCondition"];
							var condition = lengthConditionGroup.Success ? lengthConditionGroup.Value[0] switch
							{
								'<' => LengthCondition.LessThan,
								'>' => LengthCondition.GreaterThan,
								_ => LengthCondition.Equals
							} : LengthCondition.Equals;

							return (index, condition, conditionOperand, val);
						}

						return (index, null, null, val);
					}
					catch (Exception ex)
					{
						throw new FormatException($"Invalid length-based argument: {v}", ex);
					}
				});

			public string[] GetDelimiters(int itemCount, int totalItemCount)
			{
				if (itemCount <= 0)
				{
					return new string[0];
				}
				var result = new string[itemCount - 1];

				string defaultDelimiter;
				if (Options.TryGetValue('d', out var defaultDelimiterVals))
				{
					defaultDelimiter = defaultDelimiterVals[0];
				}
				else
				{
					defaultDelimiter = "";
				}

				if (!Options.TryGetValue('D', out var customDelimiterVals))
				{
					customDelimiterVals = new string[0];
				}

				var parsedCustomDelimVals = ParseLengthBasedValues(customDelimiterVals).ToArray();

				string? FindCustomDelim(int indexFromStart, int indexFromEnd)
				{
					for (int i = 0; i < parsedCustomDelimVals.Length; i++)
					{
						var pcdv = parsedCustomDelimVals[i];
						if (pcdv.Index == indexFromStart || pcdv.Index == indexFromEnd)
						{
							if (pcdv.Condition.HasValue && pcdv.LengthConditionOperand.HasValue)
							{
								switch (pcdv.Condition)
								{
									case LengthCondition.Equals:
										if (totalItemCount != pcdv.LengthConditionOperand)
										{
											continue;
										}
										break;
									case LengthCondition.LessThan:
										if (totalItemCount >= pcdv.LengthConditionOperand)
										{
											continue;
										}
										break;
									case LengthCondition.GreaterThan:
										if (totalItemCount <= pcdv.LengthConditionOperand)
										{
											continue;
										}
										break;
								}
							}

							return pcdv.Value;
						}
					}

					return null;
				}

				for (var i = 0; i < itemCount - 1; i++)
				{
					result[i] = FindCustomDelim(i, i - itemCount + 1) ?? defaultDelimiter;
				}

				return result;
			}
		}

		private sealed class EnhancedFormatItemInfo
		{
			public EnhancedFormatItemInfo(Match source)
			{
				Index = int.Parse(source.Groups["index"].Value, InvariantCulture);

				var alignmentMatch = source.Groups["alignment"];
				Alignment = alignmentMatch.Success ? alignmentMatch.Value : null;

				var formatMatch = source.Groups["format"];
				Format = formatMatch.Success ? formatMatch.Value : null;

				var listFormatGroup = source.Groups["listFormatting"];
				if (listFormatGroup.Success)
				{
					ListFormat = listFormatGroup.Captures.SelectMany(c =>
					{
						var match = ListFormatStructurePattern.Match(c.Value);
						if (!match.Success)
						{
							throw new FormatException("Invalid list format settings.");
						}

						var lfi = new ListFormatInfo(match);
						var replaceWithPreset = lfi.ReplaceWithPreset;
						if (replaceWithPreset != null)
						{
							if (_listFormatPresets.TryGetValue(replaceWithPreset, out var presetSettings))
							{
								return presetSettings;
							}
							else
							{
								throw new FormatException($"Unknown list format preset {replaceWithPreset} invoked.");
							}
						}

						return new[] { lfi };
					}).ToArray();
				}
			}

			public int Index { get; }

			public string? Alignment { get; }

			public string? Format { get; }

			public IReadOnlyList<ListFormatInfo>? ListFormat { get; }
		}

		/// <summary>
		/// Formats a string.
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <param name="args">The arguments to embed in the string.</param>
		/// <returns>The formatted string.</returns>
		/// <exception cref="FormatException">The provided format string was invalid.</exception>
		public static string Format(string format, params object[] args) => Format(null, format, args);

		/// <summary>
		/// Formats a string with a custom format provider.
		/// </summary>
		/// <param name="provider">A custom format provider.</param>
		/// <param name="format">The format string.</param>
		/// <param name="args">The arguments to embed in the string.</param>
		/// <returns>The formatted string.</returns>
		/// <exception cref="FormatException">The provided format string was invalid.</exception>
		public static string Format(IFormatProvider? provider, string format, params object[] args)
		{
			StringBuilder preprocessedFormat = new StringBuilder(format.Length);

			var preprocessedArgs = new List<object?>(args);

			void PreprocessFormatItem(Match formatItem)
			{
				var listFormattingGroup = formatItem.Groups["listFormatting"];
				if (listFormattingGroup.Success)
				{
					EnhancedFormatItemInfo formatInfo = new EnhancedFormatItemInfo(formatItem);

					if (formatInfo.ListFormat == null)
					{
						throw new InvalidOperationException("The list format component was unexpectedly null.");
					}

					if (formatInfo.Index < 0 || formatInfo.Index >= args.Length)
					{
						throw new FormatException(String.Format(InvariantCulture,
							"The zero-based index ({0}) must be greater than or equal to zero and less than the size of the argument list ({1}).",
							formatInfo.Index, args.Length));
					}

					string FormatList(object? arg, int listFormatIndex)
					{
						ListFormatInfo listFormatInfo = formatInfo.ListFormat[listFormatIndex];

						if (arg is System.Collections.IEnumerable enumerableArg)
						{
							var itemFormat = "{0"
								+ (formatInfo.Alignment != null ? "," + formatInfo.Alignment : "")
								+ (formatInfo.Format != null ? ":" + formatInfo.Format : "")
								+ "}";

							var totalCount = enumerableArg.Cast<object>().Count();
							var sb = new StringBuilder();

							if (totalCount <= 0)
							{
								sb.Append(listFormatInfo.EmptyPlaceholder);
							}
							else
							{
								var relevantItems = enumerableArg.Cast<object>().Take(listFormatInfo.MaxCount.HasValue ? listFormatInfo.MaxCount.Value + 1 : int.MaxValue).ToArray();

								var addMoreMarker = false;
								if (listFormatInfo.MaxCount.HasValue && relevantItems.Length > listFormatInfo.MaxCount)
								{
									relevantItems = relevantItems.Take(listFormatInfo.Count ?? listFormatInfo.MaxCount.Value).ToArray();
									addMoreMarker = true;
								}
								var formattedItems = relevantItems.Select(item =>
								{
									if (listFormatIndex + 1 < formatInfo.ListFormat.Count)
									{
										return FormatList(item, listFormatIndex + 1);
									}
									return string.Format(provider, itemFormat, item);
								}).Select(formattedItem => listFormatInfo.ItemTexts.BeforeItemText + formattedItem + listFormatInfo.ItemTexts.AfterItemText).ToArray();

								if (addMoreMarker)
								{
									var remainderItem = listFormatInfo.RemainderItem;
									if (remainderItem != null)
									{
										formattedItems = formattedItems.Append(remainderItem).ToArray();
									}
								}

								var delimiters = listFormatInfo.GetDelimiters(formattedItems.Length, totalCount);

								for (var i = 0; i < formattedItems.Length; i++)
								{
									sb.Append(formattedItems[i]);
									if (i < delimiters.Length)
									{
										sb.Append(delimiters[i]);
									}
								}
								if (addMoreMarker)
								{
									sb.Append(listFormatInfo.MoreItemsMarker);
								}
							}

							return sb.ToString();
						}
						else
						{
							if (listFormatInfo.IsOptional)
							{
								return string.Format(provider, "{0"
									+ (formatInfo.Alignment != null ? "," + formatInfo.Alignment : "")
									+ (formatInfo.Format != null ? ":" + formatInfo.Format : "")
									+ "}", arg);
							}
							else
							{
								throw new FormatException(string.Format(InvariantCulture,
									"Argument {0} cannot be converted to {1}.",
									formatInfo.Index, typeof(System.Collections.IEnumerable)));
							}
						}
					}

					var formattedList = FormatList(args[formatInfo.Index], 0);
					preprocessedFormat.Append("{" + preprocessedArgs.Count.ToString(InvariantCulture) + "}");
					preprocessedArgs.Add(formattedList);
				}
				else
				{
					preprocessedFormat.Append(formatItem.Value);
				}
			}

			var matches = FormatItemPattern.Matches(format);

			var previousVerbatimStart = 0;
			foreach (var match in matches.OfType<Match>())
			{
				if (match.Index > previousVerbatimStart)
				{
					preprocessedFormat.Append(format.Substring(previousVerbatimStart, match.Index - previousVerbatimStart));
				}

				PreprocessFormatItem(match);

				previousVerbatimStart = match.Index + match.Length;
			}
			if (previousVerbatimStart < format.Length)
			{
				preprocessedFormat.Append(format.Substring(previousVerbatimStart));
			}

			return String.Format(provider, preprocessedFormat.ToString(), preprocessedArgs.ToArray());
		}
	}
}
