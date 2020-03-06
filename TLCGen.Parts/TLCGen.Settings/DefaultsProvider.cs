﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using TLCGen.Models;
using TLCGen.Models.Enumerations;

namespace TLCGen.Settings
{
    public class DefaultsProvider : IDefaultsProvider
    {
        #region Fields

        private static readonly object Locker = new object();
        private static IDefaultsProvider _default;

        #endregion // Fields

        #region Properties

        public event EventHandler DefaultsChanged;

        public ControllerModel Controller { get; set; }

        private TLCGenDefaultsModel _defaults;
        public TLCGenDefaultsModel Defaults
        {
            get => _defaults;
            set
            {
                _defaults = value;
                DefaultsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static IDefaultsProvider Default
        {
            get
            {
                if (_default != null) return _default;
                lock (Locker)
                {
                    if (_default == null)
                    {
                        _default = new DefaultsProvider();
                    }
                }
                return _default;
            }
        }

        #endregion // Properties

        #region Public Methods

        public static void OverrideDefault(IDefaultsProvider provider)
        {
            _default = provider;
        }

        public string GetVehicleTypeAbbreviation(PrioIngreepVoertuigTypeEnum type)
        {
            var t = Defaults.VehicleTypes.FirstOrDefault(x => x.VehicleType == type);
            return t?.Setting ?? "UNKNOWN_VEHICLE_TYPE";
        }

        
        public string GetMeldingShortcode(PrioIngreepInUitMeldingModel melding)
        {
            switch (melding.Type)
            {
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.Detector:
                    return "det";
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.KARMelding:
                    return "kar";
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.SelectieveDetector:
                    return "sd";
                case PrioIngreepInUitMeldingVoorwaardeTypeEnum.VecomViaDetector:
                    return "vecio";
                //case PrioIngreepInUitMeldingVoorwaardeTypeEnum.RISInput:
                //    return "ris";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion // Public Methods

        #region Private Methods

        private void CopyAllProperties<T>(T from, T to, bool onlyvalues = true)
        {
            var type = from.GetType();
            var props = type.GetProperties();
            foreach (PropertyInfo property in props)
            {
                var att = (HasDefaultAttribute)property.GetCustomAttribute(typeof(HasDefaultAttribute));
                var attMn = (ModelNameAttribute)property.GetCustomAttribute(typeof(ModelNameAttribute));
                var attVn = (VissimNameAttribute)property.GetCustomAttribute(typeof(VissimNameAttribute));
                if ((att == null || att.HasDefault == true) && 
                    attMn == null && attVn == null)
                {
                    if (property.PropertyType.IsValueType || property.PropertyType == typeof(string) ||
                        !onlyvalues)
                    {
                        object propValue = property.GetValue(from);
                        property.SetValue(to, propValue);
                    }
                }
            }
        }

        private FaseTypeEnum GetFaseCyclusTypeFromName(string name)
        {
            if (Controller == null)
                return FaseTypeEnum.Auto;

            foreach(var fc in Controller.Fasen)
            {
                if(fc.Naam == name)
                {
                    return fc.Type;
                }
            }
            return FaseTypeEnum.Auto;
        }

        private DetectorTypeEnum GetDetectorTypeFromName(string name)
        {
            if (Controller == null)
                return DetectorTypeEnum.Kop;

            foreach (var fc in Controller.Fasen)
            {
                foreach (var d in fc.Detectoren)
                {
                    if (d.Naam == name)
                    {
                        return d.Type;
                    }
                }
            }
            foreach (var d in Controller.Detectoren)
            {
                if (d.Naam == name)
                {
                    return d.Type;
                }
            }
            return DetectorTypeEnum.Kop;
        }

        #endregion // Private Methods

        #region IDefaultsProvider

        public void SetDefaultsOnModel(object model, string selector1 = null, string selector2 = null, bool onlyvalues = true)
        {
            var type = model.GetType();
            var typename = type.FullName + "," + type.Assembly.GetName().Name;

            var defs = Defaults.Defaults.Where(x => x.DataType == typename);

            if (defs == null || !defs.Any())
                return;

            if(defs.Count() == 1)
            {
                CopyAllProperties(defs.First().Data, model, onlyvalues);
            }
            else if(defs.Count() > 1)
            {
                bool found = false;
                if (selector1 == null && selector2 == null)
                {
                    CopyAllProperties(defs.First().Data, model, onlyvalues);
                    found = true;
                    //MessageBox.Show("Fout bij toepassen default instellingen voor " + type.Name + ":\nGeen selector bekend bij meerdere beschikbare defaults.", "Fout bij toepassen defaults");
                }
                else
                {
                    foreach (var def in defs)
                    {
                        if ((selector1 == null || selector1 == def.Selector1) &&
                            (selector2 == null || def.Selector2 == null || selector2 == def.Selector2))
                        {
                            CopyAllProperties(def.Data, model, onlyvalues);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    //CopyAllValueProperties(defs.First().Data, model);
                    //MessageBox.Show("Fout bij toepassen default instellingen voor " + type.Name + ":\nGeen passende default gevonden bij: " + selector1 + "/" + selector2 + ".", "Fout bij toepassen defaults");
                }
            }
        }

        public void LoadSettings()
        {
            Defaults = null;
            if (!string.IsNullOrWhiteSpace(SettingsProvider.Default.Settings.DefaultsFileLocation) &&
                File.Exists(SettingsProvider.Default.Settings.DefaultsFileLocation))
            {
                try
                {
                    Defaults = DeserializeDefaultsFile(SettingsProvider.Default.Settings.DefaultsFileLocation);
                    foreach (var d in Defaults.Defaults) d.Editable = true;
                }
                catch
                {
                    Defaults = null;
                    MessageBox.Show("Het ingestelde defaults bestand heeft een onjuist formaat:\n\n" +
                                    SettingsProvider.Default.Settings.DefaultsFileLocation +
                                    "\n\nDe meegeleverde defaults worden geladen", "Onjuist formaat defaults bestand");
					SettingsProvider.Default.Settings.DefaultsFileLocation = null;
				}
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(SettingsProvider.Default.Settings.DefaultsFileLocation) &&
                    !File.Exists(SettingsProvider.Default.Settings.DefaultsFileLocation))
                {
                    MessageBox.Show("Het ingestelde defaults bestand is niet gevonden:\n\n" +
                                    SettingsProvider.Default.Settings.DefaultsFileLocation +
                                    "\n\nDe meegeleverde defaults worden geladen", "Kan defaults bestand niet vinden");
                }
            }

            var defsetfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings\\tlcgendefaultdefaults.xml");
            if (Defaults != null && File.Exists(defsetfile))
            {
                foreach(var vtgT in Enum.GetValues(typeof(PrioIngreepVoertuigTypeEnum)))
                {
                    var vtgTSet = Defaults.VehicleTypes.FirstOrDefault(x => x.VehicleType == (PrioIngreepVoertuigTypeEnum)vtgT);
                    if (vtgTSet == null)
                    {
                        switch ((PrioIngreepVoertuigTypeEnum)vtgT)
                        {
                            case PrioIngreepVoertuigTypeEnum.Tram:
                                Defaults.VehicleTypes.Add(new VehicleTypeAbbreviationModel { VehicleType = (PrioIngreepVoertuigTypeEnum)vtgT, Default = "tram", Setting = "tram" });
                                break;
                            case PrioIngreepVoertuigTypeEnum.Bus:
                                Defaults.VehicleTypes.Add(new VehicleTypeAbbreviationModel { VehicleType = (PrioIngreepVoertuigTypeEnum)vtgT, Default = "bus", Setting = "bus" });
                                break;
                            case PrioIngreepVoertuigTypeEnum.Fiets:
                                Defaults.VehicleTypes.Add(new VehicleTypeAbbreviationModel { VehicleType = (PrioIngreepVoertuigTypeEnum)vtgT, Default = "fts", Setting = "fts" });
                                break;
                            case PrioIngreepVoertuigTypeEnum.Vrachtwagen:
                                Defaults.VehicleTypes.Add(new VehicleTypeAbbreviationModel { VehicleType = (PrioIngreepVoertuigTypeEnum)vtgT, Default = "vrw", Setting = "vrw" });
                                break;
                            case PrioIngreepVoertuigTypeEnum.NG:
                                Defaults.VehicleTypes.Add(new VehicleTypeAbbreviationModel { VehicleType = (PrioIngreepVoertuigTypeEnum)vtgT, Default = "alg", Setting = "alg" });
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                try
                {
                    var message = "";
                    var message2 = "";
                    var defaultDefaults = DeserializeDefaultsFile(defsetfile);
                    foreach (var d in defaultDefaults.Defaults)
                    {
                        bool found = false;
                        foreach (var d2 in Defaults.Defaults)
                        {
                            if (d.DataType == d2.DataType &&
                                d.Category == d2.Category &&
                                d.Selector1 == d2.Selector1 &&
                                d.Selector2 == d2.Selector2)
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            message += d.DefaultName + "; ";
							if (!string.IsNullOrWhiteSpace(SettingsProvider.Default.Settings.DefaultsFileLocation) &&
								File.Exists(SettingsProvider.Default.Settings.DefaultsFileLocation))
							{
								d.Editable = true;
							}
							Defaults.Defaults.Add(d);
                        }
                    }
                    var remDs = new List<TLCGenDefaultModel>();
                    foreach (var d in Defaults.Defaults)
                    {
                        bool found = false;
                        foreach (var d2 in defaultDefaults.Defaults)
                        {
                            if (d.DataType == d2.DataType &&
                                d.Category == d2.Category &&
                                d.Selector1 == d2.Selector1 &&
                                d.Selector2 == d2.Selector2)
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            message2 += d.DefaultName + "; ";
                            remDs.Add(d);
                        }
                    }
                    foreach (var d in remDs)
                    {
                        Defaults.Defaults.Remove(d);
                    }
                    if (message.Length > 0 || message2.Length > 0)
                    {
                        var s = "";
                        if (message.Length > 0) s += "Defaults added: " + message;
                        if (message2.Length > 0)
                        {
                            if (message.Length > 0) s += Environment.NewLine;
                            s += "Defaults removed: " + message2;
                        }
                        MessageBox.Show(s, "Defaults updated");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error occured while loading the defaults:\n " + e.ToString() + "\nPlease report this.", "Error while loading defaults");
                }
            }
            else if(File.Exists(defsetfile))
            {
                Defaults = DeserializeDefaultsFile(defsetfile);
                foreach (var d in Defaults.Defaults) d.Editable = false;
            }
        }

        private TLCGenDefaultsModel DeserializeDefaultsFile(string filename)
	    {
		    var defaults = new TLCGenDefaultsModel();
		    var doc = new XmlDocument();
		    var reader =
			    XmlReader.Create(
				    filename,
				    new XmlReaderSettings() {IgnoreComments = true});
		    doc.Load(reader);
			reader.Dispose();
		    var defs = doc.DocumentElement?.SelectSingleNode("Defaults");
		    if (defs == null) return null;
		    foreach (XmlNode def in defs.ChildNodes)
		    {
			    var x = def.SelectSingleNode("DataType");
			    var t = x.InnerText;
                // correct old files
			    if (t.EndsWith(",TLCGen.Model"))
			    {
				    t = t.Replace(",TLCGen.Model", ",TLCGen.Dependencies");
			    }
			    var type = Type.GetType(t);
			    var xRoot = new XmlRootAttribute
			    {
				    ElementName = "Data",
				    IsNullable = true
			    };
			    if (type == null) continue;
			    var ser = new XmlSerializer(type, xRoot);
			    // http://stackoverflow.com/questions/1563473/xmlnode-to-objects
			    var o = ser.Deserialize(new XmlNodeReader(def.SelectSingleNode("Data")));
			    var item = new TLCGenDefaultModel
			    {
				    DefaultName = def.SelectSingleNode("DefaultName")?.InnerText,
				    DataType = def.SelectSingleNode("DataType")?.InnerText,
				    Category = def.SelectSingleNode("Category")?.InnerText
			    };
			    var n1 = def.SelectSingleNode("Selector1");
			    if (n1 != null)
			    {
				    item.Selector1 = n1.InnerText;
			    }
			    var n2 = def.SelectSingleNode("Selector2");
			    if (n2 != null)
			    {
				    item.Selector2 = n2.InnerText;
			    }
			    item.Data = o;
			    defaults.Defaults.Add(item);
		    }
		    return defaults;
	    }

	    private object ConvertNode(XmlNode node, Type t)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();
            stm.Position = 0;
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(t);
            var result = ser.Deserialize(stm);
            return result;
        }

        public void SaveSettings()
        {
            if (!string.IsNullOrWhiteSpace(SettingsProvider.Default.Settings.DefaultsFileLocation))
            {
                if (File.Exists(SettingsProvider.Default.Settings.DefaultsFileLocation))
                {
                    File.Delete(SettingsProvider.Default.Settings.DefaultsFileLocation);
                }

                using (FileStream fs = new FileStream(SettingsProvider.Default.Settings.DefaultsFileLocation, FileMode.Create, FileAccess.Write))
                {
                    List<Type> et = new List<Type>();
                    foreach (var def in Defaults.Defaults)
                    {
                        if (!et.Contains(def.Data.GetType()))
                        {
                            et.Add(def.Data.GetType());
                        }
                    }

                    try
                    {
                        var serializer = new XmlSerializer(typeof(TLCGenDefaultsModel), et.ToArray());
                        serializer.Serialize(fs, Defaults);
                        fs.Close();
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Fout bij het opslaan van de TLCGen defaults:\n" + e.ToString(), "Fout bij opslaan defaults");
                    }
                }
            }
        }

        #endregion // IDefaultsProvider
    }
}
