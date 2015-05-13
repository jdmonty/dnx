// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Versioning;
using Microsoft.Framework.Runtime;

namespace Microsoft.Framework.PackageManager.Publish
{
    public class PublishRuntime
    {
        private readonly FrameworkName _frameworkName;
        private readonly string _runtimePath;
        private readonly bool _isMono;

        public PublishRuntime(PublishRoot root, FrameworkName frameworkName, string runtimePath, bool isMono)
        {
            _frameworkName = frameworkName;
            _runtimePath = runtimePath;
            _isMono = isMono;
            Name = new DirectoryInfo(_runtimePath).Name;
            TargetPath = Path.Combine(root.TargetPackagesPath, Name);
        }

        public string Name { get; private set; }
        public string TargetPath { get; private set; }
        public FrameworkName Framework { get { return _frameworkName; } }

        public bool Emit(PublishRoot root)
        {
            root.Reports.Quiet.WriteLine("Bundling runtime {0}", Name);

            if (Directory.Exists(TargetPath))
            {
                root.Reports.Quiet.WriteLine("  {0} already exists.", TargetPath);

                // REVIEW: IS this correct?
                return true;
            }

            if (!Directory.Exists(TargetPath))
            {
                Directory.CreateDirectory(TargetPath);
            }

            new PublishOperations().Copy(_runtimePath, TargetPath);

            if (_isMono)
            {
                // Executable permissions on dnx lost on copy. 
                var dnxPath = Path.Combine(TargetPath, "bin", "dnx");
                if (!FileOperationUtils.MarkExecutable(dnxPath))
                {
                    root.Reports.Information.WriteLine("Failed to mark {0} as executable".Yellow(), dnxPath);
                }
            }

            return true;
        }
    }
}