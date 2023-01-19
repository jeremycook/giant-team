using GiantTeam.Startup;
using GiantTeam.Text;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Console;

namespace GiantTeam.Tools
{
    public static class TypeScriptTool
    {
        static readonly string[] include = new[]
        {
            "GiantTeam.*.Controllers.*",
            "GiantTeam.*.Models.*",
            "GiantTeam.*.Services.*",
        }.Select(o => "^" + Regex.Escape(o).Replace("\\*", ".+") + "$").ToArray();

        static readonly string[] exclude = new[]
        {
            "*Controller",
            "*Service",
        }.Select(o => "^" + Regex.Escape(o).Replace("\\*\\*", ".*").Replace("\\*", ".+") + "$").ToArray();

        static readonly Dictionary<Type, string> types = new()
        {
            { typeof(object), "any" },

            { typeof(bool), "boolean" },

            { typeof(DateTime), "Date" },
            { typeof(DateTimeOffset), "Date" },

            { typeof(decimal), "number" },
            { typeof(double), "number" },
            { typeof(float), "number" },
            { typeof(int), "number" },
            { typeof(long), "number" },

            { typeof(char), "string" },
            { typeof(Guid), "string" },
            { typeof(string), "string" },
        };

        static readonly Dictionary<Type, string> arrayTypes = new()
        {
            { typeof(byte[]), "string" },
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

            string outFolder = Path.GetFullPath("./SolidUI/src/bindings/", solutionDirectory.FullName);
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

            foreach (var group in appAssemblies
                .SelectMany(a => a.ExportedTypes)
                .GroupBy(t => t.Namespace ?? string.Empty)
                .ToDictionary(t => t.Key, t => t.ToArray()))
            {
                var sb = new StringBuilder();

                var dataTypes = group.Value
                    .Where(t =>
                        include.Any(pattern => Regex.IsMatch(t.FullName, pattern)) &&
                        !exclude.Any(pattern => Regex.IsMatch(t.FullName, pattern)) &&
                        t.GetCustomAttribute<ServiceAttribute>() is null
                    )
                    .OrderBy(o => o.FullName);

                foreach (var type in dataTypes)
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
                    else if (type.IsAbstract && type.IsSealed)
                    {
                        var fields = type
                            .GetFields(BindingFlags.Public | BindingFlags.Static).Select(m => new { m.Name, Value = m.GetValue(null) })
                            .Union(type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(m => (!m.PropertyType.IsClass || m.PropertyType == typeof(string)) && m.GetSetMethod() is null).Select(m => new { m.Name, Value = m.GetValue(null) }));

                        sb.Append($"export enum {type.Name} {{\n");
                        foreach (var field in fields)
                        {
                            sb.Append(tab);
                            sb.Append(field.Name);
                            sb.Append(" = ");
                            sb.Append($"'{field.Value}'");
                            sb.Append(",\n");
                        }
                        sb.Append("}\n\n");
                    }
                    else
                    {
                        string discriminatorPropertyName = "";
                        string discrimantorValue = "";

                        sb.Append($"export interface {type.Name} ");
                        if (type.BaseType?.IsAbstract == true)
                        {
                            sb.Append($"extends {TypeScriptTypeName(type.BaseType)} ");

                            discrimantorValue = type.BaseType
                                .GetCustomAttributes<JsonDerivedTypeAttribute>()
                                .Where(o => o.DerivedType == type)
                                .SingleOrDefault()?.TypeDiscriminator as string ?? "";

                            if (discrimantorValue != "")
                            {
                                if (type.BaseType.GetCustomAttribute<JsonPolymorphicAttribute>() is JsonPolymorphicAttribute jsonPolymorphic &&
                                    jsonPolymorphic.TypeDiscriminatorPropertyName is not null)
                                {
                                    discriminatorPropertyName = CamelCase(jsonPolymorphic.TypeDiscriminatorPropertyName);
                                }
                                else
                                {
                                    discriminatorPropertyName = "$type";
                                }
                            }
                        }
                        sb.Append($"{{\n");
                        var props = new List<KeyValuePair<string, string>>();
                        if (discrimantorValue != "")
                        {
                            props.Insert(0, new(
                                discriminatorPropertyName,
                                $"'{discrimantorValue}'"
                            ));
                        }
                        props.AddRange(type
                            .GetContextualProperties()
                            .Select(o => new
                            {
                                Name = o.GetContextAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpn ? jpn.Name : CamelCase(o.Name),
                                o.PropertyType,
                                o.Nullability,
                            })
                            .Where(o => discriminatorPropertyName != o.Name)
                            .Select(o => new KeyValuePair<string, string>(
                                o.Name,
                                TypeScriptTypeName(o.PropertyType.Type) + (o.Nullability == Nullability.Nullable ? " | null" : "")
                            )));
                        foreach (var prop in props)
                        {
                            sb.Append(tab);
                            sb.Append(prop.Key);
                            sb.Append(": ");
                            sb.Append(prop.Value);
                            sb.Append(";\n");
                        }
                        sb.Append("}\n\n");
                    }
                }

                var controllers = group.Value
                    .Where(t =>
                        t.Name.EndsWith("Controller")
                    )
                    .OrderBy(o => o.FullName);

                foreach (var controller in controllers)
                {
                    string controllerName = Regex.Replace(controller.Name, "Controller$", "");
                    string controllerSlug = TextTransformers.Dashify(controllerName);

                    var postActions = controller
                        .GetMethods()
                        .Where(m => m.DeclaringType == m.ReflectedType)
                        .Where(m => m.GetCustomAttribute<HttpPostAttribute>() is not null);

                    foreach (var method in postActions)
                    {
                        // TODO: Check for ActionNameAttribute
                        string actionName = method.GetCustomAttribute<ActionNameAttribute>()?.Name ?? method.Name;
                        string actionSlug = TextTransformers.Dashify(method.Name);

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

                        string returnTypeSignature =
                            $" as DataResponse<{(returnTypeName != string.Empty ? returnTypeName : "null")}>";

                        sb.Append($"export const {functionName} = async ({string.Join(", ", parameters.Select(p => p.Name + ": " + p.Type))}) =>\n");
                        sb.Append($"    await postJson({string.Join(", ", parameters.Select(p => p.Name).Prepend($"'{path}'"))}){returnTypeSignature};");
                        sb.Append($"\n\n");
                    }
                }


                string outFile = Path.Combine(outFolder, group.Key + ".ts");
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

                    string content = (
                        DO_NOT_MODIFY_BELOW_THIS_LINE +
                        "// Generated by " + typeof(Program).Assembly.GetName().Name + "\n\n" +
                        sb.ToString()
                    ).ReplaceLineEndings("\n").TrimEnd('\n');

                    if (File.Exists(outFile))
                    {
                        string oldContent = File.ReadAllText(outFile).ReplaceLineEndings("\n");
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
            if (arrayTypes.TryGetValue(type, out var arrayTypeName))
            {
                return arrayTypeName;
            }

            var suffix = string.Empty;
            while (TryGetEnumerableItemType(type, out Type enumerableItemType))
            {
                suffix += "[]";
                type = enumerableItemType;
            }
            return (
                types.TryGetValue(type, out var typeName) ? typeName :
                type.Name
            ) + suffix;
        }

        private static bool TryGetEnumerableItemType(Type type, out Type enumerableItemType)
        {
            if (type.GetEnumerableItemType() is Type type1)
            {
                enumerableItemType = type1;
                return true;
            }
            else if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) &&
                type.GetGenericArguments()[0] is Type type2)
            {
                enumerableItemType = type2;
                return true;
            }
            else
            {
                enumerableItemType = null!;
                return false;
            }
        }
    }
}
