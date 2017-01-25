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
	public class CodeGeneratorReport
	{
		public string OutputFile { get; set; }
		public List<CodeGenerationError> Errors { get; set; } = new List<CodeGenerationError>();
		public List<CodeGenerationError> Warnings { get; set; } = new List<CodeGenerationError>();

		public string GeneratePrettyReportOutput()
		{
			var sb = new StringBuilder();
			sb.AppendLine("========================");
			sb.AppendLine("========================");
			sb.AppendLine("Overview");
			sb.AppendLine("");
			sb.AppendLine($"Output File: {OutputFile}");
			sb.AppendLine($"Error Count: {Errors.Count}");
			sb.AppendLine($"Warnings Count: {Warnings.Count}");
			sb.AppendLine("");
			sb.AppendLine("========================");
			sb.AppendLine("========================");
			if (Errors.Count > 0)
			{
				sb.AppendLine("");
				sb.AppendLine("");
				sb.AppendLine("========================");
				sb.AppendLine("========================");
				sb.AppendLine("<Errors>");
				sb.AppendLine("========================");
				sb.AppendLine("========================");

				foreach (var error in Errors)
				{
					sb.AppendLine($"\tError {error.Path}");
					if(!string.IsNullOrWhiteSpace(error.Message))
						sb.AppendLine($"\t\tMessage: {error.Message}");
				}

			}

			if (Warnings.Count > 0)
			{
				sb.AppendLine("");
				sb.AppendLine("");
				sb.AppendLine("========================");
				sb.AppendLine("========================");
				sb.AppendLine("<Warnings>");
				sb.AppendLine("========================");
				sb.AppendLine("========================");

				foreach (var warning in Warnings)
				{
					sb.AppendLine($"\tWarning {warning.Path}");
					if (!string.IsNullOrWhiteSpace(warning.Message))
						sb.AppendLine($"\t\tMessage: {warning.Message}");
				}
			}


			sb.AppendLine("========================");
			sb.AppendLine("========================");


			var s = sb.ToString();
			return s;
		}
	}

	public class CodeGenerationError
	{
		public string Message { get; set; }
		public string Path { get; set; }
		public object Data { get; set; }
	}
	class CodeGenerator
	{
		public string DefaultNameSpace { get; set; } = "Swagger";
		public string OutputDirectory { get; set; } = Directory.GetCurrentDirectory();


		CodeNamespace space;
		CodeGeneratorReport report;
		public CodeGeneratorReport CreateApi(SwaggerResponse swaggerResponse)
		{

			report = new CodeGeneratorReport();
			var target = new CodeCompileUnit();
			space = new CodeNamespace(DefaultNameSpace);
			space.Imports.Add(new CodeNamespaceImport("SimpleAuth"));
			space.Imports.Add(new CodeNamespaceImport("System.Net.Http"));
			space.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
			space.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

			target.Namespaces.Add(space);

			foreach (var def in swaggerResponse.Definitions)
			{
				try
				{
					CreateClass(def.Value);
				}
				catch (Exception ex)
				{
					report.Errors.Add(new CodeGenerationError { Data = def, Message = ex.Message });
				}
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

			var output = Path.Combine(OutputDirectory, $"{title}.cs");
			var tw = File.CreateText(output);
			tw.Write(contents);
			tw.Close();
			report.OutputFile = output;
			return report;
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
						new CodeParameterDeclarationExpression(typeof(string),"encryptionKey"),
						new CodeParameterDeclarationExpression(typeof(string),"loginUrl"),
						new CodeParameterDeclarationExpression(typeof(HttpMessageHandler),"handler = null"),
					},
					BaseConstructorArgs =
					{
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("encryptionKey"),
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
			var operationId = path.Value.OperationId;
			if (operationId == null)
			{

				report.Errors.Add(new CodeGenerationError { Data = path.Value, Message = $"{path.Key} OperationId is not set.", Path = pathAttribute });
				//Lets generate it from the path
				var parts = pathAttribute.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				var validParts = parts?.Where(x => !x.StartsWith("{"))?.Select(UpperCaseFirst)?.ToArray();
				operationId = validParts?.Count() > 0 ? string.Join("",validParts) : pathAttribute;
			}
			var betterName = CleanseName(operationId);
			if (betterName.IndexOf(path.Key, StringComparison.CurrentCultureIgnoreCase) < 0)
				betterName = UpperCaseFirst(path.Key) + betterName;
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
				try
				{
					responseType = response.Value.Schema == null ? null : GetTypeFromPropertyType(response.Value.Schema);
				}
				catch (Exception ex)
				{
					report.Errors.Add(new CodeGenerationError {Data = response, Message = $"{path.Key} Response Type: {ex.Message}", Path = $"{pathAttribute}" });
					//Default
					responseType = "string";
				}
				if (responseType != null)
				{
					returnType = $"Task<{responseType}>";
					break;
				}
			}
			member.ReturnType = new CodeTypeReference(returnType);
			var authentictated = path.Value.Security.Any(x=> x.ContainsKey(def.SecurityName));
			var QueryParameters = new Dictionary<string,string>();
			var HeaderParameters = new Dictionary<string, string>();
			var FormsDataParameters = new Dictionary<string, string>();
			string body = null;

			foreach (var parameter in path.Value.Parameters.OrderByDescending(x=> x.Required))
			{
				var type = GetTypeFromPropertyType(parameter.Type == null ? parameter.Schema : parameter, !parameter.Required);
				var defaultValue = string.IsNullOrWhiteSpace(parameter.Default) ? "null" : (type == "string" ? $"\"{parameter.Default}\"" : type == "bool" || type == "bool?" ? parameter.Default.ToLower() : parameter.Default);
				var cleanName = RemoveInvalidCharacters(parameter.Name);
				var name = !parameter.Required ? $"{cleanName} = {defaultValue}" : cleanName;
				member.Parameters.Add(new CodeParameterDeclarationExpression(type, name));
				switch (parameter.In)
				{
					case ParameterLocation.Body:
						body = parameter.Name;
						break;
					case ParameterLocation.FormData:
						FormsDataParameters[parameter.Name] = ValueToString(cleanName, type, !parameter.Required);
						break;
					case ParameterLocation.Query:
					case ParameterLocation.Path:
						QueryParameters[parameter.Name] = ValueToString(cleanName, type, !parameter.Required);
						break;
					case ParameterLocation.Header:
						HeaderParameters[parameter.Name] = ValueToString(cleanName, type, !parameter.Required);
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
			if (path.Key.ToLower() == "post" && body == null)
			{
				report.Warnings.Add(new CodeGenerationError { Data = path.Value, Message = "Posting without a body (Verify)", Path = pathAttribute });
			}
			else if (path.Key.ToLower() == "put" && body == null)
			{

				report.Errors.Add(new CodeGenerationError { Data = path.Value, Message = "Put without a body is not valid", Path = pathAttribute });
			}

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



		void CreateClass(SchemeObject swaggerObject, CodeTypeDeclaration parentClass = null)
		{
			//var baseClass = GetApiType(api);
			if (swaggerObject.Type != "object" && swaggerObject.Type != "array")
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
			if (swaggerObject.Items?.SchemeObject != null && swaggerObject.Items.Ref == null && parentClass == null)
			{
				AddPropertiesToObject(targetClass, swaggerObject.Items.SchemeObject);
			}

			//Validate
			if (targetClass.Members.Count == 0)
			{
				report.Warnings.Add(new CodeGenerationError { Data = swaggerObject, Message = $"{targetClass.Name} has no properties" });
			}
			if (parentClass != null)
			{
				parentClass.Members.Add(targetClass);
				report.Warnings.Add(new CodeGenerationError { Data = swaggerObject, Message = $"{targetClass.Name} is being nested inside {parentClass.Name}. Should this be a $ref?"});
			}
			else {
				space.Types.Add(targetClass);
			}
		}
		void AddPropertiesToObject(CodeTypeDeclaration targetClass, SchemeObject schemeObject)
		{
			if (schemeObject.Properties == null)
			{
				return;
			}
			var embeddedClasses = schemeObject.Properties.Where(x => (x.Value.Items?.SchemeObject != null && x.Value.Items.Ref == null) || x.Value.SchemeObject != null && x.Value.Ref == null).Select(x => x.Value.SchemeObject ?? x.Value.Items.SchemeObject).ToList();

			foreach (var c in embeddedClasses)
			{
				CreateClass(c, targetClass);
			}

			foreach (var propertyType in schemeObject.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Key))) {
				try
				{
					var type = GetPropertyType(propertyType.Value);
					var cleanName = CleanseName(propertyType.Key);
					var propName = cleanName != targetClass.Name ? cleanName : $"{cleanName}Value";
					var member = new CodeMemberField
					{
						//Codedom does not support pretty auto properties...
						CustomAttributes = {
						new CodeAttributeDeclaration("Newtonsoft.Json.JsonProperty",new CodeAttributeArgument(new CodeSnippetExpression($"\"{propertyType.Key}\"" ))),
					},
						Name = $"{propName} {{get; set;}}//",
						Attributes = MemberAttributes.Public,
						Type = type,
					};

					targetClass.Members.Add(member);
				}
				catch (Exception ex)
				{
					report.Errors.Add(new CodeGenerationError { Data = propertyType, Message = ex.Message, Path = $"{targetClass.Name} - {propertyType.Key}" });
				}
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
			return new CodeTypeReference(typeof(string));
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
				case "number":
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
					var arraytype = GetTypeFromPropertyType(property.Items) ?? CleanseName(property.Items.SchemeObject?.Name ?? property.Description);
					return $"{arraytype}[]";
			}
			if (property?.Enum?.Count() > 0)
			{
				return "string";
			}
			if (property?.SchemeObject != null)
			{
				return CleanseName(property.SchemeObject.Name);
			}
			throw new Exception("Unknown type");
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
			if (name == null)
				return null;
			var strings = name.Split(' ', '-', '_', '.');
			if (strings.Length == 1)
				return UpperCaseFirst(name);
			return strings.Aggregate("", (current, s) => current + UpperCaseFirst(s));
		}

		static string RemoveInvalidCharacters(string name)
		{
			if (name == "params")
				return "parameters";
			string[] invalidCharacters = new[] {
				"$",
				"\\",
				"/",
			};
			foreach (var c in invalidCharacters)
			{
				name = name.Replace(c, "");
			}
			return name;
		}

		static string UpperCaseFirst(string s)
		{
			if (string.IsNullOrWhiteSpace(s?.Trim()))
				return s;
			var a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
	}
}
