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

namespace CameraApp
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        private Mat frame;
        private bool isRunning = false;
        private bool FaceDetect = false;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            capture = new VideoCapture(0);
            frame = new Mat();
            capture.Set(VideoCaptureProperties.FrameWidth, 320);
            capture.Set(VideoCaptureProperties.FrameHeight, 240);
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

                    if (FaceDetect) // 얼굴 감지 버튼이 활성화되어 있는 경우에만 실행
                    {
                        using (var cascade = new CascadeClassifier("haarcascade_frontalface_default.xml"))
                        {
                            var gray = new Mat();
                            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                            Cv2.EqualizeHist(gray, gray);

                            // 얼굴 찾기
                            var faces = cascade.DetectMultiScale(
                                gray,
                                scaleFactor: 1.1,
                                minNeighbors: 3,
                                flags: HaarDetectionTypes.DoCannyPruning,
                                minSize: new OpenCvSharp.Size(30, 30));

                            // 찾은 얼굴에 사각형 그리기
                            foreach (var face in faces)
                            {
                                Cv2.Rectangle(frame, face, Scalar.Red, 2);
                            }
                        }
                    }
                    pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                }
                await Task.Delay(33);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FaceDetect = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FaceDetect = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isRunning = false;  
            capture.Release(); 
            this.Close();
        }
    }
}
