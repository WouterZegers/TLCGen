﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TLCGen.DataAccess;
using TLCGen.Helpers;

namespace TLCGen.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private ControllerViewModel _ControllerVM;
        private DataProvider _MyDataProvider;

        #endregion // Fields

        #region Properties

        public ControllerViewModel ControllerVM
        {
            get { return _ControllerVM; }
            set
            {
                _ControllerVM = value;
                OnPropertyChanged("ControllerVM");
                OnPropertyChanged("HasController");
            }
        }

        public bool HasController
        {
            get { return ControllerVM != null; }
        }

        public DataProvider MyDataProvider
        {
            get { return _MyDataProvider; }
        }

        public string ProgramTitle
        {
            get
            {
                if(HasController && !string.IsNullOrEmpty(MyDataProvider.FileName))
                    return "TLCGen - " + MyDataProvider.FileName;
                else
                    return "TLCGen";
            }
        }

        #endregion // Properties

        #region Commands

        RelayCommand _NewFileCommand;
        public ICommand NewFileCommand
        {
            get
            {
                if (_NewFileCommand == null)
                {
                    _NewFileCommand = new RelayCommand(NewFileCommand_Executed, NewFileCommand_CanExecute);
                }
                return _NewFileCommand;
            }
        }

        RelayCommand _OpenFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                if (_OpenFileCommand == null)
                {
                    _OpenFileCommand = new RelayCommand(OpenFileCommand_Executed, OpenFileCommand_CanExecute);
                }
                return _OpenFileCommand;
            }
        }

        RelayCommand _SaveFileCommand;
        public ICommand SaveFileCommand
        {
            get
            {
                if (_SaveFileCommand == null)
                {
                    _SaveFileCommand = new RelayCommand(SaveFileCommand_Executed, SaveFileCommand_CanExecute);
                }
                return _SaveFileCommand;
            }
        }

        RelayCommand _SaveAsFileCommand;
        public ICommand SaveAsFileCommand
        {
            get
            {
                if (_SaveAsFileCommand == null)
                {
                    _SaveAsFileCommand = new RelayCommand(SaveAsFileCommand_Executed, SaveAsFileCommand_CanExecute);
                }
                return _SaveAsFileCommand;
            }
        }


        RelayCommand _CloseFileCommand;
        public ICommand CloseFileCommand
        {
            get
            {
                if (_CloseFileCommand == null)
                {
                    _CloseFileCommand = new RelayCommand(CloseFileCommand_Executed, CloseFileCommand_CanExecute);
                }
                return _CloseFileCommand;
            }
        }

        RelayCommand _ExitApplicationCommand;
        public ICommand ExitApplicationCommand
        {
            get
            {
                if (_ExitApplicationCommand == null)
                {
                    _ExitApplicationCommand = new RelayCommand(ExitApplicationCommand_Executed, ExitApplicationCommand_CanExecute);
                }
                return _ExitApplicationCommand;
            }
        }

        #endregion // Commands

        #region Command functionality

        void NewFileCommand_Executed(object prm)
        {
            if (!ControllerHasChanged())
            {
                MyDataProvider.NewOrganizer();
                ControllerVM = new ControllerViewModel(MyDataProvider.Controller);
                OnPropertyChanged("ProgramTitle");
            }
        }

        bool NewFileCommand_CanExecute(object prm)
        {
            return true;
        }

        void OpenFileCommand_Executed(object prm)
        {
            if (!ControllerHasChanged())
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.CheckFileExists = true;
                openFileDialog.Filter = "TLCGen files|*.tlcgen";
                if (openFileDialog.ShowDialog() == true)
                {
                    MyDataProvider.FileName = openFileDialog.FileName;
                    MyDataProvider.LoadController();
                    ControllerVM = new ControllerViewModel(MyDataProvider.Controller);
                    ControllerVM.SelectedTabIndex = 0;
                    OnPropertyChanged("ProgramTitle");
                }
            }
        }

        bool OpenFileCommand_CanExecute(object prm)
        {
            return true;
        }

        void SaveFileCommand_Executed(object prm)
        {
            if (string.IsNullOrWhiteSpace(MyDataProvider.FileName))
                SaveAsFileCommand.Execute(null);
            else
            {
                MyDataProvider.SaveController();
                ControllerVM.HasChanged = false;
            }
        }

        bool SaveFileCommand_CanExecute(object prm)
        {
            return true;
        }

        void SaveAsFileCommand_Executed(object prm)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "TLCGen files|*.tlcgen";
            if (!string.IsNullOrWhiteSpace(MyDataProvider.FileName))
                saveFileDialog.FileName = MyDataProvider.FileName;
            if (saveFileDialog.ShowDialog() == true)
            {
                MyDataProvider.FileName = saveFileDialog.FileName;
                MyDataProvider.SaveController();
                ControllerVM.HasChanged = false;
                OnPropertyChanged("ProgramTitle");
            }
        }

        bool SaveAsFileCommand_CanExecute(object prm)
        {
            return true;
        }

        void CloseFileCommand_Executed(object prm)
        {
            if (!ControllerHasChanged())
            {
                MyDataProvider.CloseController();
                ControllerVM = null;
                OnPropertyChanged("ProgramTitle");
            }
        }

        bool CloseFileCommand_CanExecute(object prm)
        {
            return true;
        }

        void ExitApplicationCommand_Executed(object prm)
        {
            System.Windows.Application.Current.Shutdown();
        }

        bool ExitApplicationCommand_CanExecute(object prm)
        {
            return true;
        }

        #endregion // Command functionality

        #region Private methods

        bool ControllerHasChanged()
        {
            if (ControllerVM != null && ControllerVM.HasChanged)
            {
                System.Windows.MessageBoxResult r = System.Windows.MessageBox.Show("Wijzigingen opslaan?", "De regeling is gewijzigd. Opslaan?", System.Windows.MessageBoxButton.YesNoCancel);
                if (r == System.Windows.MessageBoxResult.Yes)
                {
                    SaveFileCommand.Execute(null);
                    if (ControllerVM.HasChanged)
                        return true;
                }
                else if (r == System.Windows.MessageBoxResult.Cancel)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion // Private methods

        #region Public methods

        #endregion // Public methods

        #region Constructor

        public MainWindowViewModel()
        {
            _MyDataProvider = new DataProvider();

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);

        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ControllerHasChanged())
            {
                e.Cancel = true;
            }
        }

        #endregion // Constructor
    }
}
