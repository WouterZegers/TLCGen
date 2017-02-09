﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Models.Enumerations;

namespace TLCGen.Models
{
    [Serializable]
    [RefersToSignalGroup("FaseCyclus")]
    public class RichtingGevoeligVerlengModel
    {
        #region Properties

        public string FaseCyclus { get; set; }
        public string VanDetector { get; set; }
        public string NaarDetector { get; set; }
        public int MaxTijdsVerschil { get; set; }
        public int VerlengTijd { get; set; }
        public int HiaatTijd { get; set; }
        public RichtingGevoeligVerlengenTypeEnum TypeVerlengen { get; set; }

        #endregion // Properties
    }
}
