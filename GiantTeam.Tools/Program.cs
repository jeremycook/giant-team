using GiantTeam.Tools;
using static System.Console;

switch (args.FirstOrDefault())
{
    case "typescript":
        bool preview = args.Contains("--preview");
        TypeScriptTool.TypeScript(preview);
        break;

    case "dataprotection":
        var format = args.Contains("--format=json") ?
            DataProtectionFormat.Json :
            DataProtectionFormat.Pem;
        DataProtectionTool.DataProtection(format);
        break;

    default:
        WriteLine(Path.GetFileName(Environment.ProcessPath) + " [tool] [--option]*");
        WriteLine();
        WriteLine("typescript");
        WriteLine("  Write TypeScript type files based on .NET types:");
        WriteLine("typescript --preview");
        WriteLine("  Preview TypeScript type files based on .NET types in the console");
        WriteLine();
        WriteLine("dataprotection");
        WriteLine("dataprotection --format=pem");
        WriteLine("  Generate a data protection certificate and output as PEM to the console");
        WriteLine("dataprotection --format=json");
        WriteLine("  Generate a data protection certificate and output as JSON to the console");
        WriteLine();
        break;
}
