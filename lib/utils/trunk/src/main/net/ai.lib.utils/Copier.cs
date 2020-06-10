/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// Copying of objects.
    /// </summary>
    public static class Copier
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// Binary Serialization is used to perform the copy, therefore the object and all members
        /// must be serializable.
        /// <remarks>This can be by 1000 times slower than a custom copy function.</remarks>
        /// </summary>
        public static T DeepCopy<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    } 
}
