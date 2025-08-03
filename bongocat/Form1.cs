using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AvoidFlickering;
using Newtonsoft.Json;

namespace bongocat
{
    public partial class Form1 : Form
    {
        private LowLevelKeyboardProc _keyboardProc;
        private LowLevelMouseProc _mouseProc;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;

        PictureBox rightPaw = new PictureBox();
        PictureBox leftPaw = new PictureBox();
        bool isMouseDown = false;
        bool isKeyDown = false;

        Image l_up, l_wave, ls_left, ls_right, r_up, r_wave, rs_left, rs_right, bg;

        Random random = new Random();
        Image[] imgs;
        Image[] imgs1;
        Image[] imgs2;
        int WM_X = 0;
        UserConfig config;
        string path = @"imgs\";

        public Form1()
        {
            InitializeComponent();
            _keyboardProc = HookCallbackKeyboard;
            _mouseProc = HookCallbackMouse;
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
            _mouseHookID = SetMouseHook(_mouseProc);
            config = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText("mode.txt"));
            string mode = config.Folder;
            string format = config.BgFormat;
            path += mode+@"\";

            l_up = (Image)Image.FromFile(path+"l_up.png").Clone();
            l_wave = (Image)Image.FromFile(path+"l_wave.png").Clone();
            ls_left = (Image)Image.FromFile(path+"ls_left.png").Clone();
            ls_right = (Image)Image.FromFile(path+"ls_right.png").Clone();
            r_up = (Image)Image.FromFile(path+"r_up.png").Clone();
            r_wave = (Image)Image.FromFile(path+"r_wave.png").Clone();
            rs_left = (Image)Image.FromFile(path+"rs_left.png").Clone();
            rs_right = (Image)Image.FromFile(path+"rs_right.png").Clone();
            bg = (Image)Image.FromFile(path+ "bg." + format).Clone();

            imgs = new Image[] { rs_left, rs_right };
            imgs2 = new Image[] { r_wave, r_up };
            imgs1 = new Image[] { l_wave, l_up };

            pictureBox1.Image = bg;

            rightPaw.Image = r_up;
            rightPaw.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Controls.Add(rightPaw);
            rightPaw.Dock = DockStyle.Fill;
            rightPaw.Location = new Point(0, 0);

            leftPaw.Image = l_up;
            leftPaw.SizeMode = PictureBoxSizeMode.StretchImage;
            rightPaw.Controls.Add(leftPaw);
            leftPaw.Dock = DockStyle.Fill;
            leftPaw.Location = new Point(0, 0);

            AntiFlicker._run(this.Handle);

            // Ensure hooks are unhooked when the form closes
            FormClosing += (s, e) =>
            {
                UnhookWindowsHookEx(_keyboardHookID);
                UnhookWindowsHookEx(_mouseHookID);
            };
        }

        #region hook setup
        // Windows API declarations
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_KEYUP = 0x0101;
        private const int WM_MOUSEUP = 0x0202;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
#endregion

        private IntPtr HookCallbackKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Log or process the key press
                this.Invoke((MethodInvoker)delegate
                {
                    ///key down
                    //textBox1.AppendText($"Key Pressed: {(Keys)vkCode}{Environment.NewLine}");
                    if (!isKeyDown)
                    {
                        rightPaw.Image = imgs[random.Next(0, 2)];
                        isKeyDown = true;
                    }
                });
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Log or process the key press
                this.Invoke((MethodInvoker)delegate
                {
                    ///key up
                    //textBox1.AppendText($"Key Pressed: {(Keys)vkCode}{Environment.NewLine}");
                    isKeyDown = false;
                    rightPaw.Image = imgs2[random.Next(0, 2)];
                });
            }
            GC.Collect();
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackMouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (wParam == (IntPtr)WM_MOUSEMOVE)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //mouse move
                        //textBox1.Text = ($"({hookStruct.pt.X}, {hookStruct.pt.Y})");
                        int x = hookStruct.pt.X;
                        if(!isMouseDown)
                        if (WM_X < x)
                        {
                            if(leftPaw.Image != imgs1[0])
                            leftPaw.Image = imgs1[0];
                        }
                        else if (WM_X > x)
                        {
                            if (leftPaw.Image != imgs1[1])
                                leftPaw.Image = imgs1[1];
                        }
                        WM_X = x;
                    });
                }
                ///mouse click
                else if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    isMouseDown = true;
                    this.Invoke((MethodInvoker)delegate
                    {
                        //textBox1.AppendText($"Left Mouse Click at ({hookStruct.pt.X}, {hookStruct.pt.Y}){Environment.NewLine}");
                        
                        leftPaw.Image = ls_left;
                    });
                }
                else if (wParam == (IntPtr)WM_RBUTTONDOWN)
                {
                    isMouseDown = true;
                    this.Invoke((MethodInvoker)delegate
                    {
                        //textBox1.AppendText($"Right Mouse Click at ({hookStruct.pt.X}, {hookStruct.pt.Y}){Environment.NewLine}");
                        leftPaw.Image = ls_right;
                    });
                }
                else
                {
                    isMouseDown = false;
                    leftPaw.Image = imgs1[random.Next(0, 2)];
                }
            }
            GC.Collect();
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                if (this.FormBorderStyle == FormBorderStyle.Sizable)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.TransparencyKey = SystemColors.Control;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.TransparencyKey = Color.BlueViolet;
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                this.TopMost = !this.TopMost;
            }
            else if (e.Control && e.KeyCode == Keys.F3)
            {
                config.App.AppSize = this.Size;
                config.App.AppPosition = this.Location;
                config.App.FormBorder = this.FormBorderStyle;
                config.App.TopMost = this.TopMost;
                config.App.TransparentKey = this.TransparencyKey;
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText("mode.txt", json);
            }
            else if (e.KeyCode == Keys.F3)
            {
                this.TopMost = config.App.TopMost;
                this.FormBorderStyle = config.App.FormBorder;
                this.Location = config.App.AppPosition;
                this.Size = config.App.AppSize;
                this.TransparencyKey = config.App.TransparentKey;
            }
        }
    }
}
    
