using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Account
{
    class AccountDataKey
    {
        private SmartQuant.AccountData data;
        protected string key;

        protected AccountDataKey(SmartQuant.AccountData data)
        {
            this.data = data;
        }

        protected string GetFieldAsString(string name)
        {
            object obj = this.data.Fields[name];
            if (obj != null)
                return obj.ToString();
            return string.Empty;
        }

        public override int GetHashCode()
        {
            return this.key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is AccountDataKey)
                return this.key.Equals(((AccountDataKey)obj).key);
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return this.key;
        }
    }

    public class AccountData : FrameworkControl
    {
        private Dictionary<int, AccountDataViewer> viewers;
        private IContainer components;
        private TabControl tabViewers;

        public AccountData()
        {
            this.InitializeComponent();
            this.viewers = new Dictionary<int, AccountDataViewer>();
        }

        protected override void OnInit()
        {
            foreach (AccountDataSnapshot accountDataSnapshot in this.framework.AccountDataManager.GetSnapshots())
            {
                foreach (AccountDataEntry accountDataEntry in accountDataSnapshot.Entries)
                {
                    this.AddAccountData(accountDataEntry.Values);
                    foreach (SmartQuant.AccountData data in accountDataEntry.Positions)
                        this.AddAccountData(data);
                    foreach (SmartQuant.AccountData data in accountDataEntry.Orders)
                        this.AddAccountData(data);
                }
            }
            this.framework.EventManager.Dispatcher.AccountData += new AccountDataEventHandler(this.Dispatcher_AccountData);
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            this.framework.EventManager.Dispatcher.AccountData -= new AccountDataEventHandler(this.Dispatcher_AccountData);
        }

        private void Dispatcher_AccountData(object sender, AccountDataEventArgs args)
        {
            this.InvokeAction((Action)(() => this.AddAccountData(args.Data)));
        }

        private void AddAccountData(SmartQuant.AccountData data)
        {
            int key = (int)data.ProviderId * 256 + (int)data.Route;
            AccountDataViewer accountDataViewer;
            if (!this.viewers.TryGetValue(key, out accountDataViewer))
            {
                accountDataViewer = new AccountDataViewer();
                accountDataViewer.Dock = DockStyle.Fill;
                this.viewers.Add(key, accountDataViewer);
                TabPage tabPage = new TabPage();
                try
                {
                    if ((int)data.ProviderId == (int)data.Route)
                        tabPage.Text = string.Format("{0}", (object)this.framework.ProviderManager.GetProvider((int)data.ProviderId).Name);
                    else
                        tabPage.Text = string.Format("{0} ({1})", (object)this.framework.ProviderManager.GetProvider((int)data.ProviderId).Name, (object)this.framework.ProviderManager.GetProvider((int)data.Route).Name);
                }
                catch (Exception ex)
                {
                    tabPage.Text = ex.Message;
                }
                tabPage.Controls.Add((Control)accountDataViewer);
                this.tabViewers.TabPages.Add(tabPage);
            }
            accountDataViewer.OnData(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabViewers = new TabControl();
            this.SuspendLayout();
            this.tabViewers.Dock = DockStyle.Fill;
            this.tabViewers.Name = "tabViewers";
            this.tabViewers.SelectedIndex = 0;
            this.tabViewers.TabIndex = 0;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.tabViewers);
            this.Name = "AccountData";
            this.ResumeLayout(false);
        }
    }
}
