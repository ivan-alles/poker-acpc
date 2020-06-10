using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;

namespace ai.pkr.metastrategy
{
    public static class ChanceAbstractionHelper
    {
        public static IChanceAbstraction CreateFromPropsFile(string fileName)
        {
            Props chanceAbstrProps = XmlSerializerExt.Deserialize<Props>(fileName);
            return CreateFromProps(chanceAbstrProps);
        }

        public static IChanceAbstraction[] CreateFromPropsFiles(string[] fileNames)
        {
            IChanceAbstraction[] result = new IChanceAbstraction[fileNames.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = CreateFromPropsFile(fileNames[i]);
            }
            return result;
        }

        public static IChanceAbstraction CreateFromProps(Props props)
        {
            ClassFactoryParams cfp = new ClassFactoryParams
            {
                TypeName = props.Get("TypeName"),
                AssemblyFile = props.Get("AssemblyFileName"),
                Arguments = new object[] { props }
            };

            if (string.IsNullOrEmpty(cfp.TypeName))
            {
                throw new ApplicationException("Missing required property 'TypeName'");
            }

            IChanceAbstraction ca = ClassFactory.CreateInstance<IChanceAbstraction>(cfp);
            return ca;
        }

    }
}
