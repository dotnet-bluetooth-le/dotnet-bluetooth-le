using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client.WinConsole
{
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
        private readonly object mutex;        
        private readonly AutoResetEvent newEntry = new AutoResetEvent(false);
        ConcurrentQueue<Entry> entries = new ConcurrentQueue<Entry>();

        public ConsoleTracer() 
        {
            mutex = new object();
            time0 = DateTime.Now;
            stopwatch = Stopwatch.StartNew();
            new Thread(WriteWorker)
            {
                IsBackground = true,
            }.Start();
        }

        public void Trace(string format, params object[] args)
        {
            var time = GetTime();
            entries.Enqueue(new Entry(time, format, args));
            newEntry.Set();
        }

        void WriteWorker()
        {
            while (newEntry.WaitOne(1000))
            {
                StringBuilder sb = new StringBuilder();                
                while (entries.TryDequeue(out Entry entry))
                {                                        
                    sb.AppendLine();
                    //Console.WriteLine(entry.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff ") + string.Format(entry.Format, entry.Args) + " ");
                    Console.WriteLine(entry.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff ") + entry.Format + " ", entry.Args);
                }                
                //Console.Write(sb.ToString());
            }
        }

        private DateTime GetTime() 
        {
            return time0.AddTicks(stopwatch.ElapsedTicks);
        }
    }
}
