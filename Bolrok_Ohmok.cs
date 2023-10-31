using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoOpenCv
{

    public partial class Form1 : Form
    {
        VideoCapture capture = new VideoCapture(0);
        Mat frame = new Mat();
        bool isRunning = false;
        bool isColor = true;
        bool boolmirror = false;
        bool ohmirror = false;
        bool canny = false;
        public Form1()
        {
            InitializeComponent();
            capture.Set(VideoCaptureProperties.FrameWidth, pictureBox1.Width);
            capture.Set(VideoCaptureProperties.FrameHeight, pictureBox1.Height);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                button1.Text = "카메라 캡처 시작";
                return;
            }

            button1.Text = "멈춤";
            isRunning = true;

            while (isRunning)
            {
                if (capture.IsOpened() == true)
                {
                    capture.Read(frame);
                    CameraFunction cameraFunction = new CameraFunction();
                    if (isColor == false)
                    {
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGR);
                    }
                    if (canny)
                    {
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                        Cv2.Canny(frame, frame, 100, 150);
                    }
                    if (boolmirror)
                    {
                        frame = cameraFunction.ApplyDistortion_Bol(frame);
                    }
                    else if (ohmirror)
                    {
                        frame = cameraFunction.ApplyDistortion_Oh(frame);
                    }
                    pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                }
                await Task.Delay(60);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isRunning = false;
            capture.Release();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isColor = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isColor = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            boolmirror = !boolmirror;
            if (ohmirror)
                boolmirror = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ohmirror = !ohmirror;
            if (boolmirror)
                ohmirror = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            canny = !canny;
        }
    }
    public class CameraFunction
    {
        public Mat ApplyDistortion_Oh(Mat inputImage)
        {
            int rows = inputImage.Height;
            int cols = inputImage.Width;

            double exp = 0.5; // 볼록, 오목 지수 설정(오목 0.1 ~ 1, 볼록 1, 1~)
            double scale = 1.0; // 변환 영역 크기(0 ~ 1)

            // 매핑 배열 생성
            Mat mapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat mapy = new Mat(rows, cols, MatType.CV_32F, 1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // 좌상단 기준 좌표에서 -1 ~ 1로 정규화된 중심점 기준 좌표로 변경
                    mapx.At<float>(i, j) = 2.0f * j / (cols - 1) - 1.0f;
                    mapy.At<float>(i, j) = 2.0f * i / (rows - 1) - 1.0f;
                }
            }

            Mat r = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat theta = new Mat(rows, cols, MatType.CV_32F, 1);

            //직교 좌표를 극 좌표로 변환
            Cv2.CartToPolar(mapx, mapy, r, theta);


            //왜곡 영역만 중심 확대/축소 지수 적용
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

            Mat newMapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat newMapy = new Mat(rows, cols, MatType.CV_32F, 1);
            // 극 좌표를 직교좌표로 변환
            Cv2.PolarToCart(r, theta, newMapx, newMapy);

            // 중심점 기준에서 좌상단 기준으로 변경
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMapx.Set(i, j, ((newMapx.At<float>(i, j) + 1.0f) * (cols - 1) / 2.0f));
                    newMapy.Set(i, j, ((newMapy.At<float>(i, j) + 1.0f) * (rows - 1) / 2.0f));
                }
            }

            Mat distorted = new Mat();
            // 재매핑 변환
            Cv2.Remap(inputImage, distorted, newMapx, newMapy, InterpolationFlags.Linear);
            return distorted;
        }
        public Mat ApplyDistortion_Bol(Mat inputImage)
        {
            int rows = inputImage.Height;
            int cols = inputImage.Width;

            double exp = 2.0;
            double scale = 1.0;

            Mat mapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat mapy = new Mat(rows, cols, MatType.CV_32F, 1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mapx.At<float>(i, j) = 2.0f * j / (cols - 1) - 1.0f;
                    mapy.At<float>(i, j) = 2.0f * i / (rows - 1) - 1.0f;
                }
            }

            Mat r = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat theta = new Mat(rows, cols, MatType.CV_32F, 1);
            Cv2.CartToPolar(mapx, mapy, r, theta);

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

            Mat newMapx = new Mat(rows, cols, MatType.CV_32F, 1);
            Mat newMapy = new Mat(rows, cols, MatType.CV_32F, 1);
            Cv2.PolarToCart(r, theta, newMapx, newMapy);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMapx.Set(i, j, ((newMapx.At<float>(i, j) + 1.0f) * (cols - 1) / 2.0f));
                    newMapy.Set(i, j, ((newMapy.At<float>(i, j) + 1.0f) * (rows - 1) / 2.0f));
                }
            }

            Mat distorted = new Mat();
            Cv2.Remap(inputImage, distorted, newMapx, newMapy, InterpolationFlags.Linear);
            return distorted;
        }
    }
}
