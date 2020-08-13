using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SueLordFromFamily
{
    class VassalBussniess
    {
		public Hero targetSpouse { set; get; }
		public Settlement targetSettlement { set; get; }
		public int takeMoney = 50000;

		public bool isTogetherWithThireChildren;

		public void reset()
		{
			this.targetSpouse = null;
			this.targetSettlement = null;
		}

		public void CreateVassal()
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
			TextObject nameTextObject = new TextObject(textObject.ToString());
			string str = Guid.NewGuid().ToString().Replace("-", "");
			RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, hero);

			SetOccupationToLord(hero);
			hero.ChangeState(Hero.CharacterStates.Active);
			Clan clan = TaleWorlds.ObjectSystem.MBObjectManager.Instance.CreateObject<Clan>("sue_clan_" + str);
			Banner banner = Banner.CreateRandomClanBanner(-1);
			clan.InitializeClan(nameTextObject, textObject, culture, banner);
			clan.SetLeader(hero);
			clan.AddRenown(150f, true);


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
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney / 2, false);
			}
			else
			{
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney, false);
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

			if (targetSpouse != null)
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

				MobileParty targetSpouseMobileParty = clan.CreateNewMobileParty(targetSpouse);

				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10, true);
				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5, true);
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, targetSpouse, takeMoney / 2, false);
			}

			// 他们孩子处理
			if (isTogetherWithThireChildren)
			{
				DealTheirChildren(hero, clan);
			}

		}

		// 递归处理孩子树
		public void DealTheirChildren(Hero hero, Clan clan)
		{
			if (hero.Children.Count > 0)
			{
				hero.Children.ForEach(
					(chilredn) =>
					{
						NewClanAllocateForHero(hero, clan);
						DealTheirChildren(chilredn, clan);
					}
					);
			}
			
		}

		public void NewClanAllocateForHero(Hero hero, Clan clan )
		{
			if (hero.Clan == Clan.PlayerClan)
			{
				RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, hero);
				hero.Clan = clan;
				hero.CompanionOf = null;
				hero.ChangeState(Hero.CharacterStates.Active);
				hero.IsNoble = true;
				SetOccupationToLord(hero);
				if(hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
				{
					MobileParty chilrenMobileParty = clan.CreateNewMobileParty(hero);
					chilrenMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10, true);
					chilrenMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5, true);
					GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, 2000, false);
				}
				
			}
		}

		public static List<Settlement> GetCandidateSettlements()
		{
			Vec2 playerPosition = Hero.MainHero.PartyBelongedTo.Position2D;
			IEnumerable<Settlement> settlements = Hero.MainHero.Clan.Settlements;
			Func<Settlement, bool> func = new Func<Settlement, bool>((settlement) => (settlement.IsTown || settlement.IsCastle));
			return (from n in settlements.Where(func)
					orderby n.Position2D.Distance(playerPosition)
					select n).ToList<Settlement>();
		}

		public static void SetOccupationToLord(Hero hero)
		{
			if (hero.CharacterObject.Occupation == Occupation.Lord) return;

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
