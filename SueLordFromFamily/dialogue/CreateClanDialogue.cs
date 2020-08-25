using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SueLordFromFamily.dialogue
{
    class CreateClanDialogue : AbsCreateDialogue
	{
		public static String FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM = "sue_clan_create_from_family_choice_settlememt_item";
		public static String FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM = "sue_clan_create_from_family_choice_settlememt_item";
		public static String FLAG_CLAN_CREATE_CHOICE_CLAN_MONEY_TIER_ITEM = "sue_clan_create_from_family_choice_clan_money_tier";

		ClanCreateBussniess clanCreateBussniess;

		public CreateClanDialogue(CampaignGameStarter campaignGameStarter) : base(campaignGameStarter)
		{
			clanCreateBussniess = new ClanCreateBussniess();
		}


		public override void GenerateDialogue()
        {
			//开始英雄对话选项， 输出请求家族分封
			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family")
				.InputOrder("hero_main_options")
				.OutOrder("sue_clan_create_from_family_request")
				.Text(LoactionText("sue_clan_create_from_family_request"))
				.Condition(this.CreateClanCondition)
				.CreateAndAdd(CampaignGameStarter);

			//屏幕提示, 选择要哪个封地
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family")
				.InputOrder("sue_clan_create_from_family_request")
				.OutOrder("sue_clan_create_from_family_start")
				.Text(LoactionText("sue_clan_create_from_family_choice_settlement_tip"))
				.CreateAndAdd(CampaignGameStarter);

			// 屏幕提示, 没老婆。 需要分配老婆
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_spouse")
				.InputOrder("sue_clan_create_from_family_choice_other")
				.OutOrder("sue_clan_create_from_family_take_spouse")
				.Text(LoactionText("sue_clan_create_from_family_need_spouse"))
				.Condition(NeedSpouse)
				.CreateAndAdd(CampaignGameStarter);

			//屏幕提示  初始有老婆， 进行下一步，需要金钱
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_money")
				.InputOrder("sue_clan_create_from_family_choice_other")
				.OutOrder("sue_clan_create_from_family_take_money")
				.Text(LoactionText("sue_clan_create_from_family_need_money"))
				.Condition(NeedNotSpouse)
				.Result(NeedNotSpouseResult)
				.CreateAndAdd(CampaignGameStarter);

			//屏幕提示  初始没老婆 ，进行下一步，需要金钱
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_money")
				.InputOrder("sue_clan_create_from_family_request_money")
				.OutOrder("sue_clan_create_from_family_take_money")
				.Text(LoactionText("sue_clan_create_from_family_need_money"))
				.CreateAndAdd(CampaignGameStarter);


		
		

			///屏幕提示  有孩子
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_request_children")
				.InputOrder("sue_clan_create_from_family_complete_take_money")
				.OutOrder("sue_clan_create_from_family_request_children_out")
				.Condition(HasChildren)
				.Text(LoactionText("sue_clan_create_from_family_vassal_request_children"))
				.CreateAndAdd(CampaignGameStarter);

			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_request_complete_no_children")
				.InputOrder("sue_clan_create_from_family_complete_take_money")
				.OutOrder("sue_clan_create_from_family_complete_2")
				.Text(LoactionText("sue_clan_create_from_family_complete"))
				.Condition(HasNoChildren)
				.Result(CreateVassal)
				.CreateAndAdd(CampaignGameStarter);


			//封臣的孩子处理
			new DialogueCreator()
			   .IsPlayer(true)
			   .Id("sue_clan_create_from_family_request_children_result_1")
			   .InputOrder("sue_clan_create_from_family_request_children_out")
			   .OutOrder("sue_clan_create_from_family_complete")
			   .Text(LoactionText("sue_clan_create_from_family_vassal_request_children_result_1"))
				.Result(TogetherWithThireChildren)
			   .CreateAndAdd(CampaignGameStarter);

			new DialogueCreator()
			  .IsPlayer(true)
			  .Id("sue_clan_create_from_family_request_children_result_2")
			  .InputOrder("sue_clan_create_from_family_request_children_out")
			  .OutOrder("sue_clan_create_from_family_complete")
			  .Text(LoactionText("sue_clan_create_from_family_vassal_request_children_result_2"))
			  .CreateAndAdd(CampaignGameStarter);

			// 孩子处理取消
			new DialogueCreator()
			.IsPlayer(true)
			.Id("sue_clan_create_from_family_money_close")
			.InputOrder("sue_clan_create_from_family_request_children_out")
			.OutOrder("close_window")
			.Text(LoactionText("sue_clan_create_from_family_of_forget"))
			.CreateAndAdd(CampaignGameStarter);


			//完成步骤， 关闭窗口
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_request_complete")
				.InputOrder("sue_clan_create_from_family_complete")
				.OutOrder("sue_clan_create_from_family_complete_2")
				.Text(LoactionText("sue_clan_create_from_family_complete"))
				.Result(CreateVassal)
				.CreateAndAdd(CampaignGameStarter);
		}


		private bool CreateClanCondition()
		{
			if (null == Hero.OneToOneConversationHero) return false;


			List<Settlement> settlements = Hero.MainHero.Clan.Settlements.Where((settlement) => (settlement.IsTown || settlement.IsCastle)).ToList();
			if (settlements.Count < 1)
			{
				return false;
			}

			// InformationManager.DisplayMessage(new InformationMessage("LordFromFamily start condition"));

			Hero hero = Hero.OneToOneConversationHero;
			if (hero != null && hero.Clan == Clan.PlayerClan && hero.PartyBelongedTo != null &&
				hero.PartyBelongedTo.LeaderHero == Hero.MainHero && Hero.MainHero.MapFaction is Kingdom
				&& !Hero.MainHero.ExSpouses.Contains(hero) && hero != Hero.MainHero.Spouse
				)
			{
				Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
				if (((kindom != null) ? kindom.Ruler : null) == Hero.MainHero)
				{
					bool flag = Hero.MainHero.Clan.Gold >= 50000;
					if (flag)
					{
						ResetDataForCreateClan();
					}
					return flag;
				}
			}
			return false;
		}

		private void CreateVassal()
		{
			this.clanCreateBussniess.CreateVassal();
		}

		private void ShowSelectSettlement()
		{
			List<Settlement> settlements = Hero.MainHero.Clan.Settlements.Where((settlement) => (settlement.IsTown || settlement.IsCastle)).ToList();
			int maxNumber = 10;
			if (settlements.Count() <= maxNumber)
			{
				settlements.ForEach((settlement) => addPlayerLineToSelectSettlement(settlement));
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
			else
			{
				List<int> canAddIndexs = RandomUtils.RandomNumbers(maxNumber, 0, settlements.Count(), new List<int>() { });
				int index = 0;
				settlements.ForEach((settlement) =>
				{
					if (canAddIndexs.Contains(index))
					{
						addPlayerLineToSelectSettlement(settlement);
					}
					index++;
				});
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "sue_clan_create_from_family_take_settlement_change", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => { GenerateDataForCreateClan(); }), 100, null);
				CampaignGameStarter.AddDialogLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_take_settlement_change", "sue_clan_create_from_family_start", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change_tip", null).ToString(), null, null, 100, null);
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
		}

		private void ShowSelectSpouseList()
		{
			IEnumerable<CharacterObject> spouses = MobileParty.MainParty.MemberRoster.Troops.Where(new Func<CharacterObject, bool>((obj) => (obj.IsHero && obj.HeroObject.Spouse == null && obj.HeroObject.IsPlayerCompanion)));
			int maxNumber = 10;

			if (spouses.Count() <= maxNumber)
			{
				spouses.ToList().ForEach((obj) =>
				{
					Hero spouse = obj.HeroObject;
					addPlayerLineToSelectSpouse(spouse);
				});
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_request_money", GameTexts.FindText("sue_clan_create_from_family_need_spouse_not", null).ToString(), null, null, 100, null);
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
			else
			{
				Hero hero = Hero.OneToOneConversationHero;
				List<CharacterObject> targets = spouses.ToList();
				int excludeIndex = targets.IndexOf(hero.CharacterObject);
				List<int> canAddIndexs = RandomUtils.RandomNumbers(maxNumber, 0, targets.Count(), new List<int>() { excludeIndex });
				int index = 0;
				targets.ForEach((obj) =>
				{
					Hero spouse = obj.HeroObject;
					if (canAddIndexs.Contains(index))
					{
						addPlayerLineToSelectSpouse(spouse);
					}
					index++;
				});
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_take_spouse_change", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => { GenerateDataForCreateClan(); }), 100, null);
				CampaignGameStarter.AddDialogLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse_change", "sue_clan_create_from_family_take_spouse", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change_tip", null).ToString(), null, null, 100, null);
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_request_money", GameTexts.FindText("sue_clan_create_from_family_need_spouse_not", null).ToString(), null, null, 100, null);
				CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => {

				}), 100, null);
			}
		}

		private void ShowClanMoneyTierList()
		{
			int canTakeMoneyMaxTier = 6;
			for (int i = 2; i < 7; i++)
			{
			 bool canShow =	PlayGetMoneyByTierCondition(i);
				if (!canShow )
				{
					canTakeMoneyMaxTier = i-1;
					break;
				}
			}

			for (int i = 2; i <= canTakeMoneyMaxTier; i++)
			{
				addPlayerLineToSelectClanMoneyTier(i);
			}
			CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_MONEY_TIER_ITEM, "sue_clan_create_from_family_take_money", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null );
		}

		public bool HasNoChildren()
		{
			return !HasChildren();
		}

		public bool HasChildren()
		{
			bool result = false;
			Hero hero = Hero.OneToOneConversationHero;
			if (hero.Children.Count > 0)
			{
				hero.Children.ForEach((child) =>
				{
					if (child.Clan == Clan.PlayerClan)
					{
						result = true;
					}
				});
			}
			return result;
		}

		public void TogetherWithThireChildren()
		{
			this.clanCreateBussniess.isTogetherWithThireChildren = true;
		}



		private void ResetDataForCreateClan()
		{
			this.clanCreateBussniess.reset();
			this.GenerateDataForCreateClan();
		}

		private bool NeedSpouse()
		{
			return !NeedNotSpouse();
		}

		private bool NeedNotSpouse()
		{
			Hero hero = Hero.OneToOneConversationHero;
			return null != hero && (hero.Spouse != null && hero.Spouse.Clan == Clan.PlayerClan);
		}

		public void NeedNotSpouseResult()
		{
			Hero hero = Hero.OneToOneConversationHero;
			if (null != hero && null != hero.Spouse && hero.Spouse != Hero.MainHero && !Hero.MainHero.ExSpouses.Contains(hero.Spouse))
			{
				if (hero.Spouse.Clan == Clan.PlayerClan)
				{
					this.clanCreateBussniess.targetSpouse = hero.Spouse;
				}
			}
		}

		public bool PlayGetMoneyByTierCondition(int tier)
		{
			int takeMoeny = clanCreateBussniess.TakeMoneyByTier(tier);
			if (Hero.MainHero.Clan.Gold >= takeMoeny)
			{

				return true;
			}else
			{
				return false;
			}
		}

		public void PlayerGetMoneyByTierResult(int tier)
		{
			this.clanCreateBussniess.selectClanTier = tier;
		}

		private void GenerateDataForCreateClan()
		{
			PlayerLineUtils.cleanRepeatableLine(CampaignGameStarter, FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM);
			PlayerLineUtils.cleanRepeatableLine(CampaignGameStarter, FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM);
			PlayerLineUtils.cleanRepeatableLine(CampaignGameStarter, FLAG_CLAN_CREATE_CHOICE_CLAN_MONEY_TIER_ITEM);
			ShowSelectSettlement();
			ShowSelectSpouseList();
			ShowClanMoneyTierList();
		}

		private void addPlayerLineToSelectSpouse(Hero spouse)
		{
			CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_request_money", spouse.Name.ToString(), new ConversationSentence.OnConditionDelegate(() =>
			{
				Hero hero = Hero.OneToOneConversationHero;
				return hero != spouse;

			}), new ConversationSentence.OnConsequenceDelegate(() =>
			{
				{
					this.clanCreateBussniess.targetSpouse = spouse;
				}
			}));

		}

		private void addPlayerLineToSelectSettlement(Settlement settlement)
		{
			CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "sue_clan_create_from_family_choice_other", settlement.Name.ToString(), null, new ConversationSentence.OnConsequenceDelegate(() =>
			{
				this.clanCreateBussniess.targetSettlement = settlement;
			}));
		}

		private void addPlayerLineToSelectClanMoneyTier(int tier)
		{
			CampaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_CLAN_MONEY_TIER_ITEM, "sue_clan_create_from_family_take_money", "sue_clan_create_from_family_complete_take_money", String.Format("{0} GLOD ( Tier = {1} )", clanCreateBussniess.TakeMoneyByTier(tier), tier), null, new ConversationSentence.OnConsequenceDelegate(() =>
			{
				this.clanCreateBussniess.selectClanTier = tier;
			}));
		}

	}
}
