
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using StoryMode;
using HarmonyLib;
using SueLordFromFamily.Behavior;

namespace SueLordFromFamily
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("mod.sue.lordFromFamily");
            harmony.PatchAll();
        }


        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            InformationManager.DisplayMessage(new InformationMessage("LordFromFamily OnGameStart"));
            if (gameStarterObject.GetType() == typeof(CampaignGameStarter))
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior(new LordFromFamilyBehavior());
                ((CampaignGameStarter)gameStarterObject).LoadGameTexts(string.Format("{0}/Modules/{1}/ModuleData/sue_clan_create_from_family.xml", BasePath.Name, "SueLordFromFamily"));
            }

               

        }
    }
}