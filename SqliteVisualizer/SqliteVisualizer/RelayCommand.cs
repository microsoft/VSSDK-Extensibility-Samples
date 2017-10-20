/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace SqliteVisualizer
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    //http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
    internal class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        bool _enabled;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
        : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
            _enabled = true;
        }
        #endregion // Constructors

        public bool Enable
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? _enabled : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members

        private void RaiseCanExecuteChanged()
        {
            var h = CanExecuteChanged;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
    }
}
