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
            lock (_windows)
                _windows.Add(window);
        }

        public void Remove(IUpdatableToolWindow window)
        {
            lock (_windows)
                _windows.Remove(window);
        }

        public void Start()
        {
            if (_started)
                return;
            _started = true;
            _timer = new Timer {Interval = 1000};
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                _timer.Stop();
                lock (_windows)
                    foreach (var current in _windows)
                        current.Update();

                _timer.Interval = 1000;
                _timer.Enabled = true;
            }
            catch (Exception arg)
            {
                Console.WriteLine($"GUI exception {arg}");
            }
        }

        private bool _started;

        private Timer _timer;

        private readonly List<IUpdatableToolWindow> _windows = new List<IUpdatableToolWindow>();
    }
}
