using PassingCar.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PassingCar.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            try
            {
                //handle exception
                PropertyChangedEventHandler? changed = PropertyChanged;
                if (changed == null)
                {
                    return;
                }
                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        #endregion

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action? onChanged = null)
        {
            try
            {
                //handle exception
                if (EqualityComparer<T>.Default.Equals(backingStore, value))
                {
                    return false;
                }

                backingStore = value;
                onChanged?.Invoke();
                OnPropertyChanged(propertyName);
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        #region IDisposable
        private bool _disposed = false;

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    // Clear PropertyChanged event handlers to prevent memory leaks
                    PropertyChanged = null;
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
