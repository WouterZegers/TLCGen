﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using TLCGen.Helpers;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;
using TLCGen.Messaging.Messages;
using System.Linq;
using TLCGen.Extensions;
using TLCGen.ModelManagement;
using TLCGen.Models;
using TLCGen.Plugins;
using TLCGen.Models.Enumerations;
using System.Collections.Generic;
using TLCGen.Dependencies.Messaging.Messages;

namespace TLCGen.ViewModels
{
    [TLCGenTabItem(index: 5, type: TabItemTypeEnum.FasenTab)]
    public class FasenRISTabViewModel : TLCGenTabItemViewModel
    {
        #region Fields

        private RISFaseCyclusDataViewModel _selectedRISFase;
        private RISSystemITFViewModel _selectedSystemITF;
        private RISDataModel _RISModel;
        private AddRemoveItemsManager<RISLaneRequestDataViewModel, RISLaneRequestDataModel, string> _lanesRequestManager;
        private AddRemoveItemsManager<RISLaneExtendDataViewModel, RISLaneExtendDataModel, string> _lanesExtendManager;
        private RelayCommand _addDefaultRequestLanesCommand;
        private RelayCommand _addDefaultExtendLanesCommand;
        private RelayCommand _addSystemITFCommand;
        private RelayCommand _removeSystemITFCommand;

        #endregion // Fields

        #region Properties

        public override string DisplayName => "RIS";

        public override ControllerModel Controller
        {
            get => base.Controller; set
            {
                base.Controller = value;
                RISModel = value?.RISData;
            }
        }

        public RISFaseCyclusDataViewModel SelectedRISFase
        {
            get => _selectedRISFase;
            set
            {
                _selectedRISFase = value;
                RaisePropertyChanged();
            }
        }

        public RISDataModel RISModel
        {
            get => _RISModel;
            set
            {
                _RISModel = value;
                RISFasen = value != null ? new ObservableCollectionAroundList<RISFaseCyclusDataViewModel, RISFaseCyclusDataModel>(_RISModel.RISFasen) : null;
                RISRequestLanes = value != null ? new ObservableCollectionAroundList<RISLaneRequestDataViewModel, RISLaneRequestDataModel>(_RISModel?.RISRequestLanes) : null;
                RISExtendLanes = value != null ? new ObservableCollectionAroundList<RISLaneExtendDataViewModel, RISLaneExtendDataModel>(_RISModel.RISExtendLanes) : null;
                _lanesRequestManager = null;
                _lanesExtendManager = null;
                if (MultiSystemITF != null) MultiSystemITF.CollectionChanged -= MultiSystemITF_CollectionChanged;
                if (value == null) return;
                MultiSystemITF = new ObservableCollectionAroundList<RISSystemITFViewModel, RISSystemITFModel>(_RISModel.MultiSystemITF);
                MultiSystemITF.CollectionChanged += MultiSystemITF_CollectionChanged;
                UpdateModel();
            }
        }

        private void MultiSystemITF_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MessengerInstance.Send(new ControllerDataChangedMessage());
        }

        public ObservableCollectionAroundList<RISFaseCyclusDataViewModel, RISFaseCyclusDataModel> RISFasen { get; private set; }
    
        public ObservableCollectionAroundList<RISLaneRequestDataViewModel, RISLaneRequestDataModel> RISRequestLanes { get; private set; }
        public ObservableCollectionAroundList<RISLaneExtendDataViewModel, RISLaneExtendDataModel> RISExtendLanes { get; private set; }
        public ObservableCollectionAroundList<RISSystemITFViewModel, RISSystemITFModel> MultiSystemITF { get; private set; }
        public ObservableCollection<RISFaseCyclusLaneDataViewModel> RISLanes { get; } = new ObservableCollection<RISFaseCyclusLaneDataViewModel>();

