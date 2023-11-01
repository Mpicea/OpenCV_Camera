using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraAppProject01
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;  // 카메라 캡처 객체
        private Mat frame;             // 현재 프레임을 저장할 객체
        private bool isRunning = false;  // 카메라가 실행 중인지 확인하는 변수
        private bool isColor = true;     // 컬러 모드인지 확인하는 변수
        private bool isBlurEnabled = false; //블러 모드 확인변수
        private bool isSharpenEnabled = false;  //샤핑모드 확인변수
        private bool isConcaveLensEnabled = false;  //오목렌즈모드 확인변수
        private bool isConvexLensEnabled = false;  //볼록렌즈모드 확인변수
        private bool FaceDetect = false;        //얼굴찾기모드 확인변수
        private bool EyeDetect = false;         //눈찾기모드 확인변수
        private bool canny = false;             //캐니에지모드 확인변수
        private bool Delete = false;    //모자이크모드 확인변수
        private bool HistoDect = false;  //히스토그램 평활화모드 확인변수

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            capture = new VideoCapture(0);  // 카메라 장치 연결
            frame = new Mat();
            capture.Set(VideoCaptureProperties.FrameWidth, pictureBox1.Width);  // 프레임 너비 설정
            capture.Set(VideoCaptureProperties.FrameHeight, pictureBox1.Height); // 프레임 높이 설정
        }

        private Mat HistogramEqualization(Mat frame)
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

            return frame;
        }

        public async void TakeVideo(object sender, EventArgs e)
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
                    if (isBlurEnabled) // 블러 활성화 상태라면
                    {
                        Mat kernel = new Mat(3, 3, MatType.CV_32F, new Scalar(1 / 9f));
                        Cv2.Filter2D(frame, frame, frame.Type(), kernel, new OpenCvSharp.Point(0, 0));
                    }
                    if (canny) //캐니 에지 활성화 상태라면
                    {
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                        Cv2.Canny(frame, frame, 100, 150);
                    }
                    if (isSharpenEnabled) // 샤프닝 활성화 상태라면
                    {
                        float[] data = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
                        Mat kernel = new Mat(3, 3, MatType.CV_32F, data);
                        Cv2.Filter2D(frame, frame, frame.Type(), kernel, new OpenCvSharp.Point(0, 0));
                    }
                    if (HistoDect) // 히스토그램 평활화 활성화 상태라면
                    {
                        frame = HistogramEqualization(frame);
                    }
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

                            //찾은 얼굴에 사각형 그리기
                            //foreach (var face in faces)
                            //{
                            //    Cv2.Rectangle(frame, face, Scalar.Red, 2);
                            //}

                            if (Delete)
                            {
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
                            else
                            {
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

                            
                            if (Delete)
                            {
                                Mat mask = new Mat(frame.Size(), MatType.CV_8UC1, Scalar.All(0));

                                foreach (var eye in eyes)
                                {
                                    //찾은 눈에 원그리기
                                    var center = new OpenCvSharp.Point
                                    {
                                        X = (int)(eye.X + eye.Width * 0.5),
                                        Y = (int)(eye.Y + eye.Height * 0.5)
                                    };
                                    var radius = (int)(Math.Max(eye.Width, eye.Height) * 0.5);
                                    Cv2.Circle(mask, center, radius, Scalar.All(255), -1);

                                    //모자이크 칠하기
                                    Cv2.Inpaint(frame, mask, frame, 5, InpaintMethod.Telea);
                                }
                            }
                            else
                            {
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

                            //찾은 눈에 원 그리기
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
                        }
                    }
                    if (isConcaveLensEnabled)   // 오목렌즈 활성화 상태라면
                    {
                        LensEffect(0.5);
                    }

                    else if (isConvexLensEnabled)   // 볼록렌즈 활성화 상태라면
                    {
                        LensEffect(1.5);
                    }

                    else
                    {
                        // 렌즈 효과가 비활성화된 경우, 원본 프레임을 PictureBox에 출력합니다.
                        pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                    }
                    await Task.Delay(50);  // fps
                }
            }
        }

        private void LensEffect(double exp)
        {
            int rows = frame.Height;  // 프레임의 높이를 얻습니다.
            int cols = frame.Width;   // 프레임의 너비를 얻습니다.

            //double exp 렌즈 왜곡 강도를 조절하는 지수 (0.5는 중간값)
            double scale = 1.0;      // 왜곡을 적용할 반경의 스케일 값

            // 새로운 좌표를 생성하기 위한 맵 생성
            Mat mapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat mapy = new Mat(rows, cols, MatType.CV_32F, 1);

            // 맵을 생성하여 각 픽셀에 새로운 좌표 값을 할당합니다.
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // 각 픽셀에 새로운 X, Y 좌표 값을 할당합니다.
                    mapx.At<float>(i, j) = 2.0f * j / (cols - 1) - 1.0f;
                    mapy.At<float>(i, j) = 2.0f * i / (rows - 1) - 1.0f;
                }
            }

            // 극좌표로 변환
            Mat r = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat theta = new Mat(rows, cols, MatType.CV_32F, 1);
            Cv2.CartToPolar(mapx, mapy, r, theta);

            // 왜곡 반경 내에 있는 점에만 왜곡을 적용합니다.
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (r.At<float>(i, j) < scale)
                    {
                        r.Set(i, j, (float)Math.Pow(r.At<float>(i, j), exp));
                    }
                }
            }

            // 다시 직교 좌표로 변환
            Mat newMapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat newMapy = new Mat(rows, cols, MatType.CV_32F, 1);
            Cv2.PolarToCart(r, theta, newMapx, newMapy);

            // 새로운 좌표 값을 화면에 맞게 조정합니다.
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMapx.Set(i, j, ((newMapx.At<float>(i, j) + 1.0f) * (cols - 1) / 2.0f));
                    newMapy.Set(i, j, ((newMapy.At<float>(i, j) + 1.0f) * (rows - 1) / 2.0f));
                }
            }

            // 왜곡을 적용한 프레임을 생성
            Mat distorted = new Mat();
            Cv2.Remap(frame, distorted, newMapx, newMapy, InterpolationFlags.Linear);

            // PictureBox에 왜곡된 영상을 출력합니다.
            pictureBox1.Image = BitmapConverter.ToBitmap(distorted);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning)  // 이미 카메라가 실행 중이면
            {
                isRunning = false;  // 실행 중 상태를 false로 변경
                btnStart.Text = "Start";  // 버튼 텍스트 변경
                return;
            }

            btnStart.Text = "Stop";  // 버튼 텍스트 변경
            isRunning = true;  // 실행 중 상태를 true로 변경

            Task task = Task.Run(() =>
            {
                TakeVideo(sender, e);
            });
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

        private void btnSharpen_Click(object sender, EventArgs e)
        {
            isSharpenEnabled = !isSharpenEnabled;
            if (isSharpenEnabled)
                isBlurEnabled = false; // 동시에 블러와 샤프닝을 사용할 수 없도록
        }

        private void btnBlur_Click(object sender, EventArgs e)
        {
            isBlurEnabled = !isBlurEnabled;
            if (isBlurEnabled)
                isSharpenEnabled = false; // 동시에 블러와 샤프닝을 사용할 수 없도록
        }

        private void btnConcaveLens_Click(object sender, EventArgs e)
        {
            isConcaveLensEnabled = !isConcaveLensEnabled;
            if (isConcaveLensEnabled)
                isConvexLensEnabled = false; // 동시에 사용할 수 없도록
        }

        private void btnConvexLens_Click(object sender, EventArgs e)
        {
            isConvexLensEnabled = !isConvexLensEnabled;
            if (isConvexLensEnabled)
                isConcaveLensEnabled = false; // 동시에 사용할 수 없도록
        }

        private void btnFace_Click(object sender, EventArgs e)
        {
            FaceDetect = !FaceDetect;
            if (FaceDetect)
                canny = false;
        }

        private void btnEye_Click(object sender, EventArgs e)
        {
            EyeDetect = !EyeDetect;
            if (EyeDetect)
                canny = false;
        }

        private void btnCannyEdge_Click(object sender, EventArgs e)
        {
            if(!EyeDetect == !FaceDetect)
                canny = !canny;
        }

        private void btnInpaint_Click(object sender, EventArgs e)
        {
            Delete = !Delete;
        }

        private void btnHisto_Click(object sender, EventArgs e)
        {
            HistoDect = !HistoDect;
        }
    }
}
