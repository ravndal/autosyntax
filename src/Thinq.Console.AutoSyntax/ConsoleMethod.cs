using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Thinq.Console.AutoSyntax
{
	internal class ConsoleMethod
	{

		public ConsoleMethod(MethodInfo methodInfo)
		{
			Name = methodInfo.Name.Substring(0, 1).ToLower() + methodInfo.Name.Substring(1);
			IsAsync = methodInfo.ReturnType == typeof(Task) || methodInfo.ReturnType.Name == typeof(Task<>).Name;
			Method = methodInfo;
			Parameters = methodInfo.GetParameters().Select(p => new ConsoleParameter(p));
		}

		public string Name { get; set; }
		public IEnumerable<ConsoleParameter> Parameters { get; set; }
		public bool IsAsync { get; set; }
		public MethodInfo Method { get; set; }

		public bool CanRun(IList<string> parameterNames)
		{
			if (!Parameters.Any())
				return true;

			if (parameterNames.Count == 0)
				return false;

			var requriedParameters = Parameters.Where(x => !x.IsOptional).Select(p => p.Name.ToLower());
			return requriedParameters.All(parameterNames.Contains);
		}

		public void Execute<TInstance>(TInstance instance, IDictionary<string, string> values) where TInstance : class
		{
			var list = Parameters.Select(parameter => parameter.ParseValue(values)).Cast<object>().ToList();

			if (IsAsync)
			{
				var task = (Task)Method.Invoke(instance, list.ToArray());
				task.Wait();
			}
			else
			{
				Method.Invoke(instance, list.ToArray());
			}
		}

		public void ShowSyntax()
		{
			var attrs = Method.GetCustomAttributes<ParameterHelpAttribute>().ToList();

			$"{Name}".Out(ConsoleColor.Magenta);

			foreach (var parameter in Parameters)
			{
				$"\t{ConsoleAutoSyntax.ParameterSwitch}{parameter.Name} | ".OutPartial(parameter.IsOptional ? ConsoleColor.DarkCyan : ConsoleColor.Cyan);

				if (parameter.ParserType == ParameterType.Switch)
				{
					$"[switch]".OutPartial();
				}
				else
				{

					if (parameter.ParserType == ParameterType.CsvValues)
						$"List<{parameter.Type.GetGenericArguments()[0].Name.ToLower()}>".OutPartial();
					else
						parameter.Type.Name.OutPartial();
				}

				if(!parameter.IsOptional)
					" [required]".OutPartial(ConsoleColor.Green);

				$" // e.g: -{parameter.Name}".OutPartial(ConsoleColor.DarkGray);
				switch (parameter.ParserType)
				{
					case ParameterType.Value:
						":value".OutPartial(ConsoleColor.DarkGray);
						break;
					case ParameterType.CsvValues:
						":val1,val2,val3".OutPartial(ConsoleColor.DarkGray);
						break;
				}
				"".Out();

				var attr = attrs.FirstOrDefault(a => a.ParameterName == parameter.Name);
				if (attr != default(ParameterHelpAttribute))
					attr.ShowHelp();

			}
			"".Out();
		}
	}
}