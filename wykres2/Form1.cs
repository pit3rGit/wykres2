using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;
using System.Deployment.Application;

namespace wykres2
{
    public partial class Form1 : Form
    {
        private string filename;
        private DateTime[] date;
        private float[] vAir;
        private float[] ch4;
        private float[] vAirMovingAvg;
        private DateTime[] dateMovingAvg;
        private Series airSeries;
        private Series ch4Series;
        private Series chartLeftRangeLine;
        private Series chartRightRangeLine;
        private Series airMovingAvgSeries;
        private string chartTipText;
        private DataPoint lastChosenPoint;
        private Series chosenPoint;
        private DateTime lastDatePicker1, lastDatePicker2;
        public int trawersId;
        private Form2 trawersyForm;
        public ArrayList vUasCollection;
        public ArrayList dateUasCollection;
        public enum instrumentType { UAS, MA };
        public instrumentType actualInstrument;
        public int airMarkerSize;
        public int ch4MarkerSize;
        public MarkerStyle airMarkerStyle;
        public MarkerStyle ch4MarkerStyle;

        public Form1()
        {
            InitializeComponent();
            this.trawersyForm = new Form2(this);
            airMarkerSize = 5;
            airMarkerStyle = MarkerStyle.Circle;
            ch4MarkerSize = 5;
            ch4MarkerStyle = MarkerStyle.Circle;
            disableDataUsableOptions();
        #if !DEBUG
            lb("Oprogramowanie IMG PAN do analizy danych z przyrządów. Do użytku wewnętrzengo. Wersja " + AssemblyVersion.Major.ToString() + "." + AssemblyVersion.Minor.ToString() + "." + AssemblyVersion.Build.ToString() + "." + AssemblyVersion.Revision.ToString());
        #endif
        }
        

        public Version AssemblyVersion
        {
            get
            {
                //if (ApplicationDeployment.IsNetworkDeployed)
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion;

            }
        }

