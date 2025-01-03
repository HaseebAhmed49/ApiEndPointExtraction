using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        // Ask the user for the Swagger JSON file path
        Console.WriteLine("Please enter the path to your Swagger JSON file:");
        string jsonFilePath = Console.ReadLine();

        // Check if the file exists
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine("The specified file does not exist. Please check the path and try again.");
            return;
        }

        // Load the Swagger JSON file
        string jsonContent = File.ReadAllText(jsonFilePath);

        // Parse the Swagger JSON
        JObject swaggerData = JObject.Parse(jsonContent);

        // Dictionary to hold controllers and their endpoints
        var controllers = new Dictionary<string, List<Endpoint>>();

        // Iterate through the paths in the Swagger JSON
        foreach (var path in swaggerData["paths"])
        {
            string pathKey = path.Path;
            JObject methods = (JObject)path.First;

            foreach (var method in methods)
            {
                string methodType = method.Key.ToUpper();

                // Safely convert method.Value to JObject using ToObject<JObject>()
                var methodDetails = method.Value.ToObject<JObject>();

                if (methodDetails != null)
                {
                    // Extract controller from tags or default to "General"
                    var tags = methodDetails["tags"]?.ToObject<List<string>>() ?? new List<string> { "General" };
                    string controller = tags.Count > 0 ? tags[0] : "General";

                    // Create endpoint object
                    var endpoint = new Endpoint
                    {
                        Method = methodType,
                        Path = pathKey,
                        Description = methodDetails["summary"]?.ToString() ?? ""
                    };

                    // Group endpoints by controller
                    if (!controllers.ContainsKey(controller))
                    {
                        controllers[controller] = new List<Endpoint>();
                    }
                    controllers[controller].Add(endpoint);
                }
            }
        }

        // Generate formatted output for endpoints by controller
        string output = GenerateFormattedOutput(controllers);

        // Save the output to a file
        string outputFilePath = "Output/swagger_endpoints_by_controller.txt";
        File.WriteAllText(outputFilePath, output);

        Console.WriteLine($"Endpoints grouped by controllers have been saved to '{outputFilePath}'.");
    }

    // Method to generate formatted output
    static string GenerateFormattedOutput(Dictionary<string, List<Endpoint>> controllers)
    {
        string formattedOutput = "";

        foreach (var controller in controllers)
        {
            formattedOutput += $"Controller: {controller.Key}\n";
            formattedOutput += "------------------------------------------\n";
            formattedOutput += "Method | Path\n";
            formattedOutput += "------------------------------------------\n";

            foreach (var endpoint in controller.Value)
            {
                formattedOutput += $"| {endpoint.Method} | {endpoint.Path}\n";
            }

            formattedOutput += "\n";
        }

        return formattedOutput;
    }
}

public class Endpoint
{
    public string? Method { get; set; }  // Nullable string
    public string? Path { get; set; }    // Nullable string
    public string? Description { get; set; }  // Nullable string
}