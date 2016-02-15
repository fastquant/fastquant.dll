using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Workspaces.Dnx;

namespace SmartRenamer
{
    public static class Program
    {
        private static string SlnPath = Path.Combine("..", "..", "src", "SmartQuant");
        private static string OutputPath = Path.Combine("..", "..", "output");

        public static void Main(string[] args)
        {

            var ws = new ProjectJsonWorkspace(SlnPath);
            var solution = ws.CurrentSolution;
            solution = RenameNamespace("SmartQuant", "FastQuant", solution);
            solution = RenameClass("Message", "Message_", solution);
            solution = RenameClass("Command", "Command_", solution);
            solution = RenameClass("Response", "Response_", solution);
            foreach (var p in solution.Projects)
            {
                GenerateDll(p);
                Console.WriteLine(p.Name);
                Console.WriteLine(p.OutputFilePath);
            }

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
            var proj = solution.Projects.First();
            var sym =
                SymbolFinder.FindDeclarationsAsync(proj, fromClsName, false)
                    .Result.First(s => s.Kind == SymbolKind.NamedType);
            var sln = Renamer.RenameSymbolAsync(proj.Solution, sym, toClsName, null).Result;
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