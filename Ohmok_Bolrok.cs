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
                    if (isColor == false)
                    {
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                        Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGR);
                    }
                    if (boolmirror == true)
                    {
                        CameraFunction cameraFunction = new CameraFunction();
                        cameraFunction.ApplyDistortion_Bol(frame);
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
            boolmirror = true;
        }
    }
    public class CameraFunction
    {
        public Mat ApplyDistortion_Oh(Mat inputImage)
        {
            int rows = inputImage.Height;
            int cols = inputImage.Width;

            double exp = 0.5;
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
