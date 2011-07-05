//
// CPPClean.cs: Task for C++ cleaning
// 
// For an overview of the CPPClean options, you can check:
//  http://msdn.microsoft.com/en-us/library/ee862481.aspx
//
// Author:
//   João Matos (triton@vapor3d.org)
// 
// (C) 2011 João Matos
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks.Cpp
{
    public class CPPClean : TaskExtension
    {
        public CPPClean()
        {
        }

        public override bool Execute()
        {
            // TODO: Add support for wildcards in directories.

            List<ITaskItem> filesDeleted = new List<ITaskItem>();

            foreach (string directory in FoldersToClean.Split(';'))
            {
                if (!Directory.Exists(directory))
                    continue;

                deleteFilesOnDir(filesDeleted, directory);
            }

            DeletedFiles = filesDeleted.ToArray();

            return true;
        }

        private void deleteFilesOnDir(List<ITaskItem> filesDeleted, string directory)
        {
            foreach (string filePattern in FilePatternsToDeleteOnClean.Split(';'))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(directory, filePattern);
                deleteFiles(filesDeleted, directory, files);
            }
        }

        private void deleteFiles(List<ITaskItem> filesDeleted, string directory, IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                if (FilesExcludedFromClean.Contains(file))
                    continue;

                string fullPath = Path.Combine(directory, file);

                if (DoDelete)
                    File.Delete(fullPath);

                filesDeleted.Add(new TaskItem(fullPath));
            }
        }

        [Output]
        public ITaskItem[] DeletedFiles
        {
            get;
            set;
        }

        public bool DoDelete
        {
            get;
            set;
        }

        [Required]
        public string FilePatternsToDeleteOnClean
        {
            get;
            set;
        }

        public string FilesExcludedFromClean
        {
            get;
            set;
        }

        [Required]
        public string FoldersToClean
        {
            get;
            set;
        }
    }
}

#endif
