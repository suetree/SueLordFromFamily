using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace SueLordFromFamily
{
    class HeroOperation
    {
		public static void NewClanAllocateForHero(Hero hero, Clan clan)
		{
			if (hero.Clan == Clan.PlayerClan)
			{
				RemoveCompanionAction.ApplyByFire(Hero.MainHero.Clan, hero);
				hero.Clan = clan;
				hero.CompanionOf = null;
				hero.ChangeState(Hero.CharacterStates.Active);
				hero.IsNoble = true;
				SetOccupationToLord(hero);

				if (hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge && hero.PartyBelongedTo == null)
				{
					MobileParty chilrenMobileParty = clan.CreateNewMobileParty(hero);
					chilrenMobileParty.ItemRoster.AddToCounts(DefaultItems.Grain, 10, true);
					chilrenMobileParty.ItemRoster.AddToCounts(DefaultItems.Meat, 5, true);
				}

				GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, 2000, false);
			}
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
