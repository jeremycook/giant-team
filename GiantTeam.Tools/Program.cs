using GiantTeam.Tools;
using static System.Console;

bool preview = args.Contains("--preview");

switch (args.FirstOrDefault())
{
    case "typescript":
        TypeScriptTool.TypeScript(preview);
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
