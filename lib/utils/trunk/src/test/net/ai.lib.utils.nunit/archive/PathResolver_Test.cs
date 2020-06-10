/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace ai.lib.utils.nunit
{
    // Conserved for possible future use, see also PathResolver.
#if false
    [TestFixture]
    public class PathResolver_Test
    {
        [Test]
        public void Test_Resolve()
        {
            string dirVar = "AI_LIB_UTILS_NUNIT_PATH_RESOLVER_TEST_DIR";
            string fileVar = "AI_LIB_UTILS_NUNIT_PATH_RESOLVER_TEST_FILE";
            string dirName = "test-dir";
            string fileName = "test-file.txt";
            Environment.SetEnvironmentVariable(dirVar, dirName, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(fileVar, fileName, EnvironmentVariableTarget.Process);

            string tempPath = Path.Combine(Path.GetTempPath(), "ai.lib.utils.nunit.PathResolver_Test");
            Directory.CreateDirectory(tempPath);

            string dirPath = Path.Combine(tempPath, dirName);
            Directory.CreateDirectory(dirPath);
            string filePath = Path.Combine(dirPath, fileName);
            using(FileStream fs = File.Create(filePath))
            {
                fs.Close();
            }

            string unresolverdDirPath = "${env." + dirVar + "}";
            string unresolvedFilePath = "${env." + dirVar + "}" + Path.DirectorySeparatorChar +  "${env." + fileVar + "}";

            PathResolver.UpdateEnvironmentVariables();

            // Path not found - return original path (with subsituted variables)
            Assert.AreEqual(dirName, PathResolver.Resolve(unresolverdDirPath, null));
            Assert.AreEqual(dirName + Path.DirectorySeparatorChar + fileName, 
                PathResolver.Resolve(unresolvedFilePath, null));

            // Give search path: path must be found.
            string resolvedDir = PathResolver.Resolve(unresolverdDirPath, new string[] { tempPath });
            string resolvedFile = PathResolver.Resolve(unresolvedFilePath, new string[] { tempPath });
            Assert.AreEqual(Path.Combine(tempPath, dirPath), resolvedDir);
            Assert.AreEqual(Path.Combine(tempPath, filePath), resolvedFile);

            // Give absolute path.
            unresolverdDirPath = tempPath + Path.DirectorySeparatorChar + unresolverdDirPath;
            unresolvedFilePath = tempPath + Path.DirectorySeparatorChar + unresolvedFilePath;
            resolvedDir = PathResolver.Resolve(unresolverdDirPath, null);
            resolvedFile = PathResolver.Resolve(unresolvedFilePath, null);
            Assert.AreEqual(Path.Combine(tempPath, dirPath), resolvedDir);
            Assert.AreEqual(Path.Combine(tempPath, filePath), resolvedFile);
        }
    }
#endif
}
