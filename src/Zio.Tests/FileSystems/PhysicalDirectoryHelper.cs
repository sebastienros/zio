﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using Zio.FileSystems;

namespace Zio.Tests.FileSystems
{
    public class PhysicalDirectoryHelper : IDisposable
    {
        private readonly DirectoryInfo _compatDirectory;

        private PhysicalDirectoryHelper(string rootPath)
        {
            _compatDirectory = new DirectoryInfo(Path.Combine(rootPath, "Physical-" + Guid.NewGuid()));
            _compatDirectory.Create();
        }

        public static async ValueTask<PhysicalDirectoryHelper> Create(string rootPath)
        {
            var helper = new PhysicalDirectoryHelper(rootPath);

            var pfs = new PhysicalFileSystem();

            helper.PhysicalFileSystem = await SubFileSystem.Create(pfs, pfs.ConvertPathFromInternal(helper._compatDirectory.FullName));

            return helper;
        }

        public IFileSystem PhysicalFileSystem { get; private set; }

        public void Dispose()
        {
            DeleteDirectoryForce(_compatDirectory);
        }

        private static void DeleteDirectoryForce(DirectoryInfo dir)
        {
            var infos = dir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories);
            foreach (var info in infos)
            {
                if ((info.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    info.Attributes = FileAttributes.Normal;
                }
                if (info is FileInfo)
                {
                    try
                    {
                        info.Delete();
                    }
                    catch { }
                }
            }

            try
            {
                dir.Delete(true);
            }
            catch {}
        }
    }
}