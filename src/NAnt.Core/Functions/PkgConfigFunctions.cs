// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ian Maclean (ian_maclean@another.com)
// Jaroslaw Kowalski (jkowalski@users.sourceforge.net)
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Globalization;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

namespace NAnt.Core.Functions {
    [FunctionSet("pkg-config", "Unix/Cygwin")]
    public class PkgConfigFunctions : FunctionSetBase {

        #region Public Instance Constructors

        public PkgConfigFunctions(Project project, PropertyDictionary propDict ) : base(project, propDict) {}

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Gets the value of a variable for the specified package.
        /// </summary>
        /// <param name="package">The package for which the variable should be retrieved.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// The value of variable <paramref name="name" /> for the specified 
        /// package.
        /// </returns>
        [Function("get-variable")]
        public string GetVariable(string package, string name) {
            return RunPkgConfigString(new Argument[] {  new Argument("--variable=\"" + name + "\""),
                                                        new Argument(package) });
        }

        /// <summary>
        /// Gets the link flags required to compile the package, including all
        /// its dependencies.
        /// </summary>
        /// <param name="package">The package for which the link flags should be retrieved.</param>
        /// <returns>
        /// The link flags required to compile the package.
        /// </returns>
        [Function("get-link-flags")]
        public string GetLinkFlags(string package) {
            return RunPkgConfigString(new Argument[] { new Argument("--libs"),
                                                       new Argument(package) });
        }

        /// <summary>
        /// Gets the compile flags required to compile the package, including all
        /// its dependencies.
        /// </summary>
        /// <param name="package">The package for which the compile flags should be retrieved.</param>
        /// <returns>
        /// The pre-processor and compile flags required to compile the package.
        /// </returns>
        [Function("get-compile-flags")]
        public string GetCompileFlags(string package) {
            return RunPkgConfigString(new Argument[] { new Argument("--cflags"),
                                                       new Argument(package) });
        }

        /// <summary>
        /// Determines the version of the given package.
        /// </summary>
        /// <param name="package">The package to get the version of.</param>
        /// <returns>
        /// The version of the given package.
        /// </returns>
        [Function("get-mod-version")]
        public string GetModVersion(string package) {
            return RunPkgConfigString( new Argument[]{ new Argument("--modversion"),
                                                       new Argument(package) });
        }

        /// <summary>
        /// Determines whether the given package is at least version 
        /// <paramref name="version" />.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <param name="version">The version the package should at least have.</param>
        /// <returns>
        /// <see langword="true" /> if the given package is at least version
        /// <paramref name="version" />; otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-atleast-version")]
        public bool IsAtLeastVersion(string package, string version) {
            return RunPkgConfigBool( new Argument[]{ new Argument("--atleast-version=\"" + version + "\""),
                                                     new Argument(package) });
        }

        /// <summary>
        /// Determines whether the given package is exactly version 
        /// <paramref name="version" />.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <param name="version">The version the package should have.</param>
        /// <returns>
        /// <see langword="true" /> if the given package is exactly version
        /// <paramref name="version" />; otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-exact-version")]
        public bool IsExactVersion(string package, string version) {
            return RunPkgConfigBool( new Argument[]{ new Argument("--exact-version=\"" + version + "\""),
                                                     new Argument(package)});
        }

        /// <summary>
        /// Determines whether the given package is at no newer than version
        /// <paramref name="version" />.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <param name="version">The version the package should maximum have.</param>
        /// <returns>
        /// <see langword="true" /> if the given package is at no newer than 
        /// version <paramref name="version" />; otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-max-version")]
        public bool IsMaxVersion(string package, string version) {
            return RunPkgConfigBool( new Argument[]{ new Argument("--max-version=\"" + version + "\""),
                                                    new Argument(package) });
        }

        /// <summary>
        /// Determines whether the given package is between two versions.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <param name="minVersion">The version the package should at least have.</param>
        /// <param name="maxVersion">The version the package should maximum have.</param>
        /// <returns>
        /// <see langword="true" /> if the given package is between <paramref name="minVersion" />
        /// and <paramref name="maxVersion" />; otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-between-version")]
        public bool IsBetweenVersion(string package, string minVersion, string maxVersion) {
            return RunPkgConfigBool( new Argument[]{ new Argument("--atleast-version=\"" + minVersion + "\""),
                                                     new Argument("--max-version=\"" + maxVersion + "\""),
                                                     new Argument(package)
                                                     } );
        }

        /// <summary>
        /// Determines whether the given package exists.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <returns>
        /// <see langword="true" /> if the package exists; otherwise, 
        /// <see langword="false" />.
        /// </returns>
        [Function("exists")]
        public bool Exists(string package) {
            return RunPkgConfigBool( new Argument[]{ new Argument("--exists"), new Argument(package)} );
        }

        #endregion Public Instance Methods

        #region Private Instance Methods
        
        /// <summary>
        /// helper method to run pkgconfig and return a boolean based on the exit code
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool RunPkgConfigBool(Argument[] args) {
            MemoryStream ms = new MemoryStream();

            ExecTask execTask = GetTask(ms);
            execTask.Arguments.AddRange(args);

            try {
                execTask.Execute();
                return true;
            } catch (Exception) {
                if (execTask.ExitCode == ExternalProgramBase.UnknownExitCode) {
                    // process could not be started or did not exit in time
                    throw;
                }
                return false;
            }
        }
        
        /// <summary>
        /// helper method to run pkgconfig and return the result as a string
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string RunPkgConfigString(Argument[] args) {
            MemoryStream ms = new MemoryStream();

            ExecTask execTask = GetTask(ms);
            execTask.Arguments.AddRange(args);

            try {
                execTask.Execute();
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string output = sr.ReadLine();
                sr.Close();
                return output;
            } catch (Exception ex) {
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string output = sr.ReadToEnd();
                sr.Close();

                if (output.Length != 0) {
                    throw new BuildException(output, ex);
                } else {
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Factory method to return a new instance of ExecTask
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private ExecTask GetTask(Stream stream) {
            ExecTask execTask = new ExecTask();
            execTask.Parent = Project;
            execTask.Project = Project;
            execTask.FileName = "pkg-config";
            execTask.Threshold = Level.None;
            execTask.ErrorWriter = execTask.OutputWriter = new StreamWriter(stream);
            return execTask;
        }

        #endregion Private Instance Methods
    }
}