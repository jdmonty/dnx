// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
#if NET45
using System.IO.Packaging;
#endif
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Runtime;
using NuGet;

namespace Microsoft.Framework.PackageManager.Restore.NuGet
{
    public class PackageFolder : IPackageFeed
    {
        private IReport _report;
        private LocalPackageRepository _repository;

        public PackageFolder(
            string physicalPath,
            IReport report)
        {
            _repository = new LocalPackageRepository(physicalPath)
            {
                Report = report
            };
            _report = report;
        }

        public Task<IEnumerable<PackageInfo>> FindPackagesByIdAsync(string id)
        {
            return Task.FromResult(_repository.FindPackagesById(id).Select(p => new PackageInfo
            {
                Id = p.Id,
                Version = p.Version
            }));
        }

        public async Task<Stream> OpenNuspecStreamAsync(PackageInfo package)
        {
            using (var nupkgStream = await OpenNupkgStreamAsync(package))
            {
#if NET45
                if (PlatformHelper.IsMono)
                {
                    // Don't close the stream
                    var archive = Package.Open(nupkgStream, FileMode.Open, FileAccess.Read);
                    var partUri = PackUriHelper.CreatePartUri(new Uri(package.Id + ".nuspec", UriKind.Relative));
                    var entry = archive.GetPart(partUri);

                    using (var entryStream = entry.GetStream())
                    {
                        var nuspecStream = new MemoryStream((int)entryStream.Length);
                        await entryStream.CopyToAsync(nuspecStream);
                        nuspecStream.Seek(0, SeekOrigin.Begin);
                        return nuspecStream;
                    }
                }
#endif

                using (var archive = new ZipArchive(nupkgStream, ZipArchiveMode.Read, leaveOpen: true))
                {
                    var entry = archive.GetEntryOrdinalIgnoreCase(package.Id + ".nuspec");
                    using (var entryStream = entry.Open())
                    {
                        var nuspecStream = new MemoryStream((int)entry.Length);
                        await entryStream.CopyToAsync(nuspecStream);
                        nuspecStream.Seek(0, SeekOrigin.Begin);
                        return nuspecStream;
                    }
                }
            }
        }

        public Task<Stream> OpenNupkgStreamAsync(PackageInfo package)
        {
            return Task.FromResult(_repository.FindPackage(package.Id, package.Version).GetStream());
        }
    }
}

