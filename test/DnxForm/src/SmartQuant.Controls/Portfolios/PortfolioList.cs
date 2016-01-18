using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{

    public delegate void PortfolioNameEventHandler(object sender, PortfolioNameEventArgs args);

    public class PortfolioNameEventArgs : EventArgs
    {
        public string PortfolioName { get; }

        public PortfolioNameEventArgs(string portfolioName)
        {
            PortfolioName = portfolioName;
        }
    }

    class PortfolioNode : TreeNode
    {
        public string PortfolioName { get; }

        public PortfolioNode(string portfolioName) : base(portfolioName)
        {
            PortfolioName = portfolioName;
        }
    }

    public class TreeView : System.Windows.Forms.TreeView
    {
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 515)
                return;
            base.DefWndProc(ref m);
        }
    }

    public class PortfolioList : FrameworkControl
  {
    private static PermanentQueue<Event> queue = new PermanentQueue<Event>();
    private Dictionary<string, PortfolioNode> nodes = new Dictionary<string, PortfolioNode>();
    private PermanentQueue<Event> messageQueue;
    private IContainer components;
    private TreeView trvPortfolios;
    private ImageList imgPortfolios;
    private ContextMenuStrip ctxPortfolios;
    private ToolStripMenuItem ctxPortfolios_AddNew;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem ctxPortfolios_Delete;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem ctxPortfolios_Properties;
    private ToolStripMenuItem ctxPortfolios_View;
    private ToolStripSeparator toolStripSeparator3;

    public override object PropertyObject
    {
      get
      {
        PortfolioNode portfolioNode = this.trvPortfolios.SelectedNode as PortfolioNode;
        if (portfolioNode != null)
          return (object) portfolioNode.PortfolioName;
        return (object) null;
      }
    }

    public event PortfolioNameEventHandler ViewPortfolio;

    public PortfolioList()
    {
      this.InitializeComponent();
    }

    protected override void OnInit()
    {
      this.trvPortfolios.BeginUpdate();
      this.trvPortfolios.Nodes.Clear();
      foreach (SmartQuant.Portfolio portfolio in this.framework.PortfolioManager.Portfolios)
        this.AddPortfolio(portfolio);
      this.trvPortfolios.EndUpdate();
    }

    private void AddPortfolio(SmartQuant.Portfolio portfolio)
    {
      if (this.nodes.ContainsKey(portfolio.Name))
        return;
      PortfolioNode portfolioNode = new PortfolioNode(portfolio.Name);
      this.nodes[portfolio.Name] = portfolioNode;
      if (portfolio.Parent == null)
        this.trvPortfolios.Nodes.Add((TreeNode) portfolioNode);
      else
        this.nodes[portfolio.Parent.Name].Nodes.Add((TreeNode) portfolioNode);
    }

    private void RemovePortfolio(SmartQuant.Portfolio portfolio)
    {
      this.nodes[portfolio.Name].Remove();
      this.nodes.Remove(portfolio.Name);
    }

    private void ChangeParent(SmartQuant.Portfolio portfolio)
    {
      if (!this.nodes.ContainsKey(portfolio.Name))
        return;
      PortfolioNode portfolioNode = this.nodes[portfolio.Name];
      portfolioNode.Remove();
      this.nodes[portfolio.Parent.Name].Nodes.Add((TreeNode) portfolioNode);
    }

    private void FrameworkCleared()
    {
      this.trvPortfolios.Nodes.Clear();
      this.nodes.Clear();
    }

    private void ctxPortfolios_Opening(object sender, CancelEventArgs e)
    {
      if (!(this.trvPortfolios.SelectedNode is PortfolioNode))
      {
        this.ctxPortfolios_AddNew.Enabled = true;
        this.ctxPortfolios_View.Enabled = false;
        this.ctxPortfolios_Delete.Enabled = false;
        this.ctxPortfolios_Properties.Enabled = false;
      }
      else
      {
        this.ctxPortfolios_AddNew.Enabled = true;
        this.ctxPortfolios_View.Enabled = true;
        this.ctxPortfolios_Delete.Enabled = true;
        this.ctxPortfolios_Properties.Enabled = true;
      }
    }

    private void ctxPortfolios_AddNew_Click(object sender, EventArgs e)
    {
      NewPortfolioForm newPortfolioForm = new NewPortfolioForm();
      if (newPortfolioForm.ShowDialog((IWin32Window) this) == DialogResult.OK)
        this.framework.PortfolioManager.Add(new SmartQuant.Portfolio(this.framework, newPortfolioForm.PortfolioName), true);
      newPortfolioForm.Dispose();
    }

    private void ctxPortfolios_View_Click(object sender, EventArgs e)
    {
      this.ShowPorfolio();
    }

    private void ctxPortfolios_Delete_Click(object sender, EventArgs e)
    {
      string portfolioName = ((PortfolioNode) this.trvPortfolios.SelectedNode).PortfolioName;
      if (MessageBox.Show((IWin32Window) this, string.Format("Do you really want to delete portfolio {0} ?", (object) portfolioName), "Delete Portfolio", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      this.framework.PortfolioManager.Remove(portfolioName);
    }

    private void ctxPortfolios_Properties_Click(object sender, EventArgs e)
    {
      this.OnShowProperties(true);
    }

    private void trvPortfolios_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
        return;
      this.trvPortfolios.SelectedNode = this.trvPortfolios.GetNodeAt(e.Location);
    }

    private void trvPortfolios_AfterSelect(object sender, TreeViewEventArgs e)
    {
      this.OnShowProperties(false);
    }

    private void trvPortfolios_DoubleClick(object sender, EventArgs e)
    {
      this.ShowPorfolio();
    }

    private void ShowPorfolio()
    {
      string portfolioName = ((PortfolioNode) this.trvPortfolios.SelectedNode).PortfolioName;
      if (this.ViewPortfolio == null)
        return;
      this.ViewPortfolio((object) this, new PortfolioNameEventArgs(portfolioName));
    }

    public void UpdateGUI()
    {
      Event[] eventArray = this.messageQueue.DequeueAll((object) this);
      if (eventArray == null)
        return;
      this.trvPortfolios.BeginUpdate();
      foreach (Event @event in eventArray)
      {
        switch (@event.TypeId)
        {
          case 99:
            this.FrameworkCleared();
            break;
          case 135:
            this.AddPortfolio((@event as OnPortfolioAdded).Portfolio);
            break;
          case 136:
            this.RemovePortfolio((@event as OnPortfolioRemoved).Portfolio);
            break;
          case 137:
            this.ChangeParent((@event as OnPortfolioParentChanged).Portfolio);
            break;
        }
      }
      this.trvPortfolios.EndUpdate();
    }

    public void Init(PermanentQueue<Event> messages)
    {
      this.messageQueue = messages;
      messages.AddReader((object) this);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (PortfolioList));
      this.trvPortfolios = new TreeView();
      this.ctxPortfolios = new ContextMenuStrip(this.components);
      this.ctxPortfolios_AddNew = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.ctxPortfolios_View = new ToolStripMenuItem();
      this.toolStripSeparator3 = new ToolStripSeparator();
      this.ctxPortfolios_Delete = new ToolStripMenuItem();
      this.toolStripSeparator2 = new ToolStripSeparator();
      this.ctxPortfolios_Properties = new ToolStripMenuItem();
      this.imgPortfolios = new ImageList(this.components);
      this.ctxPortfolios.SuspendLayout();
      this.SuspendLayout();
      this.trvPortfolios.ContextMenuStrip = this.ctxPortfolios;
      this.trvPortfolios.Dock = DockStyle.Fill;
      this.trvPortfolios.ImageIndex = 0;
      this.trvPortfolios.ImageList = this.imgPortfolios;
      this.trvPortfolios.Location = new Point(0, 0);
      this.trvPortfolios.Name = "trvPortfolios";
      this.trvPortfolios.SelectedImageIndex = 0;
      this.trvPortfolios.ShowNodeToolTips = true;
      this.trvPortfolios.Size = new Size(216, 362);
      this.trvPortfolios.TabIndex = 0;
      this.trvPortfolios.AfterSelect += new TreeViewEventHandler(this.trvPortfolios_AfterSelect);
      this.trvPortfolios.DoubleClick += new EventHandler(this.trvPortfolios_DoubleClick);
      this.trvPortfolios.MouseDown += new MouseEventHandler(this.trvPortfolios_MouseDown);
      this.ctxPortfolios.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.ctxPortfolios_AddNew,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.ctxPortfolios_View,
        (ToolStripItem) this.toolStripSeparator3,
        (ToolStripItem) this.ctxPortfolios_Delete,
        (ToolStripItem) this.toolStripSeparator2,
        (ToolStripItem) this.ctxPortfolios_Properties
      });
      this.ctxPortfolios.Name = "ctxPortfolios";
      this.ctxPortfolios.Size = new Size(130, 110);
      this.ctxPortfolios.Opening += new CancelEventHandler(this.ctxPortfolios_Opening);
      this.ctxPortfolios_AddNew.Name = "ctxPortfolios_AddNew";
      this.ctxPortfolios_AddNew.Size = new Size(129, 22);
      this.ctxPortfolios_AddNew.Text = "Add New...";
      this.ctxPortfolios_AddNew.Click += new EventHandler(this.ctxPortfolios_AddNew_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(126, 6);
      this.ctxPortfolios_View.Name = "ctxPortfolios_View";
      this.ctxPortfolios_View.Size = new Size(129, 22);
      this.ctxPortfolios_View.Text = "View...";
      this.ctxPortfolios_View.Click += new EventHandler(this.ctxPortfolios_View_Click);
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new Size(126, 6);
 //     this.ctxPortfolios_Delete.Image = (Image) Resources.delete;
      this.ctxPortfolios_Delete.Name = "ctxPortfolios_Delete";
      this.ctxPortfolios_Delete.Size = new Size(129, 22);
      this.ctxPortfolios_Delete.Text = "Delete";
      this.ctxPortfolios_Delete.Click += new EventHandler(this.ctxPortfolios_Delete_Click);
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new Size(126, 6);
 //     this.ctxPortfolios_Properties.Image = (Image) Resources.properties;
      this.ctxPortfolios_Properties.Name = "ctxPortfolios_Properties";
      this.ctxPortfolios_Properties.Size = new Size(129, 22);
      this.ctxPortfolios_Properties.Text = "Properties";
      this.ctxPortfolios_Properties.Click += new EventHandler(this.ctxPortfolios_Properties_Click);
      this.imgPortfolios.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("imgPortfolios.ImageStream");
      this.imgPortfolios.TransparentColor = Color.Transparent;
      this.imgPortfolios.Images.SetKeyName(0, "portfolio.png");
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.trvPortfolios);
      this.Name = "PortfolioList";
      this.Size = new Size(216, 362);
      this.ctxPortfolios.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
