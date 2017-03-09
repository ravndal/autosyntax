namespace Console.AutoSyntax
{
	public static class ConsoleAutoSyntax
	{
		public static char ParameterSwitch { get; set; } = '-';

		public static bool EnableColors { get; set; } = true;

		public static void Apply<TClass>(TClass obj, string[] args) where TClass : class
		{
			ConsoleInputParser.Run(obj, args);
		}
	}
}