/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
using ai.lib.utils;
using System.CodeDom.Compiler;
using System.Drawing;
using ai.lib.algorithms.color;

namespace ai.lib.algorithms.tree
{

    /// <summary>
    /// Base class for vizualization algorithm context.
    /// </summary>
    public class VisTreeContext<NodeT, IteratorT> : TextualizeTreeContext<NodeT, IteratorT>
    {
        /// <summary>
        /// Numeric node id for GraphViz. VizualizeTree will add "n" to it: n0, n1, ... n1023.
        /// </summary>
        public int GvId
        {
            set;
            get;
        }
    }

    /// <summary>
    /// Visualizes a tree by creating a text file 
    /// that can be opened in GraphViz programs (http://www.graphviz.org).
    /// <para>Can be either used directly or customized in a derived class.</para>
    /// <para>A typical use case:</para>
    /// <para>Create an object, set some properties, that call Write():</para>
    /// <para>VizualizeTree&lt;MyNode&gt; tv = new VizualizeTree&lt;MyNode&gt;{GetChildren = n =&gt; n.MyChildren};</para>
    /// <para>tv.GraphAttributes.label = "My Tree";</para>
    /// <para>tv.Write(root, Console.Out);</para>
    ///  </summary>
    /// <typeparam name="NodeT">Tree node type.</typeparam>
    public class VisTree<TreeT, NodeT, IteratorT, ContextT> : TextualizeTree<TreeT, NodeT, IteratorT, ContextT> where ContextT : VisTreeContext<NodeT, IteratorT>, new() 
    {
        #region Public types

        /// <summary>
        /// Base class for attribute collection. 
        /// See http://www.graphviz.org/doc/info/attrs.html
        /// </summary>
        public class AttributeMap
        {
            public AttributeMap()
            {
                Map = new Dictionary<string, object>();
            }

            public AttributeMap(AttributeMap other)
            {
                Map = new Dictionary<string, object>(other.Map);
            }

            public Dictionary<string, object> Map
            {
                protected set;
                get;
            }
            /// <summary>
            /// Gets attribute value.
            /// </summary>
            /// <param name="attribute">Name of the attribute</param>
            /// <returns>The value if such attribute exists, otherwise null.</returns>
            public object GetValue(string attribute)
            {
                object val = null;
                Map.TryGetValue(attribute, out val);
                return val;
            }
        }

        /// <summary>
        /// Contains graph attributes.
        /// </summary>
        public class GraphAttributeMap: AttributeMap
        {
            public GraphAttributeMap()
            {
            }