        public void lb(string str)
        {
            this.listBox1.Items.Add(str);
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            Point p = this.chart1.PointToClient(MousePosition);
            double dp1 = chart1.ChartAreas[0].AxisX.Minimum;
            double dp2 = chart1.ChartAreas[0].AxisX.Maximum;
            double xRange = dp2 - dp1;
            double width = this.chart1.Size.Width;
            double areaWidth = width * chart1.ChartAreas[0].Position.Width * 0.01;
            double axWidth = areaWidth * chart1.ChartAreas[0].InnerPlotPosition.Width * 0.01;
            double areaX = width* chart1.ChartAreas[0].Position.X * 0.01;
            double axX = width * chart1.ChartAreas[0].InnerPlotPosition.X* chart1.ChartAreas[0].Position.Width * 0.01*0.01;
            double pXLeftMargin = axX+areaX;
            double pXRightMargin = width - pXLeftMargin-axWidth;

            double x = (p.X-pXLeftMargin) * xRange / (this.chart1.Size.Width-pXLeftMargin-pXRightMargin)+dp1;
            this.chart1.ChartAreas[0].AxisY.Minimum = vAir.Min();
            this.chart1.ChartAreas[0].AxisY.Maximum = vAir.Max();
            if (mouse.Button == MouseButtons.Left)
            {
                this.chart1.Series.Remove(chartLeftRangeLine);
                setLeftRangeLine(x);
            }
            else if (mouse.Button == MouseButtons.Right)
            {
                this.chart1.Series.Remove(chartRightRangeLine);
                setRightRangeLine(x);
            }
            else if(mouse.Button == MouseButtons.Middle)
            {
                //chosenPoint.Points.Remove(lastChosenPoint);
                this.chart1.Series.Remove(chosenPoint);
                chosenPoint = this.chart1.Series.Add("ChosenPoint");
                chosenPoint.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                chosenPoint.Points.Add(lastChosenPoint);
                
                chosenPoint.MarkerSize = 6;
                chosenPoint.MarkerColor = Color.Red;
                
                lb(chartTipText);
            }
            this.chart1.ChartAreas[0].AxisY.Minimum = (double)numericUpDown2.Value;
            this.chart1.ChartAreas[0].AxisY.Maximum = (double)numericUpDown1.Value;

            /* //DEBUG
            lb(p.X.ToString());
            lb("pXLeftMargin: "+  pXLeftMargin.ToString() +"\tpXRightMargin: " + pXRightMargin.ToString());
            lb("width: " + width.ToString() + "areaWidth: " + areaWidth.ToString() + "axWidth: " + axWidth.ToString());
            lb("areaX: " + areaX.ToString() + "axX: " + axX.ToString());
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            disableDataUsableOptions();
            this.openFileDialog1.ShowDialog();           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (false == readDataFile())
            {
                lb("Problem z odczytem pliku");
                return;
            }
                
            redrawChart1();
            enableDataUsableOptions();
            if (actualInstrument == instrumentType.UAS)
            {
                button8.Enabled = true;
                disableCh4Options();
            }
                
            if (actualInstrument == instrumentType.MA)
            {
                enableCh4Options();
                button8.Enabled = false;
            }
        }
        private void disableCh4Options()
        {
            stylWykresuCH4ToolStripMenuItem.Enabled = false;
            metanToolStripMenuItem.Enabled = false;
            średniaCH4ToolStripMenuItem.Enabled = false;
            numericUpDown4.Visible = false;
            numericUpDown3.Visible = false;
            metanToolStripMenuItem.Checked = false;
            checkChartSettings();
        }
        private void enableCh4Options()
        {
            stylWykresuCH4ToolStripMenuItem.Enabled = true;
            metanToolStripMenuItem.Enabled = true;
            średniaCH4ToolStripMenuItem.Enabled = true;
            numericUpDown4.Visible = true;
            numericUpDown3.Visible = true;
            metanToolStripMenuItem.Checked = true;
            checkChartSettings();
        }
        private void disableDataUsableOptions()
        {
            chart1.Enabled = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            stylWykresuToolStripMenuItem.Enabled = false;
            stylWykresuCH4ToolStripMenuItem.Enabled = false;
            widokToolStripMenuItem.Enabled = false;
            statystykaToolStripMenuItem.Enabled = false;
            filtryToolStripMenuItem.Enabled = false;
            wydrukToolStripMenuItem.Enabled = false;
            eksportToolStripMenuItem.Enabled = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown4.Enabled = false;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
        }
        private void enableDataUsableOptions()
        {
            chart1.Enabled = true;
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            dateTimePicker1.Enabled = true;
            dateTimePicker2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            stylWykresuToolStripMenuItem.Enabled = true;
            stylWykresuCH4ToolStripMenuItem.Enabled = true;
            widokToolStripMenuItem.Enabled = true;
            statystykaToolStripMenuItem.Enabled = true;
            filtryToolStripMenuItem.Enabled = true;
            wydrukToolStripMenuItem.Enabled = true;
            eksportToolStripMenuItem.Enabled = true;
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            numericUpDown3.Enabled = true;
            numericUpDown4.Enabled = true;
            dateTimePicker1.Enabled = true;
            dateTimePicker2.Enabled = true;
        }
        private Boolean readDataFile()
        {
            this.filename = this.openFileDialog1.FileName;
            lb(this.filename);
            if (filename == "")
            {
                lb("Nie wskazano pliku do odczytu");
                return false;
            }
            string[] dataFile = File.ReadAllLines(this.filename);
            if (filename.EndsWith(".csv"))
            {
                actualInstrument = instrumentType.MA;
                this.date = new DateTime[dataFile.Length];
                this.vAir = new float[dataFile.Length];
                this.ch4 = new float[dataFile.Length];
                short i = 0;
                foreach (string line in dataFile)
                {
                    string[] dataStr = line.Split(';');
                    this.date[i] = DateTime.Parse(dataStr[0]);
                    this.vAir[i] = float.Parse(dataStr[1]);
                    this.ch4[i] = float.Parse(dataStr[2]);
                    i++;
                }
            }
            else if (filename.EndsWith(".uas"))
            {
                actualInstrument = instrumentType.UAS;
                ArrayList vAirTrawers;
                trawersyForm.listBox1.Items.Clear();
                int i = 0;
                vAirTrawers = new ArrayList();
                vUasCollection = new ArrayList();
                vAirTrawers = new ArrayList();
                dateUasCollection = new ArrayList();
                ArrayList dateTrawers = new ArrayList();
                DateTime firstDate = new DateTime();
                foreach (string line in dataFile)
                {
                    if (line.Length > 45)
                    {
                        //Array.Clear(vAirTrawers, 0, dataFile.Length);
                        //Nagłówek serii pomiarowej
                        lb("[" + i + "] " + line);
                        trawersyForm.listBox1.Items.Add(line);
                        string[] lineElements = line.Split((char)9);
                        string dateStr = lineElements[1] + lineElements[2];
                        string id = lineElements[0].Trim();
                        dateStr = dateStr.Trim();
                        firstDate = DateTime.ParseExact(dateStr, "dd.MM.yyyy   hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        lb(firstDate.ToString());
                        if (i > 1)
                        {   // Wpisywanie trawersu do kolekcji gdy wystąpi nagłówek następnego trawersu
                            vUasCollection.Add(vAirTrawers);
                            vAirTrawers = new ArrayList();
                            dateUasCollection.Add(dateTrawers);
                            dateTrawers = new ArrayList();
                        }
                    }
                    else //budowanie tablicy danych
                    {
                        string vAirStr = line.Trim();
                        vAirStr = vAirStr.Replace('.', ',');
                        if (vAirStr == "")
                        {
                            i++;
                            lb("Pusta linia nr: " + i);
                            break;
                        }
                        float vAirf = float.Parse(vAirStr);
                        vAirTrawers.Add(vAirf);
                        dateTrawers.Add(firstDate);
                        //lb("dateTrawers: "+firstDate.ToString()+", vAirTrawers: " + ((float)vAirTrawers[vAirTrawers.Count - 1]).ToString() + " line: " + i);
                        firstDate = firstDate.AddSeconds(1);
                    }
                    i++;
                }
                vUasCollection.Add(vAirTrawers);
                dateUasCollection.Add(dateTrawers);
                trawersId = 0;
                button8.Enabled = true;
            }
            else
            {
                lb("Wybrano nieprawidłowy plik");
                return false;
            }
            return true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            redrawChart1();
        }
        public void redrawChart1()
        {
            if(actualInstrument == instrumentType.UAS)
            {
                ArrayList uasV = (ArrayList)vUasCollection[trawersId];
                ArrayList uasTime = (ArrayList)dateUasCollection[trawersId];
                vAir = (float[])uasV.ToArray(typeof(float));
                date = (DateTime[])uasTime.ToArray(typeof(DateTime));
                ch4 = new float[vAir.Length];
            }
            lb("Rysowanie N = " + vAir.Length + ", początek " + date.First().ToString() + ", koniec " + date.Last().ToString());
            //for(short i=0;i<5;i++)
            //    lb(this.date[i].ToString()+"\t"+ this.vAir[i].ToString() + "\t" + this.ch4[i].ToString());
            this.chart1.Series.Clear();
            airSeries = this.chart1.Series.Add("Air");
            airSeries.ChartType = SeriesChartType.Line;
            airSeries.XValueType = ChartValueType.Time;
            for(int i = 0; i<date.Length;i++)
            {
                airSeries.Points.AddXY(this.date[i].ToOADate(), this.vAir[i]);
            }
            numericUpDown1.Value = (decimal)Math.Ceiling(this.vAir.Max()*10)/10;
            numericUpDown2.Value = (decimal)Math.Floor(this.vAir.Min() * 10) / 10;

            //Metan
            if (actualInstrument == instrumentType.MA)
            {
                chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                chart1.ChartAreas[0].AxisY2.LabelStyle.Enabled = true;
                ch4Series = this.chart1.Series.Add("ch4");
                ch4Series.ChartType = SeriesChartType.Line;
                ch4Series.XValueType = ChartValueType.Time;
                for (int i = 0; i < date.Length; i++)
                {
                    ch4Series.Points.AddXY(this.date[i].ToOADate(), this.ch4[i]);
                }
                chart1.Series["ch4"].YAxisType = AxisType.Secondary;
                chart1.Series["ch4"].Color = Color.Red;
            }
            chart1.ChartAreas [0].AxisX.Minimum = date.First().ToOADate();
            chart1.ChartAreas [0].AxisX.Maximum = date.Last().ToOADate();
            dateTimePicker2.Value = date.First();
            dateTimePicker1.Value = date.Last();
            setLeftRangeLine(date.First().ToOADate());
            setRightRangeLine(date.Last().ToOADate());

            this.reshapeAxes();

            checkChartSettings();
        }
        private void setLeftRangeLine(double x)
        {
            if (x > chart1.ChartAreas[0].AxisX.Maximum)
            {
                x = chart1.ChartAreas[0].AxisX.Maximum;
            }
            if (x < chart1.ChartAreas[0].AxisX.Minimum)
            {
                x = chart1.ChartAreas[0].AxisX.Minimum;
            }
            this.chartLeftRangeLine = this.chart1.Series.Add("LeftRangeLine");
            this.chartLeftRangeLine.Points.AddXY(x, -20);
            this.chartLeftRangeLine.Points.AddXY(x, 20);
            this.chartLeftRangeLine.ChartType = SeriesChartType.Line;
            this.chartLeftRangeLine.Color = Color.Green;
        }

        private void setRightRangeLine(double x)
        {
            if(x > chart1.ChartAreas[0].AxisX.Maximum)
            {
                x = chart1.ChartAreas[0].AxisX.Maximum;
            }
            if (x < chart1.ChartAreas[0].AxisX.Minimum)
            {
                x = chart1.ChartAreas[0].AxisX.Minimum;
            }
            this.chartRightRangeLine = this.chart1.Series.Add("RightRangeLine");
            this.chartRightRangeLine.Points.AddXY(x, -20);
            this.chartRightRangeLine.Points.AddXY(x, 20);
            this.chartRightRangeLine.ChartType = SeriesChartType.Line;
            this.chartRightRangeLine.Color = Color.Red;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Y up range
            this.chart1.ChartAreas[0].AxisY.Maximum = (double)numericUpDown1.Value;
            reshapeAxes();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            //Y down range
            this.chart1.ChartAreas[0].AxisY.Minimum = (double)numericUpDown2.Value;
            reshapeAxes();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //Górny zakres czasu
            if (dateTimePicker2.Value.ToOADate() >= dateTimePicker1.Value.ToOADate())
            {
                lb("Nieprawidłowy zakres");
                dateTimePicker1.Value = DateTime.FromOADate(this.chart1.ChartAreas[0].AxisX.Maximum);
            }
            else
            {
                TimeSpan timedif = dateTimePicker1.Value.Subtract(lastDatePicker1);
                if (timedif.Seconds == -59)
                {
                    dateTimePicker1.Value = dateTimePicker1.Value.AddMinutes(1);
                }
                else if (timedif.Seconds == 59)
                {
                    dateTimePicker1.Value = dateTimePicker1.Value.AddMinutes(-1);
                }
                else if (timedif.Minutes == -59)
                {
                    dateTimePicker1.Value = dateTimePicker1.Value.AddHours(1);
                }
                else if (timedif.Minutes == 59)
                {
                    dateTimePicker1.Value = dateTimePicker1.Value.AddHours(-1);
                }
                this.chart1.ChartAreas[0].AxisX.Maximum = dateTimePicker1.Value.ToOADate();
            }
            lastDatePicker1 = dateTimePicker1.Value;
            reshapeAxes();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            //Dolny zakres czasu
            if (dateTimePicker2.Value.ToOADate() >= dateTimePicker1.Value.ToOADate())
            {
                lb("Nieprawidłowy zakres");
                dateTimePicker2.Value = DateTime.FromOADate(chart1.ChartAreas[0].AxisX.Minimum);
            }
            else
            {
                TimeSpan timedif = dateTimePicker2.Value.Subtract(lastDatePicker2);
                if (timedif.Seconds == -59)
                {
                    dateTimePicker2.Value = dateTimePicker2.Value.AddMinutes(1);
                }
                else if (timedif.Seconds == 59)
                {
                    dateTimePicker2.Value = dateTimePicker2.Value.AddMinutes(-1);
                }
                else if (timedif.Minutes == -59)
                {
                    dateTimePicker2.Value = dateTimePicker2.Value.AddHours(1);
                }
                else if (timedif.Minutes == 59)
                {
                    dateTimePicker2.Value = dateTimePicker2.Value.AddHours(-1);
                }

                this.chart1.ChartAreas[0].AxisX.Minimum = dateTimePicker2.Value.ToOADate();
            }
            lastDatePicker2 = dateTimePicker2.Value;
            reshapeAxes();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Powiększ
            double x1 = this.chartLeftRangeLine.Points[0].XValue;
            double x2 = this.chartRightRangeLine.Points[0].XValue;
            if (x1 > x2)
            {
                this.chart1.ChartAreas[0].AxisX.Minimum = x2;
                this.chart1.ChartAreas[0].AxisX.Maximum = x1;
            }
            else if (x2 > x1)
            {
                this.chart1.ChartAreas[0].AxisX.Minimum = x1;
                this.chart1.ChartAreas[0].AxisX.Maximum = x2;
            }
            else
                lb("Nie można powiększyć bo krańce przedziału są równe sobie");
            dateTimePicker2.Value = DateTime.FromOADate(this.chart1.ChartAreas[0].AxisX.Minimum);
            dateTimePicker1.Value = DateTime.FromOADate(this.chart1.ChartAreas[0].AxisX.Maximum);
            this.reshapeAxes();
        }
        private void reshapeAxes()
        {
            DateTime maxXDate = DateTime.FromOADate(this.chart1.ChartAreas[0].AxisX.Maximum);
            DateTime minXDate = DateTime.FromOADate(this.chart1.ChartAreas[0].AxisX.Minimum);
            DateTime startLabelTime = minXDate.AddSeconds(-minXDate.Second); //zerowanie sekund
            TimeSpan timeDif = maxXDate.Subtract(minXDate);
            if (timeDif.TotalMinutes < 10)
            {
                chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
            }
            else if (timeDif.TotalMinutes < 60)
            {
                chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 5;
                startLabelTime = startLabelTime.AddMinutes(-startLabelTime.Minute % 5+5);
            }
            else if (timeDif.TotalMinutes < 120)
            {
                chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 10;
                startLabelTime = startLabelTime.AddMinutes(-startLabelTime.Minute % 10+10);
            }
            else if (timeDif.TotalMinutes >= 120)
            {
                chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 20;
                startLabelTime = startLabelTime.AddMinutes(-startLabelTime.Minute % 20 + 10);
            }
            chart1.ChartAreas[0].AxisX.LabelStyle.IntervalType = DateTimeIntervalType.Minutes;
            chart1.ChartAreas[0].AxisX.LabelStyle.IntervalOffsetType = DateTimeIntervalType.Seconds;

            //chart1.ChartAreas[0].AxisX.LabelStyle.IntervalOffset = 0;//startLabelTime.Subtract(minXDate).TotalSeconds;
            //lb("startLabelTime: " + startLabelTime.ToString() + " minXDate: " + minXDate.ToString()+ " IntervalOffset: "+ chart1.ChartAreas[0].AxisX.LabelStyle.IntervalOffset);

            //Oś Y
            double maxY = chart1.ChartAreas[0].AxisY.Maximum;
            if(double.IsNaN(maxY) == true)
            {
                maxY = vAir.Max();
            }
            double minY = chart1.ChartAreas[0].AxisY.Minimum;
            if (double.IsNaN(minY) == true)
            {
                if (vAir.Length > 1)
                {
                    minY = vAir.Min();
                }
                else
                {
                    minY = maxY - 0.2;
                }                
            }
            double yDif = maxY - minY;
            double interval = 0.1;
            double offset = 0;
            chart1.ChartAreas[0].AxisY.LabelStyle.IntervalType = DateTimeIntervalType.Number;
            if(yDif < 1)
            {
                interval = 0.1;
            }
            else if(yDif < 2)
            {
                interval = 0.2;
            }
            else if (yDif < 5)
            {
                interval = 0.5;
            }
            else if (yDif >= 5)
            {
                interval = 1;
            }
            chart1.ChartAreas[0].AxisY.LabelStyle.Interval = interval;
            chart1.ChartAreas[0].AxisY.MajorGrid.Interval = interval;
            chart1.ChartAreas[0].AxisY.MajorTickMark.Interval = interval;

            if(minY >= 0)
            {
                offset = (minY % interval) == 0 ? 0 : (interval - (minY % interval));
            }
            else
            {
                offset = (Math.Abs(minY) % interval) == 0 ? 0 : (Math.Abs(minY) % interval);
            }
            if (offset < 0.1)
                offset = 0;

            chart1.ChartAreas[0].AxisY.LabelStyle.IntervalOffset = offset;
            chart1.ChartAreas[0].AxisY.MajorGrid.IntervalOffset = offset;
            chart1.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = offset;

            lb("yDif: "+yDif+"Y IntervalOffset: " + chart1.ChartAreas[0].AxisY.LabelStyle.IntervalOffset);

            // Metan
            double maxY2 = chart1.ChartAreas[0].AxisY2.Maximum;
            if (double.IsNaN(maxY2) == true)
            {
                maxY2 = ch4.Max();
            }
            double minY2 = chart1.ChartAreas[0].AxisY2.Minimum;
            if (double.IsNaN(minY2) == true)
            {
                if (ch4.Length > 1)
                {
                    minY2 = ch4.Min();
                }
                else
                {
                    minY2 = maxY2 - 0.2;
                }
            }
            double yDif2 = maxY2 - minY2;
            double interval2 = 0.1;
            double offset2 = 0;
            chart1.ChartAreas[0].AxisY2.LabelStyle.IntervalType = DateTimeIntervalType.Number;
            if (yDif2 <= 0.1)
            {
                interval2 = 0.01;
            }
            else if (yDif2 <= 0.2)
            {
                interval2 = 0.02;
            }
            else if (yDif2 <= 0.5)
            {
                interval2 = 0.05;
            }
            else if (yDif2 <= 1.2)
            {
                interval2 = 0.1;
            }
            else if (yDif2 <= 2.5)
            {
                interval2 = 0.2;
            }
            else if (yDif2 > 2.5)
            {
                interval2 = 0.5;
            }
            chart1.ChartAreas[0].AxisY2.LabelStyle.Interval = interval2;
            chart1.ChartAreas[0].AxisY2.MajorGrid.Interval = interval2;
            chart1.ChartAreas[0].AxisY2.MajorTickMark.Interval = interval2;

            if (minY2 >= 0)
            {
                offset2 = (minY2 % interval2) == 0 ? 0 : (interval2 - (minY2 % interval2));
            }
            else
            {
                offset2 = (Math.Abs(minY2) % interval2) == 0 ? 0 : (Math.Abs(minY2) % interval2);
            }
            if (offset2 < 0.1)
                offset2 = 0;
            if (minY2 < 0)
                offset2 = Math.Abs(minY2);

            chart1.ChartAreas[0].AxisY2.LabelStyle.IntervalOffset = offset2;
            chart1.ChartAreas[0].AxisY2.MajorGrid.IntervalOffset = offset2;
            chart1.ChartAreas[0].AxisY2.MajorTickMark.IntervalOffset = offset2;
        }
        private void button5_Click(object sender, EventArgs e)
        { //Pomniejsz
            dateTimePicker2.Value = date.First();
            dateTimePicker1.Value = date.Last();
            this.chart1.ChartAreas[0].AxisX.Minimum = dateTimePicker2.Value.ToOADate();
            this.chart1.ChartAreas[0].AxisX.Maximum = dateTimePicker1.Value.ToOADate();
            reshapeAxes();
        }

        private void button6_Click(object sender, EventArgs e)
        { //Rozciąganie w Y
            if (!checkData())
                return;
            if(date.Length == 1)
            {
                double oneData = Math.Floor(vAir[0] * 10) / 10;
                numericUpDown1.Value = (decimal)(oneData + 0.1); //górny zakres
                numericUpDown2.Value = (decimal)(oneData - 0.1); //dolny zakres
            }
            double x1 = this.chart1.ChartAreas[0].AxisX.Minimum;
            double x2 = this.chart1.ChartAreas[0].AxisX.Maximum;
            int iy1 = Array.FindIndex(this.date, i => i.ToOADate() >= x1);
            int iy2 = Array.FindIndex(this.date, i => i.ToOADate() >= x2);
            if (iy2 > iy1)
            {
                float[] subVAir = new float[iy2 - iy1];
                Array.Copy(vAir, iy1, subVAir, 0, iy2 - iy1);
                numericUpDown1.Value = (decimal)Math.Ceiling(subVAir.Max() * 10) / 10;
                numericUpDown2.Value = (decimal)Math.Floor(subVAir.Min() * 10) / 10;
            }
        }

        private List<int> getRange()
        {
            double x1 = this.chartLeftRangeLine.Points[0].XValue;
            double x2 = this.chartRightRangeLine.Points[0].XValue;
            if(x1 > x2)
            {// zmiana tak żeby wcześniejsza data była pierwsza
                double temp = x1;
                x1 = x2;
                x2 = temp;
            }
            int iy1 = Array.FindIndex(this.date, i => i.ToOADate() >= x1);
            int iy2 = Array.FindLastIndex(this.date, i => i.ToOADate() <= x2);
            if(iy1 == -1)
            {
                iy1 = Array.FindLastIndex(this.date, i => i.ToOADate() <= x1);
            }
            if (iy2 == -1)
            {
                iy2 = Array.FindIndex(this.date, i => i.ToOADate() >= x2);
            }
            if(iy1 == -1 || iy2 == -1)
            {
                lb("Błąd: Nie można znaleść indeksu granicy przedziału");
            }
            if (iy1 < 0)
            {
                iy1 = 0;
            }
            if (iy2 < 0)
            {
                iy2 = 0;
            }
            if (iy1 > date.Length - 1)
            {
                iy1 = date.Length - 1;
            }
            if (iy2 > date.Length - 1)
            {
                iy2 = date.Length - 1;
            }
            /*if (iy1 > iy2)
            { // zmiana tak żeby mniejszy indeks był pierwszy
                int temp;
                temp = iy1;
                iy1 = iy2;
                iy2 = temp;
            }*/
            List<int> range = new List<int>();
            range.Add(iy1);
            range.Add(iy2);
            return range;
        }
        private float [] getSubFloat(float[] f1)
        {
            List<int> range = getRange();
            int size = range[1] - range[0] + 1;
            float[] subFloat;
            subFloat = new float[size];
            if (size != 0)
            {                
                Array.Copy(f1, range[0], subFloat, 0, size);
            }
            return subFloat;
        }
        private float[] getSubVAir()
        {
            return getSubFloat(vAir);
        }
        private float[] getSubCH4()
        {
            return getSubFloat(ch4);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            double vMean = vAir.Sum() / vAir.Length;
            double vMax = vAir.Max();
            double vMin = vAir.Min();
            lb(String.Format("Dla całości:    v_śr = {0:f3} m/s v_max = {1:f2} m/s v_min = {2:f2} m/s ", vMean, vMax, vMin));
            float[] subVAir = getSubVAir();
            double vSubMean = subVAir.Sum() / subVAir.Length;
            double vSubMax = subVAir.Max();
            double vSubMin = subVAir.Min();
            lb(String.Format("Dla przedziału: v_śr = {0:f3} m/s v_max = {1:f2} m/s v_min = {2:f2} m/s ", vSubMean, vSubMax, vSubMin));
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (actualInstrument == instrumentType.UAS)
                trawersyForm.Show();
            else
                lb("Okno trawersów dostępne jest tylko dla przyrządu typu uAS4");
        }
        private void checkChartSettings()
        {
            if (!checkData())
                return;
            // Wykres prędkości
            if (linieToolStripMenuItem.Checked == true)
            {
                airSeries.ChartType = SeriesChartType.Line;
            }
            else
            {
                airSeries.ChartType = SeriesChartType.Point;
                airSeries.MarkerStyle = airMarkerStyle;
            }
            if (punktyToolStripMenuItem.Checked == true)
            {
                airSeries.MarkerSize = airMarkerSize;
                airSeries.MarkerStyle = airMarkerStyle;
            }
            else
            {
                airSeries.MarkerSize = 0;
                airSeries.MarkerStyle = MarkerStyle.None;
            }
            if(powietrzeToolStripMenuItem.Checked == false)
            {
                airSeries.ChartType = SeriesChartType.Point;
                airSeries.MarkerStyle = MarkerStyle.None;
                airSeries.MarkerSize = 0;
                chart1.ChartAreas[0].AxisY.Enabled = AxisEnabled.False;
            }
            else
            {
                chart1.ChartAreas[0].AxisY.Enabled = AxisEnabled.True;
            }
            // Wykres CH4
            if (actualInstrument == instrumentType.MA)
            {
                if (linieToolStripMenuItem1.Checked == true)
                {
                    ch4Series.ChartType = SeriesChartType.Line;
                }
                else
                {
                    ch4Series.ChartType = SeriesChartType.Point;
                    ch4Series.MarkerStyle = ch4MarkerStyle;
                }
                if (punktyToolStripMenuItem1.Checked == true)
                {
                    ch4Series.MarkerSize = ch4MarkerSize;
                    ch4Series.MarkerStyle = ch4MarkerStyle;
                }
                else
                {
                    ch4Series.MarkerSize = 0;
                    ch4Series.MarkerStyle = MarkerStyle.None;
                }                
            }
            if (metanToolStripMenuItem.Checked == false)
            {
                ch4Series.ChartType = SeriesChartType.Point;
                ch4Series.MarkerStyle = MarkerStyle.None;
                ch4Series.MarkerSize = 0;
                chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            }
            else
            {
                chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            }

            // Wykres średniej ruchomej
            if (średniaRuchomaToolStripMenuItem.Checked)
            {
                makeAndPlotAirMovingAvg();
            }
            else
            {
                if (this.chart1.Series.Contains(airMovingAvgSeries))
                {
                    this.chart1.Series.Remove(airMovingAvgSeries);
                }
            }
        }
        private void linieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void punktyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void średniaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                lbhr();                
                lb(String.Format("Średnia z wszystkich danych: v_śr = {0:f3} m/s", vAir.Average()));
                lb(String.Format("Średnia z przedziału: v_śr = {0:f3} m/s", getSubVAir().Average()));
                if (actualInstrument == instrumentType.MA)
                {
                    lb(String.Format("Średnia z wszystkich danych: CH4_śr = {0:f3} %", ch4.Average()));
                    lb(String.Format("Średnia z przedziału: CH4_śr = {0:f3} %", getSubCH4().Average()));
                }
                
            }
        }
        public Boolean checkData()
        {
            if(vAir == null)
            {
                lb("Brak danych");
                return false;
            }
            if (getSubVAir().Length == 0)
            {
                lb("Brak danych w wyznaczonym przedziale");
                return false;
            }
            return true;
        }

        private void odchylenieStdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                lbhr();                
                lb(String.Format("Odchylenie std v z wszystkich danych: \u03C3 = {0:f3} m/s", getStandardDeviation(vAir)));
                lb(String.Format("Odchylenie std v z przedziału: \u03C3 = {0:f3} m/s", getStandardDeviation(getSubVAir())));
                if (actualInstrument == instrumentType.MA)
                {
                    lb(String.Format("Odchylenie std CH4 z wszystkich danych: \u03C3 = {0:f3} %", getStandardDeviation(ch4)));
                    lb(String.Format("Odchylenie std CH4 z przedziału: \u03C3 = {0:f3} %", getStandardDeviation(getSubCH4())));
                }
            }
        }
        private double getStandardDeviation(float[] data)
        {
            double average = data.Average();
            double sumOfDerivation = 0;
            foreach (double value in data)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (data.Length - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        }

        private void wariancjaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                double stdAll = getStandardDeviation(vAir);
                double stdRange = getStandardDeviation(getSubVAir());
                double stdAllCh4 = getStandardDeviation(ch4);
                double stdRangeCh4 = getStandardDeviation(getSubCH4());
                lbhr();
                lb(String.Format("Wariancja v z wszystkich danych: \u03C3\xB2 = {0:f3} m\xB2/s\xB2", Math.Pow(stdAll, 2)));
                lb(String.Format("Wariancja v z przedziału: \u03C3\xB2 = {0:f3} m\xB2/s\xB2", Math.Pow(stdRange, 2)));
                if (actualInstrument == instrumentType.MA)
                {
                    lb(String.Format("Wariancja CH4 z wszystkich danych: \u03C3\xB2 = {0:f3} %", Math.Pow(stdAllCh4, 2)));
                    lb(String.Format("Wariancja CH4 z przedziału: \u03C3\xB2 = {0:f3} %", Math.Pow(stdRangeCh4, 2)));
                }
            }
        }
        private void lbhr()
        {
            lb("--------------------------------------------------------");
        }

        private void ilośćDanychToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                lbhr();
                lb(String.Format("Ilość próbek w przedziale: Np = {0}", getSubVAir().Length));
                lb(String.Format("Ilość wszystkich danych: N = {0}", vAir.Length));
            }
        }

        private void maximumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                lbhr();                
                lb(String.Format("Maximum globalne: N = {0}", vAir.Max()));
                lb(String.Format("Maximum w przedziale: Np = {0}", getSubVAir().Max()));
                if (actualInstrument == instrumentType.MA) lb(String.Format("Maximum globalne: N = {0}", ch4.Max()));
                if (actualInstrument == instrumentType.MA) lb(String.Format("Maximum w przedziale: Np = {0}", getSubCH4().Max()));
            }
        }

        private void minimumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true == checkData())
            {
                lbhr();
                lb(String.Format("Minimum globalne: N = {0}", vAir.Min()));
                lb(String.Format("Minimum w przedziale: Np = {0}", getSubVAir().Min()));                
                if (actualInstrument == instrumentType.MA) lb(String.Format("Minimum globalne: N = {0}", ch4.Min()));
                if (actualInstrument == instrumentType.MA) lb(String.Format("Minimum w przedziale: Np = {0}", getSubCH4().Min()));
            }
        }

        private void otwórzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void wczytajToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readDataFile();
        }

        private void zamknijProgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void makeAndPlotAirMovingAvg()
        {
            if (this.chart1.Series.Contains(airMovingAvgSeries))
            {
                this.chart1.Series.Remove(airMovingAvgSeries);
            }
            int windowSize = 10;
            int seriesSize = vAir.Length / windowSize + 1;
            vAirMovingAvg = new float[seriesSize];
            dateMovingAvg = new DateTime[seriesSize];
            int j = 0;
            for (int i = 0; i < (vAir.Length - windowSize); i = i + windowSize)
            {
                vAirMovingAvg[j] = vAir.Skip(i).Take(windowSize).Average();
                dateMovingAvg[j] = date[i + windowSize / 2];
                j++;
            }
            airMovingAvgSeries = this.chart1.Series.Add("Średnia ruchoma");
            airMovingAvgSeries.ChartType = SeriesChartType.Line;
            airMovingAvgSeries.XValueType = ChartValueType.Time;
            for (int i = 0; i < dateMovingAvg.Length; i++)
            {
                airMovingAvgSeries.Points.AddXY(this.dateMovingAvg[i].ToOADate(), this.vAirMovingAvg[i]);
            }
            airMovingAvgSeries.Color = Color.Red;
            airMovingAvgSeries.BorderWidth = 2;
        }
        private void średniaRuchomaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void wydrukToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Show();
        }

        private void prędkośćToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void średniaToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            // Górny zakres osi Y metanu
            numericUpDown3.Maximum = (decimal)((double)numericUpDown4.Value - 0.1);
            this.chart1.ChartAreas[0].AxisY2.Maximum = (double)numericUpDown4.Value;            
            reshapeAxes();
        }

        private void numericUpDown3_ValueChanged_1(object sender, EventArgs e)
        {
            // Dolny zakres osi Y metanu
            numericUpDown4.Minimum = (decimal)((double)numericUpDown3.Value + 0.1);
            this.chart1.ChartAreas[0].AxisY2.Minimum = (double)numericUpDown3.Value;
            reshapeAxes();
        }

        private void powietrzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void linieToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void punktyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void rozstępToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[] subVAir = getSubVAir();
            float[] subCH4 = getSubCH4();
            lbhr();
            lb(string.Format("Rozstęp v: {0:f2} m/s", vAir.Max() - vAir.Min()));
            if(actualInstrument == instrumentType.MA) lb(string.Format("Rozstęp CH4: {0:f2} %", ch4.Max() - ch4.Min()));
            lb(string.Format("Rozstęp dla wybranego przedziału v: {0:f2} m/s", subVAir.Max() - subVAir.Min()));
            if (actualInstrument == instrumentType.MA) lb(string.Format("Rozstęp dla wybranego przedziału CH4: {0:f2} %", subCH4.Max() - subCH4.Min()));
        }

        private void wartŚredniokwadratowaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[] subVAir = getSubVAir();
            float[] subCH4 = getSubCH4();
            lbhr();
            lb(string.Format("RMS v: {0:f2} m/s", rootMeanSquare(vAir)));
            if (actualInstrument == instrumentType.MA) lb(string.Format("RMS CH4: {0:f2} %", rootMeanSquare(ch4)));
            lb(string.Format("RMS dla wybranego przedziału v: {0:f2} m/s", rootMeanSquare(subVAir)));
            if (actualInstrument == instrumentType.MA) lb(string.Format("RMS dla wybranego przedziału CH4: {0:f2} %", rootMeanSquare(subCH4)));
        }
        private static double rootMeanSquare(float[] x)
        {
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sum += (x[i] * x[i]);
            }
            return Math.Sqrt(sum / x.Length);
        }

        private void stylWykresuCH4ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void metanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkChartSettings();
        }

        private void średniaCH4ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //Czyszczenie listboxa
            listBox1.Items.Clear();
        }

        private void stylWykresuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void widokToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void statystykaToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void filtryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void eksportToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void siatkaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (siatkaToolStripMenuItem1.Checked)
            {
                chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = true;
                //chart1.ChartAreas[0].AxisY2.MinorGrid.Enabled = true;
            }

            else
            {
                chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                //chart1.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;
            }
        }

        private void siatkaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (siatkaToolStripMenuItem.Checked)
            {
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
                //chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            }

            else
            {
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                //chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            }
        }

        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            // Check selected chart element and set tooltip text for it
            switch (e.HitTestResult.ChartElementType)
            {
                case ChartElementType.DataPoint:
                    DataPoint dataPoint = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                    e.Text = string.Format("Wybrany punkt: {0}\t{1:F3}", DateTime.FromOADate(dataPoint.XValue), dataPoint.YValues[0]);
                    this.chartTipText = e.Text;
                    this.lastChosenPoint = dataPoint;
                    break;
            }
        }
    }
}