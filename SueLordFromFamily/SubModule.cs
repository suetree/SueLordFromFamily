
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using StoryMode;

namespace SueLordFromFamily
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
  
        }


        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (gameStarterObject.GetType() == typeof(CampaignGameStarter))
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior(new LordFromFamilyBehavior());
                ((CampaignGameStarter)gameStarterObject).LoadGameTexts(string.Format("{0}/Modules/{1}/ModuleData/sue_clan_create_from_family.xml", BasePath.Name, "SueLordFromFamily"));
            }

               

        }
    }
}