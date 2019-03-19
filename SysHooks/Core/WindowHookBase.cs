using RabanSoft.SysHooks.Interfaces;
using RabanSoft.SysInvoke;
using RabanSoft.SysInvoke.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace RabanSoft.SysHooks.Core {
    /// <summary>
    /// A parent class for all Window(Form)-based hooks
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WindowHookBase<T> : IDisposable where T : HookForm {

        /// <summary>
        /// to be thread-safe, a lock is used anywhere <see cref="_hookCallbacks" /> is used.
        /// </summary>
        private readonly object _hookCallbackAccessLocker = new object();
        private List<IHookCallback> _hookCallbacks;
        private T _hookForm;

        public WindowHookBase() {
            _hookCallbacks = new List<IHookCallback>();

            // must be sure to run hook form in STA thread to be able to perform certain operations, on System.Windows.Clipboard class in example
            var formThread = new Thread(new ThreadStart(initHookForm));
            formThread.SetApartmentState(ApartmentState.STA);
            formThread.Start();
        }

        private void initHookForm() {
            // to be able to hook, it is required to have a real Window (form) related to our process.
            // in reality we could check if our process already has any existing Window, but to simplify (and with no big overhead or limitation) we always create a local Window for our hook.
            _hookForm = (T)Activator.CreateInstance(typeof(T), new User32.HOOKPROC(hookProcCallback));
            Application.Run(_hookForm);
        }
        
        private IntPtr hookProcCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            lock (_hookCallbackAccessLocker) {
                _hookCallbacks.ForEach(callback => {
                    try {
                        callback.OnHookProc(nCode, wParam, lParam);
                    } catch (Exception ex) {
                        callback.OnError(new HookProcException(ex.Message, ex));
                    }
                });
            }

            return default;
        }

        private void freeHookCallbacks() {
            if (_hookCallbacks == null)
                return;

            lock (_hookCallbackAccessLocker)
                _hookCallbacks.Clear();

            _hookCallbacks = null;
        }

        private void disposeHookForm() {
            if (_hookForm == null)
                return;

            using (_hookForm)
                _hookForm.Destroy();

            _hookForm = null;
        }

        public void Subscribe(IHookCallback callback) {
            lock (_hookCallbackAccessLocker)
                _hookCallbacks.Add(callback);
        }

        public void Unsubscribe(IHookCallback callback) {
            lock (_hookCallbackAccessLocker)
                _hookCallbacks.Remove(callback);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    freeHookCallbacks();
                    disposeHookForm();
                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WindowHookBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
