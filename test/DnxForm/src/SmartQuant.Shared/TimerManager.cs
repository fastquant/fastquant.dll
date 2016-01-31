using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartQuant.Shared
{
    public interface IUpdatableToolWindow
    {
        void Update();
    }

    public class TimerManager
    {
        public void Add(IUpdatableToolWindow window)
        {
            Start();
            lock (this.windows)
                this.windows.Add(window);
        }

        public void Remove(IUpdatableToolWindow window)
        {
            lock (this.windows)
                this.windows.Remove(window);
        }

        public void Start()
        {
            if (this.started)
                return;
            this.started = true;
            this.timer = new Timer {Interval = 1000};
            this.timer.Tick += OnTick;
            this.timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                this.timer.Stop();
                lock (this.windows)
                    foreach (var current in this.windows)
                        current.Update();

                this.timer.Interval = 1000;
                this.timer.Enabled = true;
            }
            catch (Exception arg)
            {
                Console.WriteLine($"GUI exception {arg}");
            }
        }

        private bool started;

        private Timer timer;

        private readonly List<IUpdatableToolWindow> windows = new List<IUpdatableToolWindow>();
    }
}
