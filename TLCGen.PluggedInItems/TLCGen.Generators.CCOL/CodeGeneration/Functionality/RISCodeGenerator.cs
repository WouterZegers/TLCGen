﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLCGen.Extensions;
using TLCGen.Generators.CCOL.Settings;
using TLCGen.Models;
using TLCGen.Models.Enumerations;

namespace TLCGen.Generators.CCOL.CodeGeneration.Functionality
{
    [CCOLCodePieceGenerator]
    public class RISCodeGenerator : CCOLCodePieceGeneratorBase
    {
#pragma warning disable 0649
        private CCOLGeneratorCodeStringSettingModel _prmrisastart;
        private CCOLGeneratorCodeStringSettingModel _prmrisaend;
        private CCOLGeneratorCodeStringSettingModel _prmrisvstart;
        private CCOLGeneratorCodeStringSettingModel _prmrisvend;
        private CCOLGeneratorCodeStringSettingModel _prmrislaneid;
        private CCOLGeneratorCodeStringSettingModel _schrisgeencheckopsg;
        private CCOLGeneratorCodeStringSettingModel _schris;
#pragma warning restore 0649

        public override bool HasSettings()
        {
            return true;
        }

        public override void CollectCCOLElements(ControllerModel c)
        {
            _myElements = new List<CCOLElement>();
            _myBitmapInputs = new List<CCOLIOElement>();

            var risModel = c.RISData;

            if (risModel.RISToepassen)
            {
                _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                    $"{_schrisgeencheckopsg}",
                    0,
                    CCOLElementTimeTypeEnum.SCH_type,
                    _schrisgeencheckopsg));

