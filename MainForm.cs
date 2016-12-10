using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ddesolver;
using ZedGraph;
using System.Diagnostics;

namespace ddesolver
{
    public partial class MainForm : Form
    {
        List<ddesolver.DDESolver.Point> temp;
        DDESolver mySolver;
        GraphPane myPane;
        PointPairList[] points;
        List<double> X;
        ////
        double step_size = 0.1;
        int sample_method;
        long[] timerCollector;
        DDESolver.BoundaryValues bVi = new DDESolver.BoundaryValues { type = DDESolver.interval.closed, a = -1, b = 0 };
        DDESolver.BoundaryValues bVs = new DDESolver.BoundaryValues { type = DDESolver.interval.closed, a = 0, b = 20 };

        ////

        double fmain(double x, double f, double ftao) 
        {
            richTextBox2.Text = "y'= y(x-1)\ny=1";
            //return (-5)*f-((-5)*0.8)*ftao;// f=1,tao=1
            //return ((0.2*ftao)/(1+Math.Pow(ftao,10))) - 0.1*f; // tao=20 y=0.1//0.5*Math.Cosh(x)
            //return 1.4 * f * (1 - ftao); // tao=1, y=0.1// page 7
            //return f+ftao;
            return ftao;
            //return -f +ftao*(2.5-1.5*Math.Pow(ftao/1000,2));// y=999 tao=2
            //return -f + ftao + (x / 20) * Math.Cos(x / 20) + Math.Sin(x / 20) - Math.Sin((x - 1 + Math.Sin(x)) / 20);
            //return -3.5 * f + 4*ftao; //f=-x+1, tao=1
            //return (-50 * Math.Sin((2 * 3.14 / 3) * (x - 0.25)) * Math.Sin((2 * 3.14 / 3) * (x - 0.25))) * (f - 0.8 * ftao);
        }

        double taof(double x)
        { return 1; }

        double finit(double x)
        {
            return 1;
        }

