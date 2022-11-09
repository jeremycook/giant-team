using GiantTeam.Text;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;

bool preview = args.Contains("--preview");

switch (args.FirstOrDefault())
{
    case "typescript":
        TypeScript(preview);
        break;
    default:
        WriteLine(Path.GetFileName(Environment.ProcessPath) + " [tool] [--option]*");
        WriteLine();
        WriteLine("Tools:");
        WriteLine("typescript   Write TypeScript type files based on .NET types");
        WriteLine();
        WriteLine("Options:");
        WriteLine("--preview    Preview the output without modifying anything");
        WriteLine();
        break;
}

static void TypeScript(bool preview)
{
    // Search up the path for the solution directory
    var solutionDirectory = new DirectoryInfo(Environment.CurrentDirectory);
    while (solutionDirectory is not null && solutionDirectory.EnumerateFiles("GiantTeam.sln").Count() != 1)
    {
        solutionDirectory = solutionDirectory.Parent;
    }
    if (solutionDirectory is null)
    {
        throw new InvalidOperationException("The solution directory could not be determined.");
    }

    string inFolder = Path.GetFullPath(
        Path.GetDirectoryName(Environment.ProcessPath) ??
        throw new InvalidOperationException("The input directory could not be determined.")
    , solutionDirectory.FullName);

    string outFolder = Path.GetFullPath("./SolidUI/src/api/", solutionDirectory.FullName);

    var inFiles = Directory
        .EnumerateFiles(inFolder, "*.dll")
        .OrderBy(f => f);

    var appAssemblies = new List<Assembly>();
    foreach (var file in inFiles)
    {
        var filename = Path.GetFileName(file);
        if (filename.StartsWith("GiantTeam."))
        {
            var assembly = Assembly.LoadFrom(file);
            appAssemblies.Add(assembly);
        }
    }

    const string tab = "    ";

    foreach (var assembly in appAssemblies)
    {
        var sb = new StringBuilder();

        var types = assembly
            .ExportedTypes
            .Where(t =>
                t.Name.EndsWith("Input") ||
                t.Name.EndsWith("Output") ||
                t.Name.EndsWith("Status")
            );

        foreach (var type in types)
        {
            if (type.IsEnum)
            {
                sb.Append($"export enum {type.Name} {{\n");
                foreach (var (field, value) in Enum.GetNames(type).Zip(Enum.GetValues(type).Cast<int>()))
                {
                    sb.Append(tab);
                    sb.Append(field);
                    sb.Append(" = ");
                    sb.Append(value);
                    sb.Append(",\n");
                }
                sb.Append("}\n\n");
            }
            else if (type.IsInterface)
            {
                sb.Append($"export interface {type.Name} {{\n");
                foreach (var prop in type.GetProperties())
                {
                    var contextualProperty = prop.ToContextualProperty();
                    bool udt = appAssemblies.Contains(prop.PropertyType.Assembly);
                    bool nullable = contextualProperty.Nullability == Nullability.Nullable;

                    sb.Append(tab);
                    sb.Append(prop.Name[0..1].ToLower() + prop.Name[1..]);
                    if (nullable) sb.Append("?");
                    sb.Append(": ");
                    sb.Append(udt ? prop.PropertyType.Name : prop.PropertyType.Name.ToLower());
                    sb.Append(";\n");

                }
                sb.Append("}\n\n");
            }
            else
            {
                sb.Append($"export interface {type.Name} {{\n");
                foreach (var prop in type.GetProperties())
                {
                    var contextualProperty = prop.ToContextualProperty();
                    bool nullable = contextualProperty.Nullability == Nullability.Nullable;

                    sb.Append(tab);
                    sb.Append(CamelCase(prop.Name));
                    if (nullable) sb.Append("?");
                    sb.Append(": ");
                    sb.Append(TypeScriptTypeName(prop.PropertyType));
                    sb.Append(";\n");

                }
                sb.Append("}\n\n");
            }
        }

        var controllers = assembly
            .ExportedTypes
            .Where(t =>
                t.Name.EndsWith("Controller")
            );

        foreach (var controller in controllers)
        {
            string controllerName = TextTransformers.Slugify(Regex.Replace(controller.Name, "Controller$", ""));

            var postActions = controller
                .GetMethods()
                .Where(m => m.DeclaringType == m.ReflectedType)
                .Where(m => m.GetCustomAttribute<HttpPostAttribute>() is not null);

            foreach (var method in postActions)
            {
                // TODO: Check for ActionNameAttribute
                string actionName = TextTransformers.Slugify(method.Name);

                string functionName = actionName + controllerName[..1].ToUpper() + controllerName[1..];

                var parameters = method
                    .GetParameters()
                    .Where(p => !p.CustomAttributes.Any()) // ignore parameters with attributes
                    .Select(p => p.ToContextualParameter())
                    .Select(p => new
                    {
                        Name = CamelCase(p.Name) + (p.Nullability == Nullability.Nullable ? "?" : ""),
                        Type = TypeScriptTypeName(p.Type)
                    });

                Type? returnType =
                    method.ReturnType == typeof(void) || method.ReturnType == typeof(Task) ? null :
                    method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ? method.ReturnType.GetGenericArguments()[0] :
                    method.ReturnType;

                if (returnType?.IsAssignableTo(typeof(IActionResult)) == true)
                {
                    returnType = null;
                }

                string returnTypeName = returnType is not null ?
                    TypeScriptTypeName(returnType) :
                    string.Empty;

                string path;
                if (method.GetCustomAttribute<HttpPostAttribute>() is HttpPostAttribute httpPost &&
                    httpPost.Template is not null)
                {
                    // Based on route
                    path = httpPost.Template
                        .Replace("[controller]", controllerName, StringComparison.InvariantCultureIgnoreCase)
                        .Replace("[action]", actionName, StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    // Guess
                    path = $"/api/{controllerName}/{actionName}";
                }

                sb.Append($"export const {functionName} = async ({string.Join(", ", parameters.Select(p => p.Name + ": " + p.Type))}){(returnTypeName != string.Empty ? $": Promise<{returnTypeName}>" : "")} => {{\n");
                sb.Append($"    const response = await fetch(\"{path}\", {{\n");
                sb.Append($"        method: \"POST\",\n");
                sb.Append($"        headers: {{ \"Content-Type\": \"application/json\" }},\n");
                if (parameters.Any())
                { sb.Append($"        body: JSON.stringify({string.Join(", ", parameters.Select(p => p.Name))})\n"); }
                sb.Append($"    }});\n");
                if (returnTypeName != string.Empty)
                { sb.Append($"    return response.json();\n"); }
                sb.Append($"}}\n\n");
            }
        }


        string outFile = Path.Combine(outFolder, assembly.GetName().Name + ".ts");
        if (sb.Length <= 0)
        {
            if (File.Exists(outFile))
            {
                if (preview)
                {
                    WriteLine("DEL: " + outFile);
                }
                else
                {
                    File.Delete(outFile);
                    WriteLine("DEL: " + outFile);
                }
            }
        }
        else
        {
            string content =
                "// DO NOT MODIFY!\n" +
                "// Generated by " + typeof(Program).Assembly.GetName().Name + "\n\n" +
                sb.ToString().TrimEnd('\n');

            if (File.Exists(outFile))
            {
                if (File.ReadAllText(outFile) != content)
                {
                    if (preview)
                    {
                        WriteLine("MOD: " + outFile);
                        WriteLine(outFile);
                    }
                    else
                    {
                        File.WriteAllText(outFile, content);
                        WriteLine("MOD: " + outFile);
                    }
                }
            }
            else
            {
                if (preview)
                {
                    WriteLine("ADD: " + outFile);
                    WriteLine(outFile);
                }
                else
                {
                    File.WriteAllText(outFile, content);
                    WriteLine("ADD: " + outFile);
                }
            }
        }
    }
}

static string CamelCase(string propName)
{
    return propName[0..1].ToLower() + propName[1..];
}

static string TypeScriptTypeName(Type type)
{
    return type.Namespace == "System" ?
        type.Name.ToLower() :
        type.Name;
}