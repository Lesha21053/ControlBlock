using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LIDARDataViewer
{
    public partial class Form1 : Form
    {
        private DataListener _dl;

        private delegate void AddMesaurment(string data);
        private readonly AddMesaurment _mesaurment;

        private CancellationTokenSource _dataListenertokenSource;

        private int counter;

         public Form1()
         {
             InitializeComponent();
             _mesaurment = ViewData;
         }

        private void StartButtom_Click(object sender, EventArgs e)
        {
            _dl = new DataListener("127.0.0.1", Convert.ToInt32(portTextBox.Text));
            drawPoint(pictureBox.Height / 2, pictureBox.Height / 2);
            _dl.PackageReceived += DataHendler;
            _dataListenertokenSource = new CancellationTokenSource();
            var  cts = _dataListenertokenSource.Token;
            var lidarListener = new Task(() => { _dl.Run(cts); }, cts);
            lidarListener.Start();

        }

        private void GraffInit(int step)
        {
            Graphics battlefield = pictureBox.CreateGraphics();
            for(int i=1; i< pictureBox.Height/step; i++)
               battlefield.DrawEllipse(new Pen(Brushes.Black, 1),new Rectangle(i * step/2, i * step/2, pictureBox.Height-i*step, pictureBox.Width-i*step));
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
           // _dl.PackageReceived -= DataHendler;
            _dataListenertokenSource.Cancel();

        }

        private void DataHendler(string data)
        {
            pictureBox.Invoke(_mesaurment, new object[] { data});
        }

        private void ViewData(string data)
        {
            GetPoints(ParseData(data), 15000);
        }

        public void ClearGraff()
        {
            pictureBox.Image = null;
            pictureBox.Invalidate();
            GraffInit(100);
        }
        private List<double[]> ParseData(string dataArrayStr)
        { 
            List<string> datastr  = dataArrayStr.Split(';').AsEnumerable().Where(s=>!string.IsNullOrEmpty(s)).ToList();
           List<double[]> data =new List<double[]>();
           foreach (var s in datastr) 
           {
               var mes = s.Split(' ');
               data.Add(new double[] { Convert.ToDouble(mes[0]), Convert.ToDouble(mes[1]) });
           }
          
           return data;
        }

        private void GetPoints(List<double[]> mes, int maxDis_mm)
        {
            GraffInit(100);
            double coeffScale = 0.25;
            double radianConvert = Math.PI / 180;
            foreach (var m in mes)
            {
                if (m[0] > 2.0 && m[0] < 4.0  )
                {
                    if (counter > 10)
                    {
                        ClearGraff();
                        counter = 0;
                        GraffInit(100);
                    }
                   
                    counter++;
                }
                   

                int x =  (int)Math.Round(m[1] * coeffScale * Math.Cos(m[0] * radianConvert))+ pictureBox.Width/2;
                int y =(int)Math.Round(m[1] * coeffScale * Math.Sin(m[0] * radianConvert))+ pictureBox.Width/2;
                drawPoint(x, y);
            }
        }

        public void drawPoint(int x, int y)
        {
            Graphics g = Graphics.FromHwnd(pictureBox.Handle);
            SolidBrush brush = new SolidBrush(Color.LimeGreen);
            Point dPoint = new Point(x, (pictureBox.Height - y));
            dPoint.X = dPoint.X - 2;
            dPoint.Y = dPoint.Y - 2;
            Rectangle rect = new Rectangle(dPoint, new Size(5, 5));
            g.FillRectangle(brush, rect);
            g.Dispose();
        }


    }
}
