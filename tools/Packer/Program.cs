using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;
using Microsoft.CodeAnalysis.Workspaces.Dnx;
using Microsoft.Framework.Runtime;

namespace Packer
{
    public class Program
    {
        private readonly IApplicationEnvironment env;

        public Program(IApplicationEnvironment env)
        {
            this.env = env;
        }

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
           
            var basePath = Path.GetFullPath(Path.Combine("..", "..", "src", "FastQuant.Core"));
            Console.WriteLine(basePath);
            var workspace = new ProjectJsonWorkspace(basePath);
            foreach (var p in workspace.CurrentSolution.Projects)
            {
                Console.WriteLine(p.Name);
            }
        }
    }
}
