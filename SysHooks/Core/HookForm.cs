using System;
using System.Windows.Forms;
using static RabanSoft.SysInvoke.User32;

namespace RabanSoft.SysHooks.Core {
    /// <summary>
    /// A Form specifically created for purpose of receiving and contextualizing a Hook with a process Window.
    /// </summary>
    public abstract class HookForm : Form {

        internal HOOKPROC HookProcImplementation { get; private set; }

        /// <summary>
        /// abstract method to be implemented by child classes
        /// </summary>
        internal abstract void setHook();

        /// <summary>
        /// abstract method to be implemented by child classes
        /// </summary>
        internal abstract void releaseHook();

        public HookForm(HOOKPROC hookProcCallback) {
            HookProcImplementation = hookProcCallback;

            // we completely hide the form, as it is not really intended for GUI purpose.
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            Hide();

            // at this stage we are sure we have a Handle and the Window is fully initialized, so we can set the hook
            setHook();
        }
        
        public void Destroy() {
            Invoke(new MethodInvoker(delegate {
                using (this)
                    Close();
            }));
        }

        protected override void Dispose(bool disposing) {
            if (!IsDisposed)
                releaseHook();

            base.Dispose(disposing);
        }
    }
}
