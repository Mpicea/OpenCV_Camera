using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraApp_histo
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        private Mat frame;
        private bool isRunning = false;
        private bool HistoDetect = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            capture = new VideoCapture(0);
            frame = new Mat();
            capture.Set(VideoCaptureProperties.FrameWidth, 640);
            capture.Set(VideoCaptureProperties.FrameHeight, 480);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                return;
            }

            isRunning = true;
            while (isRunning)
            {
                if (capture.IsOpened())
                {
                    capture.Read(frame);
                    if (HistoDetect)
                    {
                        HistogramEqualization(frame);
                    }
                    pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                }
                await Task.Delay(33);
            }
        }
        private void HistogramEqualization(Mat frame)
        {
            // 흑백
            //Mat grayFrame = new Mat();
            //Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);
            //Cv2.EqualizeHist(grayFrame, grayFrame);
            //Cv2.CvtColor(grayFrame, frame, ColorConversionCodes.GRAY2BGR);

            // 컬러
            Mat[] channels = Cv2.Split(frame);

            for (int i = 0; i < channels.Length; i++)
            {
                Mat grayFrame = new Mat();
                Cv2.EqualizeHist(channels[i], grayFrame); 
                channels[i] = grayFrame;
            }
            Cv2.Merge(channels, frame); 
        }
        private void button2_Click(object sender, EventArgs e)
        {
            HistoDetect = !HistoDetect;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isRunning = false;
            capture.Release();
            this.Close();
        }
    }
}
