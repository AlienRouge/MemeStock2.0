using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cur
{
    public partial class MemeStock : Form
    {
        public MemeStock()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;
            chart1.ChartAreas[0].AxisX.ScrollBar.LineColor = Color.Aquamarine;
            btStart.Text = "Start";
        }

        private bool _started;

        private void UpdateForm()
        {
            label2.Text = Math.Round(Player.Rubles, 3).ToString();
            label3.Text = Math.Round(Player.Dollars, 3).ToString();
            label5.Text = Math.Round(Player.GetProfit(), 3).ToString();

            if (Player.Dollars >= 100)
            {
                btSell100.Visible = true;
                btSell10.Visible = true;
                btSell1.Visible = true;
            }
            else if (Player.Dollars >= 10)
            {
                btSell100.Visible = false;
                btSell10.Visible = true;
                btSell1.Visible = true;
            }
            else if (Player.Dollars >= 1)
            {
                btSell100.Visible = false;
                btSell10.Visible = false;
                btSell1.Visible = true;
            }
            else
            {
                btSell100.Visible = false;
                btSell10.Visible = false;
                btSell1.Visible = false;
            }

            if (Player.Rubles >= Stock.TransferRUB(100))
            {
                btBuy100.Visible = true;
                btBuy10.Visible = true;
                btBuy1.Visible = true;
            }
            else if (Player.Rubles >= Stock.TransferRUB(10))
            {
                btBuy100.Visible = false;
                btBuy10.Visible = true;
                btBuy1.Visible = true;
            }
            else if (Player.Rubles >= Stock.TransferRUB(1))
            {
                btBuy100.Visible = false;
                btBuy10.Visible = false;
                btBuy1.Visible = true;
            }
            else
            {
                btBuy100.Visible = false;
                btBuy10.Visible = false;
                btBuy1.Visible = false;
            }
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            if (!_started)
            {
                _started = true;
                btStart.Text = "Stop";
                Stock.InitStock((double) edRate.Value);
                Player.InitPlayer();
                chart1.Series[0].Points.Clear();
                chart1.Series[0].Points.AddXY(0, Stock.Rate);
                edRate.Enabled = false;
                label1.Enabled = false;
                chart1.ChartAreas[0].AxisX.ScrollBar.Axis.ScaleView.Position = 0;
                timer1.Start();
            }
            else
            {
                _started = false;
                btStart.Text = "Start";
                edRate.Enabled = true;
                label1.Enabled = true;
                timer1.Stop();
            }
        }

        private void btBuy1_Click(object sender, EventArgs e)
        {
            Player.BuyUSD(1);
            UpdateForm();
        }

        private void btBuy10_Click(object sender, EventArgs e)
        {
            Player.BuyUSD(10);
            UpdateForm();
        }

        private void btBuy100_Click(object sender, EventArgs e)
        {
            Player.BuyUSD(100);
            UpdateForm();
        }

        private void btSell1_Click(object sender, EventArgs e)
        {
            Player.SellUSD(1);
            UpdateForm();
        }

        private void btSell10_Click(object sender, EventArgs e)
        {
            Player.SellUSD(10);
            UpdateForm();
        }

        private void btSell100_Click(object sender, EventArgs e)
        {
            Player.SellUSD(100);
            UpdateForm();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Stock.StepStock();
            UpdateForm();

            chart1.Series[0].Points.AddXY(Stock.Days, Stock.Rate);
            chart1.ChartAreas[0].AxisX.ScaleView.Size = 10;
            if (Stock.Days <= 10) return;
            chart1.ChartAreas[0].AxisX.ScrollBar.Axis.ScaleView.Position = Stock.Days - 10;
        }
    }

    public static class Stock
    {
        public static double Rate;
        public static int Days;
        private const double Sigma = 0.459;
        private const double Mu = 0.201;
        static double dt = 0.1;
        private static readonly Random Random = new Random();

        public static void InitStock(double initRate)
        {
            Rate = initRate;
            Days = 0;
        }

        private static double NormalDistribution()
        {
            double u1 = 1.0 - Random.NextDouble();
            double u2 = 1.0 - Random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            return randStdNormal;
        }


        public static void StepStock()
        {
            Days++;
            Rate *= Math.Exp((Mu - Math.Pow(Sigma, 2) / 2) * dt + Sigma * Math.Sqrt(dt) * NormalDistribution());
        }

        public static double TransferRUB(double dollars)
        {
            return dollars * Rate;
        }
    }

    public static class Player
    {
        private const double StartBalance = 1000;
        public static double Dollars;
        public static double Rubles = StartBalance;

        public static void BuyUSD(int quantity)
        {
            var broker = Stock.TransferRUB(quantity) + Stock.TransferRUB(quantity) * 0.003;
            if (broker > Rubles) return;
            Rubles -= broker;
            Dollars += quantity;
        }

        public static void SellUSD(int quantity)
        {
            var broker = Stock.TransferRUB(quantity) - Stock.TransferRUB(quantity) * 0.003;
            if (Dollars < quantity) return;
            Dollars -= quantity;
            Rubles += broker;
        }

        public static double GetProfit()
        {
            return Rubles - StartBalance;
        }

        public static void InitPlayer()
        {
            Dollars = 0;
            Rubles = StartBalance;
        }
    }
}