using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace FuzzyLogicAct
{
    public partial class MainForm : Form
    {
        // ================= INPUT CONTROLS =================
        private TrackBar tbHealth;
        private TrackBar tbSkill;
        private Label lblHealthVal;
        private Label lblSkillVal;

        // Preset Edge Testing Buttons
        private Button btnEdgeMin;
        private Button btnEdgeMax;
        private Button btnEdgeCenter;
        private Button btnClutchPro;
        private Button btnNoviceTank;

        // ================= RESULTS & VERIFICATION =================
        private Label lblFuzzStatus;
        private Label lblActiveRule;
        private Label lblMamdaniResult;
        private Label lblSugenoResult;
        private Label lblVariance;
        private TextBox txtRuleAudit;

        // ================= VISUALIZATIONS =================
        private DoubleBufferedPanel pnlCurves;
        private DoubleBufferedPanel pnl3DSurface;

        // ================= STATE CACHE FOR RENDERING =================
        private double curHp = 35.0;
        private double curSkill = 75.0;
        private double curMamAggression = 50.0;
        private double curSugAggression = 50.0;
        private double curMerciful = 0.0;
        private double curBalanced = 0.0;
        private double curRelentless = 0.0;

        public MainForm()
        {
            // 1. Calls the auto-generated Designer initialization
            InitializeComponent();

            // 2. Builds the complete Fuzzy Logic DDA Interface
            InitializeFuzzyControls();

            // 3. Executes initial fuzzy computation pipeline
            ExecutePipeline();
        }

        private void InitializeFuzzyControls()
        {
            this.Text = "Fuzzy Logic DDA - Video Game Boss Combat Controller [Mamdani & Sugeno Dual Engine]";
            this.Size = new Size(1220, 840);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ----------------- 1. INPUT CONTROLS & PRESETS -----------------
            var grpInputs = new GroupBox
            {
                Text = "1. Crisp Combat Inputs & Edge-Testing Presets",
                Bounds = new Rectangle(15, 12, 530, 175),
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
            };

            var lblH = new Label { Text = "Player Health (HP):", Bounds = new Rectangle(15, 24, 150, 18), Font = new Font("Segoe UI", 8.5f) };
            tbHealth = new TrackBar { Bounds = new Rectangle(12, 42, 410, 45), Minimum = 0, Maximum = 100, Value = 35, TickFrequency = 10 };
            lblHealthVal = new Label { Bounds = new Rectangle(430, 42, 85, 25), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.DarkRed };
            tbHealth.ValueChanged += (s, e) => ExecutePipeline();

            var lblS = new Label { Text = "Combat Performance (Skill):", Bounds = new Rectangle(15, 85, 180, 18), Font = new Font("Segoe UI", 8.5f) };
            tbSkill = new TrackBar { Bounds = new Rectangle(12, 103, 410, 45), Minimum = 0, Maximum = 100, Value = 75, TickFrequency = 10 };
            lblSkillVal = new Label { Bounds = new Rectangle(430, 103, 85, 25), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.DarkGreen };
            tbSkill.ValueChanged += (s, e) => ExecutePipeline();

            // Preset Buttons for Live Rubric Edge-Testing
            btnEdgeMin = new Button { Text = "Min Edge (0/0)", Bounds = new Rectangle(15, 145, 95, 24), Font = new Font("Segoe UI", 7.8f) };
            btnEdgeMin.Click += (s, e) => SetInputValues(0, 0);

            btnEdgeCenter = new Button { Text = "Center (50/50)", Bounds = new Rectangle(115, 145, 95, 24), Font = new Font("Segoe UI", 7.8f) };
            btnEdgeCenter.Click += (s, e) => SetInputValues(50, 50);

            btnEdgeMax = new Button { Text = "Max Edge (100/100)", Bounds = new Rectangle(215, 145, 115, 24), Font = new Font("Segoe UI", 7.8f) };
            btnEdgeMax.Click += (s, e) => SetInputValues(100, 100);

            btnClutchPro = new Button { Text = "Clutch Pro (15/90)", Bounds = new Rectangle(335, 145, 100, 24), Font = new Font("Segoe UI", 7.8f) };
            btnClutchPro.Click += (s, e) => SetInputValues(15, 90);

            btnNoviceTank = new Button { Text = "Novice Tank (90/15)", Bounds = new Rectangle(440, 145, 80, 24), Font = new Font("Segoe UI", 7.8f) };
            btnNoviceTank.Click += (s, e) => SetInputValues(90, 15);

            grpInputs.Controls.AddRange(new Control[] { lblH, tbHealth, lblHealthVal, lblS, tbSkill, lblSkillVal, btnEdgeMin, btnEdgeCenter, btnEdgeMax, btnClutchPro, btnNoviceTank });

            // ----------------- 2. SYSTEM OUTPUTS & AUDIT LOG -----------------
            var grpResults = new GroupBox
            {
                Text = "2. Inference Engine Outputs & Q&A Defense Rule Audit",
                Bounds = new Rectangle(555, 12, 635, 175),
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
            };

            lblFuzzStatus = new Label { Bounds = new Rectangle(15, 20, 310, 34), Font = new Font("Consolas", 8.0f) };
            lblActiveRule = new Label { Bounds = new Rectangle(15, 56, 310, 22), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.DarkSlateBlue };

            lblMamdaniResult = new Label { Bounds = new Rectangle(15, 80, 290, 26), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.Navy };
            lblSugenoResult = new Label { Bounds = new Rectangle(15, 106, 290, 26), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblVariance = new Label { Bounds = new Rectangle(15, 134, 300, 20), Font = new Font("Segoe UI", 8.2f, FontStyle.Italic) };

            txtRuleAudit = new TextBox
            {
                Bounds = new Rectangle(325, 20, 295, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(250, 250, 252),
                Font = new Font("Consolas", 7.8f)
            };

            grpResults.Controls.AddRange(new Control[] { lblFuzzStatus, lblActiveRule, lblMamdaniResult, lblSugenoResult, lblVariance, txtRuleAudit });

            // ----------------- 3. 2D MEMBERSHIP CURVES GRAPH -----------------
            var grpCurves = new GroupBox
            {
                Text = "3. Input / Output Membership Curves & Live Tracking (Centroid Clipped Area)",
                Bounds = new Rectangle(15, 195, 530, 595),
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
            };
            pnlCurves = new DoubleBufferedPanel { Bounds = new Rectangle(15, 25, 500, 555), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            pnlCurves.Paint += DrawMembershipCurves;
            grpCurves.Controls.Add(pnlCurves);

            // ----------------- 4. 3D CONTROL SURFACE -----------------
            var grpSurface = new GroupBox
            {
                Text = "4. 3D Rule Control Surface Plot (Health x Skill -> Aggression)",
                Bounds = new Rectangle(555, 195, 635, 595),
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
            };
            pnl3DSurface = new DoubleBufferedPanel { Bounds = new Rectangle(15, 25, 605, 530), BackColor = Color.FromArgb(18, 22, 28), BorderStyle = BorderStyle.FixedSingle };
            pnl3DSurface.Paint += Draw3DControlSurface;

            var lblSurfaceLegend = new Label
            {
                Text = "Z-Axis: Boss Aggression (0%-100%) | [●] Live Operating Point Beacon | Wireframe Mesh",
                Bounds = new Rectangle(15, 560, 605, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray
            };
            grpSurface.Controls.AddRange(new Control[] { pnl3DSurface, lblSurfaceLegend });

            this.Controls.AddRange(new Control[] { grpInputs, grpResults, grpCurves, grpSurface });
        }

        private void SetInputValues(int hp, int skill)
        {
            tbHealth.Value = Math.Max(0, Math.Min(100, hp));
            tbSkill.Value = Math.Max(0, Math.Min(100, skill));
            ExecutePipeline();
        }

        private void ExecutePipeline()
        {
            curHp = tbHealth.Value;
            curSkill = tbSkill.Value;

            lblHealthVal.Text = $"{curHp:F0}%";
            lblSkillVal.Text = $"{curSkill:F0}%";

            // Fuzzification (Partition of Unity with smooth 50% overlap, no coverage gaps)
            double hLow = TrapezoidMF(curHp, 0, 0, 25, 50);
            double hMed = TriangularMF(curHp, 25, 50, 75);
            double hHigh = TrapezoidMF(curHp, 50, 75, 100, 100);

            double sLow = TrapezoidMF(curSkill, 0, 0, 25, 50);
            double sMed = TriangularMF(curSkill, 25, 50, 75);
            double sHigh = TrapezoidMF(curSkill, 50, 75, 100, 100);

            lblFuzzStatus.Text = $"HP:    Low={hLow:F2}, Med={hMed:F2}, High={hHigh:F2}\nSkill: Low={sLow:F2}, Med={sMed:F2}, High={sHigh:F2}";

            // Evaluate all 9 complete rules
            double[] r = EvaluateAll9Rules(hLow, hMed, hHigh, sLow, sMed, sHigh);

            // Determine dominant active rule
            int dominantIdx = 0;
            for (int i = 1; i < 9; i++)
            {
                if (r[i] > r[dominantIdx]) dominantIdx = i;
            }
            lblActiveRule.Text = $"Dominant Rule: R{dominantIdx + 1} (Firing Strength α = {r[dominantIdx]:F2})";

            // Aggregation for 3 consequents (Max operator / S-Norm)
            curMerciful = Math.Max(r[0], Math.Max(r[1], r[3]));
            curBalanced = Math.Max(r[2], Math.Max(r[4], r[6]));
            curRelentless = Math.Max(r[5], Math.Max(r[7], r[8]));

            // Mamdani Centroid Defuzzification
            curMamAggression = ComputeMamdaniCentroid(curRelentless, curBalanced, curMerciful);
            lblMamdaniResult.Text = $"Mamdani (Centroid): {curMamAggression:F2}% Aggression";

            // Sugeno Zero-Order Weighted Average Defuzzification
            curSugAggression = ComputeSugenoWeightedAverage(curRelentless, curBalanced, curMerciful);
            lblSugenoResult.Text = $"Sugeno (Weighted Avg): {curSugAggression:F2}% Aggression";

            double variance = Math.Abs(curMamAggression - curSugAggression);
            lblVariance.Text = $"Variance (|Mamdani - Sugeno|): {variance:F2}%";

            // Populate the Q&A Defense Audit Trail
            var sb = new StringBuilder();
            sb.AppendLine("ACTIVE RULE FIRING AUDIT:");
            string[] ruleNames = new string[]
            {
                "R1: HP Low  & Skill Low  -> Merciful",
                "R2: HP Low  & Skill Med  -> Merciful",
                "R3: HP Low  & Skill High -> Balanced",
                "R4: HP Med  & Skill Low  -> Merciful",
                "R5: HP Med  & Skill Med  -> Balanced",
                "R6: HP Med  & Skill High -> Relentless",
                "R7: HP High & Skill Low  -> Balanced",
                "R8: HP High & Skill Med  -> Relentless",
                "R9: HP High & Skill High -> Relentless"
            };

            for (int i = 0; i < 9; i++)
            {
                if (r[i] > 0.001)
                {
                    sb.AppendLine($"• {ruleNames[i]} [α={r[i]:F2}]");
                }
            }
            sb.AppendLine($"Aggregated Strengths: Merc={curMerciful:F2}, Bal={curBalanced:F2}, Rel={curRelentless:F2}");
            txtRuleAudit.Text = sb.ToString();

            pnlCurves.Invalidate();
            pnl3DSurface.Invalidate();
        }

        // --- 9 EXHAUSTIVE AND NON-CONFLICTING RULES ---
        private static double[] EvaluateAll9Rules(double hL, double hM, double hH, double sL, double sM, double sH)
        {
            return new double[9]
            {
                Math.Min(hL, sL), // R1
                Math.Min(hL, sM), // R2
                Math.Min(hL, sH), // R3
                Math.Min(hM, sL), // R4
                Math.Min(hM, sM), // R5
                Math.Min(hM, sH), // R6
                Math.Min(hH, sL), // R7
                Math.Min(hH, sM), // R8
                Math.Min(hH, sH)  // R9
            };
        }

        // --- MAMDANI CENTROID (DISCRETE RIEMANN INTEGRATION) ---
        public static double ComputeMamdaniCentroid(double rRelentless, double rBalanced, double rMerciful)
        {
            double sumNumerator = 0.0;
            double sumDenominator = 0.0;
            double step = 0.5;

            for (double y = 0.0; y <= 100.0; y += step)
            {
                double outMerciful = TrapezoidMF(y, 0.0, 0.0, 25.0, 50.0);
                double outBalanced = TriangularMF(y, 25.0, 50.0, 75.0);
                double outRelentless = TrapezoidMF(y, 50.0, 75.0, 100.0, 100.0);

                double clippedMerciful = Math.Min(rMerciful, outMerciful);
                double clippedBalanced = Math.Min(rBalanced, outBalanced);
                double clippedRelentless = Math.Min(rRelentless, outRelentless);

                double aggregatedY = Math.Max(clippedMerciful, Math.Max(clippedBalanced, clippedRelentless));

                sumNumerator += y * aggregatedY * step;
                sumDenominator += aggregatedY * step;
            }

            return sumDenominator > 0.0001 ? (sumNumerator / sumDenominator) : 50.0;
        }

        // --- SUGENO ZERO-ORDER WEIGHTED AVERAGE METHOD ---
        public static double ComputeSugenoWeightedAverage(double rRelentless, double rBalanced, double rMerciful)
        {
            double cMerciful = 20.0;
            double cBalanced = 50.0;
            double cRelentless = 85.0;

            double numerator = (rMerciful * cMerciful) + (rBalanced * cBalanced) + (rRelentless * cRelentless);
            double denominator = rMerciful + rBalanced + rRelentless;

            return denominator > 0.0001 ? (numerator / denominator) : 50.0;
        }

        public static double TriangularMF(double x, double a, double b, double c)
        {
            if (x <= a || x >= c) return 0.0;
            if (Math.Abs(x - b) < 1e-9) return 1.0;
            return x < b ? (x - a) / (b - a) : (c - x) / (c - b);
        }

        public static double TrapezoidMF(double x, double a, double b, double c, double d)
        {
            if (x <= a || x >= d) return 0.0;
            if (x >= b && x <= c) return 1.0;
            if (x < b) return (b > a) ? (x - a) / (b - a) : 1.0;
            return (d > c) ? (d - x) / (d - c) : 1.0;
        }

        // ================= 2D MEMBERSHIP CURVE VISUALIZATIONS =================
        private void DrawMembershipCurves(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Plot 1: Player Health
            DrawUniversePlot(g, 15, 10, 465, 150, "Input 1: Player Health (HP)", curHp,
                ("Low", Color.Crimson, x => TrapezoidMF(x, 0, 0, 25, 50)),
                ("Med", Color.DarkOrange, x => TriangularMF(x, 25, 50, 75)),
                ("High", Color.SeaGreen, x => TrapezoidMF(x, 50, 75, 100, 100)));

            // Plot 2: Combat Performance
            DrawUniversePlot(g, 15, 185, 465, 150, "Input 2: Combat Skill / Performance", curSkill,
                ("Low", Color.Crimson, x => TrapezoidMF(x, 0, 0, 25, 50)),
                ("Med", Color.DarkOrange, x => TriangularMF(x, 25, 50, 75)),
                ("High", Color.SeaGreen, x => TrapezoidMF(x, 50, 75, 100, 100)));

            // Plot 3: Output Aggression with Mamdani Shaded Clipping & Dual Defuzzification Lines
            DrawOutputAggressionPlot(g, 15, 360, 465, 180);
        }

        private void DrawUniversePlot(Graphics g, int x, int y, int w, int h, string title, double currentVal,
            params (string Label, Color Col, Func<double, double> Func)[] sets)
        {
            g.DrawString(title, new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.Black, x, y);
            int plotY = y + 22;
            int plotH = h - 35;
            g.FillRectangle(Brushes.WhiteSmoke, x, plotY, w, plotH);
            g.DrawRectangle(Pens.LightGray, x, plotY, w, plotH);

            foreach (var set in sets)
            {
                using (var pen = new Pen(set.Col, 2))
                {
                    PointF prev = PointF.Empty;
                    for (int px = 0; px <= w; px++)
                    {
                        double input = (double)px / w * 100.0;
                        double mu = set.Func(input);
                        float sx = x + px;
                        float sy = plotY + (float)((1.0 - mu) * plotH);

                        if (px > 0) g.DrawLine(pen, prev, new PointF(sx, sy));
                        prev = new PointF(sx, sy);
                    }
                }
            }

            // Current Crisp Input Tracking Line
            if (currentVal >= 0)
            {
                float markerX = x + (float)(currentVal / 100.0 * w);
                using (var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash })
                {
                    g.DrawLine(pen, markerX, plotY, markerX, plotY + plotH);
                }
                g.DrawString($"{currentVal:F0}%", new Font("Segoe UI", 7.8f, FontStyle.Bold), Brushes.Black, markerX - 10, plotY - 15);
            }
        }

        private void DrawOutputAggressionPlot(Graphics g, int x, int y, int w, int h)
        {
            g.DrawString("Output: Aggression (%) [Shaded = Clipped Area, Blue = Mamdani COG, Green = Sugeno]",
                new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.Black, x, y);

            int plotY = y + 22;
            int plotH = h - 35;
            g.FillRectangle(Brushes.WhiteSmoke, x, plotY, w, plotH);
            g.DrawRectangle(Pens.LightGray, x, plotY, w, plotH);

            // Shaded Mamdani Aggregated Area
            PointF[] polygon = new PointF[w + 2];
            polygon[0] = new PointF(x, plotY + plotH);
            for (int px = 0; px <= w; px++)
            {
                double input = (double)px / w * 100.0;
                double outMerc = TrapezoidMF(input, 0.0, 0.0, 25.0, 50.0);
                double outBal = TriangularMF(input, 25.0, 50.0, 75.0);
                double outRel = TrapezoidMF(input, 50.0, 75.0, 100.0, 100.0);

                double agg = Math.Max(Math.Min(curMerciful, outMerc),
                             Math.Max(Math.Min(curBalanced, outBal), Math.Min(curRelentless, outRel)));

                float sx = x + px;
                float sy = plotY + (float)((1.0 - agg) * plotH);
                polygon[px + 1] = new PointF(sx, sy);
            }

            using (var brush = new SolidBrush(Color.FromArgb(90, 100, 149, 237)))
            {
                g.FillPolygon(brush, polygon);
            }

            // Outline curves
            DrawUniversePlotCurvesOnly(g, x, plotY, w, plotH,
                (Color.RoyalBlue, input => TrapezoidMF(input, 0.0, 0.0, 25.0, 50.0)),
                (Color.Goldenrod, input => TriangularMF(input, 25.0, 50.0, 75.0)),
                (Color.Firebrick, input => TrapezoidMF(input, 50.0, 75.0, 100.0, 100.0)));

            // Mamdani Centroid Marker (Blue Line)
            float mamX = x + (float)(curMamAggression / 100.0 * w);
            using (var pen = new Pen(Color.Navy, 2.5f) { DashStyle = DashStyle.Solid })
            {
                g.DrawLine(pen, mamX, plotY, mamX, plotY + plotH);
            }
            g.DrawString($"Mam: {curMamAggression:F1}%", new Font("Segoe UI", 7.8f, FontStyle.Bold), Brushes.Navy, mamX - 25, plotY + plotH + 2);

            // Sugeno Marker (Green Line)
            float sugX = x + (float)(curSugAggression / 100.0 * w);
            using (var pen = new Pen(Color.DarkGreen, 2.5f) { DashStyle = DashStyle.DashDot })
            {
                g.DrawLine(pen, sugX, plotY, sugX, plotY + plotH);
            }
            g.DrawString($"Sug: {curSugAggression:F1}%", new Font("Segoe UI", 7.8f, FontStyle.Bold), Brushes.DarkGreen, sugX - 25, plotY - 14);
        }

        private void DrawUniversePlotCurvesOnly(Graphics g, int x, int y, int w, int h, params (Color Col, Func<double, double> Func)[] sets)
        {
            foreach (var set in sets)
            {
                using (var pen = new Pen(set.Col, 1.5f))
                {
                    PointF prev = PointF.Empty;
                    for (int px = 0; px <= w; px++)
                    {
                        double input = (double)px / w * 100.0;
                        double mu = set.Func(input);
                        float sx = x + px;
                        float sy = y + (float)((1.0 - mu) * h);

                        if (px > 0) g.DrawLine(pen, prev, new PointF(sx, sy));
                        prev = new PointF(sx, sy);
                    }
                }
            }
        }

        // ================= 3D ISOMETRIC CONTROL SURFACE =================
        private void Draw3DControlSurface(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int gridSteps = 16;
            PointF[,] projected = new PointF[gridSteps + 1, gridSteps + 1];
            double[,] zValues = new double[gridSteps + 1, gridSteps + 1];

            float originX = pnl3DSurface.Width / 2f;
            float originY = pnl3DSurface.Height * 0.70f;
            float scaleX = 15.5f;
            float scaleY = 8.5f;
            float scaleZ = 2.2f;

            // Generate grid points
            for (int i = 0; i <= gridSteps; i++)
            {
                double hp = (double)i / gridSteps * 100.0;
                double hL = TrapezoidMF(hp, 0, 0, 25, 50);
                double hM = TriangularMF(hp, 25, 50, 75);
                double hH = TrapezoidMF(hp, 50, 75, 100, 100);

                for (int j = 0; j <= gridSteps; j++)
                {
                    double skill = (double)j / gridSteps * 100.0;
                    double sL = TrapezoidMF(skill, 0, 0, 25, 50);
                    double sM = TriangularMF(skill, 25, 50, 75);
                    double sH = TrapezoidMF(skill, 50, 75, 100, 100);

                    double[] r = EvaluateAll9Rules(hL, hM, hH, sL, sM, sH);
                    double rMerc = Math.Max(r[0], Math.Max(r[1], r[3]));
                    double rBal = Math.Max(r[2], Math.Max(r[4], r[6]));
                    double rRel = Math.Max(r[5], Math.Max(r[7], r[8]));

                    double z = ComputeSugenoWeightedAverage(rRel, rBal, rMerc);
                    zValues[i, j] = z;

                    float gridX = i - (gridSteps / 2f);
                    float gridY = j - (gridSteps / 2f);

                    float sx = originX + (gridX - gridY) * scaleX;
                    float sy = originY + (gridX + gridY) * scaleY - (float)(z * scaleZ);

                    projected[i, j] = new PointF(sx, sy);
                }
            }

            // Draw Wireframe Quads Back-to-Front
            for (int i = 0; i < gridSteps; i++)
            {
                for (int j = 0; j < gridSteps; j++)
                {
                    PointF[] quad = new PointF[]
                    {
                        projected[i, j],
                        projected[i + 1, j],
                        projected[i + 1, j + 1],
                        projected[i, j + 1]
                    };

                    double avgZ = (zValues[i, j] + zValues[i + 1, j] + zValues[i + 1, j + 1] + zValues[i, j + 1]) / 4.0;
                    Color faceColor = Get3DHeightColor(avgZ);

                    using (var brush = new SolidBrush(faceColor))
                    {
                        g.FillPolygon(brush, quad);
                    }
                    using (var pen = new Pen(Color.FromArgb(45, 255, 255, 255), 1))
                    {
                        g.DrawPolygon(pen, quad);
                    }
                }
            }

            // Render Dynamic Operating Point Beacon
            float curGridX = (float)(curHp / 100.0 * gridSteps) - (gridSteps / 2f);
            float curGridY = (float)(curSkill / 100.0 * gridSteps) - (gridSteps / 2f);
            float beaconSx = originX + (curGridX - curGridY) * scaleX;
            float beaconFloorSy = originY + (curGridX + curGridY) * scaleY;
            float beaconSy = beaconFloorSy - (float)(curSugAggression * scaleZ);

            // Ground projection drop-line
            using (var linePen = new Pen(Color.Yellow, 1.5f) { DashStyle = DashStyle.Dot })
            {
                g.DrawLine(linePen, beaconSx, beaconFloorSy, beaconSx, beaconSy);
            }

            // Beacon glowing head
            g.FillEllipse(Brushes.Yellow, beaconSx - 6, beaconSy - 6, 12, 12);
            g.DrawEllipse(Pens.White, beaconSx - 6, beaconSy - 6, 12, 12);
            g.DrawString($"Operating State ({curHp:F0}%, {curSkill:F0}%, {curSugAggression:F1}%)",
                new Font("Segoe UI", 8.2f, FontStyle.Bold), Brushes.White, beaconSx + 8, beaconSy - 8);

            // 3D Axis Orientation Labels
            g.DrawString("Player Health (HP) [0% -> 100%] ->", new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.LightSkyBlue, projected[gridSteps, 0].X - 60, projected[gridSteps, 0].Y + 12);
            g.DrawString("<- Combat Skill [100% <- 0%]", new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.LightGreen, projected[0, gridSteps].X - 85, projected[0, gridSteps].Y + 12);
            g.DrawString("^ Enemy Boss Aggression (Z)", new Font("Segoe UI", 8.5f, FontStyle.Bold), Brushes.Salmon, originX - 70, 20);
        }

        private Color Get3DHeightColor(double z)
        {
            double norm = Math.Max(0.0, Math.Min(1.0, z / 100.0));
            int red = (int)(norm * 220 + 20);
            int blue = (int)((1.0 - norm) * 220 + 20);
            int green = (int)(Math.Sin(norm * Math.PI) * 160);
            return Color.FromArgb(190, red, green, blue);
        }
    }

    // Declared after the Form class so the Visual Studio Form Designer opens cleanly
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }
    }
}