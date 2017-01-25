using System;
using System.Reflection;

namespace SwaggerSharp
{
	public static class Extensions
	{
		public static void CopyPropertiesFrom(this object destination, object source)
		{
			if (source == null || destination == null)
				throw new Exception("Source or/and Destination Objects are null");
			
			Type typeDest = destination.GetType();
			Type typeSrc = source.GetType();

			PropertyInfo[] srcProps = typeSrc.GetProperties();
			foreach (PropertyInfo srcProp in srcProps)
			{
				if (!srcProp.CanRead)
				{
					continue;
				}
				PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
				if (targetProperty == null)
				{
					continue;
				}
				if (!targetProperty.CanWrite)
				{
					continue;
				}
				if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
				{
					continue;
				}
				if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
				{
					continue;
				}
				if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
				{
					continue;
				}

				targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
			}
		}
	}
}
