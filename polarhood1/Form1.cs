using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Common.Size;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Stocks.Client;
using System.IO;
using System.Net;
using Microsoft.Data.Analysis;
using System.Management;
using System.Windows.Forms.DataVisualization.Charting;
using Graph = System.Windows.Forms.DataVisualization.Charting;

namespace polarhood1
{
    public partial class Form1 : Form
    {
        public static Series mainSeries;
       




        public Form1()
        {
            InitializeComponent();
        }

 

        private void Form1_Load(object sender, EventArgs e)
        {
            


            TimeSpan start = new TimeSpan(16, 30, 0); //10 o'clock
            TimeSpan end = new TimeSpan(23, 0, 0); //12 o'clock
            TimeSpan now = DateTime.Now.TimeOfDay;

            if ((now > start) && (now < end))
            {
                label3.Text = ("Open");
                label3.ForeColor = Color.Lime;
            }
            else
            {
                label3.Text = ("Close");
                label3.ForeColor = Color.Red;
            }

            TimeSpan start1 = new TimeSpan(10, 0, 0); //10 o'clock
            TimeSpan end1 = new TimeSpan(18, 30, 0); //12 o'clock
            TimeSpan now1 = DateTime.Now.TimeOfDay;

            if ((now1 > start1) && (now1 < end1))
            {
                label4.Text = ("Open");
                label4.ForeColor = Color.Lime;
            }
            else
            {
                label4.Text = ("Close");
                label4.ForeColor = Color.Red;
            }

        }

 


