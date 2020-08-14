using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace SueLordFromFamily.dialogue
{
    public class ChaneHeroClanDialogue : AbsCreateDialogue
    {
		public static String FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM = "sue_clan_create_from_family_choice_clan_item";

		Clan targetChangeClan;

		public ChaneHeroClanDialogue(CampaignGameStarter campaignGameStarter) : base(campaignGameStarter)
		{
		}


		public override void GenerateDialogue()
        {
			//开始英雄对话选项， 请求对方去别的家族
			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family_change_clan")
				.InputOrder("hero_main_options")
				.OutOrder("sue_clan_create_from_family_change_clan_request")
				.Text(LoactionText("sue_clan_create_from_family_change_clan_request"))
				.Condition(this.ChangeClanCondition)
				.CreateAndAdd(CampaignGameStarter);

			new DialogueCreator()
			   .IsPlayer(false)
			   .Id("sue_clan_create_from_family_change_clan_answer")
			   .InputOrder("sue_clan_create_from_family_change_clan_request")
			   .OutOrder("sue_clan_create_from_family_change_clan_answer_select")
			   .Text(LoactionText("sue_clan_create_from_family_change_clan_answer"))
			   .CreateAndAdd(CampaignGameStarter);

			//完成步骤， 关闭窗口
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_change_clan_complete")
				.InputOrder("sue_clan_create_from_family_change_clan_answer_select_result")
				.OutOrder("sue_clan_create_from_family_complete_2")
				.Text(LoactionText("sue_clan_create_from_family_complete"))
				.Result(ChangeHeroToOtherClan)
				.CreateAndAdd(CampaignGameStarter);

			
		}
		private void GenerateDialogueForSelectClan()
		{
			PlayerLineUtils.cleanRepeatableLine(CampaignGameStarter, FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM);
			Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
			List<Clan> clans = kindom.Clans.Where((clan) => clan != Clan.PlayerClan).ToList();
			int maxNumber = 10;
			if (clans.Count() <= maxNumber)
			{
				clans.ForEach((clan) => addPlayerLineToSelectClan(clan));
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM, "sue_clan_create_from_family_change_clan_answer_select", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
			else
			{
				List<int> canAddIndexs = RandomUtils.RandomNumbers(maxNumber, 0, clans.Count(), new List<int>() { });
				int index = 0;
				clans.ForEach((clan) =>
				{
					if (canAddIndexs.Contains(index))
					{
						addPlayerLineToSelectClan(clan);
					}
					index++;
				});
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM, "sue_clan_create_from_family_change_clan_answer_select", "sue_clan_create_from_family_take_clan_change", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => { GenerateDialogueForSelectClan(); }), 100, null);
				CampaignGameStarter.AddDialogLine(FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM, "sue_clan_create_from_family_take_clan_change", "sue_clan_create_from_family_start", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change_tip", null).ToString(), null, null, 100, null);
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM, "sue_clan_create_from_family_change_clan_answer_select", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
		}



		private void addPlayerLineToSelectClan(Clan clan)
		{
			CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_ITEM, "sue_clan_create_from_family_change_clan_answer_select", "sue_clan_create_from_family_change_clan_answer_select_result", clan.Name.ToString(), null, new ConversationSentence.OnConsequenceDelegate(() =>
			{
				this.targetChangeClan = clan;
			}));
		}


		private bool ChangeClanCondition()
		{
			Hero hero = Hero.OneToOneConversationHero;
			if (null == hero) return false;
			bool canChange = false;
			if (hero.Clan == Clan.PlayerClan)
			{
				Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
				if (((kindom != null) ? kindom.Ruler : null) == Hero.MainHero && kindom.Clans.Count > 1)
				{
					ResetDataForChangeClan();
					canChange = true;
				}
			}
			return canChange;
		}

		private void ResetDataForChangeClan()
		{
			this.targetChangeClan = null;
			GenerateDialogueForSelectClan();
		}

		private void ChangeHeroToOtherClan()
		{
			Hero hero = Hero.OneToOneConversationHero;
			if (null != this.targetChangeClan)
			{
				HeroOperation.NewClanAllocateForHero(hero, this.targetChangeClan);
			}else
			{
				InformationManager.DisplayMessage(new InformationMessage("LordFromFamily error occurred, cant change the hero to other clan!"));
			}
			
		}

	}
}
