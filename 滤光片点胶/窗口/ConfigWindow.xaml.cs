using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace 滤光片点胶
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(10); // 3秒
            
            timer.Tick += TimerTick; // 注册计时器到点后触发的回调
            timer.Start();
        }

        /// <summary>
        /// 界面线程锁
        /// </summary>
        private System.Threading.Mutex myMutex = null;

        /// <summary>
        /// 窗口载入前 上锁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 禁止同时打开2个
            bool mutexIsNew = false;
            try
            {
                myMutex = new System.Threading.Mutex(true, "相机设置", out mutexIsNew);
            }
            catch { }

            if (!mutexIsNew)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 窗口关闭前 解锁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            myMutex.Close();
        }

        /// <summary>
        /// 定时关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick; // 取消注册

            this.Close();
        }
    }
}
