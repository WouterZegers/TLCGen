﻿using GalaSoft.MvvmLight.Messaging;
using System;
using GalaSoft.MvvmLight;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;

namespace TLCGen.ViewModels
{
    public class RoBuGroverConflictGroepViewModel : ViewModelBase, IViewModelWithItem
    {
        #region Fields

        private RoBuGroverConflictGroepModel _ConflictGroep;
        private RoBuGroverConflictGroepFaseViewModel _SelectedFase;

        #endregion // Fields

        #region Properties

        public RoBuGroverConflictGroepModel ConflictGroep => _ConflictGroep;

        public ObservableCollectionAroundList<RoBuGroverConflictGroepFaseViewModel, RoBuGroverConflictGroepFaseModel> Fasen
        {
            get;
            private set;
        }

        public RoBuGroverConflictGroepFaseViewModel SelectedFase
        {
            get => _SelectedFase;
            set
            {
                _SelectedFase = value;
                RaisePropertyChanged<object>(nameof(SelectedFase), broadcast: true);
            }
        }

        #endregion Properties

        #region Commands

        #endregion // Commands

        #region Command functionality

        #endregion // Command functionality

        #region Private methods

        #endregion // Private methods

        #region Public methods

        #endregion // Public Methods

        #region IViewModelWithItem

        public object GetItem()
        {
            return _ConflictGroep;
        }

        #endregion // IViewModelWithItem

        #region TLCGen Events

        private void OnFasenChanged(FasenChangedMessage message)
        {
            Fasen.Rebuild();
        }
        private void OnConflictsChanged(ConflictsChangedMessage message)
        {
            Fasen.Rebuild();
        }

        #endregion // TLCGen Events

        #region Constructor

        public RoBuGroverConflictGroepViewModel(RoBuGroverConflictGroepModel conflictgroep)
        {
            _ConflictGroep = conflictgroep;
            Fasen = new ObservableCollectionAroundList<RoBuGroverConflictGroepFaseViewModel, RoBuGroverConflictGroepFaseModel>(_ConflictGroep.Fasen);
            Messenger.Default.Register(this, new Action<FasenChangedMessage>(OnFasenChanged));
            Messenger.Default.Register(this, new Action<ConflictsChangedMessage>(OnConflictsChanged));
        }

        #endregion // Constructor

    }
}
