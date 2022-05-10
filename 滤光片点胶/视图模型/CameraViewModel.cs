using Amib.Threading;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace 滤光片点胶
{
    public class CameraViewModel:ViewModelBase
    {
        /// <summary>
        /// 相机信息
        /// </summary>
        public HiKhelper HiKCamera
        {
            get => GetProperty(() => HiKCamera);
            set => SetProperty(() => HiKCamera, value);
        }
        /// <summary>
        /// /构造函数
        /// </summary>
        public CameraViewModel()
        {
            HiKCamera = new HiKhelper();
            camInfos = HiKhelper.CamInfos;
            stp = new SmartThreadPool { MaxThreads = 1 };
            HiKCamera.MV_OnOriFrameInvoked += Hik_MV_OnOriFrameInvoked;
            
            ImSrc_test = new WriteableBitmap(new BitmapImage(new Uri(@"./图片/null.png", UriKind.Relative))); ;
        }

        #region 前台绑定
        
        /// <summary>
        /// Image控件显示的资源
        /// </summary>
        public WriteableBitmap ImSrc_test
        {
            get => GetProperty(() => ImSrc_test);
            set => SetProperty(() => ImSrc_test, value);
        }

        /// <summary>
        /// 相机信息 用于连接相机
        /// </summary>
        public ObservableCollection<HiKhelper.CamInfo> camInfos
        {
            get => GetProperty(() => camInfos);
            set => SetProperty(() => camInfos, value);
        }

        /// <summary>
        /// 被选中相机
        /// </summary>
        public int SelectedCam
        {
            get => GetProperty(() => SelectedCam);
            set => SetProperty(() => SelectedCam, value,()=>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("SelectedCam").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 被选中算法
        /// </summary>
        public int SelectedAlg
        {
            get => GetProperty(() => SelectedAlg);
            set => SetProperty(() => SelectedAlg, value,()=>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("SelectedAlg").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        #endregion
        
        #region 相机操作

        public void Connect()
        {
            HiKCamera.Connect(SelectedCam);
        }

        public void Disconnect()
        {
            HiKCamera.Disconnect();
        }

        #endregion

        #region 画面显示

        /// <summary>
        /// 线程锁对象
        /// </summary>
        private static readonly object objlock = new object();

        /// <summary>
        /// 画面显示缓冲
        /// </summary>
        CVAlgorithms.BmpBuf bmpBuf = new CVAlgorithms.BmpBuf();

        /// <summary>
        /// 线程执行函数
        /// </summary>
        /// <param name="i"></param>
        /// <param name="e"></param>
        private void MV_STPAction(int i, HiKhelper.MV_IM_INFO e)
        {

            bmpBuf.Width = e.width;
            bmpBuf.Height = e.height;
            bmpBuf.pData_IntPtr = e.pData;
            //CVAlgorithms.MV_Upload(e.width, e.height, ref bmpBuf, 3);


            //foreach (var item in AlgPros.ElementAt(0).ProList)
            //{
            //    if (0 != item.Function()) break;
            //}
            //CVAlgorithms.MV_Download(ref bmpBuf);
            float[] outParam = new float[5];
            string[] inParam = new string[17];

            if (SelectedAlg < 2)
            {
                inParam[0] = ShowMode.ToString();

                inParam[1] = LocThresh.ToString();
                inParam[2] = MaxRadius.ToString();
                inParam[3] = MinRadius.ToString();

                inParam[4] = Radius.ToString();
                inParam[5] = nMaxRadius.ToString();
                inParam[6] = nMinRadius.ToString();

                inParam[7] = D1thresh.ToString();
                inParam[8] = D1SizeMax.ToString();
                inParam[9] = D1SizeMin.ToString();

                inParam[10] = D2AdapSize.ToString();
                inParam[11] = D2AdapC.ToString();
                inParam[12] = D2RoundnessMin.ToString();
                inParam[13] = D2RectangularityMin.ToString();
                inParam[14] = D2sizeMax.ToString();
                inParam[15] = D2sizeMin.ToString();
            }
            else
            {
                inParam[0] = ShowMode.ToString();
                inParam[1] = WorkMode.ToString();

                inParam[2] = X_Defult.ToString();
                inParam[3] = Y_Defult.ToString();

                inParam[4] = Scale.ToString();
                inParam[5] = OffsetMax.ToString();

                inParam[4] = Scale.ToString();
                inParam[5] = OffsetMax.ToString();

                inParam[6] = MinRadius.ToString();
                inParam[7] = MaxRadius.ToString();

                inParam[8] = X_flip.ToString();
                inParam[9] = Y_flip.ToString();
                inParam[10] = XY_flip.ToString();

                inParam[11] = R_min.ToString();
                inParam[12] = G_min.ToString();
                inParam[13] = B_min.ToString();

                inParam[14] = R_max.ToString();
                inParam[15] = G_max.ToString();
                inParam[16] = B_max.ToString();
            }

            CVAlgorithms.MV_EntryPoint(SelectedAlg, ref bmpBuf, inParam, ref outParam[0]);
            
            if (HiKCamera.isTrigger)
            {
                string str = "";
                if (outParam[0] == 1) str += "T";
                else str += "F";

                if (SelectedAlg >= 2)
                {
                    int num = (int)outParam[2];
                    str += string.Format("{0:000}", outParam[1]);
                    if (num >= 0) str += "+";
                    str += string.Format("{0:0000}", outParam[2]);
                    num = (int)outParam[3];
                    if (num >= 0) str += "+";
                    str += string.Format("{0:0000}", outParam[3]);
                }
                MV_OnSendMess?.Invoke(this, str);

                //if (outParam[0] == 1)
                //{
                //    MV_OnSendMess?.Invoke(this, "T");
                //}
                //else
                //{
                //    MV_OnSendMess?.Invoke(this, "F");
                //}
            }

            if (SelectedAlg == 2)
            {
                OffsetRotate.IR_angle = (int)outParam[1];
                OffsetRotate.actualX = outParam[2];
                OffsetRotate.actualY = outParam[3];

                OffsetRotate.defaultX = X_Defult;
                OffsetRotate.defaultY = Y_Defult;
            }

            if (SelectedAlg == 3)
            {
                OffsetRotate.HD_angle = (int)outParam[1];
            }

            //显示
            Application.Current.Dispatcher.Invoke(() =>
            {
                int size = (int)bmpBuf.size;
                if (ImSrc_test == null || ImSrc_test.Width != bmpBuf.Width || ImSrc_test.Height != bmpBuf.Height)
                {
                    if (size > 3 * bmpBuf.Width * bmpBuf.Height / 2)
                        ImSrc_test = new WriteableBitmap(bmpBuf.Width, bmpBuf.Height, 96.0, 96.0, PixelFormats.Bgr24, null);
                    else
                        ImSrc_test = new WriteableBitmap(bmpBuf.Width, bmpBuf.Height, 24.0, 24.0, PixelFormats.Gray8, null);
                }

                lock (objlock)
                {
                    if ((e.pData != (IntPtr)0x00000000))
                    {
                        ImSrc_test.Lock();
                        ImSrc_test.WritePixels(new Int32Rect(0, 0, bmpBuf.Width, bmpBuf.Height), bmpBuf.pData, size, ImSrc_test.BackBufferStride);
                        ImSrc_test.AddDirtyRect(new Int32Rect(0, 0, bmpBuf.Width, bmpBuf.Height));
                        ImSrc_test.Unlock();

                    }
                }

                CVAlgorithms.MV_Release(ref bmpBuf);
            });
            

            ////这里是固定的
            //CVAlgorithms.MV_Release(ref bmpBuf);

            
        }

        /// <summary>
        /// 回调函数，回调帧为YUV格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hik_MV_OnOriFrameInvoked(object sender, HiKhelper.MV_IM_INFO e)
        {
            stp.QueueWorkItem(new Action<int, HiKhelper.MV_IM_INFO>(MV_STPAction), 0, e, WorkItemPriority.Normal);
            //stp.QueueWorkItem(new Amib.Threading.Action<int, HiKhelper.MV_IM_INFO>(MV_STPAction), 0, e, Amib.Threading.WorkItemPriority.Normal);
        }

        /// <summary>
        /// 智能线程池，用于处理回调图像算法
        /// </summary>
        private SmartThreadPool stp;

        #endregion

        #region 外部通讯
        /// <summary>
        /// 路由事件，回调原始帧
        /// </summary>
        public event EventHandler<string> MV_OnSendMess;
        #endregion

        #region 配置读写
        public int CamID;

        string filePath = "";

        void InitErr()
        {
            //MainWindow.ErrInfo(filePath + "丢失");
            try
            {
                //获得文件路径
                string localFilePath = "";
                localFilePath = filePath;
                XDocument xdoc = new XDocument();
                XDeclaration xdec = new XDeclaration("1.0", "utf-8", "yes");
                xdoc.Declaration = xdec;

                XElement rootEle;
                XElement classEle;
                //XElement childEle;

                //添加根节点
                rootEle = new XElement("CamConfig");
                xdoc.Add(rootEle);

                classEle = new XElement("SelectedCam", 0);
                rootEle.Add(classEle);

                classEle = new XElement("SelectedAlg", 0);
                rootEle.Add(classEle);

                classEle = new XElement("ShowMode", 0);
                rootEle.Add(classEle);

                classEle = new XElement("LocThresh", 128);
                rootEle.Add(classEle);

                classEle = new XElement("MaxRadius", 100);
                rootEle.Add(classEle);

                classEle = new XElement("MinRadius", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Radius", 128);
                rootEle.Add(classEle);

                classEle = new XElement("nMaxRadius", 100);
                rootEle.Add(classEle);

                classEle = new XElement("nMinRadius", 0);
                rootEle.Add(classEle);

                classEle = new XElement("D1thresh", 128);
                rootEle.Add(classEle);

                classEle = new XElement("D1SizeMax", 100);
                rootEle.Add(classEle);

                classEle = new XElement("D1SizeMin", 0);
                rootEle.Add(classEle);

                classEle = new XElement("D2AdapSize", 2);
                rootEle.Add(classEle);

                classEle = new XElement("D2AdapC", 2);
                rootEle.Add(classEle);

                classEle = new XElement("D2RoundnessMin", 0.5);
                rootEle.Add(classEle);

                classEle = new XElement("D2RectangularityMin", 0.5);
                rootEle.Add(classEle);

                classEle = new XElement("D2sizeMax", 100);
                rootEle.Add(classEle);

                classEle = new XElement("D2sizeMin", 0);
                rootEle.Add(classEle);

                classEle = new XElement("WorkMode", 0);
                rootEle.Add(classEle);

                classEle = new XElement("X_Defult", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Y_Defult", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Scale", 0);
                rootEle.Add(classEle);

                classEle = new XElement("OffsetMax", 0);
                rootEle.Add(classEle);

                classEle = new XElement("X_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Y_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("XY_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("R_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("G_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("B_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("R_max", 0);
                rootEle.Add(classEle);

                classEle = new XElement("G_max", 0);
                rootEle.Add(classEle);

                classEle = new XElement("B_max", 0);
                rootEle.Add(classEle);

                xdoc.Save(localFilePath);
            }
            catch
            {
                Growl.Error("[" + filePath + "]生成失败！");
            }
        }

        public int Init(string path)
        {
            int ret = 0;
            filePath = path + "/CamConfig.xml";
            if (XmlHelper.Exists(path, "CamConfig.xml"))
            {
                try
                {
                    XDocument Config = XDocument.Load(path + "/CamConfig.xml");

                    SelectedCam = int.Parse(Config.Descendants("SelectedCam").ElementAt(0).Value);
                    SelectedAlg = int.Parse(Config.Descendants("SelectedAlg").ElementAt(0).Value);

                    ShowMode = int.Parse(Config.Descendants("ShowMode").ElementAt(0).Value);

                    LocThresh = int.Parse(Config.Descendants("LocThresh").ElementAt(0).Value);
                    MaxRadius = int.Parse(Config.Descendants("MaxRadius").ElementAt(0).Value);
                    MinRadius = int.Parse(Config.Descendants("MinRadius").ElementAt(0).Value);

                    Radius = int.Parse(Config.Descendants("Radius").ElementAt(0).Value);
                    nMaxRadius = int.Parse(Config.Descendants("nMaxRadius").ElementAt(0).Value);
                    nMinRadius = int.Parse(Config.Descendants("nMinRadius").ElementAt(0).Value);

                    D1thresh = int.Parse(Config.Descendants("D1thresh").ElementAt(0).Value);
                    D1SizeMax = int.Parse(Config.Descendants("D1SizeMax").ElementAt(0).Value);
                    D1SizeMin = int.Parse(Config.Descendants("D1SizeMin").ElementAt(0).Value);

                    D2AdapSize = int.Parse(Config.Descendants("D2AdapSize").ElementAt(0).Value);
                    D2AdapC = int.Parse(Config.Descendants("D2AdapC").ElementAt(0).Value);
                    D2RoundnessMin = float.Parse(Config.Descendants("D2RoundnessMin").ElementAt(0).Value);
                    D2RectangularityMin = float.Parse(Config.Descendants("D2RectangularityMin").ElementAt(0).Value);
                    D2sizeMax = int.Parse(Config.Descendants("D2sizeMax").ElementAt(0).Value);
                    D2sizeMin = int.Parse(Config.Descendants("D2sizeMin").ElementAt(0).Value);

                    WorkMode = int.Parse(Config.Descendants("WorkMode").ElementAt(0).Value);

                    X_Defult = float.Parse(Config.Descendants("X_Defult").ElementAt(0).Value);
                    Y_Defult = float.Parse(Config.Descendants("Y_Defult").ElementAt(0).Value);

                    Scale = float.Parse(Config.Descendants("Scale").ElementAt(0).Value);
                    OffsetMax = float.Parse(Config.Descendants("OffsetMax").ElementAt(0).Value);

                    X_flip = int.Parse(Config.Descendants("X_flip").ElementAt(0).Value);
                    Y_flip = int.Parse(Config.Descendants("Y_flip").ElementAt(0).Value);
                    XY_flip = int.Parse(Config.Descendants("XY_flip").ElementAt(0).Value);

                    R_min = int.Parse(Config.Descendants("R_min").ElementAt(0).Value);
                    G_min = int.Parse(Config.Descendants("G_min").ElementAt(0).Value);
                    B_min = int.Parse(Config.Descendants("B_min").ElementAt(0).Value);

                    R_max = int.Parse(Config.Descendants("R_max").ElementAt(0).Value);
                    G_max = int.Parse(Config.Descendants("G_max").ElementAt(0).Value);
                    B_max = int.Parse(Config.Descendants("B_max").ElementAt(0).Value);

                }
                catch (Exception err)
                {
                    Growl.Error("相机" + CamID + "配置信息丢失！已重新生成");
                    InitErr();
                }
            }
            else
            {
                Growl.Error("相机" + CamID + "初始化失败！");
                InitErr();
                ret++;
            }

            return ret;
        }

        #endregion

        #region 算法参数

        /// <summary>
        /// 显示模式
        /// </summary>
        public int ShowMode
        {
            get => GetProperty(() => ShowMode);
            set => SetProperty(() => ShowMode, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("ShowMode").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 工作模式
        /// </summary>
        public int WorkMode
        {
            get => GetProperty(() => WorkMode);
            set => SetProperty(() => WorkMode, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("WorkMode").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// X基准中心
        /// </summary>
        public float X_Defult
        {
            get => GetProperty(() => X_Defult);
            set => SetProperty(() => X_Defult, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("X_Defult").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// Y基准中心
        /// </summary>
        public float Y_Defult
        {
            get => GetProperty(() => Y_Defult);
            set => SetProperty(() => Y_Defult, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Y_Defult").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 比例尺
        /// </summary>
        public float Scale
        {
            get => GetProperty(() => Scale);
            set => SetProperty(() => Scale, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Scale").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 补偿上限
        /// </summary>
        public float OffsetMax
        {
            get => GetProperty(() => OffsetMax);
            set => SetProperty(() => OffsetMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("OffsetMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// X方向翻转
        /// </summary>
        public int X_flip
        {
            get => GetProperty(() => X_flip);
            set => SetProperty(() => X_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("X_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// Y方向翻转
        /// </summary>
        public int Y_flip
        {
            get => GetProperty(() => Y_flip);
            set => SetProperty(() => Y_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Y_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// XY翻转
        /// </summary>
        public int XY_flip
        {
            get => GetProperty(() => XY_flip);
            set => SetProperty(() => XY_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("XY_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// R最小值
        /// </summary>
        public int R_min
        {
            get => GetProperty(() => R_min);
            set => SetProperty(() => R_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("R_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// G最小值
        /// </summary>
        public int G_min
        {
            get => GetProperty(() => G_min);
            set => SetProperty(() => G_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("G_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// B最小值
        /// </summary>
        public int B_min
        {
            get => GetProperty(() => B_min);
            set => SetProperty(() => B_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("B_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// R最大值
        /// </summary>
        public int R_max
        {
            get => GetProperty(() => R_max);
            set => SetProperty(() => R_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("R_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// G最大值
        /// </summary>
        public int G_max
        {
            get => GetProperty(() => G_max);
            set => SetProperty(() => G_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("G_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// B最大值
        /// </summary>
        public int B_max
        {
            get => GetProperty(() => B_max);
            set => SetProperty(() => B_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("B_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }


        /// <summary>
        /// 定位-灰度阈值
        /// </summary>
        public int LocThresh
        {
            get => GetProperty(() => LocThresh);
            set => SetProperty(() => LocThresh, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("LocThresh").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 定位-半径上限
        /// </summary>
        public int MaxRadius
        {
            get => GetProperty(() => MaxRadius);
            set => SetProperty(() => MaxRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("MaxRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 定位-半径下限
        /// </summary>
        public int MinRadius
        {
            get => GetProperty(() => MinRadius);
            set => SetProperty(() => MinRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("MinRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-有效径半径
        /// </summary>
        public int Radius
        {
            get => GetProperty(() => Radius);
            set => SetProperty(() => Radius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Radius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-屏蔽径上限
        /// </summary>
        public int nMaxRadius
        {
            get => GetProperty(() => nMaxRadius);
            set => SetProperty(() => nMaxRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("nMaxRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-屏蔽径下限
        /// </summary>
        public int nMinRadius
        {
            get => GetProperty(() => nMinRadius);
            set => SetProperty(() => nMinRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("nMinRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-灰度阈值
        /// </summary>
        public int D1thresh
        {
            get => GetProperty(() => D1thresh);
            set => SetProperty(() => D1thresh, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1thresh").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-面积上限
        /// </summary>
        public int D1SizeMax
        {
            get => GetProperty(() => D1SizeMax);
            set => SetProperty(() => D1SizeMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1SizeMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-面积下限
        /// </summary>
        public int D1SizeMin
        {
            get => GetProperty(() => D1SizeMin);
            set => SetProperty(() => D1SizeMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1SizeMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-强度
        /// </summary>
        public int D2AdapSize
        {
            get => GetProperty(() => D2AdapSize);
            set => SetProperty(() => D2AdapSize, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2AdapSize").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-容差
        /// </summary>
        public int D2AdapC
        {
            get => GetProperty(() => D2AdapC);
            set => SetProperty(() => D2AdapC, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2AdapC").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-圆度下限
        /// </summary>
        public float D2RoundnessMin
        {
            get => GetProperty(() => D2RoundnessMin);
            set => SetProperty(() => D2RoundnessMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2RoundnessMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-矩形度下限
        /// </summary>
        public float D2RectangularityMin
        {
            get => GetProperty(() => D2RectangularityMin);
            set => SetProperty(() => D2RectangularityMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2RoundnessMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-面积上限
        /// </summary>
        public int D2sizeMax
        {
            get => GetProperty(() => D2sizeMax);
            set => SetProperty(() => D2sizeMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2sizeMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-面积下限
        /// </summary>
        public int D2sizeMin
        {
            get => GetProperty(() => D2sizeMin);
            set => SetProperty(() => D2sizeMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2sizeMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }


        #endregion

    }
}
