using EmpyrionScripting.Interface;
using Newtonsoft.Json;
using System.Globalization;

namespace EmpyrionScripting
{
    public class EntityCultureInfo : IEntityCultureInfo
    {
        [JsonIgnore]
        public CultureInfo CultureInfo { 
            get{ 
                if(_CultureInfo == null)
                {
                    try  { _CultureInfo = CreateFormatCulture(CultureInfo.CreateSpecificCulture(LanguageTag)); }
                    catch{ _CultureInfo = FormatCulture;                                                                            }
                }
                return _CultureInfo; 
            } 
            set => _CultureInfo = value; 
        }
        private CultureInfo _CultureInfo;

        public string LanguageTag { get; set; } = FormatCulture.Name;
        public string i18nDefault { get; set; } = "English";
        public int UTCplusTimezone { get; set; } = 0;

        public static CultureInfo FormatCulture { get; } = CreateFormatCulture(CultureInfo.CurrentUICulture);

        public static CultureInfo CreateFormatCulture(CultureInfo currentUICulture)
        {
            var c = (CultureInfo)currentUICulture.Clone();

            c.NumberFormat.PercentSymbol          = "%";  // z.B. fr-FR benutzt ein von EGS nicht darstellbares Zeichen
            c.NumberFormat.PercentNegativePattern = 1;
            c.NumberFormat.PercentPositivePattern = 1;

            return c;
        }
    }
}