using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinq.Console.AutoSyntax
{
	internal static class ConsoleInputParser
	{
		public static void Run<TApp>(TApp instance, string[] args) where TApp : class
		{
			var consoleMethods = Parse<TApp>();
			if (args == null || !args.Any())
			{
				ShowSyntax(consoleMethods);
				return;
			}

			var method = consoleMethods.FirstOrDefault(p => p.Name == args[0]);
			if(method == null)
			{
				ShowSyntax(consoleMethods);
				return;
			}

			var parameters = ExtractParameterSets(args);
			if (!method.CanRun(parameters.Select(x => x.Key).ToList()))
			{
				var missing = String.Join($" {ConsoleAutoSyntax.ParameterSwitch}",method.Parameters.Where(p=>!p.IsOptional).Select(x => x.Name).Except(parameters.Select(p => p.Key)));
				$"Missing parameters: {ConsoleAutoSyntax.ParameterSwitch}{missing}".Out(ConsoleColor.Red);

				method.ShowSyntax();
				return;
			}

			method.Execute(instance, parameters);
		}

		private static void ShowSyntax(List<ConsoleMethod> consoleMethods)
		{
			"Invalid input!".Out(ConsoleColor.DarkYellow);

			foreach (var m in consoleMethods)
				m.ShowSyntax();
		}

		private static List<ConsoleMethod> Parse<TApp>()
		{
			var defaultMethods = typeof(object).GetMethods().Select(m => m.Name).ToList();
			var functions = typeof(TApp).GetMethods().Where(x => !defaultMethods.Contains(x.Name)).ToList();
			return functions.Select(f => new ConsoleMethod(f)).ToList();
		}

		private static Dictionary<string, string> ExtractParameterSets(string[] args)
		{
			var parameters = new Dictionary<string, string>();
			if (args.Length <= 1)
				return parameters;

			for (var i = 1; i < args.Length; i++)
			{
				var arg = args[i];
				if (string.IsNullOrEmpty(arg))
				{
					i++;
					continue;
				}

				if (arg[0] != ConsoleAutoSyntax.ParameterSwitch)
					continue;

				arg = arg.Substring(1);
				var next = i + 1;
				if (arg.Contains(':'))
				{
					var parts = arg.Split(':');
					parameters.Add(parts[0].ToLower(), parts[1]);
				}
				else if (args.Length > next && !string.IsNullOrEmpty(args[next]) && args[next][0] != ConsoleAutoSyntax.ParameterSwitch)
				{
					parameters.Add(arg.ToLower(), args[next]);
					i++;
				}
				else
				{
					parameters.Add(arg.ToLower(), null);
				}
			}
			return parameters;
		}


	}
}