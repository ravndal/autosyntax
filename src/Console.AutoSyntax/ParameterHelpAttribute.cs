using System;

namespace Console.AutoSyntax
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ParameterHelpAttribute : Attribute
	{
		public string ParameterName { get; set; }
		public string Description { get; set; }
		public string Usage { get; set; }

		internal void ShowHelp()
		{
			
			if (!string.IsNullOrEmpty(Description))
			{
				$"\t\t\"{Description}\"".Out(ConsoleColor.Red);
			}
			if (!string.IsNullOrEmpty(Usage))
			{
				$"\t\t\"Usage: {Usage}\"".Out(ConsoleColor.Red);
			}
		}
	}
}