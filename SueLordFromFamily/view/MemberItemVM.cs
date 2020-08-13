using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SueLordFromFamily.view
{
    class MemberItemVM :  ViewModel
    {
        private readonly Action<MemberItemVM> _onCharacterSelect;

        private readonly Hero _hero;

        private ImageIdentifierVM _visual;

        private ImageIdentifierVM _banner_9;

        private bool _isSelected;

        private bool _isChild;

        private bool _isMainHero;

        private bool _isFamilyMember;

        private string _name;

        private string _locationText;

        private string _relationToMainHeroText;

        private string _governorOfText;

        private string _currentActionText;

		[DataSourceProperty]
		public bool IsSelected
		{
			get
			{
				return this._isSelected;
			}
			set
			{
				if (value != this._isSelected)
				{
					this._isSelected = value;
					base.OnPropertyChanged("IsSelected");
				}
			}
		}

		[DataSourceProperty]
		public bool IsChild
		{
			get
			{
				return this._isChild;
			}
			set
			{
				if (value != this._isChild)
				{
					this._isChild = value;
					base.OnPropertyChanged("IsChild");
				}
			}
		}

		[DataSourceProperty]
		public bool IsMainHero
		{
			get
			{
				return this._isMainHero;
			}
			set
			{
				if (value != this._isMainHero)
				{
					this._isMainHero = value;
					base.OnPropertyChanged("IsMainHero");
				}
			}
		}

		[DataSourceProperty]
		public bool IsFamilyMember
		{
			get
			{
				return this._isFamilyMember;
			}
			set
			{
				if (value != this._isFamilyMember)
				{
					this._isFamilyMember = value;
					base.OnPropertyChanged("IsFamilyMember");
				}
			}
		}

		[DataSourceProperty]
		public ImageIdentifierVM Visual
		{
			get
			{
				return this._visual;
			}
			set
			{
				if (value != this._visual)
				{
					this._visual = value;
					base.OnPropertyChanged("Visual");
				}
			}
		}

		[DataSourceProperty]
		public ImageIdentifierVM Banner_9
		{
			get
			{
				return this._banner_9;
			}
			set
			{
				if (value != this._banner_9)
				{
					this._banner_9 = value;
					base.OnPropertyChanged("Banner_9");
				}
			}
		}

		[DataSourceProperty]
		public string LocationText
		{
			get
			{
				return this._locationText;
			}
			set
			{
				if (value != this._locationText)
				{
					this._locationText = value;
					base.OnPropertyChanged("LocationText");
				}
			}
		}

		[DataSourceProperty]
		public string RelationToMainHeroText
		{
			get
			{
				return this._relationToMainHeroText;
			}
			set
			{
				if (value != this._relationToMainHeroText)
				{
					this._relationToMainHeroText = value;
					base.OnPropertyChanged("RelationToMainHeroText");
				}
			}
		}

		[DataSourceProperty]
		public string GovernorOfText
		{
			get
			{
				return this._governorOfText;
			}
			set
			{
				if (value != this._governorOfText)
				{
					this._governorOfText = value;
					base.OnPropertyChanged("GovernorOfText");
				}
			}
		}

		[DataSourceProperty]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (value != this._name)
				{
					this._name = value;
					base.OnPropertyChanged("Name");
				}
			}
		}


		[DataSourceProperty]
        public string CurrentActionText
        {
            get
            {
                return this._currentActionText;
            }
            set
            {
                if (value != this._currentActionText)
                {
                    this._currentActionText = value;
                    base.OnPropertyChanged("CurrentActionText");
                }
            }
        }

        public MemberItemVM(Hero hero, Action<MemberItemVM> onCharacterSelect)
        {
			this._hero = hero;
			this._onCharacterSelect = onCharacterSelect;
			CharacterCode characterCode = CharacterCode.CreateFrom(hero.CharacterObject);
			this.Visual = new ImageIdentifierVM(characterCode);
			this.IsFamilyMember = Hero.MainHero.Clan.Nobles.Contains(this._hero);
			this.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(hero.ClanBanner), true);
			this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Name = this._hero.Name.ToString();
            this.CurrentActionText = ((this._hero != Hero.MainHero) ? CampaignUIHelper.GetHeroBehaviorText(this._hero) : "");
            if (this._hero.PartyBelongedToAsPrisoner != null)
            {
                TextObject textObject = new TextObject("{=a8nRxITn}Prisoner of {PARTY_NAME}", null);
                textObject.SetTextVariable("PARTY_NAME", this._hero.PartyBelongedToAsPrisoner.Name);
                this.LocationText = textObject.ToString();
            }
            else
            {
                this.LocationText = ((this._hero != Hero.MainHero) ? StringHelpers.GetLastKnownLocation(this._hero).ToString() : " ");
            }

        }
    }
}
