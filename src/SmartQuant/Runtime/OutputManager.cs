using System;
using System.IO;

namespace SmartQuant
{
    class OutputWriter : StringWriter
    {
        public OutputWriter(OutputManager manager)
        {
            this.outputManager = manager;
        }

        public void Clear()
        {
            if (this.queue != null)
            {
                this.outputManager.Framework.EventBus.CommandPipe.Remove(this.queue);
                this.queue.Clear();
                this.queue = null;
            }
        }

        private void DoWrite(string text)
        {
            this.outputManager.textWriter?.Write(text);
            this.outputManager.streamWriter?.Write(text);

            if (this.queue == null)
            {
                this.queue = new EventQueue(EventQueueId.Service, EventQueueType.Master, EventQueuePriority.Normal, 102400, null);
                this.queue.Enqueue(new OnQueueOpened(this.queue));
                this.outputManager.Framework.EventBus.CommandPipe.Add(this.queue);
            }
            this.queue.Enqueue(new Output(DateTime.Now, text));
        }

        private void DoWriteLine(string value)
        {
            DoWrite($"{value}{Environment.NewLine}");
        }

        public override void Write(bool value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(char value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(char[] buffer)
        {
            Write(new string(buffer));
        }

        public override void Write(decimal value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(double value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(float value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(int value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(long value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(string value)
        {
            DoWrite(value);
        }

        public override void Write(uint value)
        {
            DoWrite(value.ToString());
        }

        public override void Write(ulong value)
        {
            DoWrite(value.ToString());
        }

        public override void WriteLine()
        {
            DoWriteLine(string.Empty);
        }

        public override void WriteLine(bool value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(char value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(decimal value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(double value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(float value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(int value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(long value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(uint value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(ulong value)
        {
            DoWriteLine(value.ToString());
        }

        public override void WriteLine(string value)
        {
            DoWriteLine(value);
        }

        private EventQueue queue;

        private OutputManager outputManager;
    }

    public class OutputManager
    {
        internal Framework Framework { get; }
        internal TextWriter textWriter;
        internal StreamWriter streamWriter;
        internal OutputWriter outputWriter;

        public OutputManager(Framework framework, string path = null)
        {
            Framework = framework;
            this.textWriter = Console.Out;
            if (path != null)
            {
                path = path.Replace("%d", DateTime.Now.ToString("ddMMyyHHmmss"));
                path = path.Replace("%n", framework.Name);
                try
                {
                    this.streamWriter = File.CreateText(path);
                    this.streamWriter.AutoFlush = true;
                }
                catch (Exception)
                {
                    Console.WriteLine($"OutputManager::OutputManager Error. Can not open output log file {path}");
                    this.streamWriter = null;
                }
            }
            this.outputWriter = new OutputWriter(this);
            Console.SetOut(this.outputWriter);
        }

        public void Clear()
        {
            this.outputWriter.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                this.streamWriter?.Dispose();
        }
    }
}