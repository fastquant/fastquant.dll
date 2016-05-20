using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.DotNet.ProjectModel.Workspaces;
    
namespace SmartRenamer
{
    public static class Program
    {
        private static string projectPath = Path.Combine("..", "..", "src", "FastQuant");
        private static string NewAssemblyName = "SmartQuant";
        private static string OutputPath = Path.Combine("..", "..", "output");

        public static void Main(string[] args)
        {
            var solution = new ProjectJsonWorkspace(projectPath).CurrentSolution;
            solution = RenameNamespace("FastQuant", "SmartQuant", solution);
            solution = RenameClass("Message", "Message_", solution);
            solution = RenameClass("Command", "Command_", solution);
            solution = RenameClass("Response", "Response_", solution);
            var pids = solution.Projects.Where(p => p.Name.StartsWith("FastQuant")).Select(p => p.Id);
            foreach (var id in pids)
                solution = solution.WithProjectAssemblyName(id, NewAssemblyName);
            foreach (var p in solution.Projects)
                GenerateDll(p);
            Console.WriteLine("Succeed!");
        }

        private static Solution RenameNamespace(string oldName, string newName, Solution solution)
        {
            var proj = solution.Projects.First();
            var sym =
                SymbolFinder.FindDeclarationsAsync(proj, oldName, false)
                    .Result.First(s => s.Kind == SymbolKind.Namespace);
            return Renamer.RenameSymbolAsync(proj.Solution, sym, newName, null).Result;
        }

        private static Solution RenameClass(string fromClsName, string toClsName, Solution solution)
        {
            var proj = solution.Projects.First();
            var sym = SymbolFinder.FindDeclarationsAsync(proj, fromClsName, false)
                    .Result.First(s => s.Kind == SymbolKind.NamedType);
            var options = solution.Workspace.Options;
            return Renamer.RenameSymbolAsync(solution, sym, toClsName, options).Result;
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