        private void button1_Click(object sender, EventArgs e)
        {
            
            // cleans the series
            chart1.Series.Clear();
            
            string stock = textBox1.Text;
            label5.Text = (stock);
            button3.Text = ("1Min");
            //set up annotation area
            
           
            


            chart1.Series.Add(stock);
            chart1.Series[stock].ChartType = Graph.SeriesChartType.Candlestick;

            // downloads csv and pastes it to textbox2
            string api2 = textBox5.Text;
            string symbol = textBox1.Text;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://" + $@"www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={api2}&datatype=csv");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();
            File.WriteAllText("stockdata.csv", results);
            DataFrame df = DataFrame.LoadCsv("stockdata.csv");
            textBox2.Text = df.ToString();

            string line1 = File.ReadLines("stockdata.csv").Skip(1).Take(1).First();
            var values1 = line1.Split(',');
            var stringDateArr1 = values1[0].Split('-');
            label6.Text = ("$" + values1[4]);


            // reads the csv and displays the content to chart
            using (var reader = new StreamReader("stockdata.csv"))
            {
                bool isFirstLine = true;
                while (!reader.EndOfStream)
                {
                    //to get rid of the first line of gaff that alpha-vantage gives
                    if (isFirstLine)
                    {
                        reader.ReadLine();
                        isFirstLine = false;
                    }

                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //date stuff
                    var stringDateArr = values[0].Split('-');
                    int[] dateInfo = new int[3];
                    dateInfo[0] = Convert.ToInt32(stringDateArr[0]);
                    dateInfo[1] = Convert.ToInt32(stringDateArr[1]);
                    dateInfo[2] = Convert.ToInt32(stringDateArr[2]);

                    chart1.Series[stock].XValueType = ChartValueType.DateTime;
                    DateTime x = new DateTime(dateInfo[0], dateInfo[1], dateInfo[2]);

                    //candle stick data
                    double open = Convert.ToDouble(values[1]);
                    double high = Convert.ToDouble(values[2]);
                    double low = Convert.ToDouble(values[3]);
                    double close = Convert.ToDouble(values[4]);
                    double[] data = { high, low, open, close };
                    DataPoint candleStick = new DataPoint(x.ToOADate(), data);
                    chart1.Series[stock].Points.Add(candleStick);
         



                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataFrame df = DataFrame.LoadCsv("stockdata.csv");
            string df1 = df.ToString();
            string stock = textBox1.Text;

            using (DcWebHook dcWeb = new DcWebHook())
            {

                {
                    string webhook = textBox5.Text;
                    dcWeb.ProfilePicture = "https://variety.com/wp-content/uploads/2013/06/wolf-of-wall-street.jpg?w=681&h=383&crop=1";
                    dcWeb.UserName = "pomp the shit out of " + stock;
                    dcWeb.WebHook = "https://" + $@"discord.com/api/webhooks/{webhook}";
                    dcWeb.SendMessage("```" + ": " + df1 + "```");
                }
            }

        }


        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            // candlestick colors
            string stock = textBox1.Text;
            for (int i = 0; i < chart1.Series[stock].Points.Count; i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint point = chart1.Series[stock].Points[i];
                double open = point.YValues[2];
                double close = point.YValues[3];

                if (open > close)
                {
                    point.Color = Color.Green;
                }
                else
                {
                    point.Color = Color.Red;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string api1 = textBox3.Text;
            string stock = textBox1.Text;
            chart1.Series.Clear();

            chart1.Series.Add(stock);
            chart1.Series[stock].ChartType = Graph.SeriesChartType.Candlestick;


            string remoteUri = "https://" + $"api.twelvedata.com/time_series?symbol={stock}&interval=1min&apikey={api1}&format=CSV";
            string fileName = "stockdata.csv";

            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(remoteUri, fileName);
            DataFrame df = DataFrame.LoadCsv("stockdata.csv");
            this.textBox2.Text = df.ToString();


            
            using (var reader = new StreamReader("stockdata.csv"))
            {
                bool isFirstLine = true;
                while (!reader.EndOfStream)
                {
                    //to get rid of the first line of gaff that alpha-vantage gives
                    if (isFirstLine)
                    {
                        reader.ReadLine();
                        isFirstLine = false;
                    }

                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    //date stuff
                    var stringTimeArray = values[0].Split(' ');

                    var stringDateArr = stringTimeArray[1].Split(':');
                    var stringDateArr1 = stringTimeArray[0].Split('-');
                    int[] dateInfo = new int[6];
                    dateInfo[0] = Convert.ToInt32(stringDateArr[0]);
                    dateInfo[1] = Convert.ToInt32(stringDateArr[1]);
                    dateInfo[2] = Convert.ToInt32(stringDateArr[2]);
                    dateInfo[3] = Convert.ToInt32(stringDateArr1[0]);
                    dateInfo[4] = Convert.ToInt32(stringDateArr1[1]);
                    dateInfo[5] = Convert.ToInt32(stringDateArr1[2]);

                    chart1.Series[stock].XValueType = ChartValueType.DateTime;
                    DateTime x = new DateTime(dateInfo[3], dateInfo[4], dateInfo[5], dateInfo[0], dateInfo[1], dateInfo[2]);
                    // DateTime x = DateTime.Parse(values[0]);

                    //candle stick data
                    double open = Convert.ToDouble(values[1]);
                    double high = Convert.ToDouble(values[2]);
                    double low = Convert.ToDouble(values[3]);
                    double close = Convert.ToDouble(values[4]);
                    double[] data = { high, low, open, close };
                    DataPoint candleStick = new DataPoint(x.ToOADate(), data);
                    chart1.Series[stock].Points.Add(candleStick);




                }
            }
        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            // candlestick colors
            string stock = textBox1.Text;
            for (int i = 0; i < chart1.Series[stock].Points.Count; i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint point = chart1.Series[stock].Points[i];
                double open = point.YValues[2];
                double close = point.YValues[3];

                if (open > close)
                {
                    point.Color = Color.Green;
                }
                else
                {
                    point.Color = Color.Red;
                }
            }

            for (int i = 0; i < chart1.Series[stock].Points.Count; i++)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint point = chart1.Series[stock].Points[i];
                double high = point.YValues[0];
                double low = point.YValues[1];

                if (high > low)
                {
                    label6.ForeColor = Color.Green;
                }
                else
                {
                    label6.ForeColor = Color.Red;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }
    }
}
