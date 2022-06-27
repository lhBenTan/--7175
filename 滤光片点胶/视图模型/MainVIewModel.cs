using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace 滤光片点胶
{
    public class MainViewModel:ViewModelBase
    {
        /// <summary>
        /// 通讯段口选择
        /// true表示网口
        /// false表示串口
        /// </summary>
        public static bool lanORser = true;

        /// <summary>
        /// 是否单一网络服务器,false表示每个相机分配一个服务器
        /// </summary>
        public static bool isOneServer = false;

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel()
        {
            HiKhelper.Search();

            SeletedCam = 0;
            camParamViewModel = MultiView.DictPanel[0].CamVM;
            IsCamOn = new ObservableCollection<bool>();

            int i = 0;

            if (!lanORser)
            {
                MySerial.MV_Mess += Serial_OnTrigger;
            }
            else if (isOneServer)
            {
                mySocket.MV_onMess += Serial_OnTrigger;
                MV_OnSendMess += SY_MV_OnSendMsg;
            }
            
            foreach (var item in MultiView.DictPanel.Values)
            {
                IsCamOn.Add(false);
                item.Init("./Para/Cam"+i.ToString());
                item.CamVM.MV_OnSendMess += SY_MV_OnSendMsg;
                i++;
            }
            IsCamOn.Add(false);

            CommboxID = new int[10];
            for (i = 0; i < 10; i++)
            {
                CommboxID[i] = i;
            }

            Version = Application.ResourceAssembly.GetName().Version.ToString();
        }

        #endregion

        #region 绑定参数

        /// <summary>
        /// 前台绑定参数
        /// </summary>
        public CameraViewModel camParamViewModel
        {
            get => GetProperty(() => camParamViewModel);
            set => SetProperty(() => camParamViewModel, value);
        }

        /// <summary>
        /// 当前被选中相机
        /// </summary>
        public int SeletedCam
        {
            get => GetProperty(() => SeletedCam);
            set => SetProperty(() => SeletedCam, value, () =>
            {
                if (value >= 0)
                {
                    camParamViewModel = MultiView.DictPanel[SeletedCam].CamVM;
                }
            });
        }

        /// <summary>
        /// 通讯连接状态
        /// </summary>
        public ObservableCollection<bool> IsCamOn
        {
            get => GetProperty(() => IsCamOn);
            set => SetProperty(() => IsCamOn, value);
        }

        /// <summary>
        /// 触发模式
        /// </summary>
        public bool IsTrigger
        {
            get => GetProperty(() => IsTrigger);
            set => SetProperty(() => IsTrigger, value);
        }

        /// <summary>
        /// Commbox选项
        /// </summary>
        public int[] CommboxID
        {
            get => GetProperty(() => CommboxID);
            set => SetProperty(() => CommboxID, value);
        }

        /// <summary>
        /// 软件版本号
        /// </summary>
        public string Version
        {
            get => GetProperty(() => Version);
            set => SetProperty(() => Version, value);
        }
        
        #endregion

        #region Commands

        /// <summary>
        /// 模式切换
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void ModeSwitchCommand(object obj)
        {
            camParamViewModel.HiKCamera.TriggerMode();
            IsCamOn[SeletedCam] = camParamViewModel.HiKCamera.isTrigger;

            // 切换触发模式时需要释放缓存
            if (camParamViewModel.HiKCamera.isTrigger)
            {
                camParamViewModel.stp.Cancel();
            }
        }

        /// <summary>
        /// 开始测试
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void StartCommand(object obj)
        {
            Growl.Clear();

            for (int i = 0; i < MultiView.DictPanel.Count; i++)
            {
                MultiView.DictPanel[i].CamVM.Connect();
                if (MultiView.DictPanel[i].CamVM.HiKCamera.isConnect)
                {
                    IsCamOn[i] = true;
                    //MultiView.DictPanel[i].camera.camera.TriggerMode();
                }
            }

            if (isOneServer)
            {
                if (lanORser)
                    IsCamOn[MultiView.DictPanel.Count] = mySocket.Listen();
                else
                    IsCamOn[MultiView.DictPanel.Count] = MySerial.OpenPort();
            }
            else
            {
                IsCamOn[MultiView.DictPanel.Count] = true;
            }
        }

        /// <summary>
        /// 停止测试
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void StopCommand(object obj)
        {
            Growl.Clear();

            for (int i = 0; i < MultiView.DictPanel.Count; i++)
            {
                MultiView.DictPanel[i].CamVM.Disconnect();
                if (!MultiView.DictPanel[i].CamVM.HiKCamera.isConnect)
                {
                    IsCamOn[i] = false;
                }
                MultiView.DictPanel[i].CamVM.stp.Cancel();
            }

            IsCamOn[MultiView.DictPanel.Count] = false;

            if (isOneServer)
            {
                if (lanORser)
                    mySocket.StopListen();
                else
                    MySerial.ClosePort();
            }
            
        }

        /// <summary>
        /// 参数设置
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void ConfigCommand(object obj)
        {
            Growl.Clear();
            LoginWindow loginWindow = new LoginWindow();
            if (true == loginWindow.ShowDialog())
            {
                ConfigWindow configWindow = new ConfigWindow();
                configWindow.DataContext = this;
                configWindow.Show();
            }
        }

        /// <summary>
        /// 通讯设置
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void CommunicationCommand(object obj)
        {
            Growl.Clear();
            LoginWindow loginWindow = new LoginWindow();
            if (true == loginWindow.ShowDialog())
            {   
                ComWindow comConfig = new ComWindow();

                if (isOneServer)
                {
                    if (lanORser)
                        comConfig.DataContext = mySocket;
                    else
                        comConfig.DataContext = MySerial;
                }
                else
                {
                    comConfig.DataContext = this;
                }
                comConfig.Show();
            }
        } 
        
        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void UploadCommand(object obj)
        {
            Growl.Clear();
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "请选择文件夹:";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //tbPath.Text = fbd.SelectedPath;
                try
                {
                    FileUtility.CopyDirectory(@"./Para", fbd.SelectedPath,true);
                    Growl.Success("配置导出成功");
                }
                catch (Exception)
                {
                    Growl.Error("配置导出失败");   
                }
                
            }
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void DownloadCommand(object obj)
        {
            Growl.Clear();

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "请选择文件夹:";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //tbPath.Text = fbd.SelectedPath;
                try
                {
                    FileUtility.CopyDirectory(fbd.SelectedPath, @"./Para",true);
                    Growl.Success("配置导入成功");

                    HandyControl.Controls.MessageBox.Success("配置导入成功,即将重启软件","提示");
                    Application.Current.Shutdown(0);
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);

                }
                catch (Exception)
                {
                    Growl.Error("配置导入失败");
                }

               
            }
        }

        /// <summary>
        /// 退出软件
        /// </summary>
        /// <param name="obj"></param>
        [AsyncCommand]
        public void ExitCommand(object obj)
        {
            for (int i = 0; i < MultiView.DictPanel.Count; i++)
            {
                MultiView.DictPanel[i].CamVM.Disconnect();
                if (!MultiView.DictPanel[i].CamVM.HiKCamera.isConnect)
                {
                    IsCamOn[i] = false;
                }
            }
            IsCamOn[MultiView.DictPanel.Count] = false;

            if (lanORser)
                mySocket.StopListen();
            else
                MySerial.ClosePort();

            Application.Current.Shutdown(-1);
        }

        #endregion

        #region 通讯相关

        /// <summary>
        /// 路由事件，回调原始帧
        /// </summary>
        public event EventHandler<string> MV_OnSendMess;
        //delegate void bbb(object sender, string str);

        /// <summary>
        /// 串口通讯实例
        /// </summary>
        MySerialPort MySerial = new MySerialPort();

        /// <summary>
        /// 网口通讯实例
        /// </summary>
        MySocket mySocket = new MySocket();

        /// <summary>
        /// 信息分发
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Serial_OnTrigger(object sender, string e)
        {
            //只有单服务器时调用此程序
            if (isOneServer)
            {
                string str = new string(e.ToList().Where(c => c != '\0').ToArray());
                if (str.Length != 2)
                {
                    if (lanORser)
                        mySocket.WriteLine("接收无效信息：" + str, "Red");
                    else
                        MySerial.WriteLine("接收无效信息：" + str, "Red");

                    return;
                }

                //switch (str[0])
                //{
                //    case 'A':
                //        nRet = MultiView.DictPanel[0].CamVM.HiKCamera.TriggerOnce(); break;
                //    case 'B':
                //        nRet = MultiView.DictPanel[1].CamVM.HiKCamera.TriggerOnce(); break;
                //    case 'C':
                //        nRet = MultiView.DictPanel[2].CamVM.HiKCamera.TriggerOnce(); break;
                //    case 'D':
                //        {
                //            float[] data = OffsetRotate.Rotate();
                //            string _str = "";
                //            _str += "T";
                //            _str += string.Format("{0:000}", data[0]);
                //            int num = (int)data[1];
                //            if (num >= 0) _str += "+";
                //            _str += string.Format("{0:0000}", num);
                //            num = (int)data[2];
                //            if (num >= 0) _str += "+";
                //            _str += string.Format("{0:0000}", num);
                //            MV_OnSendMess?.Invoke(this, _str);
                //        }
                //        break;
                //    default:
                //        break;
                //}

                CellPanel cell = null;

                switch (str[0])
                {
                    case 'A': cell = MultiView.DictPanel[0]; break;
                    case 'B': cell = MultiView.DictPanel[1]; break;
                    case 'C': cell = MultiView.DictPanel[2]; break;
                    case 'D': cell = MultiView.DictPanel[3]; break;
                    default:
                        break;
                }

                switch (str[1])
                {
                    case '1': cell.CamVM.AlgNum = 0; break;
                    case '2': cell.CamVM.AlgNum = 1; break;
                    default:
                        break;
                }

                if (cell != null) cell.CamVM.HiKCamera.TriggerOnce();



                if (lanORser)
                    mySocket.WriteLine("接收信息：" + str);
                else
                    MySerial.WriteLine("接收信息：" + str);
            }
            
        }

        /// <summary>
        /// 结果回复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void SY_MV_OnSendMsg(object sender, string msg)
        {
            if (isOneServer)
            {
                if (lanORser)
                    mySocket.SocketSend(Encoding.Default.GetBytes(msg));
                else
                    MySerial.SendCommand(msg);
            }
        }

        #endregion
    }
}
