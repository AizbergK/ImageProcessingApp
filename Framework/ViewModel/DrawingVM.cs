using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Framework.ViewModel
{
    public class DrawingForm : Form
    {
        private const int MaxPoints = 5;
        private List<PointF> points = new List<PointF>();
        private const int CoordMax = 255;
        private Button btnConfirm;

        public DrawingForm()
        {
            this.Text = "Fereastră de Desenare";
            this.DoubleBuffered = true;
            this.Width = 600;
            this.Height = 600;
            this.BackColor = Color.White;

            this.MouseClick += DrawingForm_MouseClick;
            this.Paint += DrawingForm_Paint;

            //btnConfirm = new Button
            //{
            //    Text = "Confirmă punctele",
            //    Dock = DockStyle.Bottom,
            //    Height = 30
            //};
            //btnConfirm.Click += BtnConfirm_Click;
            //this.Controls.Add(btnConfirm);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // Închide cu rezultat OK
            this.Close();
        }

        private void DrawingForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (points.Count >= MaxPoints)
                return;

            float xNorm = e.X * CoordMax / (float)this.ClientSize.Width;
            float yNorm = CoordMax - (e.Y * CoordMax / (float)this.ClientSize.Height); // inversat pentru Y

            points.Add(new PointF(xNorm, yNorm));
            this.Invalidate(); // redesenează
        }

        private void DrawingForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Pen axisPen = new Pen(Color.Black, 2);
            g.DrawLine(axisPen, 0, this.ClientSize.Height, this.ClientSize.Width, this.ClientSize.Height); // Ox
            g.DrawLine(axisPen, 0, 0, 0, this.ClientSize.Height); // Oy

            foreach (var p in points)
            {
                float x = p.X * this.ClientSize.Width / CoordMax;
                float y = (CoordMax - p.Y) * this.ClientSize.Height / CoordMax;
                g.FillEllipse(Brushes.Red, x - 4, y - 4, 8, 8);
            }

            for (int i = 0; i <= CoordMax; i += 51)
            {
                float x = i * this.ClientSize.Width / CoordMax;
                float y = (CoordMax - i) * this.ClientSize.Height / CoordMax;
                g.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, x, this.ClientSize.Height - 20);
                g.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Black, 0, y);
            }
        }

        public byte[] GetPoints()
        {
            points.Insert(0, new Point(0, 0));
            points.Add(new Point(255, 255));
            return HermiteCurveLUT(points);
        }

        public static byte[] HermiteCurveLUT(IReadOnlyList<PointF> points)
        {
            if (points == null || points.Count < 2)
                throw new ArgumentException("Need at least two points.");

            // 1.  Sort by X so the spline is monotone in the parameter we’re sampling (the X-axis).
            var pts = points.OrderBy(p => p.X).ToList();
            int n = pts.Count;

            // 2.  Estimate derivatives (Catmull-Rom style: central difference inside, one-sided at the ends).
            double[] d = new double[n];
            for (int i = 0; i < n; ++i)
            {
                if (i == 0)
                    d[i] = (pts[1].Y - pts[0].Y) / (pts[1].X - pts[0].X);
                else if (i == n - 1)
                    d[i] = (pts[i].Y - pts[i - 1].Y) / (pts[i].X - pts[i - 1].X);
                else
                    d[i] = (pts[i + 1].Y - pts[i - 1].Y) / (pts[i + 1].X - pts[i - 1].X);
            }

            // 3.  Allocate output LUT.
            var lut = new byte[256];

            // 4.  March through each segment Pi--Pi+1 and fill in the samples that fall within its X span.
            for (int seg = 0; seg < n - 1; ++seg)
            {
                var p0 = pts[seg];
                var p1 = pts[seg + 1];
                double dx = p1.X - p0.X;
                if (dx <= 0) continue;                      // Degenerate or reversed points: skip.

                // Integer pixel positions covered by this segment.
                int xStart = Math.Max(0, (int)Math.Ceiling(p0.X));
                int xEnd = Math.Min(255, (int)Math.Floor(p1.X));

                for (int x = xStart; x <= xEnd; ++x)
                {
                    double t = (x - p0.X) / dx;            // Normalised parameter in [0,1].

                    // Hermite basis.
                    double h00 = 2 * t * t * t - 3 * t * t + 1;
                    double h10 = t * t * t - 2 * t * t + t;
                    double h01 = -2 * t * t * t + 3 * t * t;
                    double h11 = t * t * t - t * t;

                    double y =
                        h00 * p0.Y +
                        h10 * dx * d[seg] +
                        h01 * p1.Y +
                        h11 * dx * d[seg + 1];

                    int yi = (int)Math.Round(y);
                    yi = yi < 0 ? 0 : yi > 255 ? 255 : yi;

                    lut[x] = (byte)yi;
                }
            }

            // 5.  Plug any untouched slots by forward/backward fill (only matters outside first/last control points).
            for (int i = 1; i < 256; ++i)
                if (lut[i] == 0) lut[i] = lut[i - 1];
            for (int i = 254; i >= 0; --i)
                if (lut[i] == 0) lut[i] = lut[i + 1];

            return lut;
        }
    }
}