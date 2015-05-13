// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Framework.CommonTestUtils;
using Microsoft.Framework.Runtime;

namespace Microsoft.Framework.PackageManager
{
    public static class DnuTestUtils
    {
        public static int ExecDnu(string runtimeHomePath,
                                  string subcommand,
                                  string arguments,
                                  out string stdOut,
                                  out string stdErr,
                                  IDictionary<string, string> environment = null,
                                  string workingDir = null)
        {
            string program;
            string commandLine;
            string runtimeRoot;

            var dnxDev = Environment.GetEnvironmentVariable("DNX_DEV");
            if (string.Equals(dnxDev, "1"))
            {
                runtimeRoot = runtimeHomePath;
            }
            else
            {
                runtimeRoot = Directory.EnumerateDirectories(Path.Combine(runtimeHomePath, "runtimes"), Constants.RuntimeNamePrefix + "*").First();
            }

            var isMono = RuntimeEnvironmentHelper.IsMono(TestUtils.CurrentRuntimeEnvironment);
            if (isMono)
            {
                program = Path.Combine(runtimeRoot, "bin", "dnu");
                commandLine = string.Format("{0} {1}", subcommand, arguments);
            }
            else
            {
                program = "cmd";
                var dnuCmdPath = Path.Combine(runtimeRoot, "bin", "dnu.cmd");
                commandLine = string.Format("/C {0} {1} {2}", dnuCmdPath, subcommand, arguments);
            }

            var exitCode = TestUtils.Exec(program, commandLine, out stdOut, out stdErr, environment, workingDir);

            return exitCode;
        }

        public static int ExecDnu(string runtimeHomePath,
                                  string subcommand,
                                  string arguments,
                                  IDictionary<string, string> environment = null,
                                  string workingDir = null)
        {
            string stdOut, stdErr;
            return ExecDnu(runtimeHomePath, subcommand, arguments, out stdOut, out stdErr, environment, workingDir);
        }
    }
}
