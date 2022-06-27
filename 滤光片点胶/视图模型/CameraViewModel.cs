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
        #region 构造函数

        /// <summary>
        /// /构造函数
        /// </summary>
        public CameraViewModel()
        {
            HiKCamera = new HiKhelper();
            camInfos = HiKhelper.CamInfos;
            stp = new SmartThreadPool { MaxThreads = 1 };
            HiKCamera.MV_OnOriFrameInvoked += Hik_MV_OnOriFrameInvoked;
            ParamsVMs = new ObservableCollection<ParamsModelView>
            {
                new ParamsModelView(0),
                new ParamsModelView(1)
            };
            ImSrc_test = new WriteableBitmap(new BitmapImage(new Uri(@"./图片/null.png", UriKind.Relative)));
        }

        #endregion
        
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
        ///当前算法
        /// </summary>
        public int AlgNum
        {
            get => GetProperty(() => AlgNum);
            set => SetProperty(() => AlgNum, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("AlgNum").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 当前算法 前台单向绑定
        /// </summary>
        public int _AlgNum
        {
            get => GetProperty(() => _AlgNum);
            set => SetProperty(() => _AlgNum, value, () =>
            {
                AlgNum = value;
            });
        }

        /// <summary>
        /// 算法参数
        /// </summary>
        public ObservableCollection<ParamsModelView>  ParamsVMs
        {
            get => GetProperty(() => ParamsVMs);
            set => SetProperty(() => ParamsVMs, value);
        }

        /// <summary>
        /// 网口通讯实例
        /// </summary>
        public MySocket mySocket
        {
            get => GetProperty(() => mySocket);
            set => SetProperty(() => mySocket, value);
        }
        #endregion

        #region 相机操作

        /// <summary>
        /// 相机信息
        /// </summary>
        public HiKhelper HiKCamera
        {
            get => GetProperty(() => HiKCamera);
            set => SetProperty(() => HiKCamera, value);
        }

        public void Connect()
        {
            ///这里需要在优化
            if (HiKCamera.Connect(SelectedCam) == 0)
            {
                if (!MainViewModel.isOneServer)
                {
                    mySocket.Listen();
                }
            }

            
        }

        public void Disconnect()
        {
            HiKCamera.Disconnect();
            if (!MainViewModel.isOneServer && mySocket != null)
            {
                mySocket.StopListen();
            }
            
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
            try
            {
                
                bmpBuf.Width = e.width;
                bmpBuf.Height = e.height;
                bmpBuf.pData_IntPtr = e.pData;
                bmpBuf.size = e.nFrameLen;

                float[] outParam = new float[5];
                string[] inParam = new string[18];

                if (ParamsVMs[AlgNum].SelectedAlg < 2)
                {
                    inParam[0] = ParamsVMs[AlgNum].ShowMode.ToString();//showmode

                    inParam[1] = ParamsVMs[AlgNum].LocThresh.ToString();//LocThresh
                    inParam[2] = ParamsVMs[AlgNum].MaxRadius.ToString();//MaxRadius
                    inParam[3] = ParamsVMs[AlgNum].MinRadius.ToString();//MinRadius

                    //inParam[4] = ParamsVMs[AlgNum].Radius.ToString();
                    //inParam[5] = ParamsVMs[AlgNum].nMaxRadius.ToString();//LensRadius
                    //inParam[6] = ParamsVMs[AlgNum].nMinRadius.ToString();//HoleRadius

                    //inParam[7] = ParamsVMs[AlgNum].D1thresh.ToString();
                    //inParam[8] = ParamsVMs[AlgNum].D1SizeMax.ToString();
                    //inParam[9] = ParamsVMs[AlgNum].D1SizeMin.ToString();

                    //inParam[10] = ParamsVMs[AlgNum].D2AdapSize.ToString();
                    //inParam[11] = ParamsVMs[AlgNum].D2AdapC.ToString();
                    //inParam[12] = ParamsVMs[AlgNum].D2RoundnessMin.ToString();
                    //inParam[13] = ParamsVMs[AlgNum].D2RectangularityMin.ToString();
                    //inParam[14] = ParamsVMs[AlgNum].D2sizeMax.ToString();
                    //inParam[15] = ParamsVMs[AlgNum].D2sizeMin.ToString();

                    inParam[4] = ParamsVMs[AlgNum].LensRadius.ToString();
                    inParam[5] = ParamsVMs[AlgNum].HoleRadius.ToString();

                    inParam[6] = ParamsVMs[AlgNum].AutoCover.ToString();
                    inParam[7] = ParamsVMs[AlgNum].CoverRadiusMax.ToString();
                    inParam[8] = ParamsVMs[AlgNum].CoverRadiusMin.ToString();

                    inParam[9] = ParamsVMs[AlgNum].HighCut.ToString();
                    inParam[10] = ParamsVMs[AlgNum].LowCut.ToString();

                    inParam[11] = ParamsVMs[AlgNum].sSize.ToString();
                    inParam[12] = ParamsVMs[AlgNum].mSize.ToString();
                    inParam[13] = ParamsVMs[AlgNum].lSize.ToString();
                }
                else
                {
                    inParam[0] = ParamsVMs[AlgNum].ShowMode.ToString();
                    inParam[1] = ParamsVMs[AlgNum].WorkMode.ToString();

                    inParam[2] = ParamsVMs[AlgNum].X_Defult.ToString();
                    inParam[3] = ParamsVMs[AlgNum].Y_Defult.ToString();

                    inParam[4] = ParamsVMs[AlgNum].Scale.ToString();
                    inParam[5] = ParamsVMs[AlgNum].OffsetMax.ToString();

                    inParam[4] = ParamsVMs[AlgNum].Scale.ToString();
                    inParam[5] = ParamsVMs[AlgNum].OffsetMax.ToString();

                    inParam[6] = ParamsVMs[AlgNum].MinRadius.ToString();
                    inParam[7] = ParamsVMs[AlgNum].MaxRadius.ToString();

                    inParam[8] = ParamsVMs[AlgNum].X_flip.ToString();
                    inParam[9] = ParamsVMs[AlgNum].Y_flip.ToString();
                    inParam[10] = ParamsVMs[AlgNum].XY_flip.ToString();

                    inParam[11] = ParamsVMs[AlgNum].R_min.ToString();
                    inParam[12] = ParamsVMs[AlgNum].G_min.ToString();
                    inParam[13] = ParamsVMs[AlgNum].B_min.ToString();

                    inParam[14] = ParamsVMs[AlgNum].R_max.ToString();
                    inParam[15] = ParamsVMs[AlgNum].G_max.ToString();
                    inParam[16] = ParamsVMs[AlgNum].B_max.ToString();

                    inParam[17] = ParamsVMs[AlgNum].nMaxRadius.ToString();
                }

                CVAlgorithms.MV_EntryPoint(ParamsVMs[AlgNum].SelectedAlg, ref bmpBuf, inParam, ref outParam[0]);

                if (ParamsVMs[AlgNum].SelectedAlg == 2)
                {
                    OffsetRotate.IR_angle = (int)outParam[1];
                    OffsetRotate.actualX = outParam[2];
                    OffsetRotate.actualY = outParam[3];

                    OffsetRotate.defaultX = ParamsVMs[AlgNum].X_Defult;
                    OffsetRotate.defaultY = ParamsVMs[AlgNum].Y_Defult;
                }

                if (ParamsVMs[AlgNum].SelectedAlg == 3)
                {
                    OffsetRotate.HD_angle = (int)outParam[1];
                }

                if (HiKCamera.isTrigger)
                {
                    string str = "";
                    if (outParam[0] == 1) str += "T";
                    else str += "F";

                    if (ParamsVMs[AlgNum].SelectedAlg >= 2)
                    {
                        int num;
                        str += string.Format("{0:000}", outParam[1]);

                        num = (int)outParam[2];
                        if (num >= 0) str += "+";
                        str += string.Format("{0:0000}", num);

                        num = (int)outParam[3];
                        if (num >= 0) str += "+";
                        str += string.Format("{0:0000}", num);
                    }

                    if (MainViewModel.isOneServer)
                    {
                        MV_OnSendMess?.Invoke(this, str);
                    }
                    else
                    {
                        mySocket.SocketSend(Encoding.Default.GetBytes(str));
                    }
                    

                    //if (outParam[0] == 1)
                    //{
                    //    MV_OnSendMess?.Invoke(this, "T");
                    //}
                    //else
                    //{
                    //    MV_OnSendMess?.Invoke(this, "F");
                    //}
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
            }
            catch (Exception)
            {

                throw;
            }
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
        public SmartThreadPool stp;

        #endregion

        #region 外部通讯
        /// <summary>
        /// 路由事件，回调原始帧
        /// </summary>
        public event EventHandler<string> MV_OnSendMess;
        
        /// <summary>
        /// 信息分发
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Serial_OnTrigger(object sender, string e)
        {
            //单服务器不允许运行以下代码
            if (MainViewModel.isOneServer) return;

            string str = new string(e.ToList().Where(c => c != '\0').ToArray());
            if (str.Length != 2)
            {
                mySocket.WriteLine("接收无效信息：" + str, "Red");

                return;
            }

            //选择相机
            switch (str[1])
            {
                case '1': AlgNum = 0; break;
                case '2': AlgNum = 1; break;
                default:
                    AlgNum = 0;
                    mySocket.WriteLine("接收错误信息：" + str, "Red");
                    return;
            }

            if (str[0] == 'M') HiKCamera.TriggerOnce();
            else
            {
                mySocket.WriteLine("接收错误信息：" + str, "Red");
                return;
            }

            mySocket.WriteLine("接收信息：" + str);
        }
        #endregion

        #region 参数读取

        public int CamID;

        string filePath = "";

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
                    AlgNum = int.Parse(Config.Descendants("AlgNum").ElementAt(0).Value);
                }
                catch (Exception err)
                {
                    Growl.Error("相机" + CamID + "配置信息丢失！已重新生成");
                    InitErr();
                }

                foreach (var item in ParamsVMs)
                {
                    item.Init(path + "/AlgConfig");
                }

                if (!MainViewModel.isOneServer)
                {
                    mySocket = new MySocket(CamID);
                    mySocket.MV_onMess += Serial_OnTrigger;
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

        private void InitErr()
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

                classEle = new XElement("AlgNum", 0);
                rootEle.Add(classEle);

                xdoc.Save(localFilePath);
            }
            catch
            {
                Growl.Error("[" + filePath + "]生成失败！");
            }
        }
        #endregion
    }
}
