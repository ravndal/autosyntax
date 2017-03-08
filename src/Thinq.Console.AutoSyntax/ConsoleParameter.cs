using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Thinq.Console.AutoSyntax
{
	internal class ConsoleParameter
	{
		private static readonly Type[] SupportedTypes =
		{
			typeof(string),
			typeof(int),
			typeof(decimal),
			typeof(double),
			typeof(bool),
			typeof(char),
			typeof(IEnumerable<string>),
			typeof(IEnumerable<int>),
			typeof(DateTime)
		};

		public string Name { get; set; }
		public object DefaultValue { get; set; }
		public Type Type { get; set; }
		public bool IsOptional { get; set; }
		public ParameterType ParserType { get; set; }
		public string Parser => ParserType.ToString();
		public object Value { get; set; }
		public bool HasValue => Value != DefaultValue;

		public ConsoleParameter(ParameterInfo parameterInfo)
		{
			if (!SupportedTypes.Contains(parameterInfo.ParameterType))
				throw new NotSupportedException($"The provided type '{parameterInfo.ParameterType.Name}' on parameter '{parameterInfo.Name}' is not supported");
			Name = parameterInfo.Name;
			Type = parameterInfo.ParameterType;
			ParserType = FindParserType();
			DefaultValue = parameterInfo.ParameterType == typeof(bool) ? false : parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : null;
			IsOptional = parameterInfo.IsOptional || ParserType == ParameterType.Switch;
		}

		private ParameterType FindParserType()
		{
			if (Type == typeof(string))
				return ParameterType.Value;

			if (Type.IsArray || Type.Name == typeof(IEnumerable<>).Name)
				return ParameterType.CsvValues;

			if (Type == typeof(bool))
				return ParameterType.Switch;

			return ParameterType.Value;
		}

		private static readonly string[] TrueValues = { "true", "yes", "1", "on" };

		public object ParseValue(IDictionary<string,string> values)
		{
			if (!values.ContainsKey(Name))
				return DefaultValue;

			var paramValue = values[Name];
			if (paramValue == null)
				return ParserType == ParameterType.Switch ? true : DefaultValue;

			try
			{
				switch (ParserType)
				{
					case ParameterType.Switch:
						return TrueValues.Contains(paramValue);

					case ParameterType.Value:
						return Type == typeof(string) ? paramValue : Convert.ChangeType(paramValue, Type);

					case ParameterType.CsvValues:

						var list = paramValue.Split(',').ToList();
						if (Type == typeof(IEnumerable<string>))
						{
							return list;
						}
						if (Type == typeof(IEnumerable<int>))
						{
							var res = new List<int>();
							foreach (var i in list)
								res.Add(int.Parse(i));

							return res;
						}
						return list;
				}
			}
			catch 
			{
				$"The value '{paramValue}' is invalid for option '{Name}' ".Out();
			}
			return DefaultValue;
		}
	}
}