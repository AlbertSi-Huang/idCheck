using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace doublescreen
{
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    };



    class MessageBoxEx
    {
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int hookid,
            HookProc pfnhook, IntPtr hinst, int threadid);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhook,
            int code, IntPtr wparam, IntPtr lparam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string modName);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhook);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        private static extern bool MoveWindow(
            IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;
        private const int GW_OWNER = 4;
        private static IntPtr hookHandle = IntPtr.Zero;
        private static RECT GetOwnerRect(IntPtr hwnd)
        {
            RECT ownerRect = new RECT();
            IntPtr ownerHwnd = GetWindow(hwnd, GW_OWNER);
            GetWindowRect(ownerHwnd, ref ownerRect);
            return ownerRect;
        }
        private static IntPtr CBTHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            switch (nCode)
            {
                case HCBT_ACTIVATE:
                    RECT vRectangle = new RECT();
                    RECT ownerRect = GetOwnerRect(wParam);
                    GetWindowRect(wParam, ref vRectangle);
                    int width = vRectangle.right - vRectangle.left;
                    int height = vRectangle.bottom - vRectangle.top;
                    int ownerWidth = ownerRect.right - ownerRect.left;
                    int ownerHeight = ownerRect.bottom - ownerRect.top;
                    int left = Math.Max(ownerRect.left + (ownerWidth - width) / 2, 0);
                    int top = Math.Max(ownerRect.top + (ownerHeight - height) / 2, 0);
                    MoveWindow(wParam,
                        left,
                        top,
                        width, height, false);
                    UnhookWindowsHookEx(hookHandle);
                    break;
            }
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }
        private static void Lock()
        {
            hookHandle = SetWindowsHookEx(WH_CBT, new HookProc(CBTHookCallback),
                GetModuleHandle(null), 0);
        }
        //根据需要重载
        public static DialogResult Show(string text)
        {
            Lock();
            return MessageBox.Show(text);
        }
        public static DialogResult Show(IWin32Window owner, string text)
        {
            Lock();
            return MessageBox.Show(owner, text);
        }
        public static DialogResult Show(string text, string caption)
        {
            Lock();
            return MessageBox.Show(text, caption);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            Lock();
            return MessageBox.Show(owner, text, caption);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            Lock();
            return MessageBox.Show(text, caption, buttons);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            Lock();
            return MessageBox.Show(owner, text, caption, buttons);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Lock();
            return MessageBox.Show(text, caption, buttons, icon);
        }
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Lock();
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            Lock();
            return MessageBox.Show(text, caption, buttons, icon, defaultButton);
        }
    }
}


