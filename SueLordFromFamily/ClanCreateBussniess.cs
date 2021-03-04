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
    class ClanCreateBussniess
    {
		public Hero targetSpouse { set; get; }
		public Settlement targetSettlement { set; get; }
		public int selectClanTier = 2;

		public bool isTogetherWithThireChildren;

		public void reset()
		{
			this.targetSpouse = null;
			this.targetSettlement = null;
			this.isTogetherWithThireChildren = false;
		
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
			TextObject clanName = NameGenerator.Current.GenerateClanName(culture, targetSettlement);
			string str = Guid.NewGuid().ToString().Replace("-", "");

			if (null  == hero.LastSeenPlace)
			{
				hero.CacheLastSeenInformation(hero.HomeSettlement, true);
				hero.SyncLastSeenInformation();
			}
			//RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, hero);
			//for  1.4.3 200815
			HeroOperation.DealApplyByFire(Hero.MainHero.Clan, hero);

			HeroOperation.SetOccupationToLord(hero);
			hero.ChangeState(Hero.CharacterStates.Active);
			Clan clan = TaleWorlds.ObjectSystem.MBObjectManager.Instance.CreateObject<Clan>("sue_clan_" + str);
			Banner banner = Banner.CreateRandomClanBanner(-1);
			clan.InitializeClan(clanName, clanName, culture, banner);
			clan.SetLeader(hero);

			//clan.Tier = 5; 修改家族等级
			 FieldInfo fieldInfoId = clan.GetType().GetField("_tier", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (null != fieldInfoId)
			{
				fieldInfoId.SetValue(clan, selectClanTier);
			}
			//增加一些影响力
			clan.AddRenown(50 * selectClanTier, true);


			hero.Clan = clan;
			hero.CompanionOf = null;
			hero.IsNoble = true;
			hero.SetTraitLevel(DefaultTraits.Commander, 1);

			MobileParty mobileParty = clan.CreateNewMobileParty(hero);
			mobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10);
			mobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5);

			ChangeOwnerOfSettlementAction.ApplyByKingDecision(hero, targetSettlement);
			clan.UpdateHomeSettlement(targetSettlement);


			int takeMoney = TakeMoneyByTier(selectClanTier);
			if (targetSpouse != null)
			{
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney / 2, false);
			}
			else
			{
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, takeMoney, false);
			}

			//关系处理
			//新晋家族关系增加
			int shipIncreate = ShipIncreateByTier(selectClanTier);
			ChangeRelationAction.ApplyPlayerRelation(hero, shipIncreate, true, true);
			if (targetSpouse != null)
			{
				ChangeRelationAction.ApplyPlayerRelation(targetSpouse, shipIncreate, true, true);
			}

			//以前家族关系减低，
			int shipReduce = ShipReduceByTier(selectClanTier);
			Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
			if(null != kindom && shipReduce > 0)
			{
				kindom.Clans.ToList().ForEach((obj) => 
				{
					if (obj != Clan.PlayerClan)
					{
						ChangeRelationAction.ApplyPlayerRelation(obj.Leader, shipReduce * -1, true, true);
					}
				}
				);
			}

			if (targetSpouse != null)
			{
				targetSpouse.Spouse = hero;
				InformationManager.AddQuickInformation(new TextObject($"{hero.Name} marry with {targetSpouse.Name}"), 0, null, "event:/ui/notification/quest_finished");

				HeroOperation.DealApplyByFire(Hero.MainHero.Clan, targetSpouse);
				//RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, targetSpouse);

				targetSpouse.ChangeState(Hero.CharacterStates.Active);
				targetSpouse.IsNoble = true;
				HeroOperation.SetOccupationToLord(targetSpouse);
				targetSpouse.CompanionOf = null;
				targetSpouse.Clan = clan;
				targetSpouse.SetTraitLevel(DefaultTraits.Commander, 1);
				//AddCompanionAction.Apply(clan, targetSpouse);

				MobileParty targetSpouseMobileParty = clan.CreateNewMobileParty(targetSpouse);

				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10);
				targetSpouseMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5);
				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, targetSpouse, takeMoney / 2, false);
			}

			// 他们孩子处理
			if (isTogetherWithThireChildren)
			{
				DealTheirChildren(hero, clan);
			}


			//加入王国
			ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, true);

		}

		public int TakeMoneyByTier(int tier)
		{
			// y=5(x次方) * 10 * 1000；
			return (int)Math.Pow(5, tier) * 1000;
		}

		/**
		 * 关系增加关系
		 * 应该家族越少值越高
		 */
		public int ShipIncreateByTier(int tier)
		{
			return tier * 10;
		}

		/**
		 * 关系降低
		 * 根据玩家王国，的家族数进行计算，家族越多，评分到每个人的关系应该更低
		 */
		public int ShipReduceByTier(int tier)
		{
			int total = tier * 10;
			Kingdom kindom = Hero.MainHero.MapFaction as Kingdom;
			if (null != kindom && kindom.Clans.Count  >= 3)
			{
				total = total / (kindom.Clans.Count - 1);
			}
			return total;
		}


		// 递归处理孩子树
		public void DealTheirChildren(Hero hero, Clan clan)
		{
			if (hero.Children.Count > 0)
			{
				hero.Children.ForEach(
					(chilredn) =>
					{
						HeroOperation.NewClanAllocateForHero(chilredn, clan);
						DealTheirChildren(chilredn, clan);
					}
					);
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




	}
}
