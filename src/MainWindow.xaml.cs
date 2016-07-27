using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ZClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SetNextLockTime(true);
            timer_lockScreen.Interval = TimeSpan.FromSeconds(5);
            timer_lockScreen.Tick += TimerLockScreenTick;
            timer_lockScreen.Start();

            // 托盘
            notifyIcon = new NotifyIcon {Visible = true, Icon = Properties.Resources.tubiao};
            notifyIcon.DoubleClick += delegate { RestoreMainWindow(); };
        }

        #region 整点报时

        private readonly DispatcherTimer timer_lockScreen = new DispatcherTimer();
        private DateTime m_nextLockTime;
        private readonly Random rnd = new Random();

        /// <summary>
        /// 跳过交易时段，然后随机一段时间间隔休息一次
        /// 初始化时时间间隔需要短一点
        /// </summary>
        private void SetNextLockTime(bool isInit)
        {
            int span = isInit ? 20 : 50;
            m_nextLockTime = DateTime.Now;
            do
            {
                m_nextLockTime = m_nextLockTime.AddMinutes(span + rnd.Next(10));
            } while (IsTradingHour(m_nextLockTime));
            tbMsg.Text = "下一次休息时间：" + m_nextLockTime.ToString("HH:mm:ss");
        }

        private static bool IsTradingHour(DateTime t)
        {
            int m = t.Hour*100 + t.Minute;

            // 开盘前需要一段时间做准备工作，夜盘不用全管
            // 8:30 - 11:30
            // 12:30 - 15:00
            // 20:30 - 22:30
            return (830 <= m && m <= 1130) || (1230 <= m && m <= 1500) || (2030 <= m && m <= 2230);
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();

        /// <summary>
        /// 三种情况：
        /// 1、未到时间点
        /// 2、已到时间点，且处于一分钟内，属于锁屏状态
        /// 3、已超过时间点一分钟，可以解锁，配置下一个时间点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerLockScreenTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (now < m_nextLockTime)
                return;
            else if (m_nextLockTime <= now && now <= m_nextLockTime.AddMinutes(1))
            {
                LockWorkStation();
                //Trace.WriteLine($"{now} Lock now!!!");
            }
            else
                SetNextLockTime(false);
        }

        #endregion

        private readonly NotifyIcon notifyIcon;

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            // 躲进托盘里
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        private void RestoreMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private bool user_exit;

        private void btnClose_OnClick(object sender, RoutedEventArgs e)
        {
            user_exit = true;
            Close();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!user_exit)
            {
                e.Cancel = true;
                Hide();
            }
            else
                notifyIcon.Dispose();
        }
    }
}