        public RISSystemITFViewModel SelectedSystemITF
        {
            get => _selectedSystemITF;
            set
            {
                _selectedSystemITF = value;
                RaisePropertyChanged();
            }
        }

        public bool RISToepassen
        {
            get => _RISModel?.RISToepassen == true;
            set
            {
                _RISModel.RISToepassen = value;
                RaisePropertyChanged<object>(broadcast: true);
                if (string.IsNullOrWhiteSpace(SystemITF)) SystemITF = Controller.Data.Naam;
                foreach (var fc in RISFasen)
                {
                    var sg = Controller.Fasen.First(x => x.Naam == fc.FaseCyclus);
                    if (sg != null && fc.Lanes.Any())
                    {
                        foreach (var l in fc.Lanes)
                        {
                            if (l.SimulatedStations.Any())
                            {
                                switch (sg.Type)
                                {
                                    case FaseTypeEnum.Auto:
                                        if (l.SimulatedStations[0].Type != RISStationTypeSimEnum.PASSENGERCAR) l.SimulatedStations[0].Type = RISStationTypeSimEnum.PASSENGERCAR;
                                        break;
                                    case FaseTypeEnum.Fiets:
                                        if (l.SimulatedStations[0].Type != RISStationTypeSimEnum.CYCLIST) l.SimulatedStations[0].Type = RISStationTypeSimEnum.CYCLIST;
                                        break;
                                    case FaseTypeEnum.Voetganger:
                                        if (l.SimulatedStations[0].Type != RISStationTypeSimEnum.PEDESTRIAN) l.SimulatedStations[0].Type = RISStationTypeSimEnum.PEDESTRIAN;
                                        break;
                                    case FaseTypeEnum.OV:
                                        if (l.SimulatedStations[0].Type != RISStationTypeSimEnum.BUS) l.SimulatedStations[0].Type = RISStationTypeSimEnum.BUS;
                                        break;
                                }
                            }
                        }
                    }
                }
                UpdateRISLanes();
                for (var l = 0; l < RISLanes.Count; l++)
                {
                    RISLanes[l].LaneID = l + 1;
                }
            }
        }

        public bool HasMultipleSystemITF
        {
            get => _RISModel?.HasMultipleSystemITF == true;
            set
            {
                _RISModel.HasMultipleSystemITF = value;
                RaisePropertyChanged<object>(broadcast: true);
                RaisePropertyChanged(nameof(NoHasMultipleSystemITF));
            }
        }

        public bool NoHasMultipleSystemITF => !HasMultipleSystemITF;

