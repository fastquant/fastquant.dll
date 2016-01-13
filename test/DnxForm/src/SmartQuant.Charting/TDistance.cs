// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

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

