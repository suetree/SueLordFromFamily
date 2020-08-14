
using System;
using System.Collections.Generic;

using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SueLordFromFamily
{
    class PlayerLineUtils
    {

        public static void cleanRepeatableLine(CampaignGameStarter campaignGameStarter, String flag)
		{
			//InformationManager.DisplayMessage(new InformationMessage("LordFromFamilyBehavior cleanRepeatableLine"));
			ConversationManager conversationManager;
			FieldInfo fieldInfo = campaignGameStarter.GetType().GetField("_conversationManager", BindingFlags.NonPublic | BindingFlags.Instance);
			Object obj = fieldInfo.GetValue(campaignGameStarter);
			if (null != obj)
			{
				conversationManager = (ConversationManager)fieldInfo.GetValue(campaignGameStarter);
				FieldInfo objectListInfo = conversationManager.GetType().GetField("_sentences", BindingFlags.NonPublic | BindingFlags.Instance);
				if (null != objectListInfo)
				{
					List<ConversationSentence> sentences = (List<ConversationSentence>)objectListInfo.GetValue(conversationManager);

					//InformationManager.DisplayMessage(new InformationMessage("sentences.size=" + sentences.Count));
					 sentences.RemoveAll((ConversationSentence s) => flag == s.Id);
					//sentences.RemoveAll((ConversationSentence s) => LordFromFamilyBehavior.FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM == s.Id);
					//InformationManager.DisplayMessage(new InformationMessage("sentences.size=" + sentences.Count));

				}
			}
		}
	}
}
