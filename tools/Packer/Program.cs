using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;

namespace Packer
{
    public class Program
    {
        public void Main(string[] args)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var sln = Path.GetFullPath(Path.Combine("..", "..", "FastQuant.sln"));
            var solution = MSBuildWorkspace.Create().OpenSolutionAsync(sln).Result;
            foreach (var p in solution.Projects)
            {
                Console.WriteLine(p.Name);
            }
            Console.WriteLine(solution.FilePath);
            Console.WriteLine(solution.ProjectIds);
        }
    }
}
