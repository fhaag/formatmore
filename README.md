# FormatMore

A library that offers a more versatile alternative to the String.Format method from .NET.

## Quick Start

Download and reference `FormatMore.dll`.
Add

```csharp
using FormatMoreUtilities;
```

(or the appropriate equivalent command in your language of choice) to your file.

Invoke `FormatMore.Format` instead of `string.Format`.

## Full Syntax Specification

The [`String.Format` method of .NET](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) defines a [format item](https://docs.microsoft.com/en-us/dotnet/api/system.string.format#the-format-item) as

    {index[,alignment][:formatString]}

The *FormatMore* library provides an alternative `FormatMore.Format` method that accepts extended format items:

    {index[listFormat][,alignment][:formatString]}

The additional *listFormat* element applies to arguments that can be converted to `System.Collections.IEnumerable`.
It is defined as follows:

- It starts with an opening square bracket `[`.
- It may contain any number of *arguments* that are separated with the pipe character `|`.
- The first argument specifies the item count. It is a positive number (e.g. `15`) or a number range of the form `2..5`. This argument is optional.
- Any further arguments start with a single identifying character that may be followed by any number of characters.
- It ends with a closing square bracket `]`.
- It may be followed by an optional question mark `?` to indicate that list formatting is optional. In this case, non-enumerable argument values will be formatted as specified while ignoring the list formatting options. (Without the optional marker, applying list formatting options to non-enumerable argument values will cause a `FormatException`.)

Multiple list format elements may be used to format nested enumerables.

### Item Count

If the item count is specified as a single integer number, it indicates the maximum number of items to output.
If the enumerable contains more items, a *more items marker* will be appended.

If the item count is specified as an integer range (e.g. `8..10`), the *upper bound* of the range indicates the maximum number of items to output.
If the enumerable contains more items, only the number of items specified as the *lower bound* of the range will be output.
Also, a *more items marker* will be appended.

If no item count is specified, all items from the enumeration will be output.

### Further Arguments

The following table lists additionally supported arguments along with their identifying characters.

| Identifying Character | Meaning |
| -- | -- |
| `a` | Specifies a string that is inserted *after* each item. |
| `b` | Specifies a string that is inserted *before* each item. |
| `d` | Specifies the delimiter to use between items of the enumerable. |
| `D` | Specifies an individual delimiter between items. Use *indexed values* (see below) to indicate which delimiter you would like to define, with the index referring to the delimiters between actually output items.<br />You may specify several `D` arguments to assign delimiters for different indexes. |
| `e` | If the enumerable contains no items, the alternative text from this argument will be used instead. |
| `m` | If the enumerable contains more items than can be displayed as per *Item Count*, this string will be appended after the items. |
| `r` | If the enumerable contains more items than can be displayed as per *Item Count*, this string will be appended as an additional item representing the remainder of the items. |

If any of these arguments appears multiple times, they will be considered in their order of occurrence.

### Indexed Values

An indexed value is specified in the form

    index[/condition]=value

In there:

- *index* is an integer number. Count the index from zero upwards to reference indexes from the start of the collection, and from `-1` downwards to start from the end.
- *condition* is an optional condition that checks the total number of items available in the source enumerable. The condition consists of one of the operators `<`, `>`, or `=` (if no operator is specified, `=` will be assumed), followed by an integer number that is compared to the total number of items.
- *value* is the resulting value for the specified index if the condition is met.

## Examples

### Sum

All items from the enumerable are output and joined with the specified delimiter.

```csharp
var format = "{0[d + ]}";

Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2 } }));
// 1 + 2
Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2, 4 } }));
// 1 + 2 + 4
Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2, 4, 8, 16 } }));
// 1 + 2 + 4 + 8 + 16
```

### Shortened List

The output list is shortened once it exceeds three items.

```csharp
var format = "{0[3|d;|m...]}";

Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2 } }));
// 1;2
Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2, 4 } }));
// 1;2;4
Console.WriteLine(FormatMore.Format(format,
  new object[] { new[] { 1, 2, 4, 8, 16 } }));
// 1;2;4...
```

### Text Enumeration

The enumeration is properly formatted for a natural language text (depending on which styleguide is used, but that question is out of scope here). Note how the case of two items is treated separately.

```csharp
var format = "My pizza topping consists of {0[2|d, |rothers|D0/2= and |D-1=, and ]}.";
var ingredients = new[] { "tomatoes", "mushrooms", "onions", "sliced sausages", "spinach leaves" };

Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(1) }));
// My pizza topping consists of tomatoes.
Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(2) }));
// My pizza topping consists of tomatoes and mushrooms.
Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(3) }));
// My pizza topping consists of tomatoes, mushrooms, and others.
Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(5) }));
// My pizza topping consists of tomatoes, mushrooms, and others.
```

### Authors List

Author names are formatted according to the conventions of some widespread styleguides for scientific publications. Up to three authors are spelt out, but for anything above, only the first author and the remark "et al." are output.

```csharp
var format = "{0[1..3|d, |D0/2= and |D-1=, and |m et al.]}";
(string GivenName, string Surname)[] authors =
{
    ("Eddie", "Example"),
    ("Sue", "Sample"),
    ("Bert", "Beispiel"),
    ("Eduardo", "Ejemplo")
};
var formattedAuthors = authors.Select(a => $"{a.GivenName[0]}. {a.Surname}");

Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(1) }));
// E. Example
Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(2) }));
// E. Example and S. Sample
Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(3) }));
// E. Example, S. Sample, and B. Beispiel
Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(4) }));
// E. Example et al.
```

### Logical Formula in DNF

```csharp
var format = "{0[d or |b(|a)][d and ]}";
var terms = new[] { new[] { "A", "not B", "C" }, new[] { "not A", "B", "not C", "D" } };

Console.WriteLine(FormatMore.Format(format, new object[] { terms }));
// (A and not B and C) or (not A and B and not C and D)
```

## License

The *FormatMore* library is published under the terms of the MIT license.