using FanslationStudio.Domain;
using FanslationStudio.ScriptToTranslate;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Services
{
    public static class DefaultProjectCreatorService
    {
        /// <summary>
        /// HLTS has all the text files loaded in AssetBundles\config
        /// 
        /// To export it you need AssetStudio from (https://github.com/Perfare/AssetStudio)
        /// - Only need the assets in \textfiles since all the other stuff is code
        /// - Then weed out all the config ones that dont actually contain text to translate
        /// </summary>
        public static void CreateDefaultHLTSProject(Config config)
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
                        OldGameTranslator = true,
                        GameTranslatorSource = @"X:\repos\GameTranslator\GameTranslator\Hlts\finaloutput1.29",
                    },
                    new ProjectVersion()
                    {
                        Version = "1.33",
                        RawInputFolder = @"X:\repos\GameTranslator\GameTranslator\Hlts\input1.33",
                    },
                },

                ScriptsToTranslate = new List<IScriptToTranslate>
                {
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\AchievementItem.txt",
                        SplitIndexes = new List<int> { 1, 2, 12 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\AreaItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\BufferItem.txt",
                        SplitIndexes = new List<int> { 1, 2 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\CharacterPropertyItem.txt",
                        SplitIndexes = new List<int> { 5 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\CreatePlayerQuestionItem.txt",
                        SplitIndexes = new List<int> { 1 },
                        //OverrideFontSize = 18,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\DefaultSkillItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\DefaultTalentItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\EquipInventoryItem.txt",
                        SplitIndexes = new List<int> { 1, 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\EventCubeItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\HelpItem.txt",
                        SplitIndexes = new List<int> { 3, 4 },
                        //OverrideFontSize = 20,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NicknameItem.txt",
                        SplitIndexes = new List<int> { 1, 2 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NormalBufferItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NormalInventoryItem.txt",
                        SplitIndexes = new List<int> { 1, 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NpcItem.txt",
                        SplitIndexes = new List<int> { 1 },
                        //OverrideFontSize = 16,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\NpcTalkItem.txt",
                        SplitIndexes = new List<int> { 6 },
                        OverrideFontSize = 16,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\ReforgeItem.txt",
                        SplitIndexes = new List<int> { 3 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\SkillNodeItem.txt",
                        SplitIndexes = new List<int> { 1, 2 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\SkillTreeItem.txt",
                        SplitIndexes = new List<int> { 1, 3 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\StringTableItem.txt",
                        SplitIndexes = new List<int> { 1 },
                        //OverrideFontSize = 22,
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\TalentItem.txt",
                        SplitIndexes = new List<int> { 1, 2 },
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\TeleporterItem.txt",
                        SplitIndexes = new List<int> { 1 }
                    },
                    new SplitTextFileToTranslate()
                    {
                        UseIndexForLineId = true,
                        LineIdIndex = 0,
                        SplitCharacters = "\t",
                        SourcePath = @"TextAsset\QuestItem.txt",
                        SplitIndexes = new List<int> { 1, 3 },
                        //OverrideFontSize = 18,
                    },
                }
            };

            project.CreateWorkspaceFolder(config);
            project.WriteProjectFile();
        }
    }
}
