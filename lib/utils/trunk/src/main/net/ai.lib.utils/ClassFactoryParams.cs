/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.utils
{
    /// <summary>
    /// Parameters to create an instance of a class.
    /// </summary>
    [Serializable]
    public class ClassFactoryParams
    {
        public ClassFactoryParams()
        {
        }

        public ClassFactoryParams(string typeName, string assemblyFile)
        {
            TypeName = typeName;
            AssemblyFile = assemblyFile;
        }

        /// <summary>
        /// Create from string.
        /// </summary>
        /// <param name="typeAndAssembly"> Type name and assembly file separated by a semicolon. 
        /// If assembly file is empty, the semicolon can be omitted.</param>
        public ClassFactoryParams(string typeAndAssembly)
        {
            int semicolon = typeAndAssembly.IndexOf(';');
            if (semicolon == -1)
            {
                TypeName = typeAndAssembly;
            }
            else
            {
                TypeName = typeAndAssembly.Substring(0, semicolon);
                AssemblyFile = typeAndAssembly.Substring(semicolon + 1);
            }
        }

        /// <summary>
        /// Type name. It must be a full name including namespaces, for example:
        /// "System.Reflection.Assembly"
        /// <para>If assembly file name is not specified, you have to specify the qualified name of the assembly,
        /// at least its name (version, etc. are optional), for example:</para>
        /// <para>"namespace.Type,myassembly"</para>
        /// <para>The assembly can be used like this even if it is not referenced in compile type. It will be loaded
        /// provided that it is in the binary directory.</para>
        /// <para>If the assembly file name is specified, you MUST NOT specify anything (assembly, version, etc.)
        /// except the type name.</para>
        /// </summary>
        public string TypeName
        {
            set;
            get;
        }

        /// <summary>
        /// Optional, if specified, the file is loaded by Assembly.LoadFrom() 
        /// and the type is taken from this assembly. In this you MUST NOT specify anything 
        /// except the name of the type in TypeName.
        /// </summary>
        public string AssemblyFile
        {
            set;
            get;
        }

        /// <summary>
        /// If non-null, the arguments will be passed to the constructor, otherwise
        /// a parameterless constructor will be called.
        /// </summary>
        public object[] Arguments
        {
            set;
            get;
        }
    }
}
