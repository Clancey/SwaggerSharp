using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.CSharp;

namespace SwaggerSharp
{
	class CodeGenerator
	{
		CodeNamespace space;
		public async Task CreateApi(SwaggerResponse swaggerResponse)
		{

			var target = new CodeCompileUnit();
			space = new CodeNamespace("Swagger");
			target.Namespaces.Add(space);

			foreach (var def in swaggerResponse.Definitions)
			{
				CreateClass(def.Value);
			}

			var api = swaggerResponse.SecurityDefinitions.FirstOrDefault().Value;
			
			var title = CleanseName(swaggerResponse.Info.Title);
			var apiClassName = $"{title}Api";
			CreateApiClass(apiClassName, swaggerResponse, api);
			var baseClass = GetApiType(api);
			var targetClass = new CodeTypeDeclaration(apiClassName);
				targetClass.BaseTypes.Add(new CodeTypeReference(baseClass));

			targetClass.IsClass = baseClass.IsClass;
			targetClass.Attributes = MemberAttributes.Public;
			AddConstructors(targetClass, baseClass, api);
			//AddCodeProperties(targetClass, theClass);
			//AddCodeMethods(targetClass, theClass);
			
			space.Types.Add(targetClass);
			var output = Directory.GetCurrentDirectory();
            var csProvider = new CSharpCodeProvider();

			//Write to memory, then clean up ugly code from autogen properties
			var memStream = new MemoryStream();
			var streamWriter = new StreamWriter(memStream);
			
			csProvider.GenerateCodeFromCompileUnit(target, streamWriter, null);
			streamWriter.Flush();
			memStream.Position = 0;
			string contents;
			using (var reader = new StreamReader(memStream))
			{
				contents = reader.ReadToEnd();
				contents = contents.Replace("//;", "");
			}


			var tw = File.CreateText(Path.Combine(output, $"{apiClassName}.cs"));
			tw.Write(contents);
			tw.Close();
		}

		void CreateClass(SchemeObject swaggerObject)
		{

			//var baseClass = GetApiType(api);
			if (swaggerObject.Type != "object")
			{
				
				throw new Exception($"Unknown definitio type: {swaggerObject.Name} - {swaggerObject.Type}");
			}
			var baseClass = swaggerObject.AllOf.Select(x=> x.SchemeObject).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.Discriminator));
			var targetClass = new CodeTypeDeclaration(swaggerObject.Name);
			if(baseClass != null)
				targetClass.BaseTypes.Add(new CodeTypeReference(baseClass.Name));
			
			targetClass.Attributes = MemberAttributes.Public;
			targetClass.IsPartial = true;
			AddPropertiesToObject(targetClass, swaggerObject);
			foreach (var reference in swaggerObject.AllOf?.Where(x=> x.SchemeObject != baseClass))
			{
				AddPropertiesToObject(targetClass, reference.SchemeObject);
			}

