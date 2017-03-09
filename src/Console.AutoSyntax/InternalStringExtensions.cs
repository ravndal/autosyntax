using System;

namespace Console.AutoSyntax
{
	internal static class InternalStringExtensions
	{


		public static void OutPartial(this string str, ConsoleColor? color = null)
		{
			Out(str, true, color);
		}

		public static void Out(this string str, ConsoleColor? color = null)
		{
			Out(str, false, color);
		}

		private static void Out(string str, bool partial, ConsoleColor? color)
		{
			if (color.HasValue)
				System.Console.ForegroundColor = color.Value;

			if (partial)
				System.Console.Write(str);
			else
				System.Console.WriteLine(str);
			System.Console.ResetColor();
		}
	}
}