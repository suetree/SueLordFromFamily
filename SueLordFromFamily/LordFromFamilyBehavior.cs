using Helpers;
using SueLordFromFamily.dialogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;


namespace SueLordFromFamily
{
    class LordFromFamilyBehavior : CampaignBehaviorBase
    {

		public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
		}

        public override void SyncData(IDataStore dataStore)
        {
          
        }

		public LordFromFamilyBehavior()
		{
			
		}

		

		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
			new ChaneHeroClanDialogue(campaignGameStarter).GenerateDialogue();
		    new CreateClanDialogue(campaignGameStarter).GenerateDialogue();
			InformationManager.DisplayMessage(new InformationMessage("LordFromFamily OnSessionLaunched"));
		;	
	    }
	
	}
}