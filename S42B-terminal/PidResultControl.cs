using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;

namespace S42B_terminal
{
	public partial class PidResultControl : UserControl
	{
		private List<TestPoint> pointLog;
		private Dictionary<string, int> pars;
		private PlotModel model;
		private int e_max;
		private int e_min;
		private double e_avg;
		private double e_rms;

		public PidResultControl()
		{
			InitializeComponent();
		}

		private static readonly Padding labelPadding = new Padding(10, 5, 2, 10);

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			plotView1.Model = model;

			var things = new (string name, double value)[]
			{
				("E max", e_max),
				("E min", e_min),
				("|E| avg", e_avg),
				("E RMS", e_rms),
			};

			foreach(var t in things)
			{
				var label = new Label() { AutoSize = true, Padding = labelPadding, Text = t.name };
				var box = new TextBox() { Text = t.value.ToString("0.##"), MaxLength = 8, ReadOnly = true, Width = 40 };

				pnlInfo.Controls.Add(label);
				pnlInfo.Controls.Add(box);
			}

			foreach(var t in pars)
			{
				var label = new Label() { AutoSize = true, Padding = labelPadding, Text = t.Key };
				var box = new TextBox() { Text = t.Value.ToString("0.##"), MaxLength = 8, ReadOnly = true, Width = 30 };

				pnlInfo.Controls.Add(label);
				pnlInfo.Controls.Add(box);
			}

			{
				var label = new Label() { AutoSize = true, Padding = labelPadding, Text = "Notes" };
				var box = new TextBox() { Text = "", Width = 300 };

				pnlInfo.Controls.Add(label);
				pnlInfo.Controls.Add(box);
			}
		}

		public PidResultControl(List<TestPoint> pointLog, Dictionary<string, int> pars)
		{
			InitializeComponent();

			this.pointLog = pointLog;
			this.pars = pars;

			var model = new PlotModel()
			{
				Title = "PID Tune"
			};

			var startPos = pointLog.First().PosTarget;
			var startSeq = pointLog.First().Sequence;

			var header = pointLog.TakeWhile(x => Math.Abs(x.PosError) < 6 && x.PosTarget == startPos).Count();
			if (header != pointLog.Count)
				pointLog.RemoveRange(0, header);

			var endPos = pointLog.Last().PosTarget;
			var tail = pointLog.Reverse<TestPoint>().TakeWhile(x => Math.Abs(x.PosError) < 6 && x.PosTarget == endPos).Count();
			if (tail != pointLog.Count)
				pointLog.RemoveRange(pointLog.Count - tail, tail);


			foreach(var x in pointLog)
			{
				x.Sequence -= startSeq;
			}

			var colors = new[] { "0072bd", "f95319", "edb120", "9e2ffe", "77ac30", "4dbeee", "a2142f" };

			foreach (var field in new[] { "PosMeasured", "PosTarget", "PosError", "VelMeasured", "VelTarget", "VelError", "PidI" })
			{

				var series = new LineSeries()
				{
					Title = field,
					ItemsSource = pointLog,
					DataFieldY = field,
					DataFieldX = "Sequence"
				};

				model.Series.Add(series);
			}
			(model.Series[0] as LineSeries).YAxisKey = "position";
			(model.Series[1] as LineSeries).YAxisKey = "position";
			(model.Series[2] as LineSeries).YAxisKey = "posError";
			(model.Series[5] as LineSeries).YAxisKey = "velError";

			for (int i = 0; i < model.Series.Count; i++)
			{
				var s = model.Series[i] as LineSeries;
				var colorValue = UInt32.Parse(colors[i], System.Globalization.NumberStyles.HexNumber) | 0xff000000u;
				var color = OxyColor.FromUInt32(colorValue);
				var lightColor = OxyColor.FromUInt32(colorValue & 0x7fffffffu);

				if (s.DataFieldY.EndsWith("Target"))
				{

					//s.MarkerType = MarkerType.Cross;
					//s.MarkerFill = OxyColors.Transparent;
					//s.MarkerStroke = OxyColors.Automatic;
					//s.LineStyle = LineStyle.None;
					//s.Color = OxyColors.Transparent;
					s.Color = color;
					s.LineStyle = LineStyle.Dot;
				}
				else
				{
					s.Color = color;
				}

				if (s.DataFieldY == "PosError")
				{
					s.MarkerType = MarkerType.Circle;
					s.MarkerStroke = color;
				}
			}

			model.Axes.Add(new LinearAxis() { Position = AxisPosition.Right, Title = "Velocity and Integral (derivative units)", Key = "default", EndPosition = 0.45 });
			model.Axes.Add(new LinearAxis() { Position = AxisPosition.Right, Title = "Position (16384th of a rotation)", Key = "position", StartPosition = 0.55 });
			model.Axes.Add(new LinearAxis()
			{
				Position = AxisPosition.Left,
				Title = "Position Error (16384th of a rotation)",
				Key = "posError",
				StartPosition = 0.55,
				MajorGridlineStyle = LineStyle.Automatic,
				Maximum = 300,
				Minimum = -300,
				MinorGridlineStyle = LineStyle.Dot,
				MajorStep = 100
			});
			model.Axes.Add(new LinearAxis()
			{
				Position = AxisPosition.Left,
				Title = "Velocity Error (derivative units)",
				Key = "velError",
				EndPosition = 0.45,
				MajorGridlineStyle = LineStyle.Automatic,
				Maximum = 3000,
				Minimum = -3000,
				MinorGridlineStyle = LineStyle.Dot,
				MajorStep = 1000
			});

			this.e_max = pointLog.Max(x => x.PosError);
			this.e_min = pointLog.Min(x => x.PosError);
			this.e_avg = pointLog.Average(x => Math.Abs(x.PosError));
			this.e_rms = Math.Sqrt(pointLog.Average(x => Math.Pow(x.PosError, 2)));
			
			this.model = model;
		}
	}
}
