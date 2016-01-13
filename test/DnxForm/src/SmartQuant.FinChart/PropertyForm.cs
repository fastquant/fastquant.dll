// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;

#if GTK
using Compatibility.Gtk;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.FinChart
{
    #if GTK
    public class PropertyForm : Form
    {
        public PropertyForm(object properties)
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }
    }
    #else
    public class PropertyForm : Form
    {
        private PropertyGrid propertyGrid;
        private Button btnClose;

        public PropertyForm(object properties)
        {
            InitializeComponent();
            propertyGrid.SelectedObject = properties;
        }

        private void InitializeComponent()
        {
            this.propertyGrid = new PropertyGrid();
            this.btnClose = new Button();
            this.SuspendLayout();
            this.propertyGrid.CommandsVisibleIfAvailable = true;
            this.propertyGrid.Dock = DockStyle.Fill;
            this.propertyGrid.LargeButtons = false;
            this.propertyGrid.LineColor = SystemColors.ScrollBar;
            this.propertyGrid.Location = new Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new Size(232, 310);
            this.propertyGrid.TabIndex = 2;
            this.propertyGrid.Text = "propertyGrid1";
            this.propertyGrid.ViewBackColor = SystemColors.Window;
            this.propertyGrid.ViewForeColor = SystemColors.WindowText;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Location = new Point(168, 280);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(56, 24);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(232, 310);
            this.ControlBox = false;
            this.Controls.Add((Control) this.btnClose);
            this.Controls.Add((Control) this.propertyGrid);
            this.Name = "PropertyForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "PropertyForm";
            this.ResumeLayout(false);
        }
    }
    #endif
}

