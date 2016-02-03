using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Workspaces.Dnx;

namespace SmartRenamer
{
    public static class Program
    {
        public static void Main()
        {
            var ws = new ProjectJsonWorkspace(@"C:\Users\alex\works\smartquant.dll\src\SmartQuant");
            var solution = ws.CurrentSolution;
            solution = RenameNamespace("SmartQuant", "FastQuant", solution);
            solution = RenameClass("Message", "Message_", solution);
            solution = RenameClass("Command", "Command_", solution);
            solution = RenameClass("Response", "Response_", solution);
            GenerateDll("d:\\SQ.dll", solution.Projects.First().GetCompilationAsync().Result);
            Console.WriteLine("Done");
        }

        private static Solution RenameNamespace(string oldName, string newName, Solution solution)
        {
            var proj = solution.Projects.First();
            var sym =
                SymbolFinder.FindDeclarationsAsync(proj, oldName, false)
                    .Result.First(s => s.Kind == SymbolKind.Namespace);
            var sln = Renamer.RenameSymbolAsync(proj.Solution, sym, newName, null).Result;
            //GenerateDll("d:\\SQ.dll", s.Projects.First().GetCompilationAsync().Result);
            return sln;
        }

        private static Solution RenameClass(string fromClsName, string toClsName, Solution solution)
        {
            var proj = solution.Projects.First();
            var sym =
                SymbolFinder.FindDeclarationsAsync(proj, fromClsName, false)
                    .Result.First(s => s.Kind == SymbolKind.NamedType);
            var sln = Renamer.RenameSymbolAsync(proj.Solution, sym, toClsName, null).Result;
            //GenerateDll($"d:\\SQ.{fromClsName}.dll", sln.Projects.First().GetCompilationAsync().Result);
            return sln;
        }

        private static void GenerateDll(string fileName, Compilation c)
        {
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