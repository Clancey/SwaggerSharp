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
		public string DefaultNameSpace { get; set; } = "Swagger";
		CodeNamespace space;
		public async Task CreateApi(SwaggerResponse swaggerResponse)
		{

			var target = new CodeCompileUnit();
			space = new CodeNamespace(DefaultNameSpace);
			target.Namespaces.Add(space);

			foreach (var def in swaggerResponse.Definitions)
			{
				CreateClass(def.Value);
			}

			var api = swaggerResponse.SecurityDefinitions.FirstOrDefault();
			if(api.Value != null)
				api.Value.Name = api.Key;
			var title = CleanseName(swaggerResponse.Info.Title);
			var apiClassName = $"{title}Api";
			CreateApiClass(apiClassName, swaggerResponse, api.Value);
		
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
				//@ is added to all default types :(
				contents = contents.Replace("@","");
			}


			var tw = File.CreateText(Path.Combine(output, $"{apiClassName}.cs"));
			tw.Write(contents);
			tw.Close();
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
			AddPathsToApi(targetClass,swaggerResponse,api);
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

		void AddPathsToApi(CodeTypeDeclaration targetClass, SwaggerResponse swaggerResponse, SecurityDefinition def)
		{
			foreach (var path in swaggerResponse.Paths)
			{
				var pathvalue = path.Key;
				foreach (var pathDescription in path.Value)
				{
					var included = !pathDescription.Value.Security.Any() || pathDescription.Value.Security.Any(x=> x.ContainsKey(def.Name));
					if (!included)
						continue;
					AddPathToApi(targetClass,pathvalue,pathDescription,swaggerResponse);
				}

			}
		}

		void AddPathToApi(CodeTypeDeclaration targetClass, string pathAttribute,KeyValuePair<string,PathDescription> path,
			SwaggerResponse swaggerResponse)
		{
			var member = new CodeMemberMethod
			{
				Attributes = MemberAttributes.Public,
				Name = UpperCaseFirst(path.Value.OperationId),
				CustomAttributes =
				{
					new CodeAttributeDeclaration("Path",new CodeAttributeArgument {Value = new CodePrimitiveExpression(pathAttribute)})
				}
			};

			string returnType = "Task";
			foreach (var response in path.Value.Responses.Where(x=> x.Key == "200" || x.Key == "default" ))
			{
				var responseType = GetTypeFromPropertyType(response.Value.Schema);
				if (responseType != null)
				{
					returnType = $"Task<{responseType}>";
					break;
				}
			}
			member.ReturnType = new CodeTypeReference(returnType);
			if (pathAttribute == "/user/createWithArray")
			{
				Console.WriteLine("foo");
			}
			foreach (var parameter in path.Value.Parameters.OrderByDescending(x=> x.Required))
			{
				var type = GetTypeFromPropertyType(parameter.Type == null ? parameter.Schema : parameter, !parameter.Required);
				var name = !parameter.Required ? $"{parameter.Name} = null" : parameter.Name;
                member.Parameters.Add(new CodeParameterDeclarationExpression(type,name));
			}


			targetClass.Members.Add(member);

		}


		void CreateClass(SchemeObject swaggerObject)
		{

			//var baseClass = GetApiType(api);
			if (swaggerObject.Type != "object")
			{

				throw new Exception($"Unknown definitio type: {swaggerObject.Name} - {swaggerObject.Type}");
			}
			var baseClass = swaggerObject.AllOf.Select(x => x.SchemeObject).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.Discriminator));
			var targetClass = new CodeTypeDeclaration(swaggerObject.Name);
			if (baseClass != null)
				targetClass.BaseTypes.Add(new CodeTypeReference(baseClass.Name));

			targetClass.Attributes = MemberAttributes.Public;
			targetClass.IsPartial = true;
			AddPropertiesToObject(targetClass, swaggerObject);
			foreach (var reference in swaggerObject.AllOf?.Where(x => x.SchemeObject != baseClass))
			{
				AddPropertiesToObject(targetClass, reference.SchemeObject);
			}

			space.Types.Add(targetClass);
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

		static string GetTypeFromPropertyType(PropertyType property, bool nullable = false)
		{
			switch (property?.Type)
			{
				case "integer":
					return nullable ? "?int" : "int";
				case "long":
					return nullable ? "?long" : "long";
				case "float":
					return nullable ? "?float" : "float";
				case "double":
					return nullable ? "?double" : "double";
				case "string":
				case "password":
					return "string";
				case "byte":
				case "binary":
				case "file":
					return "byte[]";
				case "boolean":
					return nullable ? "?bool" : "bool";
				case "date":
				case "dateTime":
					return nullable ? "?DateTime" : "DateTime";
				case "array":
					var arraytype = GetTypeFromPropertyType(property.Items) ?? property.Items.SchemeObject?.Name;
					return $"{arraytype}[]";
			}
			if (property?.SchemeObject != null)
			{
				return property.SchemeObject.Name;
			}
			Console.WriteLine("Should not happen!!!");
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
