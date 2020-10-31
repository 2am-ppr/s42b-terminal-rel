using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S42B_terminal
{
	public class TestPoint
	{
		public uint Sequence { get; set; }

		public int PosMeasured { get; set; }
		public int PosTarget { get; set; }
		public int PosError {  get => PosMeasured - PosTarget; }

		public int VelMeasured { get; set; }
		public int VelTarget { get; set; }
		public int VelError { get => VelMeasured - VelTarget; }

		public int PidI { get; set; }

		public static string TSVHeader =>
			"time\tPosMeasured\tPosTarget\tPosError\tVelMeasured\tVelTarget\tVelError\tPidI";

		public string ToTSV()
		{
			return string.Format("{0}\t"
					+ "{1}\t"
					+ "{2}\t"
					+ "{3}\t"
					+ "{4}\t"
					+ "{5}\t"
					+ "{6}\t"
					+ "{7}",
				Sequence,
				PosMeasured,
				PosTarget,
				PosError,
				VelMeasured,
				VelTarget,
				VelError,
				PidI);
	}
	}

}
