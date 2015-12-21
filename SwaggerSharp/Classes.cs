using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SwaggerSharp
{

	public class SwaggerResponse
	{
		public string Swagger { get; set; }
		public Info Info { get; set; }
		public string Host { get; set; }
		public string BasePath { get; set; }
		public Tag[] Tags { get; set; } = new Tag[0];
		public string[] Schemes { get; set; } = new string[0];
		public Dictionary<string,Dictionary<string,PathDescription>> Paths { get; set; } = new Dictionary<string, Dictionary<string, PathDescription>>();
		public Dictionary<string,SecurityDefinition> SecurityDefinitions { get; set; } = new Dictionary<string, SecurityDefinition>();
		public Dictionary<string, SchemeObject> Definitions { get; set; } = new Dictionary<string, SchemeObject>();
		public Externaldocs ExternalDocs { get; set; }

		public SchemeObject GetFromRef(string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
				return null;
			try
			{
				var obj = reference.Split('/').LastOrDefault();
				return string.IsNullOrWhiteSpace(obj) ? null : Definitions[obj];
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return null;
		}

		public void FillReferences()
		{
			foreach (var def in Definitions)
			{
				def.Value.Name = def.Key;
				foreach (var prop in def.Value.Properties)
				{
					FillPropertyType(prop.Value);
				}
				foreach (var reference in def.Value.AllOf.Where(x=> !string.IsNullOrWhiteSpace(x?.Ref)))
				{
					FillPropertyType(reference);
				}
			}
			foreach (var value in Paths.SelectMany(path => path.Value.Values))
			{
				foreach (var response in value.Responses)
				{
					FillPropertyType(response.Value.Schema);
				}
				foreach (var parameter in value.Parameters)
				{
					FillPropertyType(parameter.Schema);
				}
			}
		}

		void FillPropertyType(Reference reference)
		{
			if (!string.IsNullOrWhiteSpace(reference?.Ref))
				reference.SchemeObject = GetFromRef(reference.Ref);
			var property = reference as PropertyType;
			if (!string.IsNullOrWhiteSpace(property?.Items?.Ref))
				property.Items.SchemeObject = GetFromRef(property.Items.Ref);
		}
	}

	public class Info
	{
		public string Description { get; set; }
		public string Version { get; set; }
		public string Title { get; set; }
		public string TermsOfService { get; set; }
		public Contact Contact { get; set; }
		public License License { get; set; }
	}

	public class Tag
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public Externaldocs  ExternalDocs { get; set; }
	}


	public class Externaldocs
	{
		public string Description { get; set; }
		public string Url { get; set; }
	}

	public enum SecurityType
	{
		NoAuth,
		Oauth2,
		ApiKey,
		Basic,
		OauthApiKey,
	}

	[JsonConverter((typeof(DefualtEnumConverter)))]
	public enum AuthLocation
	{
		Query,
		Header
	}
    public class SecurityDefinition
	{
		public SecurityType Type { get; set; }

		public string Description { get; set; }

		public string SecurityName { get; set; }

		[JsonProperty("Name")]
		public string ApiKey { get; set; }

	    public string ApikeyName { get; set; } = "";

		public AuthLocation In { get; set; }

		public string Flow { get; set; }

		public string AuthorizationUrl { get; set; }
		public string TokenUrl { get; set; }
		public Dictionary<string,string> Scopes { get; set; } 

    }

	[JsonObject(IsReference = true)]
	public class SchemeObject
	{
		public string Name { get; set; }

		public string Type { get; set; } = "object";

		public string Discriminator { get; set; }

		public Dictionary<string, PropertyType> Properties { get; set; }

		public string[] Required { get; set; } = new string[0];

		public Reference[] AllOf { get; set; } = new Reference[0];
	}

	public class PropertyType : Reference
	{
		public string Type { get; set; }

		public string Format { get; set; }

		public string Description { get; set; }

		public string Default { get; set; }

		public string Maximum { get; set; }

		public string ExclusiveMaximum { get; set; }

		public string Minimum { get; set; }

		public string ExclusiveMinimum { get; set; }

		public int MaxLength { get; set; }

		public int MinLength { get; set; }

		public string Pattern { get; set; }

		public int MaxItems { get; set; }

		public int MinItems { get; set; }

		public bool UniqueItems { get; set; }

		public bool Required { get; set; }

		public PropertyType Items { get; set; }
	}

	public class Reference
	{

		[Newtonsoft.Json.JsonProperty("$ref")]
		public string Ref { get; set; }

		public SchemeObject SchemeObject { get; set; }
	}

	public class PathDescription
	{
		public string[] Tags { get; set; } = new string[0];
		public string Summary { get; set; }
		public string Description { get; set; }
		public string OperationId { get; set; }
		public string[] Consumes { get; set; } = new string[0];
		public string[] Produces { get; set; } = new string[0];
		public Parameter[] Parameters { get; set; } = new Parameter[0];
		public Dictionary<string,Response> Responses { get; set; } = new Dictionary<string, Response>();
		public Dictionary<string,string[]>[] Security { get; set; } = new Dictionary<string, string[]>[0];

	}

	public class Contact
	{
		public string Email { get; set; }

		public string Url { get; set; }
		
		public string Name { get; set; }
	}

	public class License
	{
		public string Name { get; set; }
		public string Url { get; set; }
	}
	
	[JsonConverter(typeof(DefualtEnumConverter))]
	public enum ParameterLocation
	{
		Query,
		Header,
		Path,
		FormData,
		Body
	}

	public class Parameter : PropertyType
	{
		public ParameterLocation In { get; set; }
		public string Name { get; set; }

		public string Description { get; set; }

		public bool Required { get; set; }

		public PropertyType Schema { get; set; }

		//TODO:
		//public Item[] Items { get; set; }

		public string CollectionFormat { get; set; }
	}

	public class Response
	{
		public string Description { get; set; }

		public PropertyType Schema { get; set; }

		public Dictionary<string,Parameter> Headers { get; set; }

		//public Dictionary<string, > Examples = new	Dictionary<string,???>(); 
	}

	public class DefualtEnumConverter : StringEnumConverter
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (string.IsNullOrWhiteSpace(reader.Value.ToString()))
			{
				return Enum.Parse(objectType, "0");
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}
	}
}
