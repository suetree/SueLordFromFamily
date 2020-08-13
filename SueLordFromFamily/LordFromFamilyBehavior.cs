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

		CampaignGameStarter campaignGameStarter;
		VassalBussniess vassalBussniess;

		public static String FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM = "sue_clan_create_from_family_choice_settlememt_item";
		public static String FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM = "sue_clan_create_from_family_choice_settlememt_item";

		String spouseItemsuff = "";
		int times = 0;

		public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
		}

        public override void SyncData(IDataStore dataStore)
        {
          
        }

		public LordFromFamilyBehavior()
		{
			this.vassalBussniess = new VassalBussniess();
		}

		public void PreConversation()
		{
			spouseItemsuff = "";
			times = 0;
			InformationManager.DisplayMessage(new InformationMessage(" PreConversation spouseItemsuff=" + spouseItemsuff + "  times=" + times));
		}

		/**
		 * 读取城市和配偶数据
		 */
		public void PreConversationChatData()
		{
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
			ShowSelectSettlement();
			ShowSelectSpouseList();
		
		}

		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
			InformationManager.DisplayMessage(new InformationMessage("LordFromFamily OnSessionLaunched"));
			this.vassalBussniess.reset();
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
			this.campaignGameStarter = campaignGameStarter;

			//开始英雄对话选项， 输出请求家族分封
			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family")
				.InputOrder("hero_main_options")
				.OutOrder("sue_clan_create_from_family_request")
				.Text(LoactionText("sue_clan_create_from_family_request"))
				.Condition(this.CreateClanCondition)
				.CreateAndAdd(campaignGameStarter);

			//开始英雄对话选项条件那里就已经输入了城市数据
			 
			//屏幕提示, 选择要哪个城市
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family")
				.InputOrder("sue_clan_create_from_family_request")
				.OutOrder("sue_clan_create_from_family_start")
				.Text(LoactionText("sue_clan_create_from_family_choice_settlement_tip"))
				.CreateAndAdd(campaignGameStarter);

			//屏幕提示, 第二步选择 是否需要配偶
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_spouse")
				.InputOrder("sue_clan_create_from_family_choice_other")
				.OutOrder("sue_clan_create_from_family_take_spouse")
				.Text(LoactionText("sue_clan_create_from_family_need_spouse"))
				.Condition(ScreenAllocateSpouseCondition)
				.CreateAndAdd(campaignGameStarter);

			// 屏幕提示, 第二步选择 是否要金钱
			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_money")
				.InputOrder("sue_clan_create_from_family_choice_other")
				.OutOrder("sue_clan_create_from_family_take_money")
				.Text(LoactionText("sue_clan_create_from_family_need_spouse"))
				.Condition(ScreenAllocateMoneyCondition)
				.Result(ScreenAllocateMoneyResult)
				.CreateAndAdd(campaignGameStarter);

			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_need_money")
				.InputOrder("sue_clan_create_from_family_request_money")
				.OutOrder("sue_clan_create_from_family_take_money")
				.Text(LoactionText("sue_clan_create_from_family_need_money"))
				.CreateAndAdd(campaignGameStarter);

			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family_money_50k")
				.InputOrder("sue_clan_create_from_family_take_money")
				.OutOrder("sue_clan_create_from_family_complete")
				.Text("50000")
				.Result(PlayerGetMoney50KResult)
				.CreateAndAdd(campaignGameStarter);

			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family_money_100k")
				.InputOrder("sue_clan_create_from_family_take_money")
				.OutOrder("sue_clan_create_from_family_complete")
				.Text("100000")
				.Result(PlayerGetMoney100KResult)
				.CreateAndAdd(campaignGameStarter);

			new DialogueCreator()
				.IsPlayer(true)
				.Id("sue_clan_create_from_family_money_close")
				.InputOrder("sue_clan_create_from_family_take_money")
				.OutOrder("close_window")
			    .Text(LoactionText("sue_clan_create_from_family_of_forget"))
				.Result(PlayerGetMoney100KResult)
				.CreateAndAdd(campaignGameStarter);

			new DialogueCreator()
				.IsPlayer(false)
				.Id("sue_clan_create_from_family_request_complete")
				.InputOrder("sue_clan_create_from_family_complete")
				.OutOrder("sue_clan_create_from_family_complete_2")
				.Text(LoactionText("sue_clan_create_from_family_complete"))
				.Result(CreateVassal)
				.CreateAndAdd(campaignGameStarter);


		    /*	//修正不是指挥官的Lord 新版本就不需要这个了
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_change_hero_to_commander", "hero_main_options", "change_hero_to_commander", GameTexts.FindText("sue_clan_create_from_family_your_are_commander", null).ToString(), new ConversationSentence.OnConditionDelegate(CanChangeHeroToCommander), new ConversationSentence.OnConsequenceDelegate(() => {
				Hero hero = Hero.OneToOneConversationHero;
				if(null != hero && null != hero.Clan)
				{
					hero.Clan.CreateNewMobileParty(hero);
				}
			}), 100, null, null);
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_change_hero_to_commander", "change_hero_to_commander", "", GameTexts.FindText("sue_clan_create_from_family_your_are_commander_accept", null).ToString(), null, null, 100, null);
            */
			//测试
			//campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_test", "hero_main_options", "tell_me_u_occupation", "你职业是啥", null, new ConversationSentence.OnConsequenceDelegate(ShowOccupation), 100, null, null);
			//campaignGameStarter.AddDialogLine("sue_clan_create_from_family_test", "tell_me_u_occupation", "", "我职业是 {OCCUPATION_NAME}  - {ORGIN_OCCUPATION_NAME}", null, null, 100, null);
		}


	/*	private bool CanChangeHeroToCommander()
		{
			bool canChangeHeroToCommander = false;
			Hero hero = Hero.OneToOneConversationHero;
			if (null != hero && hero.CharacterObject.Occupation == Occupation.Lord && hero.Clan != null && hero.Clan != Clan.PlayerClan && !hero.Clan.CommanderHeroes.ToList().Contains(hero))
			{
				canChangeHeroToCommander = true;

			}
		
			return canChangeHeroToCommander;
		}*/

		private void CreateVassal()
		{
			this.vassalBussniess.CreateVassal();
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
		}

		private void ShowSelectSettlement()
		{
			List<Settlement> settlements = Hero.MainHero.Clan.Settlements.Where((settlement) => (settlement.IsTown || settlement.IsCastle)).ToList();
			int maxNumber = 10;
			if (settlements.Count() <= maxNumber)
			{
				settlements.ForEach( (settlement) => addPlayerLineToSelectSettlement(settlement));
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
			else
			{
				List<int> canAddIndexs = RandomUtils.RandomNumbers(maxNumber, 0, settlements.Count(), new List<int>() {  });
				int index = 0;
				settlements.ForEach((settlement) =>
				{
					if (canAddIndexs.Contains(index))
					{
						addPlayerLineToSelectSettlement(settlement);
					}
					index++;
				});
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "sue_clan_create_from_family_take_settlement_change", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => { changeSuff(); PreConversationChatData(); }), 100, null);
				campaignGameStarter.AddDialogLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_take_settlement_change", "sue_clan_create_from_family_start", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change_tip", null).ToString(), null, null, 100, null);
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
			}
		}

		private void ShowSelectSpouseList()
		{
			IEnumerable<CharacterObject> spouses =  MobileParty.MainParty.MemberRoster.Troops.Where(new Func<CharacterObject, bool>((obj) => (obj.IsHero && obj.HeroObject.Spouse == null && obj.HeroObject.IsPlayerCompanion)));
			int maxNumber = 10;
			
			if(spouses.Count() <= maxNumber)
			{
				spouses.ToList().ForEach((obj) =>
				{
					Hero spouse = obj.HeroObject;
					addPlayerLineToSelectSpouse(spouse);
				});
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_request_money", GameTexts.FindText("sue_clan_create_from_family_need_spouse_not", null).ToString(), null, null, 100, null);
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null);
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
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse" , "sue_clan_create_from_family_take_spouse_change", GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(()=> { changeSuff(); PreConversationChatData(); }), 100, null);
				campaignGameStarter.AddDialogLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse_change", "sue_clan_create_from_family_take_spouse" , GameTexts.FindText("sue_clan_create_from_family_choice_spouse_item_change_tip", null).ToString(), null, null, 100, null);
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse", "sue_clan_create_from_family_request_money", GameTexts.FindText("sue_clan_create_from_family_need_spouse_not", null).ToString(), null, null, 100, null);
				campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse" , "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(() => {
					
				}), 100, null);
			}
		}

		private void changeSuff() {
			times++;
			spouseItemsuff = times + "";
			//InformationManager.DisplayMessage(new InformationMessage("changeSuff spouseItemsuff=" + spouseItemsuff + "  times=" + times));
		}

		private void addPlayerLineToSelectSettlement(Settlement settlement)
		{
			campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM, "sue_clan_create_from_family_start", "sue_clan_create_from_family_choice_other", settlement.Name.ToString() , null, new ConversationSentence.OnConsequenceDelegate(() =>
			{
				this.vassalBussniess.targetSettlement = settlement;
			}));
		}

		private void addPlayerLineToSelectSpouse(Hero spouse)
		{
			campaignGameStarter.AddRepeatablePlayerLine(FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM, "sue_clan_create_from_family_take_spouse" , "sue_clan_create_from_family_request_money", spouse.Name.ToString(), new ConversationSentence.OnConditionDelegate(() =>
			{
				Hero hero = Hero.OneToOneConversationHero;
				return hero != spouse;

			}), new ConversationSentence.OnConsequenceDelegate(() =>
			{
				{
					this.vassalBussniess.targetSpouse = spouse;
				}
			}));

		}

		private bool ScreenAllocateSpouseCondition()
		{
			Hero hero = Hero.OneToOneConversationHero;
			return null != hero && (hero.Spouse == null || (hero.Spouse != null && hero.Spouse.Clan != Clan.PlayerClan));
		}

		private bool ScreenAllocateMoneyCondition()
		{
			Hero hero = Hero.OneToOneConversationHero;
			return null != hero && (hero.Spouse != null && hero.Spouse.Clan == Clan.PlayerClan);
		}

		public void ScreenAllocateMoneyResult()
		{
			Hero hero = Hero.OneToOneConversationHero;
			if (null != hero && null != hero.Spouse && hero.Spouse != Hero.MainHero && !Hero.MainHero.ExSpouses.Contains(hero.Spouse))
			{
				if (hero.Spouse.Clan == Clan.PlayerClan)
				{
					this.vassalBussniess.targetSpouse = hero.Spouse;
				}
			}
		}


		public void PlayerGetMoney50KResult()
		{
			this.vassalBussniess.takeMoney = 50000;
		}

		public void PlayerGetMoney100KResult()
		{
			this.vassalBussniess.takeMoney = 100000;
		}

		private bool CreateClanCondition()
		{
			if (null == Hero.OneToOneConversationHero) return false;

			this.vassalBussniess.reset();
			List<Settlement> settlements = Hero.MainHero.Clan.Settlements.Where((settlement) => (settlement.IsTown || settlement.IsCastle)).ToList();
			if (settlements.Count < 1)
			{
				return false;
			}
			Hero hero = Hero.OneToOneConversationHero;
			if (hero != null && hero.Clan == Clan.PlayerClan && hero.PartyBelongedTo != null &&
				hero.PartyBelongedTo.LeaderHero == Hero.MainHero  && Hero.MainHero.MapFaction is Kingdom
				&& !Hero.MainHero.ExSpouses.Contains(hero) && hero  != Hero.MainHero.Spouse
				)
			{
				Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
				if (((kindom != null) ? kindom.Ruler : null) == Hero.MainHero)
				{
					bool flag = Hero.MainHero.Clan.Gold >= 50000;
					if (flag)
					{
						PreConversationChatData();
					}
					return flag;


				}
			}
			return false;
		}

		private String LoactionText(String idStr)
		{
			return GameTexts.FindText(idStr, null).ToString();
		}
	}
}