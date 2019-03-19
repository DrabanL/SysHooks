using System;

namespace RabanSoft.SysHooks.Interfaces {
    public interface IHookCallback {
        void OnError(Exception ex);
        void OnHookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
