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
        private VideoCapture capture;  // 카메라 캡처 객체
        private Mat frame;             // 현재 프레임을 저장할 객체
        private bool isRunning = false;  // 카메라가 실행 중인지 확인하는 변수
        private bool isColor = true;     // 컬러 모드인지 확인하는 변수
        private bool isBlurEnabled = false; //블러 모드 확인변수
        private bool isSharpenEnabled = false;  //샤핑모드 확인변수
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            capture = new VideoCapture(0);  // 카메라 장치 연결
            frame = new Mat();
            capture.Set(VideoCaptureProperties.FrameWidth, 640);  // 프레임 너비 설정
            capture.Set(VideoCaptureProperties.FrameHeight, 480); // 프레임 높이 설정
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning)  // 이미 카메라가 실행 중이면
            {
                isRunning = false;  // 실행 중 상태를 false로 변경
                btnStart.Text = "Start";  // 버튼 텍스트 변경
                return;
            }

            btnStart.Text = "Stop";  // 버튼 텍스트 변경
            isRunning = true;  // 실행 중 상태를 true로 변경

            while (isRunning)  // 카메라가 실행 중이면
            {
                if (capture.IsOpened())  // 카메라가 연결되어 있으면
                {
                    capture.Read(frame);  // 프레임 읽기

                    if (!isColor)  // 흑백 모드이면
                    {
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);  // 컬러를 흑백으로 변경
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGR);  // 흑백을 다시 컬러로 변경 (PictureBox 호환을 위해)
                    }
                    if (isBlurEnabled) // 블러 활성화 상태라면
                    {
                        Mat kernel = new Mat(3, 3, MatType.CV_32F, new Scalar(1 / 9f));
                        Cv2.Filter2D(frame, frame, frame.Type(), kernel, new OpenCvSharp.Point(0, 0));
                    }
                    else if (isSharpenEnabled) // 샤프닝 활성화 상태라면
                    {
                        float[] data = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
                        Mat kernel = new Mat(3, 3, MatType.CV_32F, data);
                        Cv2.Filter2D(frame, frame, frame.Type(), kernel, new OpenCvSharp.Point(0, 0));
                    }
                    pictureBox1.Image = BitmapConverter.ToBitmap(frame);  // PictureBox에 영상 출력                 
                }
                await Task.Delay(50);  // fps
            }
        }

        private void btnBlack_Click(object sender, EventArgs e)
        {
            isColor = false;  // 흑백 모드로 변경
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            isColor = true;   // 컬러 모드로 변경
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            isRunning = false;  // 카메라 중지
            capture.Release();  // 카메라 자원 해제
            this.Close();       // 프로그램 종료

        }

        private async void btnSharpen_Click(object sender, EventArgs e)
        {
            isSharpenEnabled = !isSharpenEnabled;
            if (isSharpenEnabled)
                isBlurEnabled = false; // 동시에 블러와 샤프닝을 사용할 수 없도록
        }

        private async void btnBlur_Click(object sender, EventArgs e)
        {
            isBlurEnabled = !isBlurEnabled;
            if (isBlurEnabled)
                isSharpenEnabled = false; // 동시에 블러와 샤프닝을 사용할 수 없도록
        }
    }
}