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

using FormatMoreUtilities;

{
	var format = "{0[d + ]}";

	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2 } }));
	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2, 4 } }));
	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2, 4, 8, 16 } }));
}

{
	var format = "{0[3|d;|m...]}";

	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2 } }));
	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2, 4 } }));
	Console.WriteLine(FormatMore.Format(format, new object[] { new[] { 1, 2, 4, 8, 16 } }));
}

{
	var format = "My pizza topping consists of {0[2|d, |rothers|D0/2= and |D-1=, and ]}.";
	var ingredients = new[] { "tomatoes", "mushrooms", "onions", "sliced sausages", "spinach leaves" };

	Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(1) }));
	Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(2) }));
	Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(3) }));
	Console.WriteLine(FormatMore.Format(format, new object[] { ingredients.Take(5) }));
}

{
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
	Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(2) }));
	Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(3) }));
	Console.WriteLine(FormatMore.Format(format, new object[] { formattedAuthors.Take(4) }));
}
