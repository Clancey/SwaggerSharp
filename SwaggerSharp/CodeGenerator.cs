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
		public string OutputDirectory { get; set; } = Directory.GetCurrentDirectory();

		CodeNamespace space;
		public async Task CreateApi(SwaggerResponse swaggerResponse)
		{

			var target = new CodeCompileUnit();
			space = new CodeNamespace(DefaultNameSpace);
			space.Imports.Add(new CodeNamespaceImport("SimpleAuth"));
			space.Imports.Add(new CodeNamespaceImport("System.Net.Http"));
			space.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
			space.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

			target.Namespaces.Add(space);

			foreach (var def in swaggerResponse.Definitions)
			{
				CreateClass(def.Value);
			}

			var title = CleanseName(swaggerResponse.Info.Title);
			var apis = swaggerResponse.SecurityDefinitions.ToList();
			if (apis.Count(x => x.Value.Type == SecurityType.ApiKey || x.Value.Type == SecurityType.Oauth2) == 2)
			{
				var oauth = apis.FirstOrDefault(x => x.Value.Type == SecurityType.Oauth2);
				var apikey = apis.FirstOrDefault(x => x.Value.Type == SecurityType.ApiKey);
				oauth.Value.ApiKey = apikey.Value.ApiKey;
				oauth.Value.ApikeyName = apikey.Key;
				oauth.Value.In = apikey.Value.In;
				oauth.Value.Type = SecurityType.OauthApiKey;
				apis.Remove(apikey);
			}
			if (!apis.Any())
			{
				apis.Add(new KeyValuePair<string, SecurityDefinition>("",new SecurityDefinition ()));
			}
			foreach (var api in apis)
			{
				if (api.Value != null)
					api.Value.SecurityName = api.Key;
				CreateApiClass(title, swaggerResponse, api.Value);
			}
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

		
			var tw = File.CreateText(Path.Combine(OutputDirectory, $"{title}.cs"));
			tw.Write(contents);
			tw.Close();
		}
		void CreateApiClass(string title, SwaggerResponse swaggerResponse, SecurityDefinition api)
		{

			var baseClass = GetApiType(api);
			var apiClassName = $"{title}{baseClass.Name}";
			var targetClass = new CodeTypeDeclaration(apiClassName);
			targetClass.BaseTypes.Add(new CodeTypeReference(baseClass));

			targetClass.IsClass = baseClass.IsClass;
			targetClass.IsPartial = true;
			targetClass.Attributes = MemberAttributes.Public;
			AddConstructors(targetClass, baseClass, api,swaggerResponse);
			AddPathsToApi(targetClass,swaggerResponse,api);
			//AddCodeProperties(targetClass, theClass);
			//AddCodeMethods(targetClass, theClass);

			space.Types.Add(targetClass);
		}

		void AddConstructors(CodeTypeDeclaration targetClass, Type theClass, SecurityDefinition def, SwaggerResponse swaggerResponse)
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
					},
					Statements =
					{
						CreateBaseUrl(swaggerResponse),
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
					},
					Statements =
					{
						CreateBaseUrl(swaggerResponse),
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
						new CodeArgumentReferenceExpression($"\"{def.ApiKey}\""),
						new CodeArgumentReferenceExpression($"AuthLocation.{def.In.ToString("G")}"),
						new CodeArgumentReferenceExpression("handler")
					},
					Statements =
					{
						CreateBaseUrl(swaggerResponse),
					}
				};
				targetClass.Members.Add(constructor);
				return;
			}


			if (theClass == typeof(SimpleAuth.OAuthApi))
			{
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"identifier"),
						new CodeParameterDeclarationExpression(typeof(string),"clientId"),
						new CodeParameterDeclarationExpression(typeof(string),"clientSecret"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("clientId"),
						new CodeArgumentReferenceExpression("clientSecret"),
					},
					Statements =
					{
						CreateBaseUrl(swaggerResponse),
					}
				};


				if (string.IsNullOrWhiteSpace(def.TokenUrl))
				{
					constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "tokenUrl"));
					constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("tokenUrl"));
				}
				else
					constructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"\"{def.TokenUrl}\""));

				if (string.IsNullOrWhiteSpace(def.AuthorizationUrl))
				{
					constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof (string), "authorizationUrl"));
					constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("authorizationUrl"));
				}
				else
					constructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"\"{def.AuthorizationUrl}\""));

				constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "redirectUrl = \"http://localhost\""));
				constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("redirectUrl"));
				constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof (HttpMessageHandler), "handler = null"));
				constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("handler"));

				targetClass.Members.Add(constructor);
				return;
			}



			if (theClass == typeof(SimpleAuth.OauthApiKeyApi))
			{
				var constructor = new CodeConstructor
				{
					Name = theClass.Name,
					Attributes = MemberAttributes.Public,
					Parameters =
					{
						new CodeParameterDeclarationExpression(typeof(string),"identifier"),
						new CodeParameterDeclarationExpression(typeof(string),"apiKey"),
						new CodeParameterDeclarationExpression(typeof(string),"clientId"),
						new CodeParameterDeclarationExpression(typeof(string),"clientSecret"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("apiKey"),
						new CodeArgumentReferenceExpression($"\"{def.ApiKey}\""),
						new CodeArgumentReferenceExpression($"AuthLocation.{def.In.ToString("G")}"),
						new CodeArgumentReferenceExpression("clientId"),
						new CodeArgumentReferenceExpression("clientSecret"),
					},
					Statements =
					{
						CreateBaseUrl(swaggerResponse),
					}
				};

				if (string.IsNullOrWhiteSpace(def.TokenUrl))
				{
					constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "tokenUrl"));
					constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("tokenUrl"));
				}
				else
					constructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"\"{def.TokenUrl}\""));

				if (string.IsNullOrWhiteSpace(def.AuthorizationUrl))
				{
					constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "authorizationUrl"));
					constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("authorizationUrl"));
				}
				else
					constructor.BaseConstructorArgs.Add(new CodeSnippetExpression($"\"{def.AuthorizationUrl}\""));

				constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "redirectUrl = \"http://localhost\""));
				constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("redirectUrl"));
				constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpMessageHandler), "handler = null"));
				constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("handler"));


				targetClass.Members.Add(constructor);
				return;
			}


		}

		CodeExpression CreateBaseUrl(SwaggerResponse response)
		{
			var scheme = response.Schemes.FirstOrDefault(x => x == "https") ?? response.Schemes.First();
			var url = $"{scheme}://{response.Host}";
			if (!url.EndsWith("/"))
				url += "/";
			var path = response.BasePath?.TrimStart('/') ?? "";
			if (!path.EndsWith("/"))
				path += "/";
			var uri = Path.Combine(url,path);
			return new CodeSnippetExpression($"BaseAddress = new System.Uri(\"{uri}\");");
        }

		void AddPathsToApi(CodeTypeDeclaration targetClass, SwaggerResponse swaggerResponse, SecurityDefinition def)
		{
			foreach (var path in swaggerResponse.Paths)
			{
				var pathvalue = path.Key;
				foreach (var pathDescription in path.Value)
				{
					var included = !pathDescription.Value.Security.Any() || pathDescription.Value.Security.Any(x=> x.ContainsKey(def.SecurityName) || x.ContainsKey(def.ApikeyName));
					if (!included)
						continue;
					AddPathToApi(targetClass,pathvalue,pathDescription,def);
				}

			}
		}

		void AddPathToApi(CodeTypeDeclaration targetClass, string pathAttribute,KeyValuePair<string,PathDescription> path,
			SecurityDefinition def)
		{
			//Always prepend operaton type to the operation name
			var betterName = CleanseName(path.Value.OperationId);
			//if (betterName.IndexOf(path.Key, StringComparison.CurrentCultureIgnoreCase) < 0)
			//	betterName = UpperCaseFirst(path.Key) + betterName;
            var member = new CodeMemberMethod
			{
				Attributes = MemberAttributes.Public,
				Name = betterName,
				CustomAttributes =
				{
					CreateCustomAttribute("Path",pathAttribute)
				}
			};

			//add accepts and content type look for json, since thats all the api handles right now
			var contentType = path.Value.Consumes.FirstOrDefault(x => x.Contains("json"));
			if (!string.IsNullOrWhiteSpace(contentType))
				member.CustomAttributes.Add(
					CreateCustomAttribute("ContentType", contentType));


			var accepts = path.Value.Produces.FirstOrDefault(x => x.Contains("json"));
			if (!string.IsNullOrWhiteSpace(accepts))
				member.CustomAttributes.Add(
					CreateCustomAttribute("Accepts", accepts));


			string returnType = "Task";
			string responseType = null;
			foreach (var response in path.Value.Responses.Where(x=> x.Key == "200" || x.Key == "default" ))
			{
				responseType = GetTypeFromPropertyType(response.Value.Schema);
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
			var authentictated = path.Value.Security.Any(x=> x.ContainsKey(def.SecurityName));
			var QueryParameters = new Dictionary<string,string>();
			var HeaderParameters = new Dictionary<string, string>();
			var FormsDataParameters = new Dictionary<string, string>();
			string body = null;

			foreach (var parameter in path.Value.Parameters.OrderByDescending(x=> x.Required))
			{
				var type = GetTypeFromPropertyType(parameter.Type == null ? parameter.Schema : parameter, !parameter.Required);
				var defaultValue = string.IsNullOrWhiteSpace(parameter.Default) ? "null" :( type == "string" ? $"\"{parameter.Default}\"" : parameter.Default);
				var name = !parameter.Required ? $"{parameter.Name} = {defaultValue}" : parameter.Name;
                member.Parameters.Add(new CodeParameterDeclarationExpression(type,name));
				switch (parameter.In)
				{
					case ParameterLocation.Body:
						body = parameter.Name;
						break;
					case ParameterLocation.FormData:
						FormsDataParameters[parameter.Name] = ValueToString(parameter.Name, type, !parameter.Required);
						break;
					case ParameterLocation.Query:
					case ParameterLocation.Path:
						QueryParameters[parameter.Name] = ValueToString(parameter.Name, type, !parameter.Required);
                        break;
					case ParameterLocation.Header:
						HeaderParameters[parameter.Name] = ValueToString(parameter.Name, type, !parameter.Required);
                        break;
				}
			}
			
			string queryCode = !QueryParameters.Any() ? null : $"var queryParameters = new Dictionary<string,string>{{ {string.Join(",",QueryParameters.Select(x=> $"{{ \"{x.Key}\" , {x.Value} }}"))} }}";
			if(queryCode != null)
				member.Statements.Add(new CodeSnippetExpression(queryCode));

			string headerCode = !HeaderParameters.Any() ? null : $"var headers = new Dictionary<string,string>{{ {string.Join(",", HeaderParameters.Select(x => $"{{ \"{x.Key}\" , {x.Value} }}"))} }}";
			if (headerCode != null)
				member.Statements.Add(new CodeSnippetExpression(headerCode));

			
			string formsCode = !FormsDataParameters.Any() ? null : $"var formsParameters = new Dictionary<string,string>{{ {string.Join(",", FormsDataParameters.Select(x => $"{{ \"{x.Key}\" , {x.Value} }}"))} }}";
			if (formsCode != null)
			{
				
				member.Statements.Add(new CodeSnippetExpression(formsCode));
				member.Statements.Add(new CodeSnippetExpression("var formsContent = new FormUrlEncodedContent(formsParameters);"));
				
			}

			var method = UpperCaseFirst(path.Key.ToLower());

			var returnStatement = CreateMethodCall(method, responseType, body, formsCode != null, queryCode != null,
				headerCode != null, authentictated);
			member.Statements.Add(returnStatement);
			targetClass.Members.Add(member);

		}

		static CodeAttributeDeclaration CreateCustomAttribute(string attribute, string value)
		{
			return new CodeAttributeDeclaration(attribute,
				new CodeAttributeArgument {Value = new CodePrimitiveExpression(value)});
		}

		static CodeExpression CreateMethodCall(string method, string responseType, string body, bool hasForms, bool hasQuery,
			bool hasHeaders, bool authenticated)
		{
			var methodCall = string.IsNullOrWhiteSpace(responseType) ? method : $"{method}<{responseType}>";

			var parameters = new List<string>();
			if(!string.IsNullOrWhiteSpace(body))
				parameters.Add(body);
			else if(hasForms)
				parameters.Add("formsContent");
			else if (method == "Post")
				parameters.Add("body: null");

			if(hasQuery)
				parameters.Add("queryParameters: queryParameters");
			if(hasHeaders)
				parameters.Add("headers: headers");
			
			parameters.Add($"authenticated: {authenticated.ToString().ToLower()}");

			var joined = string.Join(", ", parameters);

			var returnStatmenet = $"return {methodCall}( {joined} )";
			return new CodeSnippetExpression(returnStatmenet);

		}

		static string ValueToString(string name, string type, bool isNullable)
		{
			if (type == "string[]")
			{
				return $"string.Join(\",\",{name})";
			}
			return type == "string" ? name : isNullable ? $"{name}?.ToString()" : $"{name}.ToString()";
		}



		void CreateClass(SchemeObject swaggerObject)
		{

			//var baseClass = GetApiType(api);
			if (swaggerObject.Type != "object")
			{

				throw new Exception($"Unknown definition type: {swaggerObject.Name} - {swaggerObject.Type}");
			}
			var baseClass = swaggerObject.AllOf.Select(x => x.SchemeObject).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.Discriminator));
			var targetClass = new CodeTypeDeclaration(CleanseName(swaggerObject.Name));
			if (baseClass != null)
				targetClass.BaseTypes.Add(new CodeTypeReference(CleanseName(baseClass.Name)));

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
			foreach (var member in from propertyType in schemeObject.Properties.Where(x=> !string.IsNullOrWhiteSpace(x.Key)) let type = GetPropertyType(propertyType.Value) select new CodeMemberField
			{
				//Codedom does not support pretty auto properties...
				CustomAttributes =
				{
					
					new CodeAttributeDeclaration("Newtonsoft.Json.JsonProperty",new CodeAttributeArgument(new CodeSnippetExpression($"\"{propertyType.Key}\"" ))),
                },
				Name = $"{CleanseName(propertyType.Key)} {{get; set;}}//",
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
					return nullable ? "int?" : "int";
				case "long":
					return nullable ? "long?" : "long";
				case "float":
					return nullable ? "float?" : "float";
				case "double":
					return nullable ? "double?" : "double";
				case "string":
				case "password":
					return "string";
				case "byte":
				case "binary":
				case "file":
					return "byte[]";
				case "boolean":
					return nullable ? "bool?" : "bool";
				case "date":
				case "dateTime":
					return nullable ? "DateTime?" : "DateTime";
				case "array":
					var arraytype = GetTypeFromPropertyType(property.Items) ?? CleanseName(property.Items.SchemeObject?.Name);
					return $"{arraytype}[]";
			}
			if (property?.SchemeObject != null)
			{
				return CleanseName(property.SchemeObject.Name);
			}
			Console.WriteLine("Should not happen!!!");
			return null;
		}

		Type GetApiType(SecurityDefinition def)
		{
			switch (def?.Type)
			{
				case SecurityType.ApiKey:
					return typeof(SimpleAuth.ApiKeyApi);
				case SecurityType.Basic:
					return typeof(SimpleAuth.BasicAuthApi);
				case SecurityType.Oauth2:
					return typeof(SimpleAuth.OAuthApi);
				case SecurityType.OauthApiKey:
					return typeof (SimpleAuth.OauthApiKeyApi);
			}
			
			return typeof(SimpleAuth.Api);
		}

		static string CleanseName(string name)
		{
			var strings = name.Split(' ','-','_','.');
			if (strings.Length == 1)
				return UpperCaseFirst(name);
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
