using System;
using SmartQuant;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

#if GTK
using Compatibility.Gtk;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Controls
{
    public class ControlSettings : Dictionary<string, string>
    {
        protected internal void SetValue(string key, string value) => this[key] = value;

        protected internal void SetValue(string key, bool value) => SetValue(key, value.ToString());

        protected internal void SetValue(string key, byte value) => SetValue(key, value.ToString());

        protected internal void SetValue(string key, double value) => SetValue(key, value.ToString(CultureInfo.InvariantCulture));

        protected internal void SetEnumValue<T>(string key, T value) where T : struct => SetValue(key, value.ToString());

        protected internal string GetStringValue(string key, string defaultValue)
        {
            string str;
            return TryGetValue(key, out str) ? str : defaultValue;
        }

        protected internal T GetEnumValue<T>(string key, T defaultValue) where T : struct
        {
            T result;
            return Enum.TryParse<T>(GetStringValue(key, defaultValue.ToString()), out result) ? result : defaultValue;
        }

        protected internal bool GetBooleanValue(string key, bool defaultValue)
        {
            bool result;
            return bool.TryParse(GetStringValue(key, defaultValue.ToString()), out result) ? result : defaultValue;
        }

        protected internal byte GetByteValue(string key, byte defaultValue)
        {
            byte result;
            return byte.TryParse(GetStringValue(key, defaultValue.ToString()), out result) ? result : defaultValue;
        }

        protected internal double GetDoubleValue(string key, double defaultValue)
        {
            string stringValue = GetStringValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            double result;
            return double.TryParse(stringValue, NumberStyles.None, CultureInfo.InvariantCulture, out result) ? result : defaultValue;
        }
    }

    public class ControlInfo
    {
    }


    public class ShowPropertiesEventArgs : EventArgs
    {
        public bool Focus { get; }

        internal ShowPropertiesEventArgs(bool focus)
        {
            Focus = focus;
        }
    }

#if GTK
    public partial class FrameworkControl : NoPaintUserControl
    {
        protected void InvokeAction(Action action) => Gtk.Application.Invoke((sender, e) => action());

        protected string GetMessageBoxCaption() => Parent != null ? Parent.Name : Name;
    }
#else
    public partial class FrameworkControl : UserControl
    {
        protected void InvokeAction(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        protected string GetMessageBoxCaption() => Parent != null ? Parent.Text : Text;
    }
#endif

    public partial class FrameworkControl
    {
        protected Framework framework;
        protected ControlSettings settings;
        protected object[] args;

        public static bool UpdatedSuspened { get; set; }

        public virtual object PropertyObject => null;

        public event EventHandler<ShowPropertiesEventArgs> ShowProperties;

        protected FrameworkControl()
        {
        }

        public void Init(Framework framework, ControlSettings settings, object[] args)
        {
            this.framework = framework;
            this.settings = settings;
            this.args = args;
            OnInit();
        }

        public void Close(CancelEventArgs args)
        {
            OnClosing(args);
        }

        protected virtual void OnInit()
        {
            // noop
        }

        protected virtual void OnClosing(CancelEventArgs args)
        {
            // noop
        }

        public void SuspendUpdates()
        {
            UpdatedSuspened = true;
            OnSuspendUpdates();
        }

        public void ResumeUpdates()
        {
            UpdatedSuspened = false;
            OnResumeUpdates();
        }

        protected virtual void OnSuspendUpdates()
        {
            // noop
        }

        protected virtual void OnResumeUpdates()
        {
            // noop
        }

        public void SetControlInfo(ControlInfo controlInfo)
        {
            throw new NotImplementedException();
        }

        protected void OnShowProperties(bool focus)
        {
            ShowProperties?.Invoke(this, new ShowPropertiesEventArgs(focus));
        }
    }
}
