/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ai.lib.utils.commandline;
using System.IO;
using System.Reflection;

namespace ai.lib.utils.nunit
{
    [TestFixture]
    public class CommandLine_Test
    {
        #region Tests
        [Test]
        public void Test_Bool()
        { 
            string [] args = new string[] {};
            BoolParameters parsed = new BoolParameters();
            
            // Default values.
            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            DoTest_Bool(args, true, false, "");

            // Long form with '--' at the beginning and default '+'
            args = new string[] { "--boolparam1", "ha-ha", "--bool-param-2" };
            DoTest_Bool(args, true, true, "ha-ha");
            args = new string[] { "--boolparam1", "--bool-param-2", "ha-ha" };
            DoTest_Bool(args, true, true, "ha-ha");

            // Long form with '--' at the beginning and explicit '+'/'-'
            args = new string[] { "--boolparam1-", "--bool-param-2+" };
            DoTest_Bool(args, false, true, "");
            args = new string[] { "--boolparam1+", "--bool-param-2-" };
            DoTest_Bool(args, true, false, "");

            // Short form with '-' at the beginning and default '+'
            args = new string[] { "-b", "-c" };
            DoTest_Bool(args, true, true, "");

            // Combined short form with '-' at the beginning and default '+'
            args = new string[] { "-bc" };
            DoTest_Bool(args, true, true, "");
            args = new string[] { "-cb" };
            DoTest_Bool(args, true, true, "");

            // Short form with '-' at the beginning and explicit '+'/'-'
            args = new string[] { "-b-", "-c+" };
            DoTest_Bool(args, false, true, "");
            args = new string[] { "-b+", "-c-" };
            DoTest_Bool(args, true, false, "");

            // Combined short form with '-' at the beginning and explicit '+'/'-'
            args = new string[] { "-bc-" };
            DoTest_Bool(args, true, false, "");
            args = new string[] { "-bc+" };
            DoTest_Bool(args, true, true, "");
            args = new string[] { "-cb-" };
            DoTest_Bool(args, false, true, "");
            args = new string[] { "-cb+" };
            DoTest_Bool(args, true, true, "");

            // Short form with default parameter
            args = new string[] { "-b-", "-c+", "bla-bla" };
            DoTest_Bool(args, false, true, "bla-bla");
            // Default parameter with spaces
            args = new string[] { "-b-", "-c+", "some silly text" };
            DoTest_Bool(args, false, true, "some silly text");
        }

        [Test]
        public void Test_PropString()
        {
            string[] args = new string[] { "--prop-string-1:hello" };
            PropStringParameters parsed = new PropStringParameters();

            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual("hello", parsed.PropString1.RawValue);

            // Test default value
            args = new string[] {  };
            parsed = new PropStringParameters();

            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual("bla-bla", parsed.PropString1.RawValue);
        }

        /// <summary>
        /// Test if help command works. It is difficult to verify the help text,
        /// so just see if it is running.
        /// </summary>
        [Test]
        public void Test_Help()
        {
            // Make sure it works for the command line descriptor without parameters.
            NoParameters cmdLine1 = new NoParameters();
            string[] args = new string[] { "-?" };
            Assert.IsFalse(Parser.ParseArgumentsWithUsage(args, cmdLine1));
            args = new string[] { "--help" };
            Assert.IsFalse(Parser.ParseArgumentsWithUsage(args, cmdLine1));

            // Make sure it works for the command line descriptor with some parameters.
            BoolParameters cmdLine2 = new BoolParameters();
            args = new string[] { "-?" };
            Assert.IsFalse(Parser.ParseArgumentsWithUsage(args, cmdLine2));
            args = new string[] { "--help" };
            Assert.IsFalse(Parser.ParseArgumentsWithUsage(args, cmdLine2));
        }

        [Test]
        public void Test_StandardParameters()
        {
            // Test definition of properties.
            NoParametersStd parsed = new NoParametersStd();
            string[] args = new string[] { "-d:var1=value1", "-d:var2=value2" };

            Props.Global.Set("var1", null);
            Props.Global.Set("var2", null);
            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual("profile.xml", parsed.ProfileFile);
            Assert.IsNull(parsed.Profile);
            Assert.AreEqual("value1", Props.Global.Get("var1"));
            Assert.AreEqual("value2", Props.Global.Get("var2"));

            // Overwrite a standard variable
            string tmp = Props.Global.Get("bds.VarDir");
            parsed = new NoParametersStd();
            args = new string[] { "-d:bds.VarDir=c:\temp"};
            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual("c:\temp", Props.Global.Get("bds.VarDir"));
            Props.Global.Set("bds.VarDir", tmp);


            // Test a profile.
            Props.Global.Set("var1", null);
            Props.Global.Set("var2", null);
            Props.Global.Set("pvar1", null);
            Props.Global.Set("pvar2", null);
            string testResourcesPath = UTHelperPrivate.GetTestResourcesPath();
            string profileFile = Path.Combine(testResourcesPath, "Profile_Test-profile1.xml");
            args = new string[] { "-d:var1=value1", "-d:var2=value2", "-d:pvar2=overwritten", 
                "--profile:"+profileFile };
            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual(profileFile, parsed.ProfileFile);
            Assert.IsNotNull(parsed.Profile);
            Assert.AreEqual("value1", Props.Global.Get("var1"));
            Assert.AreEqual("value2", Props.Global.Get("var2"));
            Assert.AreEqual("pvalue1", Props.Global.Get("pvar1"));
            Assert.AreEqual("overwritten", Props.Global.Get("pvar2"));

            // Test wrong format of properties
            args = new string[] { "-d:var1*value1", "-d:=value2" };
            Assert.IsFalse(Parser.ParseArguments(args, parsed));

        }

        #endregion

        #region Implementation

        /// <summary>
        /// A command line description without any parameters and attributes.
        /// </summary>
        public class NoParameters
        {
        }

        public class NoParametersStd: StandardCmdLine
        {
        }

        [CommandLine(HelpText="Help text for bool parameters")]
        public class BoolParameters
        {
            [Argument(ArgumentType.AtMostOnce, LongName = "boolparam1",
                ShortName = "b",
                DefaultValue = true,
                HelpText = "Bool parameter 1.")]
            public bool boolParam1;

            // Use the name containing '-' to make sure the parser can tell it apart
            // from the leading '-' and trailing '-'
            [Argument(ArgumentType.AtMostOnce, LongName = "bool-param-2",
                ShortName = "c",
                DefaultValue = false,
                HelpText = "Bool parameter 2.")]
            public bool boolParam2;

            [DefaultArgument(ArgumentType.AtMostOnce, DefaultValue = "", HelpText = "Default argument.")]
            public string defaultParam;
        }

        public class PropStringParameters
        {
            [Argument(ArgumentType.AtMostOnce, LongName = "prop-string-1",
                ShortName = "p",
                DefaultValue = "bla-bla",
                HelpText = "Prop string 1")]
            public PropString PropString1;
        }

        private static void DoTest_Bool(string[] args, bool expected1, bool expected2, string expectedDefault)
        {
            BoolParameters parsed = new BoolParameters();
            Assert.IsTrue(Parser.ParseArguments(args, parsed));
            Assert.AreEqual(expected1, parsed.boolParam1);
            Assert.AreEqual(expected2, parsed.boolParam2);
            Assert.AreEqual(expectedDefault, parsed.defaultParam);
        }

        #endregion
    }
}
