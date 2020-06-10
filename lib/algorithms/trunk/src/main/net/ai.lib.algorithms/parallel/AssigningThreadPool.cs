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
    /// A thread pool with the ability to assign jobs to desired threads.
    /// This is very useful if the jobs work on group of objects that cannot be accessed from multiple threads.
    /// For example, a job is to icrement a counter. Assume, there are 10 threads, 1000 counters, 
    /// and each must be incremented 100 times. The jobs are coming in random order.
    /// The jobs can be assigned to the threads so that thread 0 increments counters 0-99, tread 2: 100-199, etc. 
    /// This ensures that no counter is accessed from multiple threads and therefore no protection is necessary.
    /// <para>
    /// To free resources, call Dispose().
    /// </para>
    /// </summary>
    public class AssigningThreadPool: ThreadPoolBase, IDisposable
    {
        #region Public API

        public AssigningThreadPool(int threadsCount)
        {
            _threads = new WorkerThread[threadsCount];
            _jobsDoneEvents = new WaitHandle[threadsCount];
            _exitEvents = new WaitHandle[threadsCount];
            for(int i = 0; i < _threads.Length; ++i)
            {
                _threads[i] = new WorkerThread();
                _jobsDoneEvents[i] = _threads[i].JobsDoneEvent;
                _exitEvents[i] = _threads[i].ExitEvent;
            }
        }

        public int ThreadsCount
        {
            get { return _threads.Length; }
        }

        /// <summary>
        /// Adds a job to the queue of the specified thread and returns immediately.
        /// </summary>
        public void QueueJob(JobBase job, int thread)
        {
            _threads[thread].QueueJob(job);
        }

        /// <summary>
        /// Blocks until all queued jobs are executed.
        /// </summary>
        public void WaitAllJobs()
        {
            WaitHandle.WaitAll(_jobsDoneEvents);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Stops threads and frees resources. All jobs will be finished first.
        /// </summary>
        public void Dispose()
        {
            if (_threads != null)
            {
                foreach (WorkerThread w in _threads)
                {
                    w.TriggerExit();
                }
                WaitHandle.WaitAll(_exitEvents);
                _jobsDoneEvents = null;
                _exitEvents = null;
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
                JobsDoneEvent = new ManualResetEvent(true);
                ExitEvent = new AutoResetEvent(false);
                _tread.Start();
            }

            /// <summary>
            /// Is set when all jobs are done and the queue is empty.
            /// Is reset when the caller puts event to the queue.
            /// </summary>
            public ManualResetEvent JobsDoneEvent
            {
                get;
                private set;
            }

            /// <summary>
            /// Is set just before the tread exits.
            /// </summary>
            public AutoResetEvent ExitEvent
            {
                get;
                private set;
            }

            public void QueueJob(JobBase job)
            {
                lock (_jobs)
                {
                    JobsDoneEvent.Reset();
                    _jobs.Enqueue(job);
                }
                _commandEvent.Set();
            }

            public void TriggerExit()
            {
                _exit = true;
                _commandEvent.Set();
            }

            #endregion

            #region Implementation


            void ThreadFunction()
            {
                for (; ; )
                {
                    _commandEvent.WaitOne();
                    // First execute jobs if available
                    ExecuteQueuedJobs();
                    if (_exit)
                    {
                        ExitEvent.Set();
                        return;
                    }
                }
            }

            void ExecuteQueuedJobs()
            {
                for (; ; )
                {
                    JobBase job;
                    lock (_jobs)
                    {
                        if (_jobs.Count == 0)
                        {
                            JobsDoneEvent.Set();
                            return;
                        }
                        job = _jobs.Dequeue();
                    }
                    job.Do();
                }
            }

            private Thread _tread;
            private bool _exit = false;
            /// <summary>
            /// Is set if any input from the caller is available.
            /// </summary>
            private AutoResetEvent _commandEvent;
            private Queue<JobBase> _jobs = new Queue<JobBase>();

            #endregion
        }

        private WorkerThread[] _threads;
        private WaitHandle[] _jobsDoneEvents;
        private WaitHandle[] _exitEvents;

        #endregion
    }
}
