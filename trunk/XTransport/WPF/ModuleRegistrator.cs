using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace XTransport.WPF
{
	public class ModuleRegistrator : DependencyObject
	{
		private static readonly Dictionary<Type, object> m_parameters = new Dictionary<Type, object>();
		private static readonly List<Type> m_moduleTypes = new List<Type>();
		private static readonly Dictionary<Type, AbstractModule> m_moduleInstances = new Dictionary<Type, AbstractModule>();

		public ModuleRegistrator()
		{
			m_parameters.Add(GetType(), this);
		}

		public void RegisterArgument<T>(T _value)
		{
			m_parameters.Add(typeof (T), _value);
		}

		public void RegisterArgument(Type _type, object _value)
		{
			if (!_type.IsAssignableFrom(_value.GetType()))
			{
				throw new ArgumentException("_value is not a " + _type.Name);
			}
			m_parameters.Add(_type, _value);
		}

		private static IEnumerable<Assembly> GetAssemblies(string _nameContains)
		{
			var results = new List<string>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.FullName.Contains(_nameContains))
				{
					results.Add(assembly.FullName);
					yield return assembly;
				}
			}
			foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "*" + _nameContains + "*.dll"))
			{
				var assembly = Assembly.LoadFile(file);
				if (!results.Contains(assembly.FullName))
				{
					results.Add(assembly.FullName);
					yield return assembly;
				}
			}
		}

		public void LoadAssembliesNamesContains(string _substring)
		{
			foreach (var assembly in GetAssemblies(_substring))
			{
				foreach (var type in assembly.GetTypes())
				{
					if (typeof (AbstractModule).IsAssignableFrom(type) && !type.IsAbstract)
					{
						m_moduleTypes.Add(type);
					}
				}
			}
		}

		public void RegisterModules()
		{
			var constructors = m_moduleTypes.ToDictionary(_type => _type, _type => _type.GetConstructors().First());
			while (constructors.Count > 0)
			{
				foreach (var pair in constructors)
				{
					var type = pair.Key;
					var constructorInfo = pair.Value;

					var constructorParametersValues = new List<object>();
					var flag = true;
					foreach (var parameterInfo in constructorInfo.GetParameters())
					{
						if (!m_parameters.ContainsKey(parameterInfo.ParameterType))
						{
							flag = false;
							break;
						}
						var value = m_parameters[parameterInfo.ParameterType];

						constructorParametersValues.Add(value);
					}
					if (!flag) continue;
					var instance = (AbstractModule) Activator.CreateInstance(type, constructorParametersValues.ToArray());
					m_moduleInstances.Add(type, instance);
					constructors.Remove(type);
					break;
				}
			}

			foreach (var module in m_moduleInstances.Values)
			{
				module.AllModulesRegistered(this);
			}
		}
	}
}