        public string SystemITF
        {
            get => _RISModel?.SystemITF;
            set
            {
                _RISModel.SystemITF = value;
                if (!HasMultipleSystemITF)
                {
                    foreach (var l in RISLanes)
                    {
                        l.SystemITF = value;
                    }
                }
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public AddRemoveItemsManager<RISLaneRequestDataViewModel, RISLaneRequestDataModel, string> LanesRequestManager =>
            _lanesRequestManager ??= new AddRemoveItemsManager<RISLaneRequestDataViewModel, RISLaneRequestDataModel, string>(
                RISRequestLanes,
                x =>
                {
                    if (!RISFasen.Any()) return null;
                    var lre = new RISLaneRequestDataViewModel(new RISLaneRequestDataModel()
                    {
                        SignalGroupName = RISRequestLanes.Any() ? RISRequestLanes.Last().SignalGroupName : RISFasen.First().FaseCyclus,
                        RijstrookIndex = 1,
                        Type = GetTypeForFase(null, RISRequestLanes.Any() ? RISRequestLanes.Last().SignalGroupName : RISFasen.First().FaseCyclus)
                    });
                    return lre;
                },
                (x, y) => false,
                () => MessengerInstance.Send(new ControllerDataChangedMessage())
            );

        public AddRemoveItemsManager<RISLaneExtendDataViewModel, RISLaneExtendDataModel, string> LanesExtendManager =>
            _lanesExtendManager ??= new AddRemoveItemsManager<RISLaneExtendDataViewModel, RISLaneExtendDataModel, string>(
                RISExtendLanes,
                x =>
                {
                    if (!RISFasen.Any()) return null;
                    var lre = new RISLaneExtendDataViewModel(new RISLaneExtendDataModel()
                    {
                        SignalGroupName = RISExtendLanes.Any() ? RISExtendLanes.Last().SignalGroupName : RISFasen.First().FaseCyclus,
                        RijstrookIndex = 1,
                        Type = GetTypeForFase(null, RISExtendLanes.Any() ? RISExtendLanes.Last().SignalGroupName : RISFasen.First().FaseCyclus)
                    });
                    return lre;
                },
                (x, y) => false,
                () => MessengerInstance.Send(new ControllerDataChangedMessage())
            );

        #endregion // Properties

        #region ITLCGenTabItem

        public override void OnSelected()
        {
            if (!RISRequestLanes.IsSorted())
            {
                RISRequestLanes.BubbleSort();
            }
            if (!RISExtendLanes.IsSorted())
            {
                RISExtendLanes.BubbleSort();
            }
        }

        #endregion

        #region Commands

        public ICommand AddDefaultRequestLanesCommand => _addDefaultRequestLanesCommand ?? (_addDefaultRequestLanesCommand = new RelayCommand(AddDefaultRequestLanesCommand_executed));

        private void AddDefaultRequestLanesCommand_executed()
        {
            foreach (var fc in Controller.Fasen)
            {
                var t = GetTypeForFase(fc);
                for (var i = 0; i < fc.AantalRijstroken; i++)
                {
                    if (RISRequestLanes.All(x => x.SignalGroupName != fc.Naam || x.SignalGroupName == fc.Naam && x.RijstrookIndex != i + 1 || x.SignalGroupName == fc.Naam && x.RijstrookIndex == i + 1 && x.Type != t))
                    {
                        RISRequestLanes.Add(new RISLaneRequestDataViewModel(new RISLaneRequestDataModel
                        {
                            SignalGroupName = fc.Naam,
                            RijstrookIndex = i + 1,
                            Type = t
                        }));
                    }
                }
            }
            RISRequestLanes.BubbleSort();
            return;
        }

        public ICommand AddDefaultExtendLanesCommand => _addDefaultExtendLanesCommand ?? (_addDefaultExtendLanesCommand = new RelayCommand(AddDefaultExtendLanesCommand_executed));

        private void AddDefaultExtendLanesCommand_executed()
        {
            foreach (var fc in Controller.Fasen)
            {
                var t = GetTypeForFase(fc);
                for (var i = 0; i < fc.AantalRijstroken; i++)
                {
                    if (RISExtendLanes.All(x => x.SignalGroupName != fc.Naam || x.SignalGroupName == fc.Naam && x.RijstrookIndex != i + 1 || x.SignalGroupName == fc.Naam && x.RijstrookIndex == i + 1 && x.Type != t))
                    {
                        RISExtendLanes.Add(new RISLaneExtendDataViewModel(new RISLaneExtendDataModel
                        {
                            SignalGroupName = fc.Naam,
                            RijstrookIndex = i + 1,
                            Type = t
                        }));
                    }
                }
            }
            RISExtendLanes.BubbleSort();
            return;
        }

        public ICommand AddSystemITFCommand => _addSystemITFCommand ?? (_addSystemITFCommand =
            new RelayCommand(
                () =>
                {
                    MultiSystemITF.Add(new RISSystemITFViewModel(new RISSystemITFModel()));
                },
                () => HasMultipleSystemITF));


        public ICommand RemoveSystemITFCommand => _removeSystemITFCommand ?? (_removeSystemITFCommand =
            new RelayCommand(
                () =>
                {
                    MultiSystemITF.Remove(SelectedSystemITF);
                    SelectedSystemITF = null;
                },
                () => HasMultipleSystemITF && SelectedSystemITF != null));

        #endregion // Commands

        private RISStationTypeEnum GetTypeForFase(FaseCyclusModel fc, string faseName = null)
        {
            if (fc == null)
            {
                fc = Controller.Fasen.FirstOrDefault(x => x.Naam == faseName);
            }
            var t = RISStationTypeEnum.UNKNOWN;
            if (fc == null) return t;
            switch (fc.Type)
            {
                case FaseTypeEnum.Auto:
                    t = RISStationTypeEnum.MOTORVEHICLES;
                    break;
                case FaseTypeEnum.Fiets:
                    t = RISStationTypeEnum.CYCLIST;
                    break;
                case FaseTypeEnum.Voetganger:
                    t = RISStationTypeEnum.PEDESTRIAN;
                    break;
                case FaseTypeEnum.OV:
                    t = RISStationTypeEnum.MOTORVEHICLES;
                    break;
            }
            return t;
        }

        #region TLCGen messaging

        private void OnFasenChanged(FasenChangedMessage msg)
        {
            if (msg.RemovedFasen != null && msg.RemovedFasen.Any())
            {
                foreach (var fc in msg.RemovedFasen)
                {
                    var RISFc = RISFasen.FirstOrDefault(x => x.FaseCyclus == fc.Naam);
                    if (RISFc != null)
                    {
                        RISFasen.Remove(RISFc);
                    }
                }
            }
            if (msg.AddedFasen != null && msg.AddedFasen.Any())
            {
                foreach (var fc in msg.AddedFasen)
                {
                    var risfc = new RISFaseCyclusDataViewModel(
                                new RISFaseCyclusDataModel { FaseCyclus = fc.Naam });
                    for (var i = 0; i < fc.AantalRijstroken; i++)
                    {
                        var sitf = SystemITF;
                        if (HasMultipleSystemITF)
                        {
                            sitf = MultiSystemITF.FirstOrDefault()?.SystemITF;
                        }
                        var l = new RISFaseCyclusLaneDataViewModel(new RISFaseCyclusLaneDataModel() { SignalGroupName = fc.Naam, RijstrookIndex = i + 1, SystemITF = sitf });
                        risfc.Lanes.Add(l);
                    }
                    RISFasen.Add(risfc);
                    RISRequestLanes.Add(new RISLaneRequestDataViewModel(new RISLaneRequestDataModel
                    {
                        SignalGroupName = fc.Naam,
                        RijstrookIndex = 1,
                        Type = GetTypeForFase(fc)
                    }));
                    RISExtendLanes.Add(new RISLaneExtendDataViewModel(new RISLaneExtendDataModel
                    {
                        SignalGroupName = fc.Naam,
                        RijstrookIndex = 1,
                        Type = GetTypeForFase(fc)
                    }));
                    MessengerInstance.Send(new ControllerDataChangedMessage());
                }
            }
            RISFasen.BubbleSort();
            RISRequestLanes.BubbleSort();
            RISExtendLanes.BubbleSort();
            UpdateRISLanes();
        }

        private void OnNameChanged(NameChangedMessage msg)
        {
            if (msg.ObjectType == TLCGenObjectTypeEnum.Fase)
            {
                RISFasen.Rebuild();
                RISRequestLanes.Rebuild();
                RISExtendLanes.Rebuild();
                RISLanes.BubbleSort();
                RISFasen.BubbleSort();
                RISRequestLanes.BubbleSort();
                RISExtendLanes.BubbleSort();
            }
        }

        private void OnAantalRijstrokenChanged(FaseAantalRijstrokenChangedMessage obj)
        {
            var risfc = RISFasen.FirstOrDefault(x => x.FaseCyclus == obj.Fase.Naam);
            if (risfc != null)
            {
                if (obj.AantalRijstroken > risfc.Lanes.Count)
                {
                    var i = risfc.Lanes.Count;
                    for (; i < obj.AantalRijstroken; i++)
                    {
                        var sitf = SystemITF;
                        if (HasMultipleSystemITF)
                        {
                            sitf = MultiSystemITF.FirstOrDefault()?.SystemITF;
                        }
                        var l = new RISFaseCyclusLaneDataViewModel(new RISFaseCyclusLaneDataModel() { SignalGroupName = obj.Fase.Naam, RijstrookIndex = i + 1, SystemITF = sitf });
                        risfc.Lanes.Add(l);
                    }
                }
                else if (obj.AantalRijstroken < risfc.Lanes.Count)
                {
                    var i = risfc.Lanes.Count - obj.AantalRijstroken;
                    for (var j = 0; j < i; j++)
                    {
                        if (risfc.Lanes.Any())
                        {
                            risfc.Lanes.Remove(risfc.Lanes.Last());
                        }
                    }
                    var rem = RISRequestLanes.Where(x => x.SignalGroupName == obj.Fase.Naam && x.RijstrookIndex >= obj.AantalRijstroken).ToList();
                    foreach (var r in rem) RISRequestLanes.Remove(r);
                    var rem2 = RISExtendLanes.Where(x => x.SignalGroupName == obj.Fase.Naam && x.RijstrookIndex >= obj.AantalRijstroken).ToList();
                    foreach (var r in rem2) RISExtendLanes.Remove(r);
                }
            }
            foreach (var lre in RISRequestLanes.Where(x => x.SignalGroupName == obj.Fase.Naam)) lre.UpdateRijstroken();
            foreach (var lre in RISExtendLanes.Where(x => x.SignalGroupName == obj.Fase.Naam)) lre.UpdateRijstroken();
            UpdateRISLanes();
        }

        private void OnSystemITFChanged(SystemITFChangedMessage msg)
        {
            foreach (var l in RISLanes)
            {
                if (l.SystemITF == msg.OldSystemITF) l.SystemITF = msg.NewdSystemITF;
            }
        }

        #endregion // TLCGen messaging

        #region Private Methods 

        internal void UpdateRISLanes()
        {
            RISLanes.Clear();
            foreach (var fc in RISFasen)
            {
                foreach (var l in fc.Lanes)
                {
                    RISLanes.Add(l);
                }
            }
        }

        internal void UpdateModel()
        {
            if (Controller != null && _RISModel != null)
            {
                var sitf = SystemITF;
                if (HasMultipleSystemITF)
                {
                    var msitf = MultiSystemITF.FirstOrDefault();
                    if (msitf != null)
                    {
                        sitf = msitf.SystemITF;
                    }
                }
                foreach (var fc in Controller.Fasen)
                {
                    if (RISFasen.All(x => x.FaseCyclus != fc.Naam))
                    {
                        var risfc = new RISFaseCyclusDataViewModel(
                                new RISFaseCyclusDataModel { FaseCyclus = fc.Naam });
                        for (var i = 0; i < fc.AantalRijstroken; i++)
                        {
                            risfc.Lanes.Add(new RISFaseCyclusLaneDataViewModel(new RISFaseCyclusLaneDataModel() { SignalGroupName = fc.Naam, RijstrookIndex = i + 1, SystemITF = sitf }));
                        }
                        RISFasen.Add(risfc);
                    }
                    else
                    {
                        var risfc = RISFasen.FirstOrDefault(x => x.FaseCyclus == fc.Naam);
                        if (risfc != null)
                        {
                            if (fc.AantalRijstroken > risfc.Lanes.Count)
                            {
                                var i = risfc.Lanes.Count;
                                for (; i < fc.AantalRijstroken; i++)
                                {
                                    risfc.Lanes.Add(new RISFaseCyclusLaneDataViewModel(new RISFaseCyclusLaneDataModel() { SignalGroupName = fc.Naam, RijstrookIndex = i + 1, SystemITF = sitf }));
                                }
                            }
                            else if (fc.AantalRijstroken < risfc.Lanes.Count)
                            {
                                var i = risfc.Lanes.Count - fc.AantalRijstroken;
                                for (var j = 0; j < i; j++)
                                {
                                    if (risfc.Lanes.Any())
                                        risfc.Lanes.Remove(risfc.Lanes.Last());
                                }
                                var rem = _RISModel.RISRequestLanes.Where(x => x.SignalGroupName == fc.Naam && x.RijstrookIndex >= fc.AantalRijstroken).ToList();
                                foreach (var r in rem) _RISModel.RISRequestLanes.Remove(r);
                                var rem2 = _RISModel.RISExtendLanes.Where(x => x.SignalGroupName == fc.Naam && x.RijstrookIndex >= fc.AantalRijstroken).ToList();
                                foreach (var r in rem2) _RISModel.RISExtendLanes.Remove(r);
                            }
                        }
                    }
                }
                var rems = RISFasen.Where(x => Controller.Fasen.All(x2 => x2.Naam != x.FaseCyclus)).ToList(); new List<RISFaseCyclusDataViewModel>();
                foreach (var sg in rems)
                {
                    RISFasen.Remove(sg);
                }
                RISFasen.BubbleSort();
                foreach (var lre in RISRequestLanes) lre.UpdateRijstroken();
                foreach (var lre in RISExtendLanes) lre.UpdateRijstroken();
                RISRequestLanes.BubbleSort();
                RISExtendLanes.BubbleSort();
                UpdateRISLanes();
                RaisePropertyChanged("");
            }
        }

        internal static RISFaseCyclusLaneSimulatedStationViewModel GetNewStationForSignalGroup(FaseCyclusModel sg, int LaneID, int RijstrookIndex, string systemITF)
        {
            var st = new RISFaseCyclusLaneSimulatedStationViewModel(new RISFaseCyclusLaneSimulatedStationModel());
            st.StationData.SignalGroupName = sg.Naam;
            st.StationData.RijstrookIndex = RijstrookIndex;
            st.StationData.LaneID = LaneID;
            st.StationData.SystemITF = systemITF;
            if (sg != null)
            {
                switch (sg.Type)
                {
                    case FaseTypeEnum.Auto:
                        st.Type = RISStationTypeSimEnum.PASSENGERCAR;
                        st.Flow = 200;
                        st.Snelheid = 50;
                        break;
                    case FaseTypeEnum.Fiets:
                        st.Type = RISStationTypeSimEnum.CYCLIST;
                        st.Flow = 20;
                        st.Snelheid = 15;
                        break;
                    case FaseTypeEnum.Voetganger:
                        st.Type = RISStationTypeSimEnum.PEDESTRIAN;
                        st.Flow = 20;
                        st.Snelheid = 5;
                        break;
                    case FaseTypeEnum.OV:
                        st.Type = RISStationTypeSimEnum.BUS;
                        st.Flow = 10;
                        st.Snelheid = 45;
                        break;
                }
            }
            st.StationData.SimulationData.RelatedName = st.StationData.Naam;
            st.StationData.SimulationData.FCNr = sg.Naam;
            return st;
        }

        #endregion // Private Methods 

        #region Public Methods

        #endregion // Public Methods

        #region Constructor

        public FasenRISTabViewModel() : base()
        {
            MessengerInstance.Register<FasenChangedMessage>(this, OnFasenChanged);
            MessengerInstance.Register<NameChangedMessage>(this, OnNameChanged);
            MessengerInstance.Register<FaseAantalRijstrokenChangedMessage>(this, OnAantalRijstrokenChanged);
            MessengerInstance.Register<SystemITFChangedMessage>(this, OnSystemITFChanged);
        }

        #endregion // Constructor
    }
}
