﻿using System;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;
using TLCGen.Models.Enumerations;
using TLCGen.Settings;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;

namespace TLCGen.ViewModels
{
    public class PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper
    {
        public PrioIngreepInUitMeldingVoorwaardeTypeEnum Value { get; }
        public string Description { get; }

        public PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum value)
        {
            Value = value;
            Description = Value.GetDescription();
        }
    }

    public class PrioIngreepInUitMeldingViewModel : ViewModelBase, IViewModelWithItem
    {
        #region Fields

        private RelayCommand _removeMeldingCommand;
        private readonly object _parent;
        private PrioIngreepViewModel _ingreep;
        private ViewModelBase _actualViewModel;
        private PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper _prioIngreepInUitMeldingType;

        #endregion // Fields

        #region Properties

        public string Naam
        {
            get => string.IsNullOrWhiteSpace(PrioIngreepInUitMelding.Naam) ? "geen_naam" : PrioIngreepInUitMelding.Naam;
            set
            {
                PrioIngreepInUitMelding.Naam = value;
                RaisePropertyChanged<object>(nameof(Naam), broadcast: true);
            }
        }

        public PrioIngreepInUitMeldingModel PrioIngreepInUitMelding { get; }
        
        public PrioIngreepInUitMeldingTypeEnum InUit => PrioIngreepInUitMelding.InUit;

        public ObservableCollection<PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper> MeldingenTypes { get; } = new ObservableCollection<PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper>();

        public PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper Type
        {
            get => _prioIngreepInUitMeldingType;
            set
            {
                _prioIngreepInUitMeldingType = value;
                
                if (value != null && PrioIngreepInUitMelding != null)
                {
                    var raise = PrioIngreepInUitMelding.Type != value.Value;
                    PrioIngreepInUitMelding.Type = value.Value;
                    if (raise) RaisePropertyChanged<object>(broadcast: true);
                }

                RaisePropertyChanged("");

                var msg = new PrioIngreepMeldingNeedsFaseCyclusAndIngreepMessage(this);
                MessengerInstance.Send(msg);
                if (msg.FaseCyclus == null) return;
                Naam = msg.FaseCyclus 
                       + msg.Ingreep 
                       + DefaultsProvider.Default.GetMeldingShortcode(PrioIngreepInUitMelding)
                       + (InUit == PrioIngreepInUitMeldingTypeEnum.Inmelding ? "in" : "uit");
                MessengerInstance.Send(new PrioIngreepMeldingChangedMessage(msg.FaseCyclus, PrioIngreepInUitMelding));
                SetActualViewModel();
            }
        }
        
        public bool HasKAR => Type?.Value == PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding;
        public bool HasSD => Type?.Value == PrioIngreepInUitMeldingVoorwaardeTypeEnum.SelectieveDetector;
        public bool HasInpSD => Type?.Value == PrioIngreepInUitMeldingVoorwaardeTypeEnum.VecomViaDetector;
        public bool HasDet => Type?.Value == PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector;
        public bool HasRis => Type?.Value == PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISVoorwaarde;

        public ViewModelBase ActualViewModel
        {
            get => _actualViewModel;
            set
            {
                _actualViewModel = value; 
                RaisePropertyChanged();
            }
        }

        public PrioIngreepRegularMeldingViewModel ViewModel { get; }

        public PrioIngreepRISMeldingViewModel RISViewModel { get; }

        public PrioIngreepPelotonMeldingViewModel PelotonViewModel { get; }

        public bool OpvangStoring
        {
            get => PrioIngreepInUitMelding.OpvangStoring;
            set
            {
                PrioIngreepInUitMelding.OpvangStoring = value;
                if (value)
                {
                    PrioIngreepInUitMelding.MeldingBijstoring = new PrioIngreepInUitMeldingModel()
                    {
                        InUit = this.InUit
                    };
                    MeldingBijstoring.Add(new PrioIngreepInUitMeldingViewModel(PrioIngreepInUitMelding.MeldingBijstoring, this, _ingreep));
                }
                else
                {
                    MeldingBijstoring.Clear();
                    PrioIngreepInUitMelding.MeldingBijstoring = null;
                }
                RaisePropertyChanged<object>(broadcast: true);
                RaisePropertyChanged(nameof(MeldingBijstoring));
            }
        }
        
        public ObservableCollection<PrioIngreepInUitMeldingViewModel> MeldingBijstoring { get; set; } = new ObservableCollection<PrioIngreepInUitMeldingViewModel>();

        #endregion // Properties

        #region Commands

        
        public ICommand RemoveMeldingCommand
        {
            get
            {
                return _removeMeldingCommand ?? (_removeMeldingCommand =
                           new RelayCommand(() =>
                           {
                               switch (_parent)
                               {
                                   case PrioIngreepInUitMeldingViewModel iu:
                                       iu.OpvangStoring = false;
                                       break;
                                   case PrioIngreepMeldingenListViewModel list:
                                       list.Meldingen.Remove(this);
                                       break;
                               }
                           }));
            }
        }

        #endregion // Commands

        #region TLCGen events

        #endregion // TLCGen events

        #region IViewModelWithItem

        public object GetItem()
        {
            return PrioIngreepInUitMelding;
        }

        #endregion // IViewModelWithItem

        #region Public Methods

        public void RefreshAvailableTypes()
        {
            var sel = Type;
            MeldingenTypes.Clear();
            switch (_ingreep.Type)
            {
                case PrioIngreepVoertuigTypeEnum.Tram:
                case PrioIngreepVoertuigTypeEnum.Bus:
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.SelectieveDetector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.VecomViaDetector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISVoorwaarde));
                    break;
                case PrioIngreepVoertuigTypeEnum.Fiets:
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.FietsMassaPeloton));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.FietsRISPeloton));
                    break;
                case PrioIngreepVoertuigTypeEnum.Vrachtwagen:
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.VrachtRIS));
                    break;
                case PrioIngreepVoertuigTypeEnum.Auto:
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.AutoMassaPeloton));
                    MeldingenTypes.Add(new PrioIngreepInUitMeldingVoorwaardeTypeEnumWrapper(PrioIngreepInUitMeldingVoorwaardeTypeEnum.AutoRISPeloton));
                    break;
                case PrioIngreepVoertuigTypeEnum.NG:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Type = MeldingenTypes.FirstOrDefault(x => x.Value == sel?.Value);
            if (Type == null) Type = MeldingenTypes.FirstOrDefault();
        }

        #endregion // Public Methods

        #region Private Methods

        private void SetActualViewModel()
        {
            if (Type == null) return;
            switch (Type.Value)
            {
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.VecomViaDetector:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.SelectieveDetector:
                    ActualViewModel = ViewModel;
                    break;
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISVoorwaarde:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.VrachtRIS:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.FietsRISPeloton:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.AutoRISPeloton:
                    ActualViewModel = RISViewModel;
                    break;
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.FietsMassaPeloton:
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.AutoMassaPeloton:
                    ActualViewModel = PelotonViewModel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ActualViewModel.RaisePropertyChanged("");
        }

        #endregion // Private Methods

        #region Constructor

        public PrioIngreepInUitMeldingViewModel(PrioIngreepInUitMeldingModel oVIngreepMassaDetectieMelding, object parent, PrioIngreepViewModel ingreep)
        {
            _ingreep = ingreep;
            RefreshAvailableTypes();
            _parent = parent;

            PrioIngreepInUitMelding = oVIngreepMassaDetectieMelding;
            
            ViewModel = new PrioIngreepRegularMeldingViewModel(this);
            RISViewModel = new PrioIngreepRISMeldingViewModel(this);
            PelotonViewModel = new PrioIngreepPelotonMeldingViewModel(this);

            if (PrioIngreepInUitMelding.MeldingBijstoring != null)
            {
                MeldingBijstoring.Add(new PrioIngreepInUitMeldingViewModel(PrioIngreepInUitMelding.MeldingBijstoring, this, ingreep));
            }

            Type = MeldingenTypes.FirstOrDefault(x => x.Value == PrioIngreepInUitMelding.Type);
            if (Type == null)
            {
                Type = MeldingenTypes.FirstOrDefault();
            }

            SetActualViewModel();
        }

        #endregion //Constructor
    }
}
