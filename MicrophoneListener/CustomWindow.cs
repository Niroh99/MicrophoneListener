using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MicrophoneListener
{
    public class CustomWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            return true;
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new DefaultCommand(new Action<object>((parameter) =>
                    {
                        Close();
                    }));
                }

                return _closeCommand;
            }
        }

        public event EventHandler TemplateApplyed;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TemplateApplyed?.Invoke(this, EventArgs.Empty);
        }
    }
}