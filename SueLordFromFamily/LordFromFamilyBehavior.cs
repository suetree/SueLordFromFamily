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
    class LordFromFamilyBehavior : CampaignBehaviorBase
    {

		int takeMoney = 50000;
		Hero targetSpouse;
		Settlement targetSettlement;
		CampaignGameStarter campaignGameStarter;
	
		public static String FLAG_CLAN_CREATE_CHOICE_SETTLEMENT_ITEM = "sue_clan_create_from_family_choice_settlememt_item";
		public static String FLAG_CLAN_CREATE_CHOICE_SPOUSE_ITEM = "sue_clan_create_from_family_choice_settlememt_item";

		String spouseItemsuff = "";
		int times = 0;

		public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));

			//CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this, this.PreConversation);
		}

        public override void SyncData(IDataStore dataStore)
        {
          
        }

		public void PreConversation()
		{
			spouseItemsuff = "";
			times = 0;
			InformationManager.DisplayMessage(new InformationMessage(" PreConversation spouseItemsuff=" + spouseItemsuff + "  times=" + times));
		}

		public void PreConversationChatData()
		{
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
			ShowSelectSettlement();
			ShowSelectSpouseList();
		
		}

		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
			targetSpouse = null;
			targetSettlement = null;
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
			this.campaignGameStarter = campaignGameStarter;
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family", "hero_main_options", "sue_clan_create_from_family_request", GameTexts.FindText("sue_clan_create_from_family_request", null).ToString(), new ConversationSentence.OnConditionDelegate(this.CreateClanCondition), null, 100, null, null);
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family", "sue_clan_create_from_family_request", "sue_clan_create_from_family_start", GameTexts.FindText("sue_clan_create_from_family_choice_settlement_tip", null).ToString(), null, null, 100, null);

			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_need_spouse", "sue_clan_create_from_family_choice_other", "sue_clan_create_from_family_take_spouse", GameTexts.FindText("sue_clan_create_from_family_need_spouse", null).ToString(), new ConversationSentence.OnConditionDelegate(RequirementSpouseCondition), null, 100, null);
			
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_need_money", "sue_clan_create_from_family_choice_other", "sue_clan_create_from_family_take_money", GameTexts.FindText("sue_clan_create_from_family_need_money", null).ToString(), new ConversationSentence.OnConditionDelegate(RequirementSpouseNotNeedCondition), new ConversationSentence.OnConsequenceDelegate(
				() => {
					Hero hero = Hero.OneToOneConversationHero;
					if (null != hero && null!= hero.Spouse && hero.Spouse != Hero.MainHero && !Hero.MainHero.ExSpouses.Contains(hero.Spouse))
					{
						if (hero.Spouse.Clan ==  Clan.PlayerClan)
						{
							targetSpouse = hero.Spouse;
						}
					}
					}
				), 100, null);
	
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_need_money", "sue_clan_create_from_family_request_money", "sue_clan_create_from_family_take_money", GameTexts.FindText("sue_clan_create_from_family_need_money", null).ToString(), null, null, 100, null);
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_money_50k", "sue_clan_create_from_family_take_money", "sue_clan_create_from_family_complete", "50000", null, new ConversationSentence.OnConsequenceDelegate(() => { takeMoney = 50000; }), 100, null, null);
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_money_100k", "sue_clan_create_from_family_take_money", "sue_clan_create_from_family_complete", "100000", null, new ConversationSentence.OnConsequenceDelegate(() => { takeMoney = 100000; }), 100, null, null);
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_money_close", "sue_clan_create_from_family_take_money", "close_window", GameTexts.FindText("sue_clan_create_from_family_of_forget", null).ToString(), null, null, 100, null, null);
			
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_request_complete", "sue_clan_create_from_family_complete", "sue_clan_create_from_family_complete_2", GameTexts.FindText("sue_clan_create_from_family_complete", null).ToString(), null, new ConversationSentence.OnConsequenceDelegate(CreateVassal), 100, null);


			//修正不是指挥官的Lord
			campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_change_hero_to_commander", "hero_main_options", "change_hero_to_commander", GameTexts.FindText("sue_clan_create_from_family_your_are_commander", null).ToString(), new ConversationSentence.OnConditionDelegate(CanChangeHeroToCommander), new ConversationSentence.OnConsequenceDelegate(() => {
				Hero hero = Hero.OneToOneConversationHero;
				if(null != hero && null != hero.Clan)
				{
					hero.Clan.CreateNewMobileParty(hero);
				}
			}), 100, null, null);
			campaignGameStarter.AddDialogLine("sue_clan_create_from_family_change_hero_to_commander", "change_hero_to_commander", "", GameTexts.FindText("sue_clan_create_from_family_your_are_commander_accept", null).ToString(), null, null, 100, null);

			//测试
			//campaignGameStarter.AddPlayerLine("sue_clan_create_from_family_test", "hero_main_options", "tell_me_u_occupation", "你职业是啥", null, new ConversationSentence.OnConsequenceDelegate(ShowOccupation), 100, null, null);
			
			//campaignGameStarter.AddDialogLine("sue_clan_create_from_family_test", "tell_me_u_occupation", "", "我职业是 {OCCUPATION_NAME}  - {ORGIN_OCCUPATION_NAME}", null, null, 100, null);
		}


		private bool CanChangeHeroToCommander()
		{
			bool canChangeHeroToCommander = false;
			Hero hero = Hero.OneToOneConversationHero;
			if (null != hero && hero.CharacterObject.Occupation == Occupation.Lord && hero.Clan != null && hero.Clan != Clan.PlayerClan && !hero.Clan.CommanderHeroes.ToList().Contains(hero))
			{
				canChangeHeroToCommander = true;

			}
		
			return canChangeHeroToCommander;
		}



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

			if(characterObject.HeroObject != null)
			{
				InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject,CLAN=" + characterObject.HeroObject.Clan.Name));
				//InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject,CLAN=" + characterObject.HeroObject.CompanionOf.Name));
				InformationManager.DisplayMessage(new InformationMessage("characterObject.HeroObject.HeroState=" + characterObject.HeroObject.HeroState));
			}

			InformationManager.DisplayMessage(new InformationMessage("Occupation=" + name));

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
				targetSettlement = settlement;
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
					targetSpouse = spouse;
				}
			}));

		}

		private bool RequirementSpouseCondition()
		{
			Hero hero = Hero.OneToOneConversationHero;
			return null != hero && (hero.Spouse == null || (hero.Spouse != null && hero.Spouse.Clan != Clan.PlayerClan));
		}



		private bool RequirementSpouseNotNeedCondition()
		{
			Hero hero = Hero.OneToOneConversationHero;
			return null != hero && (hero.Spouse != null && hero.Spouse.Clan == Clan.PlayerClan);
		}

		private bool CreateClanCondition()
		{
			if (null == Hero.OneToOneConversationHero) return false;

			targetSpouse = null;
			targetSettlement = null;

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

		private void CreateVassal()
		{
			//Settlement settlement = GetCandidateSettlements().FirstOrDefault<Settlement>();
			if (targetSettlement == null)
			{
				return;
			}
			Hero hero = Hero.OneToOneConversationHero;
			if (hero == null)
			{
				return;
			}
			Kingdom kingdom = Hero.MainHero.MapFaction as Kingdom;
			if (kingdom == null)
			{
				return;
			}

			CultureObject culture = targetSettlement.Culture;
			TextObject textObject = NameGenerator.Current.GenerateClanName(culture, targetSettlement);
			TextObject nameTextObject = new TextObject( textObject.ToString());
			string str = Guid.NewGuid().ToString().Replace("-", "");
			RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, hero);

			SetOccupationToLord(hero);
			hero.ChangeState(Hero.CharacterStates.Active);
			Clan clan = TaleWorlds.ObjectSystem.MBObjectManager.Instance.CreateObject<Clan>("sue_clan_" + str);
			Banner banner = Banner.CreateRandomClanBanner(-1);
			clan.InitializeClan(nameTextObject, textObject, culture, banner);
			clan.AddRenown(150f, true);
			clan.SetLeader(hero);
			
			hero.Clan = clan;
			hero.CompanionOf = null;
			hero.IsNoble = true;
			hero.SetTraitLevel(DefaultTraits.Commander, 1);

			MobileParty mobileParty = clan.CreateNewMobileParty(hero);
			mobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10, true);
			mobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5, true);

			ChangeOwnerOfSettlementAction.ApplyByKingDecision(hero, targetSettlement);
			clan.UpdateHomeSettlement(targetSettlement);

			if (targetSpouse != null)
			{
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney/2, false);
			}
			else
			{
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney , false);
			}
		
			if (takeMoney == 100000)
			{
				ChangeRelationAction.ApplyPlayerRelation(hero, 100, true, true);
				if (targetSpouse != null)
				{
					ChangeRelationAction.ApplyPlayerRelation(targetSpouse, 100, true, true);
				}
				
			}
			else
			{
				ChangeRelationAction.ApplyPlayerRelation(hero, 30, true, true);
				if (targetSpouse != null)
				{
					ChangeRelationAction.ApplyPlayerRelation(targetSpouse, 30, true, true);
				}
			}
			
			ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, true);

		

			if(targetSpouse  != null)
			{
				targetSpouse.Spouse = hero;
				InformationManager.AddQuickInformation(new TextObject($"{hero.Name} marry with {targetSpouse.Name}"), 0, null, "event:/ui/notification/quest_finished");
				
				RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, targetSpouse);
				
				targetSpouse.ChangeState(Hero.CharacterStates.Active);
				targetSpouse.IsNoble = true;
				SetOccupationToLord(targetSpouse);
				targetSpouse.CompanionOf = null;
				targetSpouse.Clan = clan;
				targetSpouse.SetTraitLevel(DefaultTraits.Commander, 1);
				//AddCompanionAction.Apply(clan, targetSpouse);

				MobileParty targetSpouseMobileParty = clan.CreateNewMobileParty(hero);
				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10, true);
				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5, true);
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, targetSpouse, takeMoney / 2, false);

				clan.CreateNewMobileParty(targetSpouse);
			}
			PlayerLineUtils.cleanRepeatableLine(campaignGameStarter);
		}

		private static List<Settlement> GetCandidateSettlements()
		{
			Vec2 playerPosition = Hero.MainHero.PartyBelongedTo.Position2D;
			IEnumerable<Settlement> settlements = Hero.MainHero.Clan.Settlements;
			Func<Settlement, bool> func =  new Func<Settlement, bool>( (settlement) => (settlement.IsTown || settlement.IsCastle));
			return (from n in settlements.Where(func)
					orderby n.Position2D.Distance(playerPosition)
					select n).ToList<Settlement>();
		}

		private static void SetOccupationToLord( Hero hero)
		{
			if (hero.CharacterObject.Occupation == Occupation.Lord) return ;
			
			FieldInfo fieldInfo = hero.CharacterObject.GetType().GetField("_originCharacter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo propertyInfo = typeof(CharacterObject).GetProperty("Occupation");
			if (null != propertyInfo && null != propertyInfo.DeclaringType)
			{
				propertyInfo = propertyInfo.DeclaringType.GetProperty("Occupation");
				if (null != propertyInfo)
				{
					propertyInfo.SetValue(hero.CharacterObject, Occupation.Lord, null);
				}
			}
			if (null != fieldInfo)
			{
				fieldInfo.SetValue(hero.CharacterObject, CharacterObject.PlayerCharacter);
			}
			else
			{
				FieldInfo fieldInfoId = hero.CharacterObject.GetType().GetField("_originCharacterStringId", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (null != fieldInfoId)
				{
					fieldInfoId.SetValue(hero.CharacterObject, CharacterObject.PlayerCharacter.StringId);
				}
			}
			//main_hero
		}
	}
}