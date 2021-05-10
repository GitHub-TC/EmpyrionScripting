using System.Globalization;

namespace EmpyrionScripting.Interface
{
    public interface IEntityCultureInfo
    {
        CultureInfo CultureInfo { get; set; }
        /// <summary>
        /// Language tag from https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        /// </summary>
        string LanguageTag { get; set; }
        string i18nDefault { get; set; }
        int UTCplusTimezone { get; set; }
    }
}