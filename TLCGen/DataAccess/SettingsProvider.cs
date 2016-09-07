﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Models;
using TLCGen.Models.Enumerations;
using TLCGen.Models.Settings;
using TLCGen.Utilities;

namespace TLCGen.DataAccess
{
    public static class SettingsProvider
    {
        private static TLCGenSettingsModel _Settings;
        public static TLCGenSettingsModel Settings
        {
            get { return _Settings; }
            set
            {
                _Settings = value;
            }
        }

        #region Serialize Methods

        public static void LoadApplicationSettings()
        {
            string settingsfile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TLCGenSettings.xml");
            if (File.Exists(settingsfile))
            {
                TLCGen.DataAccess.DeserializeT<TLCGenSettingsModel> deserializer = new TLCGen.DataAccess.DeserializeT<TLCGenSettingsModel>();
                _Settings = deserializer.DeSerialize(settingsfile);
            }
            if (_Settings == null)
                _Settings = new TLCGenSettingsModel();
        }

        public static void SaveApplicationSettings()
        {
            string settingsfile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TLCGenSettings.xml");
            TLCGen.DataAccess.SerializeT<TLCGenSettingsModel> serializer = new TLCGen.DataAccess.SerializeT<TLCGenSettingsModel>();
            serializer.Serialize(settingsfile, _Settings);
        }

        #endregion Serialize Methods

        #region Settings Provider Methods

        public static string GetFaseCyclusDefinePrefix()
        {
            return Settings.DefaultControllerSettings.PreFixSettings.FaseCyclusDefinePrefix;
        }

        public static string GetDetectorDefinePrefix()
        {
            return Settings.DefaultControllerSettings.PreFixSettings.DetectorDefinePrefix;
        }

        public static void ApplyDefaultFaseCyclusSettings(FaseCyclusModel fcm, string define)
        {
            fcm.Type = FaseCyclusUtilities.GetFaseTypeFromDefine(define);
            ApplyDefaultFaseCyclusSettings(fcm, fcm.Type);
        }

        public static void ApplyDefaultFaseCyclusSettings(FaseCyclusModel fcm, FaseTypeEnum type)
        {
            switch (type)
            {
                case FaseTypeEnum.Auto:
                    fcm.Kopmax = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.Kopmax;
                    fcm.TFG = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TFG;
                    fcm.TGG = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TGG;
                    fcm.TGG_min = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TGG_min;
                    fcm.TGL = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TGL;
                    fcm.TGL_min = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TGL_min;
                    fcm.TRG = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TRG;
                    fcm.TRG_min = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.TRG_min;
                    fcm.VasteAanvraag = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.VasteAanvraag;
                    fcm.Wachtgroen = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.Wachtgroen;
                    fcm.Meeverlengen = Settings.DefaultFaseCyclusSettings.DefaultAutoModel.Meeverlengen;
                    break;
                case FaseTypeEnum.Fiets:
                    fcm.Kopmax = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.Kopmax;
                    fcm.TFG = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TFG;
                    fcm.TGG = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TGG;
                    fcm.TGG_min = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TGG_min;
                    fcm.TGL = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TGL;
                    fcm.TGL_min = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TGL_min;
                    fcm.TRG = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TRG;
                    fcm.TRG_min = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.TRG_min;
                    fcm.VasteAanvraag = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.VasteAanvraag;
                    fcm.Wachtgroen = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.Wachtgroen;
                    fcm.Meeverlengen = Settings.DefaultFaseCyclusSettings.DefaultFietsModel.Meeverlengen;
                    break;
                case FaseTypeEnum.Voetganger:
                    fcm.Kopmax = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.Kopmax;
                    fcm.TFG = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TFG;
                    fcm.TGG = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TGG;
                    fcm.TGG_min = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TGG_min;
                    fcm.TGL = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TGL;
                    fcm.TGL_min = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TGL_min;
                    fcm.TRG = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TRG;
                    fcm.TRG_min = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.TRG_min;
                    fcm.VasteAanvraag = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.VasteAanvraag;
                    fcm.Wachtgroen = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.Wachtgroen;
                    fcm.Meeverlengen = Settings.DefaultFaseCyclusSettings.DefaultVoetgangerModel.Meeverlengen;
                    break;
                case FaseTypeEnum.OV:
                    fcm.Kopmax = Settings.DefaultFaseCyclusSettings.DefaultOVModel.Kopmax;
                    fcm.TFG = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TFG;
                    fcm.TGG = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TGG;
                    fcm.TGG_min = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TGG_min;
                    fcm.TGL = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TGL;
                    fcm.TGL_min = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TGL_min;
                    fcm.TRG = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TRG;
                    fcm.TRG_min = Settings.DefaultFaseCyclusSettings.DefaultOVModel.TRG_min;
                    fcm.VasteAanvraag = Settings.DefaultFaseCyclusSettings.DefaultOVModel.VasteAanvraag;
                    fcm.Wachtgroen = Settings.DefaultFaseCyclusSettings.DefaultOVModel.Wachtgroen;
                    fcm.Meeverlengen = Settings.DefaultFaseCyclusSettings.DefaultOVModel.Meeverlengen;
                    break;
            }
        }

        #endregion // Settings Provider Methods

        #region Constructor

        static SettingsProvider()
        {
        }

        #endregion // Constructor
    }
}
