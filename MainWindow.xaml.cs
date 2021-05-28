using FanslationStudio.Domain;
using FanslationStudio.ScriptToTranslate;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FanslationStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();

            _config = Config.LoadConfig();
            _config.WriteConfig();

            CreateDefaultHLTSProject(_config);

            var project = Project.LoadProjectFile(@"X:\FanslationStudioWorkshop\HLTS.project");

            project.CalculateFolders(_config);

            foreach (var version in project.Versions)
            {
                version.InitialiseProductVersion();
                version.LoadTranslationsThatExist(project);
                version.LoadTranslationsFromRaws(project);

                //if (version.OldGameTranslator)
                //    version.MigrateGtRaws(project);
            }

            //Migrate 1.29 to 1.33
            MergeVersionService.MergeOldVersion(_config, project, project.Versions[0], project.Versions[1]);
        }

        /// <summary>
        /// HLTS has all the text files loaded in AssetBundles\config
        /// 
        /// To export it you need AssetStudio from (https://github.com/Perfare/AssetStudio)
        /// - Only need the assets in \textfiles since all the other stuff is code
        /// - Then weed out all the config ones that dont actually contain text to translate
        /// </summary>
        public void CreateDefaultHLTSProject(Config config)
        {
            var project = new Project()
            {
                Name = "HLTS",

                Versions = new List<ProjectVersion>()
                {
                    new ProjectVersion()
                    {
                        Version = "1.29",
                        RawInputFolder = @"X:\repos\GameTranslator\GameTranslator\Hlts\input1.29",
                        //OldGameTranslator = true,
                        //GameTranslatorSource = @"X:\repos\GameTranslator\GameTranslator\Hlts\finaloutput1.29",
                    },
                    new ProjectVersion()
                    {
                        Version = "1.33",
                        RawInputFolder = @"X:\repos\GameTranslator\GameTranslator\Hlts\input1.33",
                    },
                },

                ScriptsToTranslate = new[]
                {
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\AchievementItem.txt",
                        SplitIndexes = new [] { 1, 2, 12 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\AreaItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\BufferItem.txt",
                        SplitIndexes = new [] { 1, 2 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\CharacterPropertyItem.txt",
                        SplitIndexes = new [] { 5 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\CreatePlayerQuestionItem.txt",
                        SplitIndexes = new [] { 1 },
                        OverrideFontSize = 18,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\DefaultSkillItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\DefaultTalentItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\EquipInventoryItem.txt",
                        SplitIndexes = new [] { 1, 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\EventCubeItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\HelpItem.txt",
                        SplitIndexes = new [] { 3, 4 },
                        OverrideFontSize = 20,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NicknameItem.txt",
                        SplitIndexes = new [] { 1, 2 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NormalBufferItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NormalInventoryItem.txt",
                        SplitIndexes = new [] { 1, 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NpcItem.txt",
                        SplitIndexes = new [] { 1 },
                        OverrideFontSize = 18,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NpcTalkItem.txt",
                        SplitIndexes = new [] { 6 },
                        OverrideFontSize = 20,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\ReforgeItem.txt",
                        SplitIndexes = new [] { 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\SkillNodeItem.txt",
                        SplitIndexes = new [] { 1, 2 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\SkillTreeItem.txt",
                        SplitIndexes = new [] { 1, 3 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\StringTableItem.txt",
                        SplitIndexes = new [] { 1 },
                        //OverrideFontSize = 22,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\TalentItem.txt",
                        SplitIndexes = new [] { 1, 2 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\TeleporterItem.txt",
                        SplitIndexes = new [] { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\QuestItem.txt",
                        SplitIndexes = new [] { 1, 3 },
                        OverrideFontSize = 18,
                    },
                }
            };

            project.CalculateFolders(config);
            project.WriteProjectFile();
        }
    }
}
