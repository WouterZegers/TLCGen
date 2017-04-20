﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Generators.CCOL.Extensions;
using TLCGen.Models;

namespace TLCGen.Generators.CCOL.CodeGeneration
{
    public static class CCOLElementCollector
    {
        #region Static Fields

        private static List<DetectorModel> AlleDetectoren;

        #endregion // Static Fields

        #region Static Public Methods

        public static void AddAllMaxElements(CCOLElemListData[] lists)
        {
            lists[0].Elements.Add(new CCOLElement() { Define = "USMAX" });
            lists[1].Elements.Add(new CCOLElement() { Define = "ISMAX" });
            lists[2].Elements.Add(new CCOLElement() { Define = "HEMAX" });
            lists[3].Elements.Add(new CCOLElement() { Define = "MEMAX" });
            lists[4].Elements.Add(new CCOLElement() { Define = "TMMAX" });
            lists[5].Elements.Add(new CCOLElement() { Define = "CTMAX" });
            lists[6].Elements.Add(new CCOLElement() { Define = "SCHMAX" });
            lists[7].Elements.Add(new CCOLElement() { Define = "PRMMAX" });
        }

        public static CCOLElemListData[] CollectAllCCOLElements(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            AlleDetectoren = new List<DetectorModel>();
            foreach (FaseCyclusModel fcm in controller.Fasen)
            {
                foreach (DetectorModel dm in fcm.Detectoren)
                    AlleDetectoren.Add(dm);
            }
            foreach (DetectorModel dm in controller.Detectoren)
                AlleDetectoren.Add(dm);

            CCOLElemListData[] lists = new CCOLElemListData[8];

            lists[0] = CollectAllUitgangen(controller, pgens);
            lists[1] = CollectAllIngangen(controller, pgens);
            lists[2] = CollectAllHulpElementen(controller, pgens);
            lists[3] = CollectAllGeheugenElementen(controller, pgens);
            lists[4] = CollectAllTimers(controller, pgens);
            lists[5] = CollectAllCounters(controller, pgens);
            lists[6] = CollectAllSchakelaars(controller, pgens);
            lists[7] = CollectAllParameters(controller, pgens);

            return lists;
        }

        #endregion // Static Public Methods

        #region Static Private Methods

        private static CCOLElemListData CollectAllUitgangen(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "US_code";

            // Segment display elements
            foreach(var item in controller.Data.SegmentenDisplayBitmapData)
            {
                var _item = item.BitmapData as IOElementModel;
                data.Elements.Add(new CCOLElement() { Define = _item.GetBitmapCoordinaatOutputDefine(), Naam = item.Naam });
            }

            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Uitgang))
                    {
                        data.Elements.Add(i);
                    }
                }
            }
            
            return data;
        }

        private static CCOLElemListData CollectAllIngangen(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "IS_code";
            
            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Ingang))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            return data;
        }

        private static CCOLElemListData CollectAllHulpElementen(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "H_code";

            // Collect everything

            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.HulpElement))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            if (data.Elements.Count == 0)
            {
                data.Elements.Add(new CCOLElement() { Define = "hedummy", Naam = "dummy" });
            }

            return data;
        }

        private static CCOLElemListData CollectAllGeheugenElementen(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "MM_code";

            data.Elements.Add(new CCOLElement() { Define = "mperiod", Naam = "PERIOD" });

            // Collect everything
            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.GeheugenElement))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            return data;
        }

        private static CCOLElemListData CollectAllTimers(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "T_code";
            data.CCOLSetting = "T_max";
            data.CCOLTType = "T_type";

            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Timer))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            if (data.Elements.Count == 0)
            {
                data.Elements.Add(new CCOLElement() { Define = "tdummy", Naam = "dummy" });
            }

            return data;
        }

        private static CCOLElemListData CollectAllSchakelaars(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "SCH_code";
            data.CCOLSetting = "SCH";
            
            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Schakelaar))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            return data;
        }

        private static CCOLElemListData CollectAllCounters(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "C_code";
            data.CCOLSetting = "C_max";
            data.CCOLTType = "C_type";

            // Collect everything
            foreach (var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Counter))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            if (data.Elements.Count == 0)
            {
                data.Elements.Add(new CCOLElement() { Define = "ctdummy", Naam = "dummy" });
            }

            return data;
        }

        private static CCOLElemListData CollectAllParameters(ControllerModel controller, List<ICCOLCodePieceGenerator> pgens)
        {
            CCOLElemListData data = new CCOLElemListData();

            data.CCOLCode = "PRM_code";
            data.CCOLSetting = "PRM";
            data.CCOLTType = "PRM_type";

            // Collect everything
            data.Elements.Add(new CCOLElement() { Define = "prmfb", Naam = "FB", Instelling = controller.Data.Fasebewaking, TType = CCOLElementTimeTypeEnum.TS_type });

            foreach(var pgen in pgens)
            {
                if (pgen.HasCCOLElements())
                {
                    foreach (var i in pgen.GetCCOLElements(CCOLElementTypeEnum.Parameter))
                    {
                        data.Elements.Add(i);
                    }
                }
            }

            // Maxgroentijden
            foreach (GroentijdenSetModel mgset in controller.GroentijdenSets)
            {
                foreach (GroentijdModel mgm in mgset.Groentijden)
                {
                    if (!mgm.Waarde.HasValue)
                        continue;

                    FaseCyclusModel thisfcm = null;
                    foreach (FaseCyclusModel fcm in controller.Fasen)
                    {
                        if (fcm.Naam == mgm.FaseCyclus)
                        {
                            thisfcm = fcm;
                            break;
                        }
                    }
                    if (thisfcm == null)
                        throw new NotImplementedException($"Maxgroentijd voor niet bestaande fase {mgm.FaseCyclus} opgegeven.");

                    data.Elements.Add(new CCOLElement()
                    {
                        Define = $"prm{mgset.Naam.ToLower()}{thisfcm.Naam}",
                        Naam = $"mk{mgset.Naam.ToLower()}{thisfcm.Naam}",
                        Instelling = mgm.Waarde.Value,
                        TType = CCOLElementTimeTypeEnum.TE_type
                    });
                }
            }

            return data;
        }

        #endregion // Static Private Methods

    }
}
