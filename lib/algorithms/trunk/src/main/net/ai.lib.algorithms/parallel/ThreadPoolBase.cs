/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

namespace ai.lib.algorithms.parallel
{
    /// <summary>
    /// A base class for a thread pool. Contains definitions of 
    /// the jobs.
    /// </summary>
    public class ThreadPoolBase
    {
        public delegate void ExecuteJobDelegate();

        public delegate void ExecuteJobDelegate<Param1T>(Param1T param1);

        public delegate void ExecuteJobDelegate<Param1T, Param2T>(Param1T param1, Param2T param2);

        public delegate void ExecuteJobDelegate<Param1T, Param2T, Param3T>(Param1T param1, Param2T param2, Param3T param3);

        /// <summary>
        /// A base class for a job. 
        /// </summary>
        public abstract class JobBase
        {
            public abstract void Do();
        }

        /// <summary>
        /// A job without parameters.
        /// </summary>
        public class Job: JobBase
        {
            /// <summary>
            /// This user delegate will be called from a worker thread.
            /// </summary>
            public ExecuteJobDelegate DoDelegate
            {
                set;
                get;
            }

            /// <summary>
            /// Does the job.
            /// </summary>
            public override void Do()
            {
                DoDelegate();
            }
        }

        /// <summary>
        /// A job with one parameter.
        /// </summary>
        public class Job<Param1T>: JobBase
        {
            /// <summary>
            /// This user delegate will be called from a worker thread.
            /// </summary>
            public ExecuteJobDelegate<Param1T> Execute
            {
                set;
                get;
            }

            public Param1T Param1
            {
                set;
                get;
            }

            public override void Do()
            {
                Execute(Param1);
            }
        }

        /// <summary>
        /// A job to be executed.
        /// </summary>
        public class Job<Param1T, Param2T> : JobBase
        {
            /// <summary>
            /// This user delegate will be called from a worker thread.
            /// </summary>
            public ExecuteJobDelegate<Param1T, Param2T> Execute
            {
                set;
                get;
            }

            public Param1T Param1
            {
                set;
                get;
            }

            public Param2T Param2
            {
                set;
                get;
            }

            public override void Do()
            {
                Execute(Param1, Param2);
            }
        }

        /// <summary>
        /// A job to be executed.
        /// </summary>
        public class Job<Param1T, Param2T, Param3T> : JobBase
        {
            /// <summary>
            /// This user delegate will be called from a worker thread.
            /// </summary>
            public ExecuteJobDelegate<Param1T, Param2T, Param3T> Execute
            {
                set;
                get;
            }

            public Param1T Param1
            {
                set;
                get;
            }

            public Param2T Param2
            {
                set;
                get;
            }

            public Param3T Param3
            {
                set;
                get;
            }

            public override void Do()
            {
                Execute(Param1, Param2, Param3);
            }
        }
    }
}