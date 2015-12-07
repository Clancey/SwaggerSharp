using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SwaggerSharp
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new HttpClient();
			var json = client.GetStringAsync("http://petstore.swagger.io/v2/swagger.json").Result;
			var settings = new JsonSerializerSettings
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
			};

			var objectTree = Newtonsoft.Json.JsonConvert.DeserializeObject<SwaggerResponse>(json, settings);
			objectTree.FillReferences();
			var pet = objectTree.Definitions["Pet"];
			var petCat = pet.Properties["category"];
			var cat = objectTree.GetFromRef(petCat?.Ref);

			var codeGen = new CodeGenerator();
			codeGen.CreateApi(objectTree).Wait();
		}
	}
}
