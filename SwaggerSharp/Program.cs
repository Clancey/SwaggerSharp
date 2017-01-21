using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SwaggerSharp
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new HttpClient();

			//var url = "http://theapistack.com/data/7digital/7digital-basket-api-swagger.json";
			//var url = "http://petstore.swagger.io/v2/swagger.json";
			//var url = "https://docs.botframework.com/en-us/restapi/directline/swagger.json";
			var url = "https://docs.botframework.com/en-us/restapi/directline3/swagger.json";
			var file =
				"C:\\Projects\\api-stack\\data\\environmental-protection-agency\\epa-safe-drinking-water-information-system-api-swagger.json";
			//var findDirectory = "C:\\Projects\\api-stack\\data";

			//var baseDirectory = Directory.GetCurrentDirectory();
			//var validDirectory = Path.Combine(baseDirectory, "Valid");
			//var validNoDefinitionsDirectory = Path.Combine(baseDirectory, "Valid-MissingDefinitions");
			//var invalidDirectory = Path.Combine(baseDirectory, "Invalid");
			//var generatedDirectory = Path.Combine(baseDirectory, "Generated");
			//ClearDirectory(validDirectory);
			//ClearDirectory(invalidDirectory);
			//ClearDirectory(generatedDirectory);
			//ClearDirectory(validNoDefinitionsDirectory);
			//         foreach (var enumerateFile in Directory.EnumerateFiles(findDirectory,"*.json",SearchOption.AllDirectories))
			//{
			//	if (enumerateFile.IndexOf("swagger", StringComparison.CurrentCultureIgnoreCase)<=0)
			//		continue;
			//	try
			//	{
			//		var hasDefinitions = ParseFile(enumerateFile,generatedDirectory);
			//		File.Copy(enumerateFile, Path.Combine(hasDefinitions ? validDirectory : validNoDefinitionsDirectory, Path.GetFileName(enumerateFile)),true);
			//	}
			//	catch (Exception)
			//	{
			//		File.Copy(enumerateFile, Path.Combine(invalidDirectory, Path.GetFileName(enumerateFile)), true);
			//	}

			//}


			var json = client.GetStringAsync(url).Result;
			//var json = File.ReadAllText(file);
			var settings = new JsonSerializerSettings
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
			};

			var objectTree = Newtonsoft.Json.JsonConvert.DeserializeObject<SwaggerResponse>(json, settings);
			objectTree.FillReferences();
			var codeGen = new CodeGenerator();
			codeGen.CreateApi(objectTree).Wait();
		}

		static void ClearDirectory(string directory)
		{
			if(Directory.Exists(directory))
				Directory.Delete(directory,true);
			Thread.Sleep(1000);
			Directory.CreateDirectory(directory);
		}
		static bool ParseFile(string fileName, string generatedDirectory = null)
		{

			var json = File.ReadAllText(fileName);
			var settings = new JsonSerializerSettings
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
			};

			var objectTree = Newtonsoft.Json.JsonConvert.DeserializeObject<SwaggerResponse>(json, settings);
			objectTree.FillReferences();
			var codeGen = new CodeGenerator();
			if(!string.IsNullOrWhiteSpace(generatedDirectory))
				codeGen.OutputDirectory = generatedDirectory;
			codeGen.CreateApi(objectTree).Wait();
			return objectTree.SecurityDefinitions.Any() && objectTree.Definitions.Any();
			
			return objectTree.Definitions.Any();
		}
	}
}
