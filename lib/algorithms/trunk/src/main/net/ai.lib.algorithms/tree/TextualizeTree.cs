/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Reflection;
using ai.lib.utils;
using System.CodeDom.Compiler;
using System.IO;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Base class for vizualization algorithm context.
    /// </summary>
    public class TextualizeTreeContext<NodeT, IteratorT> : WalkTreePPContext<NodeT, IteratorT>
    {
        /// <summary>
        /// A path to current node. By default is used only when MatchPath is not null.
        /// </summary>
        public string Path
        {
            set;
            get;
        }

        /// <summary>
        /// Convert current node to path string.
        /// </summary>
        public virtual string ToPathString()
        {
            return ToString();
        }
    }

    /// <summary>
    /// A base interface for run-time generated property evaluators.
    /// </summary>
    public interface ITextualizeTreeEval
    {
        /// <summary>
        /// Called to evaluate an expression. 
        /// </summary>
        /// <param name="c">caller class</param>
        /// <param name="t">tree</param>
        /// <param name="s">Stack</param>
        /// <param name="d">Depth</param>
        /// <returns></returns>
        object Evaluate(object c, object t, object s, int d);
    }

    /// <summary>
    /// A base class for classes that represent a tree in text form, for example XML.
    /// </summary>
    public class TextualizeTree<TreeT, NodeT, IteratorT, ContextT> : WalkTreePP<TreeT, NodeT, IteratorT, ContextT> where ContextT : TextualizeTreeContext<NodeT, IteratorT>, new()
    {
        #region Constructors

        public TextualizeTree()
        {
            Culture = CultureInfo.InvariantCulture;
            _compilerParameters.GenerateExecutable = false;
            _compilerParameters.GenerateInMemory = true;
            _compilerParameters.CompilerOptions = "/unsafe";
            _compilerParameters.ReferencedAssemblies.Add("system.dll");
            CompilerParams.ReferencedAssemblies.Add(CodeBase.Get(Assembly.GetExecutingAssembly()));
            _contextT = ConvertGenericType<ContextT>();
            _treeT = ConvertGenericType<TreeT>();

            ShowExpr = new List<ExprFormatter>();
        }

        /// <summary>
        /// Simple generic type parsing.
        /// "ai.lib.algorithms.tree.VisTreeContext`2[[ai.lib.algorithms.nunit.VisTree_Test.TestNode, ai.lib.algorithms.1.nunit, Version=1.0.1125.0, Culture=neutral, PublicKeyToken=null],[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
        /// </summary>
        private string ConvertGenericType<T>()
        {
            string type = typeof(T).FullName;
            type = type.Replace('+', '.');
            if (typeof(T).IsGenericType)
            {
                string[] typeParts = type.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                string typeName = typeParts[0].Substring(0, typeParts[0].IndexOf('`'));
                string genParam1 = typeParts[1].Substring(0, typeParts[1].IndexOf(','));
                string genParam2 = typeParts[3].Substring(0, typeParts[3].IndexOf(','));
                type = typeName + "<" + genParam1 + "," + genParam2 + ">";
            }
            return type;
        }

        #endregion

        /// <summary>
        /// Shows properties of tree nodes, using specified format.
        /// Property is specified as string as ia C# expression called within a function
        /// <para>
        /// object Evaluate(object c, class t, List&lt;Context&gt; s, int d) { return expr; }
        /// </para><para>
        /// where c is the caller class, t is the tree, s is the stack, d is the depth.
        /// </para>
        /// <para>
        /// Example: "s[d].Node" is current node
        /// </para>
        /// <para>
        /// The format is specified as for String.Format("fmt", "Name", value) call.
        /// </para><para>
        /// Examples:
        /// </para>
        /// <para>ShowExpr.Add(new ShowPropParam("s[d].Node.GameState.Round", "{0} = {1}")) prints "s[d].Node.GameState.Round = ACTUAL_VALUE"</para>
        /// <para>ShowExpr.Add(new ShowPropParam("s[d].Node.GameState.Round", "r = {1}"))   prints "r = ACTUAL_VALUE"</para>
        /// </summary>
        /// <seealso cref="CompilerParams"/>
        /// <seealso cref="ShowExprFromString"/>
        public List<ExprFormatter> ShowExpr
        {
            get;
            protected set;
        }

        /// <summary>
        /// Adds parameters to ShowExpr from an array of strings.
        /// </summary>
        public void ShowExprFromString(string[] strings)
        {
            if (strings != null)
            {
                foreach (string p in strings)
                {
                    ShowExpr.Add(new ExprFormatter(p));
                }
            }
        }

        /// <summary>
        /// Sets compiler parameters for run-time compiles used for ShowExpr.
        /// Usually you have to add assembly references:
        /// 
        /// CompilerParams.ReferencedAssemblies.Add(PathResolver.GetCodeBase(Assembly.GetExecutingAssembly()));
        /// 
        /// </summary>
        public CompilerParameters CompilerParams
        {
            get { return _compilerParameters; }
        }

        /// <summary>
        /// All string formatting will be done in context of this culture.
        /// Default value: CultureInfo.InvariantCulture.
        /// </summary>
        public CultureInfo Culture
        {
            set;
            get;
        }

        public override void Walk(TreeT tree, NodeT root)
        {
            CultureInfo cultureInfoBackup = Thread.CurrentThread.CurrentCulture;
            try
            {
                // To format doubles, etc..
                Thread.CurrentThread.CurrentCulture = Culture;
                CreateEvaluators();
                base.Walk(tree, root);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cultureInfoBackup;
            }
        }

        /// <summary>
        /// A path from the root of the tree. Show only nodes that have this path a part of their path.
        /// A path is created by calling Context.ToPathString(), nodes are separated by '/'.
        /// </summary>
        public String MatchPath
        {
            set;
            get;
        }

        #region Protected API

        /// <summary>
        /// Default processing of the node begin. 
        /// </summary>
        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            ContextT context = stack[depth];

            if (MatchPath != null)
            {
                context.Path = (depth > 0 ? stack[depth - 1].Path : "") + "/" + context.ToPathString();
                int minLen = Math.Min(MatchPath.Length, context.Path.Length);
                if (string.Compare(MatchPath, 0, context.Path, 0, minLen) != 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Converts all expressions from ShowExpr to string.
        /// </summary>
        protected string EvalExpressions(TreeT tree, List<ContextT> stack, int depth)
        {
            return EvalExpressions(tree, stack, depth, 0, ShowExpr.Count);
        }

        /// <summary>
        /// Converts count expressions from ShowExpr to string, statring from startIdx
        /// </summary>
        protected string EvalExpressions(TreeT tree, List<ContextT> stack, int depth, int startIdx, int count)
        {
            if (ShowExpr == null)
                return "";
            StringBuilder sb = new StringBuilder();
            for (int i = startIdx; i < startIdx + count; ++i)
            {
                ExprFormatter se = ShowExpr[i];
                object val = _evaluators[se.Expr].Evaluate(this, tree, stack, depth);
                if (val == null)
                    val = "null";
                sb.AppendFormat(se.Format, se.Expr, val);
            }
            return sb.ToString();
        }

        protected virtual void CreateEvaluators()
        {
            _evaluators.Clear();
            if (ShowExpr == null)
                return;
            if(EnvironmentExt.IsUnix())
            {
                // Ticket #86: temporarily disable it for Mono, because
                // RuntimeCompile.Compile() does not work there.
                return;
            }
            foreach (ExprFormatter spp in ShowExpr)
            {
                StringBuilder code = new StringBuilder();
                code.Append("using System;\n");
                code.Append("using System.Collections.Generic;\n");
                code.Append("using ai.lib.algorithms.tree;\n");
                code.Append("public unsafe class _Evaluator: ITextualizeTreeEval {\n");
                code.Append(" public object Evaluate(object c, object oTree, object oStack, int d) {\n");
                code.AppendFormat("  List<{0}> s = (List<{0}>)oStack;", _contextT);
                code.AppendFormat("  {0} t = ({0})oTree;", _treeT);
                code.AppendFormat("  return {0};\n", spp.Expr);
                code.Append("}}");
                Assembly asm = RuntimeCompile.Compile(code.ToString(), _compilerParameters);
                Object evaluator = asm.CreateInstance("_Evaluator");
                _evaluators[spp.Expr] = (ITextualizeTreeEval)evaluator;
            }
        }

        protected Dictionary<string, ITextualizeTreeEval> _evaluators = new Dictionary<string, ITextualizeTreeEval>();

        /// <summary>
        /// Textual name of ContextT.
        /// </summary>
        string _contextT;
        string _treeT;

        CompilerParameters _compilerParameters = new CompilerParameters();

        #endregion

    }
}