            public GraphAttributeMap(GraphAttributeMap other)
                : base(other)
            {
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.99</para>
            ///<para>Minimum: 0.0</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public double Damping
            {
                set { Map["Damping"] = value; }
                get { return (double)GetValue("Damping"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.3</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: sfdp fdp only</para>
            ///</summary>
            public double K
            {
                set { Map["K"] = value; }
                get { return (double)GetValue("K"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg postscript map only</para>
            ///</summary>
            public string URL
            {
                set { Map["URL"] = value; }
                get { return (string)GetValue("URL"); }
            }

            ///<summary>
            ///<para>Type: aspectType.</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string aspect
            {
                set { Map["aspect"] = value; }
                get { return (string)GetValue("aspect"); }
            }

            ///<summary>
            ///<para>Type: rect.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string bb
            {
                set { Map["bb"] = value; }
                get { return (string)GetValue("bb"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///</summary>
            public string bgcolor
            {
                set { Map["bgcolor"] = value; }
                get { return (string)GetValue("bgcolor"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool center
            {
                set { Map["center"] = value; }
                get { return (bool)GetValue("center"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "UTF-8"</para>
            ///</summary>
            public string charset
            {
                set { Map["charset"] = value; }
                get { return (string)GetValue("charset"); }
            }

            ///<summary>
            ///<para>Type: clusterMode.</para>
            ///<para>Default: local</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string clusterrank
            {
                set { Map["clusterrank"] = value; }
                get { return (string)GetValue("clusterrank"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string colorscheme
            {
                set { Map["colorscheme"] = value; }
                get { return (string)GetValue("colorscheme"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string comment
            {
                set { Map["comment"] = value; }
                get { return (string)GetValue("comment"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public bool compound
            {
                set { Map["compound"] = value; }
                get { return (bool)GetValue("compound"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool concentrate
            {
                set { Map["concentrate"] = value; }
                get { return (bool)GetValue("concentrate"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1+(avg. len)*sqrt(|V|)</para>
            ///<para>Minimum: epsilon</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public double defaultdist
            {
                set { Map["defaultdist"] = value; }
                get { return (double)GetValue("defaultdist"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 2</para>
            ///<para>Minimum: 2</para>
            ///<para>Notes: sfdp; fdp; neato only</para>
            ///</summary>
            public int dim
            {
                set { Map["dim"] = value; }
                get { return (int)GetValue("dim"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 2</para>
            ///<para>Minimum: 2</para>
            ///<para>Notes: sfdp; fdp; neato only</para>
            ///</summary>
            public int dimen
            {
                set { Map["dimen"] = value; }
                get { return (int)GetValue("dimen"); }
            }

            ///<summary>
            ///<para>Type: string bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public string diredgeconstraints
            {
                set { Map["diredgeconstraints"] = value; }
                get { return (string)GetValue("diredgeconstraints"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 96.0 0.0</para>
            ///<para>Notes: svg, bitmap output only</para>
            ///</summary>
            public double dpi
            {
                set { Map["dpi"] = value; }
                get { return (double)GetValue("dpi"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: .0001 * # nodes(mode == KK).0001(mode == major)</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public double epsilon
            {
                set { Map["epsilon"] = value; }
                get { return (double)GetValue("epsilon"); }
            }

            ///<summary>
            ///<para>Type: double pointf.</para>
            ///<para>Default: 3</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public string esep
            {
                set { Map["esep"] = value; }
                get { return (string)GetValue("esep"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string fontcolor
            {
                set { Map["fontcolor"] = value; }
                get { return (string)GetValue("fontcolor"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "Times-Roman"</para>
            ///</summary>
            public string fontname
            {
                set { Map["fontname"] = value; }
                get { return (string)GetValue("fontname"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg only</para>
            ///</summary>
            public string fontnames
            {
                set { Map["fontnames"] = value; }
                get { return (string)GetValue("fontnames"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: system-dependent</para>
            ///</summary>
            public string fontpath
            {
                set { Map["fontpath"] = value; }
                get { return (string)GetValue("fontpath"); }
            }

            ///<summary>Font size, in points, used for text. 
            ///<para>Type: double.</para>
            ///<para>Default: 14.0</para>
            ///<para>Minimum: 1.0</para>
            ///</summary>
            public double fontsize
            {
                set { Map["fontsize"] = value; }
                get { return (double)GetValue("fontsize"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string href
            {
                set { Map["href"] = value; }
                get { return (string)GetValue("href"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string id
            {
                set { Map["id"] = value; }
                get { return (string)GetValue("id"); }
            }

            ///<summary>
            ///<para>Type: lblString.</para>
            ///<para>Default: "'N" (nodes) "" (otherwise)</para>
            ///</summary>
            public string label
            {
                set { Map["label"] = value; }
                get { return (string)GetValue("label"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public int label_scheme
            {
                set { Map["label_scheme"] = value; }
                get { return (int)GetValue("label_scheme"); }
            }

            ///<summary>
            /// Justification for cluster labels. 
            /// If "r", the label is right-justified within bounding rectangle; 
            /// if "l", left-justified; 
            /// else the label is centered.
            ///<para>Type: string.</para>
            ///<para>Default: "c"</para>
            ///</summary>
            public string labeljust
            {
                set { Map["labeljust"] = value; }
                get { return (string)GetValue("labeljust"); }
            }

            ///<summary>
            /// Vertical placement of labels for nodes, root graphs and clusters. 
            /// For graphs and clusters, only "t" and "b" are allowed, 
            /// corresponding to placement at the top and bottom, respectively. 
            /// By default, root graph labels go on the bottom and cluster labels go on the top. 
            ///<para>Type: string.</para>
            ///<para>Default: "t"(clusters) "b"(root graphs)"c"(nodes)</para>
            ///</summary>
            public string labelloc
            {
                set { Map["labelloc"] = value; }
                get { return (string)GetValue("labelloc"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool landscape
            {
                set { Map["landscape"] = value; }
                get { return (bool)GetValue("landscape"); }
            }

            ///<summary>
            ///<para>Type: layerList.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string layers
            {
                set { Map["layers"] = value; }
                get { return (string)GetValue("layers"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: " :'t"</para>
            ///</summary>
            public string layersep
            {
                set { Map["layersep"] = value; }
                get { return (string)GetValue("layersep"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string layout
            {
                set { Map["layout"] = value; }
                get { return (string)GetValue("layout"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: MAXINT</para>
            ///<para>Minimum: 0.0</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public int levels
            {
                set { Map["levels"] = value; }
                get { return (int)GetValue("levels"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public double levelsgap
            {
                set { Map["levelsgap"] = value; }
                get { return (double)GetValue("levelsgap"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public double lheight
            {
                set { Map["lheight"] = value; }
                get { return (double)GetValue("lheight"); }
            }

            ///<summary>
            ///<para>Type: point.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string lp
            {
                set { Map["lp"] = value; }
                get { return (string)GetValue("lp"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public double lwidth
            {
                set { Map["lwidth"] = value; }
                get { return (double)GetValue("lwidth"); }
            }

            ///<summary>
            /// For graphs, this sets x and y margins of canvas, in inches. 
            /// If the margin is a single double, both margins are set equal to the given value.
            /// Note that the margin is not part of the drawing but just 
            /// empty space left around the drawing. It basically corresponds 
            /// to a translation of drawing, as would be necessary to center a drawing on a page. 
            /// Nothing is actually drawn in the margin. To actually extend the background
            /// of a drawing, see the pad attribute.
            ///<para>Type: double pointf.</para>
            ///</summary>
            public string margin
            {
                set { Map["margin"] = value; }
                get { return (string)GetValue("margin"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 100 * # nodes(mode == KK) 200(mode == major) 600(fdp)</para>
            ///<para>Notes: fdp, neato only</para>
            ///</summary>
            public int maxiter
            {
                set { Map["maxiter"] = value; }
                get { return (int)GetValue("maxiter"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public double mclimit
            {
                set { Map["mclimit"] = value; }
                get { return (double)GetValue("mclimit"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///<para>Notes: circo only</para>
            ///</summary>
            public double mindist
            {
                set { Map["mindist"] = value; }
                get { return (double)GetValue("mindist"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: major spring</para>
            ///<para>Notes: sfdp, neato only</para>
            ///</summary>
            public string mode
            {
                set { Map["mode"] = value; }
                get { return (string)GetValue("mode"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: shortpath</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public string model
            {
                set { Map["model"] = value; }
                get { return (string)GetValue("model"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: neato only</para>
            ///</summary>
            public bool mosek
            {
                set { Map["mosek"] = value; }
                get { return (bool)GetValue("mosek"); }
            }

            ///<summary>
            ///Minimum space between two adjacent nodes in the same rank, in inches. 
            ///<para>Type: double.</para>
            ///<para>Default: 0.25</para>
            ///<para>Minimum: 0.02</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public double nodesep
            {
                set { Map["nodesep"] = value; }
                get { return (double)GetValue("nodesep"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool nojustify
            {
                set { Map["nojustify"] = value; }
                get { return (bool)GetValue("nojustify"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public bool normalize
            {
                set { Map["normalize"] = value; }
                get { return (bool)GetValue("normalize"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public double nslimit
            {
                set { Map["nslimit"] = value; }
                get { return (double)GetValue("nslimit"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string ordering
            {
                set { Map["ordering"] = value; }
                get { return (string)GetValue("ordering"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string orientation
            {
                set { Map["orientation"] = value; }
                get { return (string)GetValue("orientation"); }
            }

            ///<summary>
            ///<para>Type: outputMode.</para>
            ///<para>Default: breadthfirst</para>
            ///</summary>
            public string outputorder
            {
                set { Map["outputorder"] = value; }
                get { return (string)GetValue("outputorder"); }
            }

            ///<summary>
            ///<para>Type: string bool.</para>
            ///<para>Default: true</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public string overlap
            {
                set { Map["overlap"] = value; }
                get { return (string)GetValue("overlap"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: -4</para>
            ///<para>Minimum: -1.0e10</para>
            ///<para>Notes: prism only</para>
            ///</summary>
            public double overlap_scaling
            {
                set { Map["overlap_scaling"] = value; }
                get { return (double)GetValue("overlap_scaling"); }
            }

            ///<summary>
            ///<para>Type: bool int.</para>
            ///<para>Default: false</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public string pack
            {
                set { Map["pack"] = value; }
                get { return (string)GetValue("pack"); }
            }

            ///<summary>
            ///<para>Type: packMode.</para>
            ///<para>Default: node</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public string packmode
            {
                set { Map["packmode"] = value; }
                get { return (string)GetValue("packmode"); }
            }

            ///<summary>
            ///<para>Type: double pointf.</para>
            ///<para>Default: 0.0555 (4 points)</para>
            ///</summary>
            public string pad
            {
                set { Map["pad"] = value; }
                get { return (string)GetValue("pad"); }
            }

            ///<summary>
            ///<para>Type: double pointf.</para>
            ///</summary>
            public string page
            {
                set { Map["page"] = value; }
                get { return (string)GetValue("page"); }
            }

            ///<summary>
            ///<para>Type: pagedir.</para>
            ///<para>Default: BL</para>
            ///</summary>
            public string pagedir
            {
                set { Map["pagedir"] = value; }
                get { return (string)GetValue("pagedir"); }
            }

            ///<summary>
            ///<para>Type: quadType bool.</para>
            ///<para>Default: normal</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public string quadtree
            {
                set { Map["quadtree"] = value; }
                get { return (string)GetValue("quadtree"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double quantum
            {
                set { Map["quantum"] = value; }
                get { return (double)GetValue("quantum"); }
            }

            ///<summary>
            ///<para>Type: rankdir.</para>
            ///<para>Default: TB</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string rankdir
            {
                set { Map["rankdir"] = value; }
                get { return (string)GetValue("rankdir"); }
            }

            ///<summary>
            /// In dot, this gives the desired rank separation, in inches. 
            /// This is the minimum vertical distance between the bottom of 
            /// the nodes in one rank and the tops of nodes in the next. 
            /// If the value contains "equally", the centers of all 
            /// ranks are spaced equally apart. Note that both settings 
            /// are possible, e.g., ranksep = "1.2 equally". 
            ///<para>Type: double doubleList.</para>
            ///<para>Default: 0.5(dot) 1.0(twopi)</para>
            ///<para>Minimum: 0.02</para>
            ///<para>Notes: twopi, dot only</para>
            ///</summary>
            public string ranksep
            {
                set { Map["ranksep"] = value; }
                get { return (string)GetValue("ranksep"); }
            }

            ///<summary>
            ///<para>Type: double string.</para>
            ///</summary>
            public string ratio
            {
                set { Map["ratio"] = value; }
                get { return (string)GetValue("ratio"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public bool remincross
            {
                set { Map["remincross"] = value; }
                get { return (bool)GetValue("remincross"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public double repulsiveforce
            {
                set { Map["repulsiveforce"] = value; }
                get { return (double)GetValue("repulsiveforce"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 96.0 0.0</para>
            ///<para>Notes: svg, bitmap output only</para>
            ///</summary>
            public double resolution
            {
                set { Map["resolution"] = value; }
                get { return (double)GetValue("resolution"); }
            }

            ///<summary>
            ///<para>Type: string bool.</para>
            ///<para>Notes: circo, twopi only</para>
            ///</summary>
            public string root
            {
                set { Map["root"] = value; }
                get { return (string)GetValue("root"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///</summary>
            public int rotate
            {
                set { Map["rotate"] = value; }
                get { return (int)GetValue("rotate"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public double rotation
            {
                set { Map["rotation"] = value; }
                get { return (double)GetValue("rotation"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 30</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public int searchsize
            {
                set { Map["searchsize"] = value; }
                get { return (int)GetValue("searchsize"); }
            }

            ///<summary>
            ///<para>Type: double pointf.</para>
            ///<para>Default: 4</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public string sep
            {
                set { Map["sep"] = value; }
                get { return (string)GetValue("sep"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public int showboxes
            {
                set { Map["showboxes"] = value; }
                get { return (int)GetValue("showboxes"); }
            }

            ///<summary>
            ///<para>Type: double pointf.</para>
            ///</summary>
            public string size
            {
                set { Map["size"] = value; }
                get { return (string)GetValue("size"); }
            }

            ///<summary>
            ///<para>Type: smoothType.</para>
            ///<para>Default: "none"</para>
            ///<para>Notes: sfdp only</para>
            ///</summary>
            public string smoothing
            {
                set { Map["smoothing"] = value; }
                get { return (string)GetValue("smoothing"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int sortv
            {
                set { Map["sortv"] = value; }
                get { return (int)GetValue("sortv"); }
            }

            ///<summary>
            ///<para>Type: bool string.</para>
            ///</summary>
            public string splines
            {
                set { Map["splines"] = value; }
                get { return (string)GetValue("splines"); }
            }

            ///<summary>
            ///<para>Type: startType.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: fdp, neato only</para>
            ///</summary>
            public string start
            {
                set { Map["start"] = value; }
                get { return (string)GetValue("start"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg only</para>
            ///</summary>
            public string stylesheet
            {
                set { Map["stylesheet"] = value; }
                get { return (string)GetValue("stylesheet"); }
            }

            ///<summary>
            ///<para>Type: escString string.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string target
            {
                set { Map["target"] = value; }
                get { return (string)GetValue("target"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Notes: bitmap output only</para>
            ///</summary>
            public bool truecolor
            {
                set { Map["truecolor"] = value; }
                get { return (bool)GetValue("truecolor"); }
            }

            ///<summary>
            ///<para>Type: viewPort.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string viewport
            {
                set { Map["viewport"] = value; }
                get { return (string)GetValue("viewport"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.05</para>
            ///<para>Minimum: 0.0</para>
            ///<para>Notes: not dot</para>
            ///</summary>
            public double voro_margin
            {
                set { Map["voro_margin"] = value; }
                get { return (double)GetValue("voro_margin"); }
            }

        }

        /// <summary>
        /// Contains cluster attributes.
        /// </summary>
        public class ClusterAttributeMap : AttributeMap
        {
            public ClusterAttributeMap()
            {
            }

            public ClusterAttributeMap(ClusterAttributeMap other)
                : base(other)
            {
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.3</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: sfdp fdp only</para>
            ///</summary>
            public double K
            {
                set { Map["K"] = value; }
                get { return (double)GetValue("K"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg postscript map only</para>
            ///</summary>
            public string URL
            {
                set { Map["URL"] = value; }
                get { return (string)GetValue("URL"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///</summary>
            public string bgcolor
            {
                set { Map["bgcolor"] = value; }
                get { return (string)GetValue("bgcolor"); }
            }

            ///<summary>
            ///<para>Type: color colorList.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string color
            {
                set { Map["color"] = value; }
                get { return (string)GetValue("color"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string colorscheme
            {
                set { Map["colorscheme"] = value; }
                get { return (string)GetValue("colorscheme"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: lightgrey(nodes) black(clusters)</para>
            ///</summary>
            public string fillcolor
            {
                set { Map["fillcolor"] = value; }
                get { return (string)GetValue("fillcolor"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string fontcolor
            {
                set { Map["fontcolor"] = value; }
                get { return (string)GetValue("fontcolor"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "Times-Roman"</para>
            ///</summary>
            public string fontname
            {
                set { Map["fontname"] = value; }
                get { return (string)GetValue("fontname"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 14.0</para>
            ///<para>Minimum: 1.0</para>
            ///</summary>
            public double fontsize
            {
                set { Map["fontsize"] = value; }
                get { return (double)GetValue("fontsize"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string href
            {
                set { Map["href"] = value; }
                get { return (string)GetValue("href"); }
            }

            ///<summary>
            ///<para>Type: lblString.</para>
            ///<para>Default: "'N" (nodes) "" (otherwise)</para>
            ///</summary>
            public string label
            {
                set { Map["label"] = value; }
                get { return (string)GetValue("label"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "c"</para>
            ///</summary>
            public string labeljust
            {
                set { Map["labeljust"] = value; }
                get { return (string)GetValue("labeljust"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "t"(clusters) "b"(root graphs)"c"(nodes)</para>
            ///</summary>
            public string labelloc
            {
                set { Map["labelloc"] = value; }
                get { return (string)GetValue("labelloc"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public double lheight
            {
                set { Map["lheight"] = value; }
                get { return (double)GetValue("lheight"); }
            }

            ///<summary>
            ///<para>Type: point.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string lp
            {
                set { Map["lp"] = value; }
                get { return (string)GetValue("lp"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public double lwidth
            {
                set { Map["lwidth"] = value; }
                get { return (double)GetValue("lwidth"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool nojustify
            {
                set { Map["nojustify"] = value; }
                get { return (bool)GetValue("nojustify"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string pencolor
            {
                set { Map["pencolor"] = value; }
                get { return (string)GetValue("pencolor"); }
            }

            ///<summary>
            /// Specifies the width of the pen, in points, used to draw lines and curves, 
            /// including the boundaries of edges and clusters. 
            /// The value is inherited by subclusters. It has no effect on text. 
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double penwidth
            {
                set { Map["penwidth"] = value; }
                get { return (double)GetValue("penwidth"); }
            }

            ///<summary>            
            /// Set number of peripheries used in polygonal shapes and cluster boundaries. 
            /// Note that user-defined shapes are treated as a form of box shape, 
            /// so the default peripheries value is 1 and the user-defined shape 
            /// will be drawn in a bounding rectangle. Setting peripheries=0 will turn this off. 
            /// Also, 1 is the maximum peripheries value for clusters. 
            ///<para>Type: int.</para>
            ///<para>Default: shape default(nodes) 1(clusters)</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int peripheries
            {
                set { Map["peripheries"] = value; }
                get { return (int)GetValue("peripheries"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int sortv
            {
                set { Map["sortv"] = value; }
                get { return (int)GetValue("sortv"); }
            }

            ///<summary>
            /// At present, the recognized style names are 
            /// "dashed", "dotted", "solid", "invis" and "bold" for nodes and edges, 
            /// and "filled", "diagonals" and "rounded" for nodes only. The styles "filled" and "rounded" are recognized for clusters.
            /// Additional styles are available in device-dependent form. 
            /// Style lists are passed to device drivers, which can use this to generate appropriate output. 
            /// <para>Type: style.</para>
            ///</summary>
            public string style
            {
                set { Map["style"] = value; }
                get { return (string)GetValue("style"); }
            }

            ///<summary>
            ///<para>Type: escString string.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string target
            {
                set { Map["target"] = value; }
                get { return (string)GetValue("target"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string tooltip
            {
                set { Map["tooltip"] = value; }
                get { return (string)GetValue("tooltip"); }
            }
        }

        /// <summary>
        /// Contains node attributes.
        /// </summary>
        public class NodeAttributeMap : AttributeMap
        {

            public NodeAttributeMap()
            {
            }

            public NodeAttributeMap(NodeAttributeMap other): base(other)
            {
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg postscript map only</para>
            ///</summary>
            public string URL
            {
                set { Map["URL"] = value; }
                get { return (string)GetValue("URL"); }
            }

            ///<summary>
            ///<para>Type: color colorList.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string color
            {
                set { Map["color"] = value; }
                get { return (string)GetValue("color"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string colorscheme
            {
                set { Map["colorscheme"] = value; }
                get { return (string)GetValue("colorscheme"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string comment
            {
                set { Map["comment"] = value; }
                get { return (string)GetValue("comment"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Minimum: -100.0</para>
            ///</summary>
            public double distortion
            {
                set { Map["distortion"] = value; }
                get { return (double)GetValue("distortion"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: lightgrey(nodes) black(clusters)</para>
            ///</summary>
            public string fillcolor
            {
                set { Map["fillcolor"] = value; }
                get { return (string)GetValue("fillcolor"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool fixedsize
            {
                set { Map["fixedsize"] = value; }
                get { return (bool)GetValue("fixedsize"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string fontcolor
            {
                set { Map["fontcolor"] = value; }
                get { return (string)GetValue("fontcolor"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "Times-Roman"</para>
            ///</summary>
            public string fontname
            {
                set { Map["fontname"] = value; }
                get { return (string)GetValue("fontname"); }
            }

            ///<summary>
            /// Font size, in points, used for text. 
            ///<para>Type: double.</para>
            ///<para>Default: 14.0</para>
            ///<para>Minimum: 1.0</para>
            ///</summary>
            public double fontsize
            {
                set { Map["fontsize"] = value; }
                get { return (double)GetValue("fontsize"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string group
            {
                set { Map["group"] = value; }
                get { return (string)GetValue("group"); }
            }

            ///<summary>
            /// Height of node, in inches. This is taken as the initial, 
            /// minimum height of the node. If fixedsize is true, this will be the final height of the node. 
            /// Otherwise, if the node label requires more height to fit, 
            /// the node's height will be increased to contain the label. 
            /// Note also that, if the output format is dot, the value given to height will be the final value.
            /// If the node shape is regular, the width and height are made identical. 
            /// In this case, if either the width or the height is set explicitly, that value is used. 
            /// In this case, if both the width or the height are set explicitly, 
            /// the maximum of the two values is used. If neither is set explicitly,
            /// the minimum of the two default values is used. 
            ///<para>Type: double.</para>
            ///<para>Default: 0.5</para>
            ///<para>Minimum: 0.02</para>
            ///</summary>
            public double height
            {
                set { Map["height"] = value; }
                get { return (double)GetValue("height"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string href
            {
                set { Map["href"] = value; }
                get { return (string)GetValue("href"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string id
            {
                set { Map["id"] = value; }
                get { return (string)GetValue("id"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string image
            {
                set { Map["image"] = value; }
                get { return (string)GetValue("image"); }
            }

            ///<summary>
            ///<para>Type: bool string.</para>
            ///<para>Default: false</para>
            ///</summary>
            public string imagescale
            {
                set { Map["imagescale"] = value; }
                get { return (string)GetValue("imagescale"); }
            }

            ///<summary>
            ///<para>Type: lblString.</para>
            ///<para>Default: "'N" (nodes) "" (otherwise)</para>
            ///</summary>
            public string label
            {
                set { Map["label"] = value; }
                get { return (string)GetValue("label"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "t"(clusters) "b"(root graphs)"c"(nodes)</para>
            ///</summary>
            public string labelloc
            {
                set { Map["labelloc"] = value; }
                get { return (string)GetValue("labelloc"); }
            }

            ///<summary>
            ///<para>Type: layerRange.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string layer
            {
                set { Map["layer"] = value; }
                get { return (string)GetValue("layer"); }
            }

            ///<summary>
            /// For nodes, this attribute specifies space left around the node's label.
            /// By default, the value is 0.11,0.055. 
            ///<para>Type: double pointf.</para>
            ///<para>Default: device-dependent</para>
            ///</summary>
            public string margin
            {
                set { Map["margin"] = value; }
                get { return (string)GetValue("margin"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool nojustify
            {
                set { Map["nojustify"] = value; }
                get { return (bool)GetValue("nojustify"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string ordering
            {
                set { Map["ordering"] = value; }
                get { return (string)GetValue("ordering"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Minimum: 360.0</para>
            ///</summary>
            public double orientation
            {
                set { Map["orientation"] = value; }
                get { return (double)GetValue("orientation"); }
            }

            ///<summary>
            /// Specifies the width of the pen, in points, used to draw lines and curves, 
            /// including the boundaries of edges and clusters. 
            /// The value is inherited by subclusters. It has no effect on text. 
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double penwidth
            {
                set { Map["penwidth"] = value; }
                get { return (double)GetValue("penwidth"); }
            }

            ///<summary>
            /// Set number of peripheries used in polygonal shapes and cluster boundaries. 
            /// Note that user-defined shapes are treated as a form of box shape, 
            /// so the default peripheries value is 1 and the user-defined shape 
            /// will be drawn in a bounding rectangle. Setting peripheries=0 will turn this off. 
            /// Also, 1 is the maximum peripheries value for clusters. 
            /// <para>Type: int.</para>
            ///<para>Default: shape default(nodes) 1(clusters)</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int peripheries
            {
                set { Map["peripheries"] = value; }
                get { return (int)GetValue("peripheries"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: fdp, neato only</para>
            ///</summary>
            public bool pin
            {
                set { Map["pin"] = value; }
                get { return (bool)GetValue("pin"); }
            }

            ///<summary>
            ///<para>Type: point splineType.</para>
            ///</summary>
            public string pos
            {
                set { Map["pos"] = value; }
                get { return (string)GetValue("pos"); }
            }

            ///<summary>
            ///<para>Type: rect.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string rects
            {
                set { Map["rects"] = value; }
                get { return (string)GetValue("rects"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool regular
            {
                set { Map["regular"] = value; }
                get { return (bool)GetValue("regular"); }
            }

            ///<summary>
            ///<para>Type: string bool.</para>
            ///<para>Default: false</para>
            ///<para>Notes: circo, twopi only</para>
            ///</summary>
            public string root
            {
                set { Map["root"] = value; }
                get { return (string)GetValue("root"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 8(output) 20(overlap and image maps)</para>
            ///</summary>
            public int samplepoints
            {
                set { Map["samplepoints"] = value; }
                get { return (int)GetValue("samplepoints"); }
            }

            ///<summary>
            /// Shape of a node. 
            /// See http://www.graphviz.org/doc/info/attrs.html#k:shape
            ///<para>Type: shape.</para>
            ///<para>Default: ellipse</para>
            ///</summary>
            public string shape
            {
                set { Map["shape"] = value; }
                get { return (string)GetValue("shape"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string shapefile
            {
                set { Map["shapefile"] = value; }
                get { return (string)GetValue("shapefile"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public int showboxes
            {
                set { Map["showboxes"] = value; }
                get { return (int)GetValue("showboxes"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 4</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int sides
            {
                set { Map["sides"] = value; }
                get { return (int)GetValue("sides"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Minimum: -100.0</para>
            ///</summary>
            public double skew
            {
                set { Map["skew"] = value; }
                get { return (double)GetValue("skew"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///</summary>
            public int sortv
            {
                set { Map["sortv"] = value; }
                get { return (int)GetValue("sortv"); }
            }

            ///<summary>
            /// At present, the recognized style names are 
            /// "dashed", "dotted", "solid", "invis" and "bold" for nodes and edges, 
            /// and "filled", "diagonals" and "rounded" for nodes only. The styles "filled" and "rounded" are recognized for clusters.
            /// Additional styles are available in device-dependent form. 
            /// Style lists are passed to device drivers, which can use this to generate appropriate output. 
            /// <para>Type: style.</para>
            ///</summary>
            public string style
            {
                set { Map["style"] = value; }
                get { return (string)GetValue("style"); }
            }

            ///<summary>
            ///<para>Type: escString string.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string target
            {
                set { Map["target"] = value; }
                get { return (string)GetValue("target"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string tooltip
            {
                set { Map["tooltip"] = value; }
                get { return (string)GetValue("tooltip"); }
            }

            ///<summary>
            ///<para>Type: pointfList.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string vertices
            {
                set { Map["vertices"] = value; }
                get { return (string)GetValue("vertices"); }
            }

            ///<summary>
            /// Width of node, in inches. This is taken as the initial, minimum width of the node. 
            /// If fixedsize is true, this will be the final width of the node.
            /// Otherwise, if the node label requires more width to fit, the node's width will be increased to contain the label. Note also that, if the output format is dot, the value given to width will be the final value.
            /// If the node shape is regular, the width and height are made identical. 
            /// In this case, if either the width or the height is set explicitly, that value is used. 
            /// In this case, if both the width or the height are set explicitly, 
            /// the maximum of the two values is used. If neither is set explicitly, 
            /// the minimum of the two default values is used. 
            /// <para>Type: double.</para>
            ///<para>Default: 0.75</para>
            ///<para>Minimum: 0.01</para>
            ///</summary>
            public double width
            {
                set { Map["width"] = value; }
                get { return (double)GetValue("width"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 0.0</para>
            ///<para>Minimum: -MAXFLOAT -1000</para>
            ///</summary>
            public double z
            {
                set { Map["z"] = value; }
                get { return (double)GetValue("z"); }
            }
        }

        /// <summary>
        /// Contains edge attributes.
        /// </summary>
        public class EdgeAttributeMap : AttributeMap
        {
            public EdgeAttributeMap()
            {
            }

            public EdgeAttributeMap(EdgeAttributeMap other)
                : base(other)
            {
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg postscript map only</para>
            ///</summary>
            public string URL
            {
                set { Map["URL"] = value; }
                get { return (string)GetValue("URL"); }
            }

            ///<summary>
            /// Style of arrowhead on the head node of an edge. 
            /// This will only appear if the dir attribute is "forward" or "both". 
            /// Possible values: http://www.graphviz.org/doc/info/attrs.html#k:arrowType
            ///<para>Type: arrowType.</para>
            ///<para>Default: normal</para>
            ///</summary>
            public string arrowhead
            {
                set { Map["arrowhead"] = value; }
                get { return (string)GetValue("arrowhead"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double arrowsize
            {
                set { Map["arrowsize"] = value; }
                get { return (double)GetValue("arrowsize"); }
            }

            ///<summary>
            ///<para>Type: arrowType.</para>
            ///<para>Default: normal</para>
            ///</summary>
            public string arrowtail
            {
                set { Map["arrowtail"] = value; }
                get { return (string)GetValue("arrowtail"); }
            }

            ///<summary>
            ///<para>Type: color colorList.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string color
            {
                set { Map["color"] = value; }
                get { return (string)GetValue("color"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string colorscheme
            {
                set { Map["colorscheme"] = value; }
                get { return (string)GetValue("colorscheme"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string comment
            {
                set { Map["comment"] = value; }
                get { return (string)GetValue("comment"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: true</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public bool constraint
            {
                set { Map["constraint"] = value; }
                get { return (bool)GetValue("constraint"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool decorate
            {
                set { Map["decorate"] = value; }
                get { return (bool)GetValue("decorate"); }
            }

            ///<summary>
            ///<para>Type: dirType.</para>
            ///<para>Default: forward(directed) none(undirected)</para>
            ///</summary>
            public string dir
            {
                set { Map["dir"] = value; }
                get { return (string)GetValue("dir"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string edgeURL
            {
                set { Map["edgeURL"] = value; }
                get { return (string)GetValue("edgeURL"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg; map only</para>
            ///</summary>
            public string edgehref
            {
                set { Map["edgehref"] = value; }
                get { return (string)GetValue("edgehref"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg; map only</para>
            ///</summary>
            public string edgetarget
            {
                set { Map["edgetarget"] = value; }
                get { return (string)GetValue("edgetarget"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg; cmap only</para>
            ///</summary>
            public string edgetooltip
            {
                set { Map["edgetooltip"] = value; }
                get { return (string)GetValue("edgetooltip"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string fontcolor
            {
                set { Map["fontcolor"] = value; }
                get { return (string)GetValue("fontcolor"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "Times-Roman"</para>
            ///</summary>
            public string fontname
            {
                set { Map["fontname"] = value; }
                get { return (string)GetValue("fontname"); }
            }

            ///<summary>
            /// Font size, in points, used for text. 
            ///<para>Type: double.</para>
            ///<para>Default: 14.0</para>
            ///<para>Minimum: 1.0</para>
            ///</summary>
            public double fontsize
            {
                set { Map["fontsize"] = value; }
                get { return (double)GetValue("fontsize"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string headURL
            {
                set { Map["headURL"] = value; }
                get { return (string)GetValue("headURL"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: true</para>
            ///</summary>
            public bool headclip
            {
                set { Map["headclip"] = value; }
                get { return (bool)GetValue("headclip"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string headhref
            {
                set { Map["headhref"] = value; }
                get { return (string)GetValue("headhref"); }
            }

            ///<summary>
            /// Text label to be placed near head of edge. 
            ///<para>Type: lblString.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string headlabel
            {
                set { Map["headlabel"] = value; }
                get { return (string)GetValue("headlabel"); }
            }

            ///<summary>
            ///<para>Type: portPos.</para>
            ///<para>Default: center</para>
            ///</summary>
            public string headport
            {
                set { Map["headport"] = value; }
                get { return (string)GetValue("headport"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string headtarget
            {
                set { Map["headtarget"] = value; }
                get { return (string)GetValue("headtarget"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string headtooltip
            {
                set { Map["headtooltip"] = value; }
                get { return (string)GetValue("headtooltip"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string href
            {
                set { Map["href"] = value; }
                get { return (string)GetValue("href"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, postscript, map only</para>
            ///</summary>
            public string id
            {
                set { Map["id"] = value; }
                get { return (string)GetValue("id"); }
            }

            ///<summary>
            ///<para>Type: lblString.</para>
            ///<para>Default: "'N" (nodes) "" (otherwise)</para>
            ///</summary>
            public string label
            {
                set { Map["label"] = value; }
                get { return (string)GetValue("label"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string labelURL
            {
                set { Map["labelURL"] = value; }
                get { return (string)GetValue("labelURL"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: -25.0</para>
            ///<para>Minimum: -180.0</para>
            ///</summary>
            public double labelangle
            {
                set { Map["labelangle"] = value; }
                get { return (double)GetValue("labelangle"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double labeldistance
            {
                set { Map["labeldistance"] = value; }
                get { return (double)GetValue("labeldistance"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool labelfloat
            {
                set { Map["labelfloat"] = value; }
                get { return (bool)GetValue("labelfloat"); }
            }

            ///<summary>
            ///<para>Type: color.</para>
            ///<para>Default: black</para>
            ///</summary>
            public string labelfontcolor
            {
                set { Map["labelfontcolor"] = value; }
                get { return (string)GetValue("labelfontcolor"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: "Times-Roman"</para>
            ///</summary>
            public string labelfontname
            {
                set { Map["labelfontname"] = value; }
                get { return (string)GetValue("labelfontname"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 14.0</para>
            ///<para>Minimum: 1.0</para>
            ///</summary>
            public double labelfontsize
            {
                set { Map["labelfontsize"] = value; }
                get { return (double)GetValue("labelfontsize"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string labelhref
            {
                set { Map["labelhref"] = value; }
                get { return (string)GetValue("labelhref"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string labeltarget
            {
                set { Map["labeltarget"] = value; }
                get { return (string)GetValue("labeltarget"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string labeltooltip
            {
                set { Map["labeltooltip"] = value; }
                get { return (string)GetValue("labeltooltip"); }
            }

            ///<summary>
            ///<para>Type: layerRange.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string layer
            {
                set { Map["layer"] = value; }
                get { return (string)GetValue("layer"); }
            }

            ///<summary>
            /// Preferred edge length, in inches.
            ///<para>Type: double.</para>
            ///<para>Default: 1.0(neato) 0.3(fdp)</para>
            ///<para>Notes: fdp, neato only</para>
            ///</summary>
            public double len
            {
                set { Map["len"] = value; }
                get { return (double)GetValue("len"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string lhead
            {
                set { Map["lhead"] = value; }
                get { return (string)GetValue("lhead"); }
            }

            ///<summary>
            ///<para>Type: point.</para>
            ///<para>Notes: write only</para>
            ///</summary>
            public string lp
            {
                set { Map["lp"] = value; }
                get { return (string)GetValue("lp"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string ltail
            {
                set { Map["ltail"] = value; }
                get { return (string)GetValue("ltail"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 1</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public int minlen
            {
                set { Map["minlen"] = value; }
                get { return (int)GetValue("minlen"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: false</para>
            ///</summary>
            public bool nojustify
            {
                set { Map["nojustify"] = value; }
                get { return (bool)GetValue("nojustify"); }
            }

            ///<summary>
            /// Specifies the width of the pen, in points, used to draw lines and curves, 
            /// including the boundaries of edges and clusters. 
            /// The value is inherited by subclusters. It has no effect on text. 
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0.0</para>
            ///</summary>
            public double penwidth
            {
                set { Map["penwidth"] = value; }
                get { return (double)GetValue("penwidth"); }
            }

            ///<summary>
            ///<para>Type: point splineType.</para>
            ///</summary>
            public string pos
            {
                set { Map["pos"] = value; }
                get { return (string)GetValue("pos"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string samehead
            {
                set { Map["samehead"] = value; }
                get { return (string)GetValue("samehead"); }
            }

            ///<summary>
            ///<para>Type: string.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public string sametail
            {
                set { Map["sametail"] = value; }
                get { return (string)GetValue("sametail"); }
            }

            ///<summary>
            ///<para>Type: int.</para>
            ///<para>Default: 0</para>
            ///<para>Minimum: 0</para>
            ///<para>Notes: dot only</para>
            ///</summary>
            public int showboxes
            {
                set { Map["showboxes"] = value; }
                get { return (int)GetValue("showboxes"); }
            }

            ///<summary>
            /// At present, the recognized style names are 
            /// "dashed", "dotted", "solid", "invis" and "bold" for nodes and edges, 
            /// and "filled", "diagonals" and "rounded" for nodes only. The styles "filled" and "rounded" are recognized for clusters.
            /// Additional styles are available in device-dependent form. 
            /// Style lists are passed to device drivers, which can use this to generate appropriate output. 
            ///<para>Type: style.</para>
            ///</summary>
            public string style
            {
                set { Map["style"] = value; }
                get { return (string)GetValue("style"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string tailURL
            {
                set { Map["tailURL"] = value; }
                get { return (string)GetValue("tailURL"); }
            }

            ///<summary>
            ///<para>Type: bool.</para>
            ///<para>Default: true</para>
            ///</summary>
            public bool tailclip
            {
                set { Map["tailclip"] = value; }
                get { return (bool)GetValue("tailclip"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string tailhref
            {
                set { Map["tailhref"] = value; }
                get { return (string)GetValue("tailhref"); }
            }

            ///<summary>
            /// Text label to be placed near tail of edge. 
            ///<para>Type: lblString.</para>
            ///<para>Default: ""</para>
            ///</summary>
            public string taillabel
            {
                set { Map["taillabel"] = value; }
                get { return (string)GetValue("taillabel"); }
            }

            ///<summary>
            ///<para>Type: portPos.</para>
            ///<para>Default: center</para>
            ///</summary>
            public string tailport
            {
                set { Map["tailport"] = value; }
                get { return (string)GetValue("tailport"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string tailtarget
            {
                set { Map["tailtarget"] = value; }
                get { return (string)GetValue("tailtarget"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string tailtooltip
            {
                set { Map["tailtooltip"] = value; }
                get { return (string)GetValue("tailtooltip"); }
            }

            ///<summary>
            ///<para>Type: escString string.</para>
            ///<para>Notes: svg, map only</para>
            ///</summary>
            public string target
            {
                set { Map["target"] = value; }
                get { return (string)GetValue("target"); }
            }

            ///<summary>
            ///<para>Type: escString.</para>
            ///<para>Default: ""</para>
            ///<para>Notes: svg, cmap only</para>
            ///</summary>
            public string tooltip
            {
                set { Map["tooltip"] = value; }
                get { return (string)GetValue("tooltip"); }
            }

            ///<summary>
            ///<para>Type: double.</para>
            ///<para>Default: 1.0</para>
            ///<para>Minimum: 0(dot) 1(neato fdp sfdp)</para>
            ///</summary>
            public double weight
            {
                set { Map["weight"] = value; }
                get { return (double)GetValue("weight"); }
            }

        }

        #endregion


        #region Constructors

        public VisTree()
        {
            // Add default expression to show node as string.
            ShowExpr.Add(new ExprFormatter("s[d].Node.ToString();{1}"));
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Attributes of the graph.
        /// </summary>
        public GraphAttributeMap GraphAttributes
        {
            set { _graphAttributes = value; }
            get { return _graphAttributes; }
        }

        public ClusterAttributeMap ClusterAttributes
        {
            set { _clusterAttributes = value; }
            get { return _clusterAttributes; }
        }

        /// <summary>
        /// Attributes applied to all nodes. Are written once at the beginning of the graph.
        /// </summary>
        public NodeAttributeMap NodeAttributes
        {
            set { _nodeAttributes = value; }
            get { return _nodeAttributes; }
        }

        /// <summary>
        /// Attributes applied to all edges. Are written once at the beginning of the graph.
        /// </summary>
        public EdgeAttributeMap EdgeAttributes
        {
            set { _edgeAttributes = value; }
            get { return _edgeAttributes; }
        }

        public TextWriter Output
        {
            set { _output = value; }
            get { return _output; }
        }

        #endregion

        #region WalkTreePP protected overridables

        /// <summary>
        /// Writes beginning of the Graphviz graph. Override to customize graph visualization.
        /// </summary>
        protected override void OnTreeBeginFunc(TreeT tree, NodeT root)
        {
            _nextNodeId = 0;
            _output.WriteLine("digraph G {");

            // Write global attributes for graph, node and edge.
            WriteGraph(GraphAttributes);

            _output.Write("node");
            WriteAttributes(NodeAttributes);
            _output.WriteLine(";");

            _output.Write("edge");
            WriteAttributes(EdgeAttributes);
            _output.WriteLine(";");
        }

        /// <summary>
        /// Writes end of the Graphviz graph. Override to customize graph visualization.
        /// </summary>
        protected override void OnTreeEndFunc(TreeT tree, NodeT root)
        {
            _output.WriteLine("}");
        }

        /// <summary>
        /// Default processing of the node begin. 
        /// </summary>
        protected override bool OnNodeBeginFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            ContextT context = stack[depth];
            context.GvId = _nextNodeId++;
            return base.OnNodeBeginFunc(tree, node, stack, depth);
        }

        /// <summary>
        /// Calls VisNode() and than VisEdge(). It is recommended to override these
        /// methods to customize visualization.
        /// </summary>
        protected override void OnNodeEndFunc(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            VisNode(tree, node, stack, depth);
            if (depth > 0)
                VisEdge(tree, node, stack[depth-1].Node, stack, depth);
        }

        #endregion

        #region Protected virtual functions to override in the derived classes

        /// <summary>
        /// Is called on each node from OnNodeEnd(), before calling VisEdge. By default:
        /// <para>1. Creates an empty instance of NodeAttributeMap. </para>
        /// <para>2. Calls CustomizeNodeAttributes(). </para>
        /// <para>3. Calls WriteNode().</para>
        /// <para>Override to completely redefine node visualization</para>
        /// </summary>
        protected virtual void VisNode(TreeT tree, NodeT node, List<ContextT> stack, int depth)
        {
            NodeAttributeMap attr = new NodeAttributeMap();
            CustomizeNodeAttributes(tree, node, stack, depth, attr);
            WriteNode(stack[depth].GvId, attr);
        }

        /// <summary>
        /// Override this to customize attributes of the node. Default implementation 
        /// shows EvalExpressions(tree, stack, depth). An expression showing s[d].Node.ToString() is added by default.
        /// </summary>
        /// <param name="attr">Contains a copy of EdgeAttributes. Default CustomizeEdge() can modify it depending on the settings.</param>
        protected virtual void CustomizeNodeAttributes(TreeT tree, NodeT node, List<ContextT> stack, int depth,
            NodeAttributeMap attr)
        {
            ContextT context = stack[depth];
            attr.label = EvalExpressions(tree, stack, depth);
        }

        /// <summary>
        /// Is called on each edge. By default:
        /// <para>1. Creates an empty instance of EdgeAttributeMap. </para>
        /// <para>2. Calls CustomizeEdgeAttributes(). </para>
        /// <para>3. Calls WriteEdge().</para>
        /// <para>Override to completely redefine edge visualization</para>
        /// </summary>
        protected virtual void VisEdge(TreeT tree, NodeT node, NodeT parent, List<ContextT> stack, int depth)
        {
            EdgeAttributeMap attr = new EdgeAttributeMap();
            CustomizeEdgeAttributes(tree, node, stack[depth - 1].Node, stack, depth, attr);
            WriteEdge(stack[depth - 1].GvId, stack[depth].GvId, attr);
        }

        /// <summary>
        /// Override this to customize attributes of the edge. 
        /// </summary>
        /// <param name="attr">Contains a copy of EdgeAttributes. Default CustomizeEdge() can modify it depending on the settings.</param>
        protected virtual void CustomizeEdgeAttributes(TreeT tree, NodeT node, NodeT parent, List<ContextT> stack, int depth, 
            EdgeAttributeMap attr)
        {
        }

        #endregion

        #region Protected API.

        protected void WriteGraph(GraphAttributeMap graphAttributes)
        {
            _output.Write("graph ");
            WriteAttributes(graphAttributes);
            _output.WriteLine(";");
        }

        protected void WriteNode(int id, NodeAttributeMap nodeAttributes)
        {
            WriteNode("n" + id.ToString(), nodeAttributes);
        }

        protected void WriteNode(string id, NodeAttributeMap nodeAttributes)
        {
            _output.Write('"' + id + '"');
            WriteAttributes(nodeAttributes);
            _output.WriteLine(";");
        }

        protected void WriteEdge(int parentId, int childId, EdgeAttributeMap edgeAttributes)
        {
            WriteEdge("n" + parentId.ToString(), "n" + childId.ToString(), edgeAttributes);
        }

        protected void WriteEdge(string parentId, string childId, EdgeAttributeMap edgeAttributes)
        {
            _output.Write("\"{0}\" -> \"{1}\"", parentId, childId);
            WriteAttributes(edgeAttributes);
            _output.WriteLine(";");
        }

        protected void WriteAttributes(AttributeMap attributes)
        {
            _output.Write("[");
            foreach (KeyValuePair<string, object> kvp in attributes.Map)
            {
                string value = kvp.Value.ToString();
                if(kvp.Key == "label" && _reHtmlLabel.IsMatch(value))
                {
                    _output.Write("{0}={1} ", kvp.Key, value);
                }
                else
                    _output.Write("{0}=\"{1}\" ", kvp.Key, value);
            }
            _output.Write("]");
        }


        /// <summary>
        /// Returns true if color has HTML format (#RRGGBB).
        /// </summary>
        protected static bool IsHtmlColor(string color)
        {
            return !string.IsNullOrEmpty(color) && color.StartsWith("#") && color.Length == 7;

        }

        /// <summary>
        /// A gradient for html-like color value (IsHtmlColor() must return true).
        /// The result is returned as HTML color.
        /// </summary>
        protected static string GradientHtmlColor(string htmlColor1, Color color2, double gradient)
        {
            if(!IsHtmlColor(htmlColor1))
            {
                throw new ArgumentException(String.Format("Expected HTML color, but was {0}", htmlColor1));
            }
            Color c1 = ColorTranslator.FromHtml(htmlColor1);
            Color result = Gradient.Calculate(c1, color2, gradient);
            string htmlResult = ColorTranslator.ToHtml(result);
            return htmlResult;
        }

        protected int _nextNodeId;
        protected TextWriter _output;

        #endregion


        #region Private members
        private GraphAttributeMap _graphAttributes = new GraphAttributeMap();
        private ClusterAttributeMap _clusterAttributes = new ClusterAttributeMap();
        private NodeAttributeMap _nodeAttributes = new NodeAttributeMap();
        private EdgeAttributeMap _edgeAttributes = new EdgeAttributeMap();
        private static Regex _reHtmlLabel = new Regex(@"^<.*>$", RegexOptions.Compiled);
        #endregion

    }

    /// <summary>
    /// A shortcut for VisTree&lt;...&gt; that uses the standard type for the context.
    /// </summary>
    public class VisTree<TreeT, NodeT, IteratorT> : VisTree<TreeT, NodeT, IteratorT, VisTreeContext<NodeT, IteratorT>>
    {
    }

    /// <summary>
    /// A shortcut for VisTree&lt;...&gt; for most typical usecase where TreeT == NodeT and standard context is used.
    /// </summary>
    public class VisTree<NodeT, IteratorT> : VisTree<NodeT, NodeT, IteratorT, VisTreeContext<NodeT, IteratorT>>
    {
    }
}
