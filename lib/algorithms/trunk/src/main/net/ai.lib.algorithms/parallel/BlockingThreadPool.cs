/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ai.lib.algorithms.parallel
{
    /// <summary>
    /// A simple thread pool to parallelize computations when it is necessary to finish 
    /// all jobs until going to the next step in the program.
    /// The object creates a pool of threads of a predefined size. The threads are started and wait 
    /// for jobs to execute. 
    /// <para>
    /// Call ExecuteJobs() to execute an array of jobs, it blocks until all threads are executed. 
    /// To free resources, call Dispose().
    /// </para>
    /// </summary>
    public class BlockingThreadPool: ThreadPoolBase, IDisposable
    {
        #region Public API

        public BlockingThreadPool(int threadsCount)
        {
            _threads = new WorkerThread[threadsCount];
            _responses = new WaitHandle[threadsCount];
            for(int i = 0; i < _threads.Length; ++i)
            {
                _threads[i] = new WorkerThread();
                _responses[i] = _threads[i].ResponseEvent;
            }
        }

        /// <summary>
        /// Executes jobs. All jobs are passed to threads in the order given.
        /// There is no way to control assignment job -> thread.
        /// The thread pool tries to use as many available threads as possible.
        /// The function blocks until all jobs are executed.
        /// </summary>
        public void ExecuteJobs(JobBase[] jobs)
        {
            int job;
            // First load as many treads as possible, but no more than needed.
            job = 0;
            int activeThreads = Math.Min(jobs.Length, _threads.Length);
            for (int t = 0; t < activeThreads; ++t, ++job)
            {
                _threads[t].StartJob(jobs[job]);
            }

            while (activeThreads > 0)
            {
                int readyThread = WaitHandle.WaitAny(_responses);
                // One thread is ready and is idle.
                if(job < jobs.Length)
                {
                    // There are still jobs to do - restart idle tread
                    _threads[readyThread].StartJob(jobs[job]);
                    job++;
                }
                else
                {
                    activeThreads--;
                    // Nothing more to do with this thread. It's response event 
                    // is auto-reset to non-signaled state, therefore it does not influence
                    // WaitAny() above.
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_threads != null)
            {
                foreach (WorkerThread w in _threads)
                {
                    w.TriggerExit();
                }
                WaitHandle.WaitAll(_responses);
                _responses = null;
                _threads = null;
            }
        }

        #endregion

        #region Implementation

        class WorkerThread
        {
            #region Public

            public WorkerThread()
            {
                _tread = new Thread(ThreadFunction);
                _commandEvent = new AutoResetEvent(false);
                ResponseEvent = new AutoResetEvent(false);
                _tread.Start();
            }

            public AutoResetEvent ResponseEvent
            {
                get;
                private set;
            }

            public void StartJob(JobBase job)
            {
                Job = job;
                ExecuteCommand(WorkerThread.CommandKind.Execute);
            }

            public void TriggerExit()
            {
                ExecuteCommand(WorkerThread.CommandKind.Exit);
            }

            #endregion

            #region Implementation

            private enum CommandKind
            {
                Execute,
                Exit
            }

            void ThreadFunction()
            {
                for (; ; )
                {
                    // Wait for command event
                    _commandEvent.WaitOne();
                    if (_command == CommandKind.Execute)
                    {
                        Job.Do();
                        ResponseEvent.Set();
                    }
                    else
                    {
                        ResponseEvent.Set();
                        return;
                    }
                }
            }

            private JobBase Job
            {
                set;
                get;
            }

            private void ExecuteCommand(CommandKind commandKind)
            {
                _command = commandKind;
                _commandEvent.Set();
            }

            private Thread _tread;
            private CommandKind _command;
            private AutoResetEvent _commandEvent;

            #endregion

        }

        private WorkerThread[] _threads;
        private WaitHandle[] _responses;

        #endregion
    }
}
