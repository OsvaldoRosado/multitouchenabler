/*
 * SynapticsMultiTouchEnabler
 * (c) 2012 Osvaldo Rosado
 * See: LICENSE.txt
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SYNCTRLLib;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;

namespace SynapticsMultiTouchEnabler
{
    public partial class Main : Form
    {
        SynAPICtrl SynCtrl;
        SynDeviceCtrl SynDevCtrl;
        long ZTouchThreshold;
        int deviceHandle;
        int gestureMode = 0;
        int scrollSpeed = Properties.Settings.Default.ScrollSpeed;
        int scrollInversion = Properties.Settings.Default.ScrollInversion;
        bool closeApp = false;
        bool isExclusivityAcquired = false;
        bool isMultitouch = false;


        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Initialize the Synaptics API
            SynCtrl = new SynAPICtrl();
            SynDevCtrl = new SynDeviceCtrl();
            SynCtrl.Initialize();

            // Set UI Elements
            trackBar1.Value = scrollSpeed;
            displayScrollDirection();

            initTouchpad();

            // Touchpad Event Handler
            SynDevCtrl.OnPacket += new _ISynDeviceCtrlEvents_OnPacketEventHandler(SynDevCtrl_OnPacket);

            // Power Event Handlers
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            Microsoft.Win32.SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
        }

        /* Helper function to properly initialize the touchpad with the Synaptics API */
        private void initTouchpad()
        {
            // Start the API and look for a Touchpad
            SynCtrl.Activate();
            deviceHandle = SynCtrl.FindDevice(SynConnectionType.SE_ConnectionAny, SynDeviceType.SE_DeviceTouchPad, -1);
            if (deviceHandle == -1)
            {
                MessageBox.Show("Could not find Synaptics Touchpad!");
                Application.Exit();
            }
            else
            {
                // Touchpad Found! Let's make sure the API is set to use it.
                // Caveat: Only one touchpad is supported here.
                SynDevCtrl.Select(deviceHandle);
                SynDevCtrl.Activate();

                // We need to record the Touchpad sensitivity
                ZTouchThreshold = SynDevCtrl.GetLongProperty(SynDeviceProperty.SP_ZTouchThreshold);
            }
        }


        /* Event handler for Synaptics API touch events */
        void SynDevCtrl_OnPacket()
        {
            SynPacketCtrl SynPacCtrl = new SynPacketCtrl();
            SynDevCtrl.LoadPacket(SynPacCtrl);

            if (SynPacCtrl.GetLongProperty(SynPacketProperty.SP_ExtraFingerState) == 2)
                isMultitouch = true;
            else if (SynPacCtrl.GetLongProperty(SynPacketProperty.SP_ExtraFingerState) == 1)
                isMultitouch = false;
            else if (SynPacCtrl.GetLongProperty(SynPacketProperty.SP_ExtraFingerState) == 0)
                isMultitouch = false;

            // 8 is a "magic number" made through testing. It specifies the boundary for a "normal" finger width.
            // This should probably be configurable in the future.

            if ((SynPacCtrl.W > 8)|| isMultitouch )
            {
                // Width is higher than normal. We probably have two fingers on the touchpad.
                // OR We actually found a native multitouch!
                // Disable touchpad input to the OS.
                // We will be exclusively handling the touchpad during these events.

                // Native multitouch caveat! Native two-finger scrolling WILL compete with this.
                // Disable it if you prever MultiTouchEnabler's scrolling.

                // Todo.. This is for coexisting nicely with native multitouch non-scrolling gestures.
                /*if (SynPacCtrl.GetLongProperty(SynPacketProperty.SP_ExtraFingerState) == 2)
                {
                    //Check space between fingers. We don't want to interfere with other gestures...
                    if (SynPacCtrl.XMickeys > 5)
                    {
                        if (isExclusivityAcquired)
                            SynDevCtrl.Unacquire();
                        isExclusivityAcquired = false;
                        gestureMode = 1;
                    }
                    
                }*/

                if (!isExclusivityAcquired && gestureMode==0)
                {
                    try
                    {
                        // This gives us exclusive control of the touchpad.
                        // Other software, like the OS, will not recieve the touch events.
                        SynDevCtrl.Acquire(0);
                        isExclusivityAcquired = true;
                    }
                    catch
                    {
                        // This might happen sometimes. It's okay.
                        isExclusivityAcquired = false;
                    }
                }
                if (gestureMode == 0)
                {
                    // Scroll the mouse! 
                    // Use the user-defined scroll speed factor and the inversion setting to determine the scroll amount
                    mouse_event(2048, 0, 0, SynPacCtrl.YMickeys * scrollSpeed * scrollInversion, GetMessageExtraInfo());
                }
                else
                {
                    gestureMode = 1;
                }
            }
            else
            {
                // We need to relinquish exclusivity of the touchpad.
                if (isExclusivityAcquired)
                    SynDevCtrl.Unacquire();
                isExclusivityAcquired = false;
            }

            // We build up a lot of memory usage if we let this go on normally
            // Force the Garbage Collector to do its job!
            SynPacCtrl = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // Set the new scroll factor into the user settings storage.
            scrollSpeed = trackBar1.Value;
            Properties.Settings.Default.ScrollSpeed = scrollSpeed;
            Properties.Settings.Default.Save();
        }

        private void inversionButton_Click(object sender, EventArgs e)
        {
            // Set the new inversion setting into the user settings storage.
            scrollInversion = scrollInversion * -1;
            Properties.Settings.Default.ScrollInversion = scrollInversion;
            Properties.Settings.Default.Save();
            displayScrollDirection();
        }

        private void displayScrollDirection()
        {
            if (scrollInversion > 0)
                button2.Text = "Disabled";
            else
                button2.Text = "Enabled";
        }

        /* Event handler for taskbar close menu */
        private void closeEnablerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeApp = true;
            this.Close();
        }

        /* Override the main window's close button */
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closeApp)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        /* Event handler for when the taskbar icon is clicked */
        private void notifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            //MessageBox.Show(e.Mode.ToString());
        }

        /* Power mode event handler */
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // We need to handle sleeping(and other related events)
            // This is so we never lose the lock on the touchpad hardware.
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    SynDevCtrl.Deactivate();
                    SynCtrl.Deactivate();
                    initTouchpad();
                    break;
                default:
                    break;
            }
        }

        // WinAPI Functions
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, IntPtr dwExtraInfo);
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetMessageExtraInfo();
    }
}
