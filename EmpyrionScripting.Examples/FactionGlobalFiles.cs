using Eleon.Modding;
using EmpyrionScripting;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using System.Collections.Generic;
using System.Linq;

public class ModMain
{
    private static Dictionary<string, string> allfiles = new Dictionary<string, string>();
    public static void Main(IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;
        if (!root.Running) return;

        var infoOutLcds = root.CsRoot.GetDevices<ILcd>(root.CsRoot.Devices(root.E.S, "FileIO"));

        var writerLcds = root.CsRoot.GetDevices<ILcd>(root.CsRoot.Devices(root.E.S, "Write:*"));
        writerLcds.ForEach(lcd => {
          var filetag = lcd.CustomName.Split(':', 2)[1];
          var filekey = $"{root.E.Faction.Id}:{filetag}";
          files[filekey] = lcd.GetText();
          WriteTo(infoOutLcds, $"{lcd.CustomName} saved.");
        })

        var readerLcds = root.CsRoot.GetDevices<ILcd>(root.CsRoot.Devices(root.E.S, "Read:*"));
        readerLcds.ForEach(lcd => {
          var filetag = lcd.CustomName.Split(':', 2)[1];
          var filekey = $"{root.E.Faction.Id}:{filetag}";
          string towrite = "";
          if (files.TryGetValue(filekey, out towrite)) {
            lcd.SetText(towrite);
            WriteTo(infoOutLcds, $"{lcd.CustomName} loaded.");
          } else {
            WriteTo(infoOutLcds, $"{lcd.CustomName} not found.");
          }
        });
    }

    private static void WriteTo(ILcd[] lcds, string text)
    {
        lcds.ForEach(L => L.SetText($"{text}\n{L.GetText()}"));
    }
}