			space.Types.Add(targetClass);
		}
		void CreateApiClass(string apiClassName, SwaggerResponse swaggerResponse, SecurityDefinition api)
		{

			var baseClass = GetApiType(api);
			var targetClass = new CodeTypeDeclaration(apiClassName);
			targetClass.BaseTypes.Add(new CodeTypeReference(baseClass));

			targetClass.IsClass = baseClass.IsClass;
			targetClass.IsPartial = true;
			targetClass.Attributes = MemberAttributes.Public;
			AddConstructors(targetClass, baseClass, api);
			//AddCodeProperties(targetClass, theClass);
			//AddCodeMethods(targetClass, theClass);

			space.Types.Add(targetClass);
		}

		void AddConstructors(CodeTypeDeclaration targetClass, Type theClass, SecurityDefinition def)
		{
			var constructors = theClass.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
			constructors.AddRange(theClass.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
			if (theClass == typeof (SimpleAuth.Api))
			{
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"identifier = null"),
						new CodeParameterDeclarationExpression(typeof(HttpMessageHandler),"handler = null"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
                        new CodeArgumentReferenceExpression("handler")
					}
				};
				targetClass.Members.Add(constructor);
				return;
			}

			if (theClass == typeof(SimpleAuth.BasicAuthApi))
			{
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"identifier"),
						new CodeParameterDeclarationExpression(typeof(string),"loginUrl"),
						new CodeParameterDeclarationExpression(typeof(HttpMessageHandler),"handler = null"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("loginUrl"),
						new CodeArgumentReferenceExpression("handler")
					}
				};
				targetClass.Members.Add(constructor);
				return;
			}


			if (theClass == typeof(SimpleAuth.ApiKeyApi))
			{
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"apiKey"),
						new CodeParameterDeclarationExpression(typeof(HttpMessageHandler),"handler = null"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("apiKey"),
						new CodeArgumentReferenceExpression($"\"{def.Name}\""),
						new CodeArgumentReferenceExpression($"AuthLocation.{def.In.ToString("G")}"),
						new CodeArgumentReferenceExpression("handler")
					}
				};
				targetClass.Members.Add(constructor);
				return;
			}


			if (theClass == typeof(SimpleAuth.OAuthApi))
			{
				//TODO fix and chage this to OAuthApi
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"identifier = null"),
						new CodeParameterDeclarationExpression(typeof(HttpMessageHandler),"handler = null"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("handler")
					}
				};
				targetClass.Members.Add(constructor);
				return;
			}


		}

		void AddPropertiesToObject(CodeTypeDeclaration targetClass, SchemeObject schemeObject)
		{
			foreach (var member in from propertyType in schemeObject.Properties let type = GetPropertyType(propertyType.Value) select new CodeMemberField
			{
				//Codedom does not support pretty auto properties...
				Name = $"{UpperCaseFirst(propertyType.Key)} {{get; set;}}//",
				Attributes = MemberAttributes.Public,
				Type = type,
			})
			{
				targetClass.Members.Add(member);
			}
		}

		static CodeTypeReference GetPropertyType(PropertyType property)
		{

			var type = GetTypeFromPropertyType(property);
			if(type != null)
				return new CodeTypeReference(type);
			if (property.SchemeObject != null)
			{
				return new CodeTypeReference(property.SchemeObject.Name);
			}

			if (property.Type == "array")
			{
				var schemeObject = property.Items.SchemeObject?.Name;
				if (!string.IsNullOrWhiteSpace(schemeObject))
				{
					return new CodeTypeReference($"{schemeObject}[]");
				}
			}
			Console.WriteLine("Should not happen!!!");
			return new CodeTypeReference(typeof (string));
		}

		static Type GetTypeFromPropertyType(PropertyType property)
		{
			switch (property.Type)
			{
				case "integer":
					return typeof(int);
				case "long":
					return typeof(long);
				case "float":
					return typeof(float);
				case "double":
					return typeof(double);
				case "string":
				case "password":
					return typeof(string);
				case "byte":
				case "binary":
					return typeof(byte[]);
				case "boolean":
					return typeof(bool);
				case "date":
				case "dateTime":
					return typeof(DateTime);
				case "array":
					var arraytype = GetTypeFromPropertyType(property.Items);
					if (arraytype == null)
					{
						return null;
					}
					return Array.CreateInstance(arraytype, 0).GetType();
			}
			return null;
		}
		Type GetApiType(SecurityDefinition def)
		{
			return typeof(SimpleAuth.Api);
			switch (def?.Type)
			{
				case SecurityType.ApiKey:
					return typeof(SimpleAuth.ApiKeyApi);
				case SecurityType.Basic:
					return typeof(SimpleAuth.BasicAuthApi);
				case SecurityType.Oauth2:
					return typeof(SimpleAuth.OAuthApi);
			}
			
			return typeof(SimpleAuth.Api);
		}

		static string CleanseName(string name)
		{
			var strings = name.Split(' ');
			return strings.Aggregate("", (current, s) => current + UpperCaseFirst(s));
		}

		static string UpperCaseFirst(string s)
		{
			if(string.IsNullOrWhiteSpace(s?.Trim()))
				return s;
			var a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
	}
}
