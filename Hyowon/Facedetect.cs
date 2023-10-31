using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
        private bool EyeDetect = false;

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

                    if (FaceDetect)
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
                            //foreach (var face in faces)
                            //{
                            //    Cv2.Rectangle(frame, face, Scalar.Red, 2);
                            //}

                            // 얼굴 영역에 오버레이할 이미지 로드
                            Mat overlayImage = Cv2.ImRead("moon.jpg");

                            // 찾은 얼굴에 이미지 합성
                            foreach (var face in faces)
                            {
                                Mat faceRegion = frame.SubMat(face); // 얼굴 영역 가져오기
                                Cv2.Resize(overlayImage, overlayImage, new OpenCvSharp.Size(face.Width, face.Height)); // 오버레이 이미지 크기 조정
                                Cv2.AddWeighted(overlayImage, 1.0, faceRegion, 0.5, 0, faceRegion); // 이미지 합성
                            }
                        }
                    }

                    if (EyeDetect)
                    {
                        using (var eyecascade = new CascadeClassifier("haarcascade_eye.xml"))
                        {
                            var gray = new Mat();
                            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                            Cv2.EqualizeHist(gray, gray);

                            // 눈 찾기
                            var eyes = eyecascade.DetectMultiScale(
                                gray,
                                scaleFactor: 1.1,
                                minNeighbors: 3,
                                flags: HaarDetectionTypes.DoCannyPruning,
                                minSize: new OpenCvSharp.Size(20, 20));

                            // 찾은 눈에 원 그리기
                            //foreach (var eye in eyes)
                            //{
                            //    var center = new OpenCvSharp.Point
                            //    {
                            //        X = (int)(eye.X + eye.Width * 0.5),
                            //        Y = (int)(eye.Y + eye.Height * 0.5)
                            //    };
                            //    var radius = (int)(Math.Max(eye.Width, eye.Height) * 0.5);
                            //    Cv2.Circle(frame, center, radius, Scalar.Blue, 2);
                            //}

                            // 눈 영역에 오버레이할 이미지 로드
                            Mat overlayImage = Cv2.ImRead("fire.jpg");

                            // 찾은 얼굴에 이미지 합성
                            foreach (var eye in eyes)
                            {
                                Mat eyeRegion = frame.SubMat(eye); // 눈 영역 가져오기
                                Cv2.Resize(overlayImage, overlayImage, new OpenCvSharp.Size(eye.Width, eye.Height)); // 오버레이 이미지 크기 조정
                                Cv2.AddWeighted(overlayImage, 1.0, eyeRegion, 0.5, 0, eyeRegion); // 이미지 합성
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
            FaceDetect = !FaceDetect;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EyeDetect = !EyeDetect;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isRunning = false;  
            capture.Release(); 
            this.Close();
        }
    }
}
