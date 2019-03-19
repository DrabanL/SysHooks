using RabanSoft.SysHooks.Core;
using RabanSoft.SysInvoke;
using RabanSoft.SysInvoke.Exceptions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RabanSoft.SysHooks.Implementations {
    public class KeyboardHook : HookForm {
        private const int WH_KEYBOARD_LL = 13;

        private IntPtr _hookPtr;

        public KeyboardHook(User32.HOOKPROC hookProcCallback) : base(hookProcCallback) { }

        internal override void setHook() {
            if (!_hookPtr.Equals(IntPtr.Zero))
                return;

            // our own process's module must be used as address for the keyboard hook
            var mainModulePtr = IntPtr.Zero;
            using (var currentProcess = Process.GetCurrentProcess())
            using (var mainModule = currentProcess.MainModule)
                mainModulePtr = mainModule.BaseAddress;

            // WH_KEYBOARD_LL is used to be able to intercept the keyboard message before Windows does.
            // SetWindowsHookEx should return a pointer to our hook, to later be able to free it.
            _hookPtr = User32.SetWindowsHookEx(User32.HookType.WH_KEYBOARD_LL, hookProcCallback, mainModulePtr, 0);

            if (_hookPtr.Equals(IntPtr.Zero))
                throw new SetWindowsHookExException($"Error code {Marshal.GetLastWin32Error()}");
        }

        private IntPtr hookProcCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            base.HookProcImplementation(nCode, wParam, lParam);

            // the keyboard message must be farwarded to the next registered hook, and eventually to Windows
            return User32.CallNextHookEx(_hookPtr, nCode, wParam, lParam);
        }

        internal override void releaseHook() {
            if (_hookPtr.Equals(IntPtr.Zero))
                return;

            if (User32.UnhookWindowsHookEx(_hookPtr))
                _hookPtr = IntPtr.Zero;
            else
                throw new UnhookWindowsHookExException($"Error code {Marshal.GetLastWin32Error()}");
        }
    }
}
