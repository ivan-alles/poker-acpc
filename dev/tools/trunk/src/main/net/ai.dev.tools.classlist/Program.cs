/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ai.dev.tools.classlist
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.LoadFrom(args[0]);
            List<Type> types = new List<Type>(assembly.GetTypes());
            types.Sort((x, y) => x.Name.CompareTo(y.Name));
            Console.WriteLine(assembly.FullName);
            foreach (Type type in types)
            {
                Console.WriteLine(type.Name);
            }
        }
    }
}
