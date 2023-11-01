using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win_Distortion_Camera_01
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;  // 카메라 캡처 객체
        private Mat frame;             // 현재 프레임을 저장할 객체
        private bool isRunning = false;  // 카메라가 실행 중인지 확인하는 변수
        private bool isColor = true;     // 컬러 모드인지 확인하는 변수
        private bool isFaceDetect = false; //안면인식

        public Form1()
        {
            InitializeComponent();

            capture = new VideoCapture(0);  // 카메라 장치 연결
            frame = new Mat();
            capture.Set(VideoCaptureProperties.FrameWidth, cameraPbox.Width);  // 프레임 너비 설정
            capture.Set(VideoCaptureProperties.FrameHeight, cameraPbox.Height); // 프레임 높이 설정 
        }


        private async void TakeVideo(object sender, EventArgs e)
        {
            try
            {
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

                        if (isFaceDetect) //안면 인식이 열려있는 상태라면
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

                                // 찾은 얼굴에 모자이크 그리기
                                Mat mask = new Mat(frame.Size(), MatType.CV_8UC1, Scalar.All(0));

                                foreach (var face in faces)
                                {
                                    //찾은 얼굴에 원 그리기
                                    var center = new OpenCvSharp.Point
                                    {
                                        X = (int)(face.X + face.Width * 0.5),
                                        Y = (int)(face.Y + face.Height * 0.5)
                                    };
                                    var radius = (int)(Math.Max(face.Width, face.Height) * 0.5);
                                    Cv2.Circle(mask, center, radius, Scalar.All(255), -1);

                                    //찾은 얼굴 모자이크 하기
                                    Cv2.Inpaint(frame, mask, frame, 5, InpaintMethod.Telea);
                                }
                            }
                        }

                        cameraPbox.Image = BitmapConverter.ToBitmap(frame);  // PictureBox에 영상 출력
                    }
                    await Task.Delay(33);  // 대략 30 fps
                }
            }
            catch (Exception ex)
            {

            }
        }


        private void gostopBtn_Click(object sender, EventArgs e)
        {
            if (isRunning)  // 이미 카메라가 실행 중이면
            {
                isRunning = false;  // 실행 중 상태를 false로 변경
                gostopBtn.Text = "Start";  // 버튼 텍스트 변경
                return;
            }

            gostopBtn.Text = "Stop";  // 버튼 텍스트 변경
            isRunning = true;  // 실행 중 상태를 true로 변경

            Task task = Task.Run(() =>
            {
                TakeVideo(sender, e);
            });
        }

        private void grayBtn_Click(object sender, EventArgs e)
        {
            isColor = false;  // 흑백 모드로 변경
        }

        private void faceBtn_Click(object sender, EventArgs e)
        {
            if (isFaceDetect)  // 이미 안면인식이 실행되는 중이라면
            {
                isFaceDetect = false;  // 실행 중 상태를 false로 변경
                faceBtn.Text = "FaceStart";  // 버튼 텍스트 변경
                return;
            }

            faceBtn.Text = "FaceStop";  // 버튼 텍스트 변경
            isFaceDetect = true;  // 실행 중 상태를 true로 변경
        }

        private void colorBtn_Click(object sender, EventArgs e)
        {
            isColor = true;   // 컬러 모드로 변경
        }
    }
}
