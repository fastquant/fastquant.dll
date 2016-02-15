namespace SmartQuant.Charting
{
	public class TDistance
	{
		public double dX { get; set; }

		public double dY { get; set; }

		public double X  { get; set; }

		public double Y  { get; set; }

		public string ToolTipText  { get; set; }

		public TDistance ()
		{
			dX = dY= double.MaxValue;
			ToolTipText = null;
		}
	}
}

