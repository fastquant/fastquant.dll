using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;

namespace SmartRenamer
{
    public static class Program
    {
        private static string PrjPath = Path.Combine("..", "..", "src", "SmartQuant.MsBuild", "SmartQuant.MsBuild.csproj");
        private static string OutputPath = Path.Combine("..", "..", "output");

        public static void Main(string[] args)
        {
            var ws = MSBuildWorkspace.Create();
            var proj = ws.OpenProjectAsync(PrjPath).Result;
            var solution = proj.Solution;
            solution = RenameNamespace("FastQuant", "SmartQuant", solution);
            solution = RenameClass("Message", "Message_", solution);
            solution = RenameClass("Command", "Command_", solution);
            solution = RenameClass("Response", "Response_", solution);
            foreach (var p in solution.Projects)
                GenerateDll(p);

            Console.WriteLine("Done");
        }

        private static Solution RenameNamespace(string oldName, string newName, Solution solution)
        {
            var proj = solution.Projects.First();
            var sym =
                SymbolFinder.FindDeclarationsAsync(proj, oldName, false)
                    .Result.First(s => s.Kind == SymbolKind.Namespace);
            var sln = Renamer.RenameSymbolAsync(proj.Solution, sym, newName, null).Result;
            return sln;
        }

        private static Solution RenameClass(string fromClsName, string toClsName, Solution solution)
        {
            var proj = solution.Projects.First(p => p.Name.StartsWith("SmartQuant"));
            var sym = SymbolFinder.FindDeclarationsAsync(proj, fromClsName, false)
                    .Result.First(s => s.Kind == SymbolKind.NamedType);
            var options = solution.Workspace.Options;
            var sln = Renamer.RenameSymbolAsync(solution, sym, toClsName, options).Result;
            return sln;
        }

        private static void GenerateDll(Project p)
        {
            var c = p.GetCompilationAsync().Result;
            var fileName = Path.Combine(OutputPath, p.Name, p.AssemblyName + ".dll");
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            var result = c.Emit(fileName);
            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics)
                    Debug.WriteLine(diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line + " " +
                                  diagnostic.GetMessage());
            }
        }
    }
}