using System;
using System.Collections.Generic;
using Helpers;
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
    class HeroInfoHelper
    {

		private void ShowOccupation()
		{
			CharacterObject characterObject = CharacterObject.OneToOneConversationCharacter;
			FieldInfo fieldInfo = characterObject.GetType().GetField("_originCharacter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			CharacterObject originalCharacterObject = (CharacterObject)fieldInfo.GetValue(characterObject);
			String name = System.Enum.GetName(characterObject.Occupation.GetType(), characterObject.Occupation);
			String orginname = System.Enum.GetName(characterObject.Occupation.GetType(), characterObject.Occupation);
			MBTextManager.SetTextVariable("OCCUPATION_NAME", name, false);
			MBTextManager.SetTextVariable("ORGIN_OCCUPATION_NAME", orginname, false);
			//MBTextManager.SetTextVariable("STATUS", characterObject.st, false);

			if (characterObject.HeroObject != null)
			{
				InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject,CLAN=" + characterObject.HeroObject.Clan.Name));
				//InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject,CLAN=" + characterObject.HeroObject.CompanionOf.Name));
				InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject.HeroState=" + characterObject.HeroObject.HeroState));
			}

			InformationManager.DisplayMessage(new InformationMessage("Occupation=" + name));

		}
	}
}