        public MainForm()
        {
            InitializeComponent();
            mySolver = new DDESolver();
            myPane = zedGraphControl1.GraphPane;
            zedGraphControl1.GraphPane.Title.Text = "";
            zedGraphControl1.GraphPane.XAxis.Title.Text = "x";
            zedGraphControl1.GraphPane.YAxis.Title.Text = "y";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timerCollector = new long[7] {-1,-1,-1,-1,-1,-1,-1}; 

            zedGraphControl1.GraphPane.CurveList.Clear();
            dataGridView1.Rows.Clear();
            int xcount = (int)((bVs.b - bVs.a) / step_size);
            X =new List<double>(xcount);
            double j = bVs.a;
            while (j <= bVs.b)
            {
                X.Add(Math.Round(j,10));
                j += step_size;
            }
            
            points = new PointPairList[7];

            points[1] = new PointPairList();
            List<double> Y;
            try
            {
                if (bVs.b - bVs.a > 5) throw new Exception();
                
                temp = mySolver.StepMethod(step_size, bVi, bVs, fmain, finit, taof);
                timerCollector[1] = mySolver.getTime();

                X = new List<double>(temp.Select<DDESolver.Point, double>(point => point.x));
                Y = new List<double>(temp.Select<DDESolver.Point, double>(point => point.y));
                for (int i = 0; i < X.Count; i++)
                    (points[1]).Add(new PointPair(X[i], Y[i]));
                LineItem myCurve1 = myPane.AddCurve("Step Method    [ms:" + timerCollector[1].ToString() + "]", points[1], Color.Blue, SymbolType.None);
                myCurve1.Line.Width = 2.0F;
            }
            catch{ }
            
            points[2] = new PointPairList();
            try
            {
                temp = mySolver.EulerMethod(step_size, bVi, bVs, fmain, finit, taof);
                timerCollector[2] = mySolver.getTime();

                X = new List<double>(temp.Select<DDESolver.Point, double>(point => point.x));
                Y = new List<double>(temp.Select<DDESolver.Point, double>(point => point.y));
                for (int i = 0; i < X.Count; i++)
                    (points[2]).Add(new PointPair(X[i], Y[i]));
                LineItem myCurve2 = myPane.AddCurve("Euler's Method    [ms:" + timerCollector[2].ToString() + "]", points[2], Color.Aqua, SymbolType.None);
                myCurve2.Line.Width = 2.0F;
            }
            catch { richTextBox1.Text = richTextBox1.Text + "Can\'t be solved by Euler method.\n"; }
            
            points[3] = new PointPairList();
            try
            {
                temp = mySolver.RKMethod(step_size, bVi, bVs, fmain, finit, taof);
                timerCollector[3] = mySolver.getTime();

                X = new List<double>(temp.Select<DDESolver.Point, double>(point => point.x));
                Y = new List<double>(temp.Select<DDESolver.Point, double>(point => point.y));
                for (int i = 0; i < X.Count; i++)
                    (points[3]).Add(new PointPair(X[i], Y[i]));
                LineItem myCurve3 = myPane.AddCurve("Runge-Kutta Method    [ms:" + timerCollector[3].ToString() + "]", points[3], Color.Green, SymbolType.None);
                myCurve3.Line.Width = 2.0F;
            }
            catch { richTextBox1.Text = richTextBox1.Text + "Can\'t be solved by RK method.\n"; }

            points[4] = new PointPairList();
            try
            {
                
                temp = mySolver.IMTMethod(step_size, bVi, bVs, fmain, finit, taof);
                timerCollector[4] = mySolver.getTime();

                X = new List<double>(temp.Select<DDESolver.Point, double>(point => point.x));
                Y = new List<double>(temp.Select<DDESolver.Point, double>(point => point.y));
                for (int i = 0; i < X.Count; i++)
                    (points[4]).Add(new PointPair(X[i], Y[i]));
                LineItem myCurve4 = myPane.AddCurve("Interpolation method of majorant type    [ms:" + timerCollector[4].ToString() + "]", points[4], Color.Red, SymbolType.None);
                myCurve4.Line.Width = 2.0F;
            }
            catch { richTextBox1.Text = richTextBox1.Text + "Can\'t be solved by IOMMT method.\n"; }

            points[5] = new PointPairList();
            try
            {
                 temp = mySolver.EMTMethod(step_size, bVi, bVs, fmain, finit, taof);
                 timerCollector[5] = mySolver.getTime();
               
                X = new List<double>(temp.Select<DDESolver.Point, double>(point => point.x));
                Y = new List<double>(temp.Select<DDESolver.Point, double>(point => point.y));
                for (int i = 0; i < X.Count; i++)
                    (points[5]).Add(new PointPair(X[i], Y[i]));
                LineItem myCurve5 = myPane.AddCurve("Extrapolation method of majorant type    [ms:" + timerCollector[5].ToString() + "]", points[5], Color.BlueViolet, SymbolType.None);
            myCurve5.Line.Width = 2.0F;
            }
            catch { richTextBox1.Text = richTextBox1.Text + "Can\'t be solved by EOMMT method.\n"; }

            for (int i = 0; i < xcount; i++)
            {
                dataGridView1.Rows.Add(new string[] { X[i].ToString(), (points[1].Count == 0 ? "not implemented" : points[1][i].Y.ToString()), (points[2].Count == 0 ? "not implemented" : points[2][i].Y.ToString()), (points[3].Count == 0 ? "not implemented" : points[3][i].Y.ToString()), (points[4].Count == 0 ? "not implemented" : points[4][i].Y.ToString()), (points[5].Count == 0 ? "not implemented" : points[5][i].Y.ToString()) });
            }
            
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
            
            this.button2.PerformClick();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            //old variant
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = bVi.a.ToString();
            textBox3.Text = bVi.b.ToString();
            textBox4.Text = bVs.a.ToString();
            textBox2.Text = bVs.b.ToString();
            textBox6.Text = step_size.ToString();
            dataGridView2.Columns["dataGridViewTextBoxColumn1"].DisplayIndex = 0;
            dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
            dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 3;
            dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 1;
            dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 4;
            dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 5;
            dataGridView2.Columns["Column7"].DisplayIndex = 6;
            sample_method = 3;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double value=Convert.ToDouble(textBox6.Text.ToString());
                if(value<10 && value>=0.0001 && value<Math.Abs(bVi.b-bVi.a)) step_size=value;
                    else textBox6.Text=step_size.ToString();
            }
            catch
            {
                textBox6.Text=step_size.ToString();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(textBox1.Text.ToString());
                if (value < 0 && value >= -20 && value < bVi.b) bVi.a = value;
                else textBox1.Text = bVi.a.ToString();
            }
            catch
            {
                textBox1.Text = bVi.a.ToString();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(textBox3.Text.ToString());
                if (value <= 0 && value > -20 && value > bVi.a) bVi.b = value;
                else textBox3.Text = bVi.b.ToString();
            }
            catch
            {
                textBox3.Text = bVi.b.ToString();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(textBox4.Text.ToString());
                if (value < 500 && value >= 0 && value < bVs.b) bVs.a = value;
                else textBox4.Text = bVs.a.ToString();
            }
            catch
            {
                textBox4.Text = bVs.a.ToString();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(textBox2.Text.ToString());
                if (value < 500 && value > 0 && value > bVs.a) bVs.b = value;
                else textBox2.Text = bVs.b.ToString();
            }
            catch
            {
                textBox2.Text = bVs.b.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //___________
            //
            //new variant
            //___________
            try
            {
                dataGridView2.Columns["dataGridViewTextBoxColumn1"].DisplayIndex = 0;
                dataGridView2.Columns["Column7"].Visible = false;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                    if (row.Cells["Column7"].Value != null)
                    {
                        foreach (DataGridViewRow row_ in dataGridView2.Rows)
                            points[7][X.IndexOf(Convert.ToDouble(row_.Cells["dataGridViewTextBoxColumn1"].Value))] = new PointPair(Convert.ToDouble(row_.Cells["dataGridViewTextBoxColumn1"].Value), Convert.ToDouble(row_.Cells["Column7"].Value));
                        dataGridView2.Columns["Column7"].Visible = false;
                        break;
                    }

                dataGridView2.Rows.Clear();
                int[] methods_sequence = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
                double[] deviation = new double[7] { 0, 0, 0, 0, 0, 0, 0 };

                switch (comboBox1.Text)
                {
                    case "Step":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 1;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 5;
                        dataGridView2.Columns["Column7"].DisplayIndex = 6;
                        sample_method = 1; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                    case "Euler\'s":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 1;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 5;
                        dataGridView2.Columns["Column7"].DisplayIndex = 6;
                        sample_method = 2; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                    case "Runge-Kutta":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 1;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 5;
                        dataGridView2.Columns["Column7"].DisplayIndex = 6;
                        sample_method = 3; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                    case "IMOMT":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 1;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 5;
                        dataGridView2.Columns["Column7"].DisplayIndex = 6;
                        sample_method = 4; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                    case "EMOMT":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 5;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 1;
                        dataGridView2.Columns["Column7"].DisplayIndex = 6;
                        sample_method = 5; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                    case "Own variant":
                        dataGridView2.Columns["dataGridViewTextBoxColumn2"].DisplayIndex = 2;
                        dataGridView2.Columns["dataGridViewTextBoxColumn3"].DisplayIndex = 3;
                        dataGridView2.Columns["dataGridViewTextBoxColumn4"].DisplayIndex = 4;
                        dataGridView2.Columns["dataGridViewTextBoxColumn5"].DisplayIndex = 5;
                        dataGridView2.Columns["dataGridViewTextBoxColumn6"].DisplayIndex = 6;
                        dataGridView2.Columns["Column7"].DisplayIndex = 1;
                        dataGridView2.Columns["Column7"].Visible = true;
                        sample_method = 6; methods_sequence = new int[7] { 0, 3, 1, 2, 4, 5, 6 };
                        break;
                }

                int xcount = (int)((bVs.b - bVs.a) / step_size);
                string[] row_str_arr;
                for (int i = 0; i <= xcount; i++)
                {
                    row_str_arr = new string[] {
                    (X[i].ToString()),
                    (points[(methods_sequence[1])].Count == 0 || points[sample_method].Count == 0 ? "not implemented" : Math.Pow(points[sample_method][i].Y-points[(methods_sequence[1])][i].Y, 2).ToString()), 
                    (points[(methods_sequence[2])].Count == 0 || points[sample_method].Count == 0 ? "not implemented" : Math.Pow(points[sample_method][i].Y-points[(methods_sequence[2])][i].Y, 2).ToString()),
                    (points[(methods_sequence[3])].Count == 0 || points[sample_method].Count == 0 ? "not implemented" : Math.Pow(points[sample_method][i].Y-points[(methods_sequence[3])][i].Y, 2).ToString()),
                    (points[(methods_sequence[4])].Count == 0 || points[sample_method].Count == 0 ? "not implemented" : Math.Pow(points[sample_method][i].Y-points[(methods_sequence[4])][i].Y, 2).ToString()),
                    (points[(methods_sequence[5])].Count == 0 || points[sample_method].Count == 0 ? "not implemented" : Math.Pow(points[sample_method][i].Y-points[(methods_sequence[5])][i].Y, 2).ToString())};
                    deviation[1] = deviation[1] + (points[methods_sequence[1]].Count == 0 || points[sample_method].Count == 0 ? 0 : Math.Pow(points[sample_method][i].Y - points[(methods_sequence[1])][i].Y, 2));
                    deviation[2] = deviation[2] + (points[methods_sequence[2]].Count == 0 || points[sample_method].Count == 0 ? 0 : Math.Pow(points[sample_method][i].Y - points[(methods_sequence[2])][i].Y, 2));
                    deviation[3] = deviation[3] + (points[methods_sequence[3]].Count == 0 || points[sample_method].Count == 0 ? 0 : Math.Pow(points[sample_method][i].Y - points[(methods_sequence[3])][i].Y, 2));
                    deviation[4] = deviation[4] + (points[methods_sequence[4]].Count == 0 || points[sample_method].Count == 0 ? 0 : Math.Pow(points[sample_method][i].Y - points[(methods_sequence[4])][i].Y, 2));
                    deviation[5] = deviation[5] + (points[methods_sequence[5]].Count == 0 || points[sample_method].Count == 0 ? 0 : Math.Pow(points[sample_method][i].Y - points[(methods_sequence[5])][i].Y, 2));

                    dataGridView2.Rows.Add(row_str_arr);
                }
                /*double deviation=0;
                foreach (DataGridViewColumn column in dataGridView2.Columns)
                {
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                        if (row.Cells[column.Name].Value != null && row.Cells[column.Name].Value.ToString() != "not implemented" && column.Name.ToString() != "dataGridViewTextBoxColumn1")
                        {

                            deviation += Convert.ToDouble(row.Cells[column.Name].Value);
                        }
                    */
                for (int i = 1; i < 7; i++)
                    if (deviation[i] > 0.0000001)
                    {

                        richTextBox1.Text = richTextBox1.Text + comboBox1.Items[methods_sequence[i] - 1].ToString() + " -error- " + (deviation[i] / xcount).ToString("0.000000") + "   -time-   " + timerCollector[methods_sequence[i]].ToString() + "\n";
                    }
                richTextBox1.Text = richTextBox1.Text + "_______________________________\n";
                //}
                dataGridView2.Refresh();
            }
            catch { richTextBox1.Text = richTextBox1.Text + "Error\n_______________________________\n"; }
        } 
    }
}
