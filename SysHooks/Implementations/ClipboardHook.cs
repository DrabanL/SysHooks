using RabanSoft.SysHooks.Core;
using RabanSoft.SysInvoke;
using RabanSoft.SysInvoke.Exceptions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RabanSoft.SysHooks.Implementations {
    public class ClipboardHook : HookForm {
        private const int WM_DRAWCLIPBOARD = 0x0308;

        private IntPtr _nextHookPtr;
        private IntPtr _hookPtr;

        public ClipboardHook(User32.HOOKPROC hookProcCallback) : base(hookProcCallback) { }

        protected override void WndProc(ref Message m) {
            switch (m.Msg) {
                case WM_DRAWCLIPBOARD:
                    HookProcImplementation?.Invoke(m.Msg, m.WParam, m.LParam);
                    break;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Initializing the hook, which would be invoked by Windows on <see cref="WndProc"/>
        /// </summary>
        internal override void setHook() {
            if (!_hookPtr.Equals(IntPtr.Zero))
                return;

            _hookPtr = Handle;

            // SetClipboardViewer will return a handle to the next clipboard listener after our own, to be referenced once we are done with our listener
            _nextHookPtr = User32.SetClipboardViewer(_hookPtr);

            {
                // TODO: find a way to correctly detect failure of SetClipboardViewer call.
                // SetClipboardViewer may return zero handle for both cases where the invoke failed, or there is no next clipboard listener chain.
                // Marshal.GetLastWin32Error() may return error code unrelated to the last call, if the invoke did not really fail.

                return;

                // unreliable
                if (_nextHookPtr.Equals(IntPtr.Zero)) {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode > 0)
                        throw new SetClipboardViewerException($"Error code {errorCode}");
                }
            }
        }

        internal override void releaseHook() {
            if (_hookPtr.Equals(IntPtr.Zero))
                return;

            // after we are done with listening for clipboard changes, we must change the clipboard chain and remove our listener
            if (User32.ChangeClipboardChain(_hookPtr, _nextHookPtr)) {
                _nextHookPtr = IntPtr.Zero;
                _hookPtr = IntPtr.Zero;
            } else
                throw new ChangeClipboardChainException($"Error code {Marshal.GetLastWin32Error()}");
        }
    }
}
