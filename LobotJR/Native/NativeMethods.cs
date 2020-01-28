using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LobotJR.Native
{
    public class NativeMethods
    {
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        public enum KeyEvents
        {
            KeyDown = 0,
            ExtendedKey = 1,
            KeyUp = 2,
            Unicode = 4,
            ScanCode = 8
        }

        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr point);
        public static int SetForegroundWindowSafe(IntPtr pointer)
        {
            return SetForegroundWindow(pointer);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        public static IntPtr SendMessageSafe(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, wMsg, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
        public static IntPtr GetMessageExtraInfoSafe()
        {
            return GetMessageExtraInfo();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        public static uint SendInput(short scanCode, bool release = false)
        {
            var inputData = new INPUT[1];
            inputData[0].type = (int)InputType.Keyboard;
            inputData[0].Union.ki.wScan = scanCode;
            inputData[0].Union.ki.dwFlags = (int)KeyEvents.ScanCode;
            if (release)
                inputData[0].Union.ki.dwFlags |= (int)KeyEvents.KeyUp;
            inputData[0].Union.ki.time = 0;
            inputData[0].Union.ki.dwExtraInfo = IntPtr.Zero;
            return SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
        }

        public static uint SendInput(char key, bool release = false)
        {
            short scanCode = (short)MapVirtualKeySafe(VkKeyScanSafe(key), 0);
            return SendInput(scanCode, release);
        }

        public static uint SendInput(char[] keys, bool release = false)
        {
            uint output = 0;
            foreach (var key in keys)
            {
                short scanCode = (short)MapVirtualKeySafe(VkKeyScanSafe(key), 0);
                output = SendInput(scanCode, release);
            }
            return output;
        }

        [DllImport("user32.dll")]
        private static extern ushort VkKeyScan(char ch);
        public static ushort VkKeyScanSafe(char ch)
        {
            return VkKeyScan(ch);
        }

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
        public static uint MapVirtualKeySafe(uint uCode, uint uMapType)
        {
            return MapVirtualKey(uCode, uMapType);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public INPUTUNION Union;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public static void SendFor(char key, int ms)
        {
            // Send a keydown, leaving the key down indefinitely. It seems you have to do this for even
            // single inputs with shorter times because putting the keyup right after the keydown in the
            // buffer is too fast for Dark Souls
            NativeMethods.SendInput(key);

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            NativeMethods.SendInput(key, true);
        }

        public static void SendFor(short key, int ms)
        {
            // Send a keydown, leaving the key down indefinitely. It seems you have to do this for even
            // single inputs with shorter times because putting the keyup right after the keydown in the
            // buffer is too fast for Dark Souls
            NativeMethods.SendInput(key);

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            NativeMethods.SendInput(key, true);
        }

        // overloaded to handle more than one character send at a time
        public static void SendFor(char[] key, int ms)
        {
            NativeMethods.SendInput(key);

            // A timer is probably more suitable
            Thread.Sleep(ms);

            // After waiting, send the keyup
            NativeMethods.SendInput(key, true);
        }
    }
}
