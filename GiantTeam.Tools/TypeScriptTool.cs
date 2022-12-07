using GiantTeam.Text;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;

namespace GiantTeam.Tools
{
    public static class TypeScriptTool
    {
        static readonly string[] include = new[]
        {
            "*Input",
            "*Output",
            "*Status",
            "GiantTeam.RecordsManagement.Data.*",
        }.Select(o => Regex.Escape(o).Replace("\\*", ".+")).ToArray();

        static readonly string[] exclude = new[]
        {
            "*DbContext",
        }.Select(o => Regex.Escape(o).Replace("\\*", ".+")).ToArray();

        static readonly Dictionary<Type, string> types = new()
        {
            { typeof(bool), "boolean" },
            { typeof(DateTime), "Date" },
            { typeof(DateTimeOffset), "Date" },
            { typeof(decimal), "number" },
            { typeof(double), "number" },
            { typeof(float), "number" },
            { typeof(int), "number" },
            { typeof(long), "number" },
            { typeof(Guid), "string" },
            { typeof(string), "string" },
        };

        public static void TypeScript(bool preview)
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
            Directory.CreateDirectory(outFolder);

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
                        include.Any(pattern => Regex.IsMatch(t.FullName, pattern)) &&
                        !exclude.Any(pattern => Regex.IsMatch(t.FullName, pattern))
                    )
                    .OrderBy(o => o.FullName);

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
                            bool nullable = contextualProperty.Nullability == Nullability.Nullable;

                            sb.Append(tab);
                            sb.Append(CamelCase(prop.Name));
                            if (nullable) sb.Append('?');
                            sb.Append(": ");
                            sb.Append(TypeScriptTypeName(contextualProperty.PropertyType.Type));
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
                            if (nullable) sb.Append('?');
                            sb.Append(": ");
                            sb.Append(TypeScriptTypeName(contextualProperty.PropertyType.Type));
                            sb.Append(";\n");

                        }
                        sb.Append("}\n\n");
                    }
                }

                var controllers = assembly
                    .ExportedTypes
                    .Where(t =>
                        t.Name.EndsWith("Controller")
                    )
                    .OrderBy(o => o.FullName);

                foreach (var controller in controllers)
                {
                    string controllerName = Regex.Replace(controller.Name, "Controller$", "");
                    string controllerSlug = TextTransformers.Slugify(controllerName);

                    var postActions = controller
                        .GetMethods()
                        .Where(m => m.DeclaringType == m.ReflectedType)
                        .Where(m => m.GetCustomAttribute<HttpPostAttribute>() is not null);

                    foreach (var method in postActions)
                    {
                        // TODO: Check for ActionNameAttribute
                        string actionName = method.Name;
                        string actionSlug = TextTransformers.Slugify(method.Name);

                        string functionName = actionName[..1].ToLower() + actionName[1..] + controllerName;

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
                                .Replace("[controller]", controllerSlug, StringComparison.InvariantCultureIgnoreCase)
                                .Replace("[action]", actionSlug, StringComparison.InvariantCultureIgnoreCase);
                        }
                        else
                        {
                            // Guess
                            path = $"/api/{controllerSlug}/{actionSlug}";
                        }

                        sb.Append($"export const {functionName} = async ({string.Join(", ", parameters.Select(p => p.Name + ": " + p.Type))}): Promise<DataResponse<{(returnTypeName != string.Empty ? returnTypeName : "null")}>> => \n");
                        sb.Append($"    await postJson({string.Join(", ", parameters.Select(p => p.Name).Prepend($"\"{path}\""))});");
                        sb.Append($"\n\n");
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
                    const string DO_NOT_MODIFY_BELOW_THIS_LINE = "// DO NOT MODIFY BELOW THIS LINE\n";

                    string content =
                        DO_NOT_MODIFY_BELOW_THIS_LINE +
                        "// Generated by " + typeof(Program).Assembly.GetName().Name + "\n\n" +
                        sb.ToString().TrimEnd('\n');

                    if (File.Exists(outFile))
                    {
                        string oldContent = File.ReadAllText(outFile);
                        if (oldContent != content)
                        {
                            if (preview)
                            {
                                WriteLine("MOD: " + outFile);
                                WriteLine(outFile);
                            }
                            else
                            {
                                int index = oldContent.IndexOf(DO_NOT_MODIFY_BELOW_THIS_LINE);
                                if (index > 0)
                                {
                                    content = oldContent[0..index] + content;
                                }

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
            bool isArray = false;
            if (type.GetEnumerableItemType() is Type enumerableItemType)
            {
                isArray = true;
                type = enumerableItemType;
            }
            return (
                types.TryGetValue(type, out var typeName) ? typeName :
                type.Name
            ) + (isArray ? "[]" : string.Empty);
        }
    }
}
