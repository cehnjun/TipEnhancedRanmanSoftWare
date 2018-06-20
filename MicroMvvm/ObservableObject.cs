using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.ObjectModel;


namespace MicroMvvm
{
    [Serializable]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) 
            => PropertyChanged?.Invoke(this, e);

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresssion) 
            => RaisePropertyChanged(propertyExpresssion.ExtractPropertyName());

        protected void RaisePropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Warns the developer if this Object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                Debug.Fail("Invalid property name: " + propertyName);
            // verify that the property name matches a real,  
            // public, instance property on this Object.
        }
    }
}
