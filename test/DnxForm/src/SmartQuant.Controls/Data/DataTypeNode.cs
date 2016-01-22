// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataTypeNode
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DataTypeNode : TreeNode
  {
    public DataTypeItem Item { get; private set; }

    public DataTypeNode(DataTypeItem item)
      : base(item.ToString(), 0, 0)
    {
      this.Item = item;
    }
  }
}
