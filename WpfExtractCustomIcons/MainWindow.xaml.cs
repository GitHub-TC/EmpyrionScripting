using EcfParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WpfExtractCustomIcons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int TokenIdSeperator = 100000;

        public IReadOnlyDictionary<string, int> BlockIdMapping { get; set; }
        public EcfFile BlocksConfig_Ecf { get; private set; }
        public EcfFile ItemsConfig_Ecf { get; private set; }
        public EcfFile TokenConfig_Ecf { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BlockIdMapping = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(IdMappingFilename.Text));

            Directory.CreateDirectory(ExtractToFolder.Text);

            BlocksConfig_Ecf = ReadEcf("BlocksConfig.ecf", true, B => { });
            ItemsConfig_Ecf  = ReadEcf("ItemsConfig.ecf", true, B => { });
            TokenConfig_Ecf  = ReadEcf("TokenConfig.ecf", false, B => {
                var idAttr = B.Values?.FirstOrDefault(A => A.Key == "Id");
                if (idAttr != null && idAttr?.Value is int id) B.Values["Id"] = id * TokenIdSeperator;
            });

            BlocksConfig_Ecf.Blocks?.ForEach(B => CheckCustomIcon(B, BlocksConfig_Ecf.Blocks));
            ItemsConfig_Ecf.Blocks?.ForEach(B => CheckCustomIcon(B, ItemsConfig_Ecf.Blocks));
            TokenConfig_Ecf.Blocks?.ForEach(B => CheckCustomIcon(B, TokenConfig_Ecf.Blocks));

            void CheckCustomIcon(EcfBlock B, List<EcfBlock> blocks)
            {
                string itemName = null;
                string iconName = null;
                string iconFile = null;
                try
                {
                    if (!(B.Values?.FirstOrDefault(A => A.Key == "Id").Value is int id)) return;

                    do
                    {
                        itemName = B.Values?.FirstOrDefault(A => A.Key == "Name").Value?.ToString();
                        iconName = B.Attr?.FirstOrDefault(A => A.Name == "CustomIcon")?.Value?.ToString() ?? itemName;
                        if (string.IsNullOrEmpty(iconName)) return;

                        iconFile = Path.Combine(IconFolder.Text, Path.GetFileNameWithoutExtension(iconName) + ".png");
                        if (!File.Exists(iconFile))
                        {
                            B = B.Values.TryGetValue("Ref", out var refBlockName) 
                                    ? blocks.FirstOrDefault(refB => refB.Values.TryGetValue("Name", out var refBName) && refBName?.ToString() == refBlockName.ToString()) 
                                    : null;
                        }

                    } while (!File.Exists(iconFile) && B != null);

                    if (!File.Exists(iconFile)) return;

                    File.Copy(iconFile, Path.Combine(ExtractToFolder.Text, $"{id}.png"), true);
                }
                catch (Exception error)
                {
                    Logging.Text = $"BlocksConfig_Ecf:{iconName} [{B?.Attr?.FirstOrDefault(A => A.Name == "Id")?.Value}]:{error}\n{Logging.Text}";
                }
            }
        }

        private EcfFile ReadEcf(string filename, bool mapIds, Action<EcfBlock> mapId)
        {
            var result = new EcfFile();
            try
            {
                result = Parse.Deserialize(File.ReadAllLines(Path.Combine(ECFFolder.Text, filename)));
                result.Blocks.ForEach(mapId);
                if (BlockIdMapping != null) Parse.ReplaceWithMappedIds(result, BlockIdMapping);
            }
            catch (Exception error) {
                Logging.Text = $"ReadEcf:{filename}:{error}\n{Logging.Text}";
            }

            return result;
        }

    }
}
