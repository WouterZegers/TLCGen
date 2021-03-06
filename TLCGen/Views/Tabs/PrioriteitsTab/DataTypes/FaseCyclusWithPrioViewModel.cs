﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;
using TLCGen.Models.Enumerations;
using TLCGen.Settings;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;

namespace TLCGen.ViewModels
{
    public class PrioItemViewModel : ViewModelBase
    {
        private bool _isExpanded;
        private bool _isSelected;

        [Browsable(false)]
        public virtual bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value; 
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (value)
                {
                    if (!_isExpanded)
                    {
                        IsExpanded = true;
                    }
                }
                RaisePropertyChanged();
            }
        }
    }

    public class FaseCyclusWithPrioViewModel : PrioItemViewModel, IComparable
    {
        private RelayCommand _addIngreepCommand;
        private RelayCommand<PrioIngreepViewModel> _removeIngreepCommand;
        private string _naam;

        #region Fields
        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public bool HasBus => Ingrepen.Any(x => x.FaseCyclus == Naam && x.Type == PrioIngreepVoertuigTypeEnum.Bus);
        [Browsable(false)]
        public bool HasTram => Ingrepen.Any(x => x.FaseCyclus == Naam && x.Type == PrioIngreepVoertuigTypeEnum.Tram);
        [Browsable(false)]
        public bool HasBicycle => Ingrepen.Any(x => x.FaseCyclus == Naam && x.Type == PrioIngreepVoertuigTypeEnum.Fiets);
        [Browsable(false)]
        public bool HasTruck => Ingrepen.Any(x => x.FaseCyclus == Naam && x.Type == PrioIngreepVoertuigTypeEnum.Vrachtwagen);

        [Browsable(false)]
        public string Naam
        {
            get => _naam;
            set
            {
                _naam = value; 
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public ObservableCollection<PrioIngreepViewModel> Ingrepen { get; }

        [Browsable(false)]
        public override bool IsExpanded
        {
            get => base.IsExpanded;
            set
            {
                base.IsExpanded = value;
                foreach (var ingreep in Ingrepen)
                {
                    ingreep.IsExpanded = value;
                    foreach (var list in ingreep.MeldingenLists)
                    {
                        list.IsExpanded = value;
                    }
                }
            }
        }

        #endregion // Proprerties

        #region Commands

        public ICommand RemoveIngreepCommand
        {
            get
            {
                return _removeIngreepCommand ??= new RelayCommand<PrioIngreepViewModel>(e => { Ingrepen.Remove(e); });
            }
        }

        public ICommand AddIngreepCommand
        {
            get
            {
                return _addIngreepCommand ??= new RelayCommand(() =>
                {
                    var prio = new PrioIngreepModel
                    {
                        FaseCyclus = Naam,
                        Type = PrioIngreepVoertuigTypeEnum.Bus
                    };
                    var newName = prio.FaseCyclus + DefaultsProvider.Default.GetVehicleTypeAbbreviation(prio.Type);
                    if (!NameSyntaxChecker.IsValidCName(newName))
                    {
                        newName = prio.FaseCyclus + "default";
                    }

                    var iNewName = 0;
                    var tempName = newName;
                    while (!Integrity.TLCGenIntegrityChecker.IsElementNaamUnique(
                        DataAccess.TLCGenControllerDataProvider.Default.Controller, tempName,
                        TLCGenObjectTypeEnum.PrioriteitsIngreep))
                    {
                        tempName = newName + ++iNewName;
                    }

                    prio.Naam = DefaultsProvider.Default.GetVehicleTypeAbbreviation(prio.Type) +
                                (iNewName == 0 ? "" : iNewName.ToString());

                    DefaultsProvider.Default.SetDefaultsOnModel(prio);
                    DefaultsProvider.Default.SetDefaultsOnModel(prio.MeldingenData);
                    PrioIngreepInUitMeldingModel inM = null;
                    PrioIngreepInUitMeldingModel uitM = null;
                    if (prio.Type == PrioIngreepVoertuigTypeEnum.Bus)
                    {
                        inM = new PrioIngreepInUitMeldingModel
                        {
                            Naam = "KAR",
                            AntiJutterTijdToepassen = true,
                            AntiJutterTijd = 15,
                            InUit = PrioIngreepInUitMeldingTypeEnum.Inmelding,
                            Type = PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding
                        };
                        prio.MeldingenData.Inmeldingen.Add(inM);
                        uitM = new PrioIngreepInUitMeldingModel
                        {
                            Naam = "KAR",
                            AntiJutterTijdToepassen = false,
                            InUit = PrioIngreepInUitMeldingTypeEnum.Uitmelding,
                            Type = PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding
                        };
                        prio.MeldingenData.Uitmeldingen.Add(uitM);
                    }

                    ControllerAccessProvider.Default.Controller.PrioData.PrioIngrepen.Add(prio);
                    ControllerAccessProvider.Default.Controller.PrioData.PrioIngrepen.BubbleSort();
                    var prioVm = new PrioIngreepViewModel(prio, this);
                    if (inM != null) MessengerInstance.Send(new PrioIngreepMeldingChangedMessage(prio.FaseCyclus, inM));
                    if (uitM != null)
                        MessengerInstance.Send(new PrioIngreepMeldingChangedMessage(prio.FaseCyclus, uitM));
                    Ingrepen.Add(prioVm);
                });
            }
        }

        #endregion // Commmands
        
        #region Private Methods

        private void IngrepenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (PrioIngreepViewModel m in e.OldItems)
                {
                    // TODO do this elsewhere
                    ControllerAccessProvider.Default.Controller.PrioData.PrioIngrepen.Remove(m.PrioIngreep);
                }
            }
        }
        
        #endregion // Private Methods
        
        #region Public Methods

        public void UpdateTypes()
        {
            RaisePropertyChanged(nameof(HasBus));
            RaisePropertyChanged(nameof(HasTram));
            RaisePropertyChanged(nameof(HasBicycle));
            RaisePropertyChanged(nameof(HasTruck));
        }

        #endregion // Public Methods

        #region IComparable

        public int CompareTo(object obj)
        {
            return string.Compare(Naam, ((FaseCyclusWithPrioViewModel)obj).Naam, StringComparison.Ordinal);
        }
        
        #endregion // IComparable

        #region Constructor

        public FaseCyclusWithPrioViewModel(string naam, IEnumerable<PrioIngreepModel> ingrepen)
        {
            Naam = naam;
            Ingrepen = new ObservableCollection<PrioIngreepViewModel>(ingrepen.Select(x => new PrioIngreepViewModel(x, this)));
            Ingrepen.CollectionChanged += IngrepenOnCollectionChanged;
        }

        #endregion // Constructor
    }
}