using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AvoidFlickering;

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

        Random random = new Random();
        string[] imgs;
        string[] imgs1;
        string[] imgs2;
        int WM_X = 0;
        string path = @"imgs\";

        public Form1()
        {
            InitializeComponent();
            _keyboardProc = HookCallbackKeyboard;
            _mouseProc = HookCallbackMouse;
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
            _mouseHookID = SetMouseHook(_mouseProc);
            string mode = File.ReadAllText(@"mode.txt").Trim();
            path += mode+@"\";
            imgs = new string[2] { path+"rs_left.png", path+"rs_right.png" };
            imgs1 = new string[2] { path+"l_up.png", path+"l_wave.png" };
            imgs2 = new string[2] { path+"r_up.png", path+"r_wave.png" };

            pictureBox1.Image = Image.FromFile(path+"bg.png");

            rightPaw.Image = Image.FromFile(path+"r_up.png");
            rightPaw.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Controls.Add(rightPaw);
            rightPaw.Dock = DockStyle.Fill;
            rightPaw.Location = new Point(0, 0);

            leftPaw.Image = Image.FromFile(path+"l_up.png");
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
                    rightPaw.Image = Image.FromFile(imgs[random.Next(0, 2)]);
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
                    rightPaw.Image = Image.FromFile(imgs2[random.Next(0, 2)]);
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
                        if (WM_X < x)
                        {
                            leftPaw.Image = Image.FromFile(imgs1[1]);
                        }
                        else if (WM_X > x)
                        {
                            leftPaw.Image = Image.FromFile(imgs1[0]);
                        }
                        WM_X = x;
                    });
                }
                ///mouse click
                else if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //textBox1.AppendText($"Left Mouse Click at ({hookStruct.pt.X}, {hookStruct.pt.Y}){Environment.NewLine}");
                        leftPaw.Image = Image.FromFile(path+"ls_left.png");
                    });
                }
                else if (wParam == (IntPtr)WM_RBUTTONDOWN)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //textBox1.AppendText($"Right Mouse Click at ({hookStruct.pt.X}, {hookStruct.pt.Y}){Environment.NewLine}");
                        leftPaw.Image = Image.FromFile(path+"ls_right.png");
                    });
                }
                else
                {
                    leftPaw.Image = Image.FromFile(imgs1[random.Next(0, 2)]);
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
            if (e.KeyCode == Keys.F2)
            {
                this.TopMost = !this.TopMost;
            }
        }
    }
}
