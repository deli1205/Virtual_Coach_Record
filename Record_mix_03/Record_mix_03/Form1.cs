using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Record_mix_03
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Kinect senser
        private KinectSensor kinectSensor = null;
        private CoordinateMapper coordinateMapper = null;//座標對應工具
        private MultiSourceFrameReader multiFrameSourceReader = null; //多來源讀取(彩影,深度,BODY....)
        private BodyFrameReader bodyFrameReader = null;

        private ColorSpacePoint[] depthMappedToColorPoints = null;
        private ColorSpacePoint[] depthMappedToCameraPoints = null;

        //-----------------------------------------
        private const double HandSize = 30;
        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private const float InferredZPositionClamp = 0.1f;

        private readonly Brush handClosedBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        private readonly Brush handOpenBrush = new SolidBrush(Color.FromArgb(128, 0, 255, 0));
        private readonly Brush handLassoBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));

        private readonly Brush trackedJointBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 255));
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        //private DrawingGroup drawingGroup;
        //private DrawingGroup drawingGroup2;

        //private DrawingImage imageSource;
        //private DrawingImage imageSource2;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        private StringBuilder sbPathColor;
        private StringBuilder sbPathDepth;
        private StringBuilder sbPath3D;
        private int FrameCnt;
        private Boolean Record;
        bool body_ready = false;
        private SaveFileDialog SaveFileDialog_Path;

        //------------------------------------------

        //錄影
        private VideoWriter VW;
        private Boolean VW_OPEN = false;
        private SaveFileDialog SaveFileDialog_Video;
        private double ttfps;
        private int fps;

        //顯示用的bmp
        private Bitmap bmp_display;
        private UInt32 size_display;
        private Rectangle Rect_display;

        //彩圖的bmp
        private Bitmap bmp_color;
        private UInt32 size_color;
        private Rectangle Rect_color;

        private double MaxV = 8000;
        private int pt = 0;

        //
        private int[] depth = null;
        private byte[] depthPixels = null;    //深度對映到0~255

        //積分影像們
        //private byte[] depthPixels = null;    //深度對映到0~255
        private int[] Integral_depth_value = null; //深度值
        private int[] Integral_depth_valid = null; //有效點

        bool dataReceived = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            //調整顯示的方式
            //pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            kinectSensor = KinectSensor.GetDefault();
            multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.Body);
            multiFrameSourceReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            //bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            coordinateMapper = kinectSensor.CoordinateMapper;
            //
            SaveFileDialog_Video = new SaveFileDialog();
            SaveFileDialog_Path = new SaveFileDialog();

            FrameDescription depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            FrameDescription colorFrameDescription = kinectSensor.ColorFrameSource.FrameDescription;
            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;

            depthMappedToColorPoints = new ColorSpacePoint[depthWidth * depthHeight];
            depthMappedToCameraPoints = new ColorSpacePoint[depthWidth * depthHeight];

            //顯示用的Bitmap
            bmp_display = new Bitmap(depthFrameDescription.Width, depthFrameDescription.Height, PixelFormat.Format32bppArgb);
            Rect_display = new Rectangle(0, 0, depthFrameDescription.Width, depthFrameDescription.Height);
            size_display = (uint)(depthFrameDescription.Width * depthFrameDescription.Height * 4);

            //暫存彩圖的Bitmap
            bmp_color = new Bitmap(colorFrameDescription.Width, colorFrameDescription.Height, PixelFormat.Format32bppArgb);
            Rect_color = new Rectangle(0, 0, colorFrameDescription.Width, colorFrameDescription.Height);
            size_color = (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4);

            //陣列群
            depth = new int[depthWidth * depthHeight];
            depthPixels = new byte[depthWidth * depthHeight];
            //積分影像
            //private byte[] depthPixels = null;    //深度對映到0~255
            Integral_depth_value = new int[depthWidth * depthHeight]; ; //深度值
            Integral_depth_valid = new int[depthWidth * depthHeight]; ; //有效點


            //-------------------------------
            // a bone defined as a line between two joints
            bones = new List<Tuple<JointType, JointType>>();

            // Torso
            bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            sbPathColor = new System.Text.StringBuilder();
            sbPathDepth = new System.Text.StringBuilder();
            sbPath3D = new System.Text.StringBuilder();
            FrameCnt = 1;
            Record = false;

            //----------------------------------

            kinectSensor.Open();
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            if (multiSourceFrame == null)
            {
                return;
            }

            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();

                if ((depthFrame == null) || (colorFrame == null))
                {
                    return;
                }

                ttfps = 1.0 / colorFrame.ColorCameraSettings.FrameInterval.TotalSeconds;
                if (ttfps < 30) { fps = 15; } else { fps = 30; }
                //label2.Text = ttfps.ToString();


                //映射
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    //映射到一个与深度图大小相同的depthMappedToColorPoints处
                    this.coordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        this.depthMappedToColorPoints);
                    // Process Depth
                    ProcessDepthData(depthFrameData.UnderlyingBuffer, depthFrameData.Size, depthFrame.DepthMinReliableDistance, depthFrame.DepthMaxReliableDistance);
                }

                //先將彩圖寫入bmp_color
                BitmapData cBmp = bmp_color.LockBits(Rect_color, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                colorFrame.CopyConvertedFrameDataToIntPtr(cBmp.Scan0, size_color, ColorImageFormat.Bgra);
                bmp_color.UnlockBits(cBmp);

                ProcessData(depthMappedToColorPoints, bmp_color);

                depthFrame.Dispose();
                colorFrame.Dispose();
            }
            finally
            {
                if (depthFrame != null) depthFrame.Dispose();
                if (colorFrame != null) colorFrame.Dispose();
            }

            using (BodyFrame bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }
                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                foreach (Body body in bodies)
                {
                    if (body.IsTracked)
                    {
                        body_ready = true;
                        if (Record == true)  ////////
                        {
                            sbPath3D.AppendLine("F");
                            sbPath3D.AppendLine(FrameCnt.ToString());
                            sbPathDepth.AppendLine("F");
                            sbPathDepth.AppendLine(FrameCnt.ToString());
                            sbPathColor.AppendLine("F");
                            sbPathColor.AppendLine(FrameCnt.ToString());
                            FrameCnt++;
                        }

                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        // convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            // sometimes the depth(Z) of an inferred joint may show as negative
                            // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = InferredZPositionClamp;
                            }

                            ColorSpacePoint ColorSpacePoint = coordinateMapper.MapCameraPointToColorSpace(position);
                            DepthSpacePoint depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(position);
                            //jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            jointPoints[jointType] = new Point((int)(ColorSpacePoint.X+0.5), (int)(ColorSpacePoint.Y+0.5));
                            if (Record == true)
                            {
                                label1.Text = jointType.ToString() + "X" + position.X + "Y" + position.Y + "Z" + position.Z;
                                sbPath3D.AppendLine(position.X.ToString());
                                sbPath3D.AppendLine(position.Y.ToString());
                                sbPath3D.AppendLine(position.Z.ToString());

                                sbPathColor.AppendLine(ColorSpacePoint.X.ToString());
                                sbPathColor.AppendLine(ColorSpacePoint.Y.ToString());
                                sbPathColor.AppendLine(position.Z.ToString());

                                sbPathDepth.AppendLine(depthSpacePoint.X.ToString());
                                sbPathDepth.AppendLine(depthSpacePoint.Y.ToString());
                                sbPathDepth.AppendLine(position.Z.ToString());
                            }
                        }
                    }
                }              
            }


        }

        private unsafe void ProcessDepthData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            MaxV = 8000;
            double tempV = 0;
            int sumP = 0;
            const int MapDepthToByte = 8000 / 256;
            int invalid_mark = 99999;

            // depth frame data is a 16 bit value\
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.kinectSensor.DepthFrameSource.FrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                //this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 255);
                if (depth >= minDepth && depth <= maxDepth)
                {
                    this.depthPixels[i] = (byte)(depth / MapDepthToByte);
                    this.depth[i] = depth;
                }
                else //無效區
                {
                    this.depthPixels[i] = (byte)(0);
                    this.depth[i] = invalid_mark;
                    depth = (ushort)invalid_mark;
                }


                //--------------------積分矩陣--------------------------------
                if (i < 512) //首行
                {
                    if (i == 0)
                    {
                        if (depth == (ushort)invalid_mark)
                        {
                            Integral_depth_valid[i] = 0;
                            Integral_depth_value[i] = 0;
                        }
                        else
                        {
                            Integral_depth_valid[i] = 1;
                            Integral_depth_value[i] = (depth);
                        }
                    }
                    else
                    {
                        if (depth == 8000)
                        {
                            Integral_depth_valid[i] = Integral_depth_valid[i - 1] + 0;
                            Integral_depth_value[i] = 0 + Integral_depth_value[i - 1];
                        }
                        else
                        {
                            Integral_depth_valid[i] = Integral_depth_valid[i - 1] + 1;
                            Integral_depth_value[i] = depth + Integral_depth_value[i - 1];
                        }
                    }
                }
                else if (i % 512 == 0) //首列
                {
                    if (depth == 8000)
                    {
                        Integral_depth_valid[i] = Integral_depth_valid[i - 512] + 0;
                        Integral_depth_value[i] = 0 + Integral_depth_value[i - 512];
                    }
                    else
                    {
                        Integral_depth_valid[i] = Integral_depth_valid[i - 512] + 1;
                        Integral_depth_value[i] = depth + Integral_depth_value[i - 512];
                    }
                }
                else
                {
                    if (depth == 8000)
                    {
                        Integral_depth_valid[i] = 0 + Integral_depth_valid[i - 512] + Integral_depth_valid[i - 1] - Integral_depth_valid[i - 513];
                        Integral_depth_value[i] = 0 + Integral_depth_value[i - 1] + Integral_depth_value[i - 512] - Integral_depth_value[i - 513];
                    }
                    else
                    {
                        Integral_depth_valid[i] = 1 + Integral_depth_valid[i - 512] + Integral_depth_valid[i - 1] - Integral_depth_valid[i - 513];
                        Integral_depth_value[i] = depth + Integral_depth_value[i - 1] + Integral_depth_value[i - 512] - Integral_depth_value[i - 513];
                    }
                }
                //---------------------------------------------------------


                //---------------求最小區塊
                if (i / 512 >= 40)
                {
                    if (i % 512 >= 40)
                    {
                        sumP = (Integral_depth_valid[i] + Integral_depth_valid[i - 512 * 40 - 40]) - Integral_depth_valid[i - 512 * 40] - Integral_depth_valid[i - 40];
                        if (sumP > 0)
                        {
                            tempV = ((Integral_depth_value[i] + Integral_depth_value[i - 512 * 40 - 40]) - Integral_depth_value[i - 512 * 40] - Integral_depth_value[i - 40]) / sumP;
                            if (tempV < MaxV)
                            {
                                MaxV = tempV;
                                pt = i - 512 * 20 - 20;
                            }
                        }
                    }
                }

            }

        }

        private void ProcessData(ColorSpacePoint[] depthMappedToColorPoints, Bitmap bitmapItem)
        {
            Image<Bgr, byte> img;
            int BytePerPixel = 4;
            int stride = 1920 * BytePerPixel;

            //鎖定記憶體位置與指標指向初始位置
            BitmapData Bmp_Data_color = bitmapItem.LockBits(Rect_color, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr Ptr_color = Bmp_Data_color.Scan0;

            //鎖定記憶體位置與指標指向初始位置
            BitmapData Bmp_Data_display = bmp_display.LockBits(Rect_display, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr Ptr_display = Bmp_Data_display.Scan0;

            unsafe
            {
                //轉換指標的格式(C#只能用於unsafe中)
                byte* P_color = (byte*)(void*)Ptr_color;
                byte* P_display = (byte*)(void*)Ptr_display;

                for (int y = 0; y < 424; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        if (!float.IsNegativeInfinity(depthMappedToColorPoints[y * 512 + x].X) &&
                            !float.IsNegativeInfinity(depthMappedToColorPoints[y * 512 + x].Y) &&
                            (depthMappedToColorPoints[y * 512 + x].X >= 0) &&
                            (depthMappedToColorPoints[y * 512 + x].Y >= 0) &&
                            (depthMappedToColorPoints[y * 512 + x].Y <= 1080) &&
                            (depthMappedToColorPoints[y * 512 + x].X <= 1920)
                            )
                        {
                            //獲得对应色彩点的内存位置（byte信息）
                            //+0.5 四捨五入
                            int yOfMemory = (int)(depthMappedToColorPoints[y * 512 + x].Y + 0.5);
                            if (yOfMemory + 1 > 1080) yOfMemory--;//除去大于等于1080的值
                                                                  //int adress = (int)(depthMappedToColorPoints[y * 512 + x].X) + (int)(depthMappedToColorPoints[y * 512 + x].Y ) * 1920;

                            int adress = (int)(depthMappedToColorPoints[y * 512 + x].X + 0.5) + yOfMemory * 1920;
                            adress = adress * BytePerPixel;

                            //顯示(bmp_display)的記憶體位置的轉換
                            int index = y * 512 + x;
                            index = index * BytePerPixel;

                            int i = y * 512 + x;

                            if (Math.Abs(depth[i] - (int)MaxV) < 500)
                            {
                                //將彩圖位置的色彩
                                //資訊寫入顯示(bmp_display)的記憶體
                                P_display[index] = P_color[adress];
                                P_display[index + 1] = P_color[adress + 1];
                                P_display[index + 2] = P_color[adress + 2];
                                P_display[index + 3] = P_color[adress + 3];
                            }
                            else
                            {
                                P_display[index] = 0xff;
                                P_display[index + 1] = 0xff;
                                P_display[index + 2] = 0xff;
                                P_display[index + 3] = 0xff;
                            }
                        }
                        else
                        {
                            //获得对应深度点的内存地址
                            int index = y * 512 + x;
                            index *= BytePerPixel;
                            //无效映射值状态显示为红色
                            P_display[index] = 0xff;
                            P_display[index + 1] = 0xff;
                            P_display[index + 2] = 0xff;
                            P_display[index + 3] = 0xff;
                        }
                    }
                }
            }
            //更新圖檔
            bitmapItem.UnlockBits(Bmp_Data_color);
            bmp_display.UnlockBits(Bmp_Data_display);
            pictureBox1.Image = bmp_display;
            //pictureBox2.Image = bitmapItem;
            if (VW_OPEN == true)
            {
                img = new Image<Bgr, Byte>(bmp_display);
                VW.WriteFrame(img);
                img.Dispose();
            }
            bitmapItem = null;
            //bmp_display.Dispose();
            //bmp_color.Dispose();
        }

        private void BT_Video_Path_Click(object sender, EventArgs e)
        {
            SaveFileDialog_Video.FileName = DateTime.Now.ToString("yyyyMMddhhmmss");
            SaveFileDialog_Video.Filter = "Image Files(*.avi)|*.avi|All files (*.*)|*.*";

            if (SaveFileDialog_Video.ShowDialog() == DialogResult.OK)
            {
                //MessageBox.Show("開始錄製，按ESC結束錄製");
                VW = new VideoWriter(SaveFileDialog_Video.FileName, CvInvoke.CV_FOURCC('X', 'V', 'I', 'D'), 30, 512, 424, true);
                label1.Text = SaveFileDialog_Video.FileName;
            }
        }

        private void BT_path_file_path_Click(object sender, EventArgs e)
        {
            SaveFileDialog_Path.FileName = DateTime.Now.ToString("yyyyMMddhhmmss");
            SaveFileDialog_Path.Filter = "All files (*.*)|*.*";
            if (SaveFileDialog_Path.ShowDialog() == DialogResult.OK)
            {
                label2.Text = SaveFileDialog_Path.FileName;
            }
        }

        private void BT_record_start_Click(object sender, EventArgs e)
        {
            if (VW_OPEN == false)
            {
                if(body_ready == true)
                {
                    VW_OPEN = true;
                    BT_record_start.Text = "停止錄製";
                }

            }
            else
            {
                VW_OPEN = false;
                BT_record_start.Text = "開始錄製";
            }


            if (Record == false)
            {
                if (body_ready==true)
                {
                    Record = true;
                    BT_record_start.Text = "ing";
                }
                else
                {
                    MessageBox.Show("NO BODY");
                }
            }
            else
            {
                Record = false;
                StreamWriter outputV = new StreamWriter(SaveFileDialog_Path.FileName+"T3D.txt", true);
                {
                    outputV.WriteLine(sbPath3D.ToString());
                }
                StreamWriter outputDepth = new StreamWriter(SaveFileDialog_Path.FileName+"Depth.txt", true);
                {
                    outputDepth.WriteLine(sbPathDepth.ToString());
                }
                StreamWriter outputColor = new StreamWriter(SaveFileDialog_Path.FileName+"Color.txt", true);
                {
                    outputColor.WriteLine(sbPathColor.ToString());
                }
            }
        }
    }
}
