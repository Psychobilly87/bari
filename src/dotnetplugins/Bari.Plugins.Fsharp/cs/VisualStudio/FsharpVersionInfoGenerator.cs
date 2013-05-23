﻿using System.IO;
using Bari.Core.Model;

namespace Bari.Plugins.Fsharp.VisualStudio
{
    /// <summary>
    /// Generates a F# file containing <c>AssemblyVersion</c> and <c>AssemblyFileVersion</c> attributes,
    /// coming from the <see cref="Project"/>.
    /// </summary>
    public class FsharpVersionInfoGenerator
    {
        private readonly Project project;

        /// <summary>
        /// Initializes the version info generator
        /// </summary>
        /// <param name="project">Project to generate version info for</param>
        public FsharpVersionInfoGenerator(Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Generates the F# code to the given output
        /// </summary>
        /// <param name="output">Output text writer to be used</param>
        public void Generate(TextWriter output)
        {
            output.WriteLine("// Version info file generated by bari for project {0}", project.Name);
            if (!string.IsNullOrWhiteSpace(project.EffectiveVersion))
            {
                output.WriteLine("[<assembly: System.Reflection.AssemblyVersion(\"{0}\")>]", project.EffectiveVersion);
                output.WriteLine("[<assembly: System.Reflection.AssemblyFileVersion(\"{0}\")>]", project.EffectiveVersion);
            }
        }
    }
}