                _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                    $"{_schris}",
                    1,
                    CCOLElementTimeTypeEnum.SCH_type,
                    _schris));

                foreach (var l in risModel.RISFasen.SelectMany(x => x.LaneData))
                {
                    _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                            $"{_prmrislaneid}{l.SignalGroupName}_{l.RijstrookIndex}",
                            l.LaneID,
                            CCOLElementTimeTypeEnum.None,
                            _prmrislaneid, l.RijstrookIndex.ToString(), l.SignalGroupName));
                }
                foreach (var l in risModel.RISRequestLanes)
                {
                    if (l.RISAanvraag)
                    {
                        _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                            $"{_prmrisastart}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}",
                            l.AanvraagStart,
                            CCOLElementTimeTypeEnum.None,
                            _prmrisastart, l.SignalGroupName));
                    }
                }
                foreach (var l in risModel.RISRequestLanes)
                {
                    if (l.RISAanvraag)
                    {
                        _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                            $"{_prmrisaend}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}",
                            l.AanvraagEnd,
                            CCOLElementTimeTypeEnum.None,
                            _prmrisaend, l.SignalGroupName));
                    }
                }
                foreach (var l in risModel.RISExtendLanes)
                {
                    if (l.RISVerlengen)
                    {
                        _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                            $"{_prmrisvstart}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}",
                            l.VerlengenStart,
                            CCOLElementTimeTypeEnum.None,
                            _prmrisvstart, l.SignalGroupName));
                    }
                }
                foreach (var l in risModel.RISExtendLanes)
                {
                    if (l.RISVerlengen)
                    {
                        _myElements.Add(CCOLGeneratorSettingsProvider.Default.CreateElement(
                            $"{_prmrisvend}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}",
                            l.VerlengenEnd,
                            CCOLElementTimeTypeEnum.None,
                            _prmrisvend, l.SignalGroupName));
                    }
                }
            }
            var lanesSim = risModel.RISFasen.SelectMany(x => x.LaneData);
            foreach (var s in lanesSim.SelectMany(x => x.SimulatedStations))
            {
                s.StationBitmapData.Naam = s.Naam;
                var e = CCOLGeneratorSettingsProvider.Default.CreateElement(s.Naam, CCOLElementTypeEnum.Ingang, "");
                e.Dummy = true;
                _myElements.Add(e);
                _myBitmapInputs.Add(new CCOLIOElement(s.StationBitmapData, $"{_ispf}{s.Naam}") { Dummy = true });
            }
        }

        public override bool HasCCOLElements()
        {
            return true;
        }

        public override IEnumerable<CCOLElement> GetCCOLElements(CCOLElementTypeEnum type)
        {
            return _myElements.Where(x => x.Type == type);
        }

        public override bool HasCCOLBitmapInputs()
        {
            return true;
        }

        public override bool HasSimulationElements(ControllerModel c)
        {
            return c.RISData.RISToepassen;
        }

        public override IEnumerable<DetectorSimulatieModel> GetSimulationElements(ControllerModel c)
        {
            return c.RISData.RISFasen.SelectMany(x => x.LaneData).SelectMany(x => x.SimulatedStations).Select(x => x.SimulationData);
        }

        public override IEnumerable<Tuple<string, string, string>> GetFunctionLocalVariables(ControllerModel c, CCOLCodeTypeEnum type)
        {
            if (!c.RISData?.RISToepassen == true) return base.GetFunctionLocalVariables(c, type);
            return type switch
            {
                CCOLCodeTypeEnum.RegCAanvragen => new List<Tuple<string, string, string>> {new Tuple<string, string, string>("int", "fc", "")},
                CCOLCodeTypeEnum.RegCMeetkriterium => new List<Tuple<string, string, string>> {new Tuple<string, string, string>("int", "fc", "")},
                _ => base.GetFunctionLocalVariables(c, type)
            };
        }

        public override int HasCode(CCOLCodeTypeEnum type)
        {
            return type switch
            {
                CCOLCodeTypeEnum.RegCInitApplication => 110,
                CCOLCodeTypeEnum.RegCAanvragen => 110,
                CCOLCodeTypeEnum.RegCMeetkriterium => 110,
                CCOLCodeTypeEnum.RegCPostSystemApplication => 110,
                CCOLCodeTypeEnum.SysHBeforeUserDefines => 110,
                _ => 0
            };
        }

        public override string GetCode(ControllerModel c, CCOLCodeTypeEnum type, string ts)
        {
            var risModel = c.RISData;

            if (!risModel.RISToepassen) return "";

            var sb = new StringBuilder();

            var lanes = risModel.RISFasen.SelectMany(x => x.LaneData);

            switch (type)
            {
                case CCOLCodeTypeEnum.SysHBeforeUserDefines:
                    sb.AppendLine($"/* Systeem naam in het topologiebestand */");
                    sb.AppendLine($"/* ------------------------------------ */");
                    if (risModel.HasMultipleSystemITF)
                    {
                        var i = 1;
                        foreach (var sitf in risModel.MultiSystemITF)
                        {
                            sb.AppendLine($"#define SYSTEM_ITF{i} \"{sitf.SystemITF}\"");
                            ++i;
                        }
                    }
                    else
                    {
                        sb.AppendLine($"#define SYSTEM_ITF \"{risModel.SystemITF}\"");
                    }
                    sb.AppendLine();
                    sb.AppendLine($"/* Definitie lane id in het topologiebestand */");
                    sb.AppendLine($"/* ----------------------------------------- */");
                    sb.AppendLine($"#define ris_conflict_gebied    0 /* connection tussen alle ingress lanes en egress lanes */");
                    return sb.ToString();
                    
                case CCOLCodeTypeEnum.RegCInitApplication:
                    sb.AppendLine($"{ts}#ifndef NO_RIS");
                    sb.AppendLine($"{ts}{ts}#if (!defined AUTOMAAT && !defined AUTOMAAT_TEST)");
                    sb.AppendLine($"{ts}{ts}{ts}/* zet display van RIS-berichten aan in de testomgeving */");
                    sb.AppendLine($"{ts}{ts}{ts}/* ---------------------------------------------------- */");
                    sb.AppendLine($"{ts}{ts}{ts}RIS_DIPRM[RIS_DIPRM_ALL] = 1;");
                    sb.AppendLine($"{ts}{ts}#endif");
                    sb.AppendLine($"{ts}#endif");
                    return sb.ToString();
                case CCOLCodeTypeEnum.RegCAanvragen:
                    sb.AppendLine($"{ts}#ifndef NO_RIS");
                    sb.AppendLine($"{ts}/* RIS aanvragen */");
                    foreach (var l in risModel.RISRequestLanes.Where(x => x.RISAanvraag))
                    {
                        var sitf = "SYSTEM_ITF";
                        if (risModel.HasMultipleSystemITF)
                        {
                            sitf = "SYSTEM_ITF1";
                            var risfcl = risModel.RISFasen.SelectMany(x => x.LaneData).FirstOrDefault(x => x.SignalGroupName == l.SignalGroupName && x.RijstrookIndex == l.RijstrookIndex);
                            if (risfcl != null)
                            {
                                var msitf = risModel.MultiSystemITF.FirstOrDefault(x => x.SystemITF == risfcl.SystemITF);
                                if (msitf != null)
                                {
                                    var i = risModel.MultiSystemITF.IndexOf(msitf);
                                    sitf = $"SYSTEM_ITF{i + 1}";
                                }
                            }
                        }
                        sb.AppendLine($"{ts}{ts}if (ris_aanvraag({_fcpf}{l.SignalGroupName}, {sitf}, PRM[{_prmpf}{_prmrislaneid}{l.SignalGroupName}_{l.RijstrookIndex}], RIS_{l.Type}, PRM[{_prmpf}{_prmrisastart}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}], PRM[{_prmpf}{_prmrisaend}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}], SCH[{_schpf}{_schrisgeencheckopsg}])) A[{_fcpf}{l.SignalGroupName}] |= BIT10;");
                    }

                    var ovRis = c.PrioData.PrioIngrepen
                        .Where(x => x.MeldingenData.Inmeldingen.Any(x2 => x2.Type == PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISVoorwaarde) ||
                                    x.MeldingenData.Uitmeldingen.Any(x2 => x2.Type == PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISVoorwaarde))
                        .ToList();
                    var hdRis = c.PrioData.HDIngrepen.Where(x => x.RIS).ToList();
                    if (ovRis.Any() || hdRis.Any())
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{ts}/* Ris PRIO: verstuur SSM */");
                        sb.AppendLine($"{ts}#ifdef RIS_SSM");
                        foreach (var ov in ovRis)
                        {
                            //// TODO !!!
                            //#if !DEBUG
                            //TODO
                            //#endif
                            //sb.AppendLine($"{ts}ris_srm_put_signalgroup_publictransport(fc01, PRM[{_prmpf}approachid{ov.FaseCyclus}], {_prmpf}allelijnen{ov.FaseCyclus}ris, 10);");
                        }

                        foreach (var hd in hdRis)
                        {
                            sb.AppendLine($"{ts}ris_srm_put_signalgroup_emergency({_fcpf}{hd.FaseCyclus}, PRM[{_prmpf}approachid{hd.FaseCyclus}]);");
                        }

                        sb.AppendLine($"{ts}for (fc = 0; fc < FC_MAX; ++fc) {{ YM[fc] &= ~BIT15; YV[fc] &= ~BIT15; }}");

                        foreach (var ov in ovRis)
                        {
                            sb.AppendLine($"{ts}ris_verstuur_ssm(prioFC{ov.FaseCyclus}{ov.Naam});");
                        }
                        
                        foreach (var hd in hdRis)
                        {
                            sb.AppendLine($"{ts}ris_verstuur_ssm(hdFC{hd.FaseCyclus});");
                        }

                        sb.AppendLine($"{ts}#endif");
                    }

                    sb.AppendLine($"{ts}#endif");
                    return sb.ToString();
                case CCOLCodeTypeEnum.RegCMeetkriterium:
                    sb.AppendLine($"{ts}#ifndef NO_RIS");
                    sb.AppendLine($"{ts}for (fc = 0; fc < FCMAX; ++fc)");
                    sb.AppendLine($"{ts}{{");
                    sb.AppendLine($"{ts}{ts}MK[fc] &= ~BIT10;");
                    sb.AppendLine($"{ts}}}");
                    foreach (var l in risModel.RISExtendLanes.Where(x => x.RISVerlengen))
                    {
                        var sitf = "SYSTEM_ITF";
                        if (risModel.HasMultipleSystemITF)
                        {
                            sitf = "SYSTEM_ITF1";
                            var risfcl = risModel.RISFasen.SelectMany(x => x.LaneData).FirstOrDefault(x => x.SignalGroupName == l.SignalGroupName && x.RijstrookIndex == l.RijstrookIndex);
                            if (risfcl != null)
                            {
                                var msitf = risModel.MultiSystemITF.FirstOrDefault(x => x.SystemITF == risfcl.SystemITF);
                                if (msitf != null)
                                {
                                    var i = risModel.MultiSystemITF.IndexOf(msitf);
                                    sitf = $"SYSTEM_ITF{i + 1}";
                                }
                            }
                        }
                        sb.AppendLine($"{ts}{ts}if (ris_verlengen({_fcpf}{l.SignalGroupName}, {sitf}, PRM[{_prmpf}{_prmrislaneid}{l.SignalGroupName}_{l.RijstrookIndex}], RIS_{l.Type}, PRM[{_prmpf}{_prmrisvstart}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}], PRM[{_prmpf}{_prmrisvend}{l.SignalGroupName}{l.Type.GetDescription()}{l.RijstrookIndex}], SCH[{_schpf}{_schrisgeencheckopsg}])) MK[{_fcpf}{l.SignalGroupName}] |= BIT10;");
                    }
                    sb.AppendLine($"{ts}#endif");
                    return sb.ToString();
                case CCOLCodeTypeEnum.RegCPostSystemApplication:
                    sb.AppendLine($"{ts}#ifndef NO_RIS");
                    sb.AppendLine($"{ts}{ts}#if (!defined AUTOMAAT && !defined AUTOMAAT_TEST)");
                    sb.AppendLine($"{ts}{ts}{ts}/* simulatie van RIS berichten */");
                    sb.AppendLine($"{ts}{ts}{ts}/* --------------------------- */");
                    sb.AppendLine($"{ts}{ts}{ts}ris_simulation(SAPPLPROG);");
                    sb.AppendLine($"{ts}{ts}#endif");
                    sb.AppendLine($"{ts}{ts}{ts}/* RIS-Controller */");
                    sb.AppendLine($"{ts}{ts}{ts}/* -------------- */");
                    sb.AppendLine($"{ts}{ts}{ts}if (SCH[{_schpf}{_schris}]) ris_controller(SAPPLPROG, TRUE);");
                    sb.AppendLine($"{ts}#endif");
                    return sb.ToString();
                default:
                    return null;
            }
        }
    }
}
