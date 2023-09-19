using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client.WinConsole
{
    /// <summary>
    /// A class, which can log trace to the console without blocking the caller
    /// </summary>
    public class ConsoleTracer
    {
        record Entry
        (
            DateTime Time,
            string Format,
            object[] Args
        );

        private readonly DateTime time0;
        private readonly Stopwatch stopwatch;
        private readonly Task worker;
        private readonly object mutex;
        private readonly AutoResetEvent newEntry = new AutoResetEvent(false);
        ConcurrentQueue<Entry> entries = new ConcurrentQueue<Entry>();

        public ConsoleTracer()
        {
            mutex = new object();
            time0 = DateTime.Now;
            stopwatch = Stopwatch.StartNew();
            worker = new Task(WriteWorker);
            worker.Start();
        }

        /// <summary>
        /// Trace something to the console - adding to queue - not blocking the caller
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args)
        {
            var time = GetTime();
            entries.Enqueue(new Entry(time, format, args));
            newEntry.Set();
        }

        void WriteWorker()
        {
            while (newEntry.WaitOne())
            {
                while (entries.TryDequeue(out Entry entry))
                {
                    //Console.WriteLine(entry.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff ") + entry.Format + " ", entry.Args);
                    Console.WriteLine(entry.Time.ToString("HH:mm:ss.fff ") + entry.Format + " ", entry.Args);
                }
            }
        }

        private DateTime GetTime()
        {
            return time0.AddTicks(stopwatch.ElapsedTicks);
        }
    }
}
