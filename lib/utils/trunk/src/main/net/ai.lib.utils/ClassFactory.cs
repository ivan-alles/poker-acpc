/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace ai.lib.utils
{
    public static class ClassFactory
    {
        /// <summary>
        /// An internal exception to see the difference between exception thrown by the implementation 
        /// of this class and other exceptions.
        /// </summary>
        private class InternalException : ApplicationException
        {
            public InternalException(string message)
                : base(message)
            {
            }
        }

        public static T CreateInstance<T>(ClassFactoryParams p)
        {
            string typeName = p.TypeName;
            string assemblyFileName = p.AssemblyFile;
            try
            {
                log.InfoFormat("ClassFactory.CreateInstance(typeName {0}, assemblyFileName {1})", typeName,
                               assemblyFileName);

                if (String.IsNullOrEmpty(typeName))
                {
                    return default(T);
                }

                Type type;

                if (String.IsNullOrEmpty(assemblyFileName))
                {
                    // If file name is unspecified, try to create the type directly.
                    // If the typeName contains the name of the assembly, and this assembly is 
                    // not loaded yet, it will be loaded automatically provided that it is present
                    // in the directory where the binaries are.
                    type = Type.GetType(typeName);
                }
                else
                {
                    Assembly assembly = LoadAssembly(assemblyFileName, typeName);
                    type = assembly.GetType(typeName);
                }
                if (type == null)
                {
                    throw new InternalException(String.Format("Cannot find type '{0}', assembly file '{1}'",
                                                        typeName, assemblyFileName));
                }

                object instance;
                if (p.Arguments == null)
                {
                    instance = Activator.CreateInstance(type);
                }
                else
                {
                    instance = Activator.CreateInstance(type, p.Arguments);
                }
                return (T)instance;
            }
            catch (Exception e)
            {
                if (e is InternalException)
                    throw e;
                // Create an exception wrapper containing information about the type name
                // because this is a very important information to analyze the problem and
                // this information is usually missing in the original exception.
                ApplicationException excWrapper = new ApplicationException(
                    String.Format("Creating type {0} failed, see inner exception for details.", typeName), e);
                throw excWrapper;
            }
        }
        
        /// <summary>
        /// Creates an instance of the type. See ClassFactoryParams for detailed descriptions
        /// of parameters.
        /// </summary>
        public static T CreateInstance<T>(string typeName, string assemblyFileName)
        {
            return CreateInstance<T>(new ClassFactoryParams(typeName, assemblyFileName));
        }

        private static Assembly LoadAssembly(string assemblyFileName, string typeName)
        {
            if (!File.Exists(assemblyFileName))
            {
                throw new InternalException(String.Format(
                    "Creating type {0} failed: assembly file {1} doesn't exits.",
                    typeName, assemblyFileName));
            }
            return Assembly.LoadFrom(assemblyFileName);
        }

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}