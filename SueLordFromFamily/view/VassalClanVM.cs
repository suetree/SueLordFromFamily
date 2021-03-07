using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SueLordFromFamily.view
{
    class VassalClanVM : ViewModel
    {
        private readonly Action<VassalClanVM> _onSelect;

        public readonly Clan Clan;

        private string _name;


        private ImageIdentifierVM _visual;

        private ImageIdentifierVM _banner;

        private ImageIdentifierVM _banner_9;

        private MBBindingList<HeroVM> _members;

        private MBBindingList<KingdomClanFiefItemVM> _fiefs;

        private int _influence;

        private int _numOfMembers;

        private int _numOfFiefs;

        private string _tierText;

        private int _clanType = -1;

        [DataSourceProperty]
        public string EditVassalBannerText
        {
            get
            {
                return new TextObject("{=sue_clan_create_from_family_edit_banner}Edit Banner", null).ToString();
            }

        }

        [DataSourceProperty]
        public string EditVassalNameText
        {
            get
            {
                return new TextObject("{=sue_clan_create_from_family_edit_name}Edit Name", null).ToString();
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
        public int ClanType
        {
            get
            {
                return this._clanType;
            }
            set
            {
                if (value != this._clanType)
                {
                    this._clanType = value;
                    base.OnPropertyChanged("ClanType");
                }
            }
        }

        [DataSourceProperty]
        public int NumOfMembers
        {
            get
            {
                return this._numOfMembers;
            }
            set
            {
                if (value != this._numOfMembers)
                {
                    this._numOfMembers = value;
                    base.OnPropertyChanged("NumOfMembers");
                }
            }
        }

        [DataSourceProperty]
        public int NumOfFiefs
        {
            get
            {
                return this._numOfFiefs;
            }
            set
            {
                if (value != this._numOfFiefs)
                {
                    this._numOfFiefs = value;
                    base.OnPropertyChanged("NumOfFiefs");
                }
            }
        }

        [DataSourceProperty]
        public string TierText
        {
            get
            {
                return this._tierText;
            }
            set
            {
                if (value != this._tierText)
                {
                    this._tierText = value;
                    base.OnPropertyChanged("TierText");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get
            {
                return this._banner;
            }
            set
            {
                if (value != this._banner)
                {
                    this._banner = value;
                    base.OnPropertyChanged("Banner");
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
        public MBBindingList<HeroVM> Members
        {
            get
            {
                return this._members;
            }
            set
            {
                if (value != this._members)
                {
                    this._members = value;
                    base.OnPropertyChanged("Members");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<KingdomClanFiefItemVM> Fiefs
        {
            get
            {
                return this._fiefs;
            }
            set
            {
                if (value != this._fiefs)
                {
                    this._fiefs = value;
                    base.OnPropertyChanged("Fiefs");
                }
            }
        }


        [DataSourceProperty]
        public int Influence
        {
            get
            {
                return this._influence;
            }
            set
            {
                if (value != this._influence)
                {
                    this._influence = value;
                    base.OnPropertyChanged("Influence");
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

        public VassalClanVM(Clan clan, Action<VassalClanVM> onSelect)
        {
            this.Clan = clan;
            this._onSelect = onSelect;

            this.RefreshValues();
            Refresh();
        }

        public void EditClanBanner()
        {
            this.OpenBannerSelectionScreen(this.Clan, this.Clan.Leader);
        }

        public void EditClanName()
        {
            InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=JJiKk4ow}Select your family name: ", null).ToString(), string.Empty, true, false, GameTexts.FindText("str_done", null).ToString(), null, new Action<string>(this.OnChangeClanNameDone), null, false, new Func<string, bool>(this.IsNewClanNameApplicable), ""), false);
        }

        private bool IsNewClanNameApplicable(string input)
        {
            return input.Length <= 50 && input.Length >= 1;
        }

        private void OnChangeClanNameDone(string newClanName)
        {
            TextObject textObject = new TextObject(newClanName ?? "", null);
            this.Clan.InitializeClan(textObject, textObject, this.Clan.Culture, this.Clan.Banner, default);
            this.Name = textObject.ToString();


        }

        private void OpenBannerSelectionScreen(Clan clan, Hero hero)
        {
            NewClanBannerEditorState state = new NewClanBannerEditorState(hero.CharacterObject, clan);
            if (null != Game.Current.GameStateManager.GameStateManagerListener)
            {
                Game.Current.GameStateManager.GameStateManagerListener.OnCreateState(state);
                // state.GameStateManager = Game.Current.GameStateManager;
            }
            Game.Current.GameStateManager.PushState(state, 0);
            // Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            // CharacterCode characterCode = CharacterCode.CreateFrom(this.Clan.Leader.CharacterObject);
            CharacterCode characterCode = CampaignUIHelper.GetCharacterCode(this.Clan.Leader.CharacterObject, false);
            this.Visual = new ImageIdentifierVM(characterCode);
            this.Banner = new ImageIdentifierVM(this.Clan.Banner);
            this.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(this.Clan.Banner), true);

            this.Name = this.Clan.Name.ToString();
            GameTexts.SetVariable("TIER", this.Clan.Tier);
            this.TierText = GameTexts.FindText("str_clan_tier", null).ToString();
        }

        public void Refresh()
        {
            this.Members = new MBBindingList<HeroVM>();
            this.ClanType = 0;
            if (this.Clan.IsUnderMercenaryService)
            {
                this.ClanType = 2;
            }
            else if (this.Clan.Kingdom.RulingClan == this.Clan)
            {
                this.ClanType = 1;
            }
            /*IEnumerable<Hero> arg_81_0 = this.Clan.Heroes.Union(this.Clan.Companions);
			Func<Hero, bool> arg_81_1;
			if ((arg_81_1 = KingdomClanItemVM.<> c.<> 9__5_0) == null)
			{
				arg_81_1 = (KingdomClanItemVM.<> c.<> 9__5_0 = new Func<Hero, bool>(KingdomClanItemVM.<> c.<> 9.< Refresh > b__5_0));
			}
			foreach (Hero current in arg_81_0.Where(arg_81_1))
			{
				this.Members.Add(new HeroVM(current));
			}*/
            this.NumOfMembers = this.Members.Count;
            this.Fiefs = new MBBindingList<KingdomClanFiefItemVM>();
            IEnumerable<Settlement> arg_100_0 = this.Clan.Settlements;
            Func<Settlement, bool> arg_100_1;
            /*if ((arg_100_1 = KingdomClanItemVM.<> c.<> 9__5_1) == null)
			{
				arg_100_1 = (KingdomClanItemVM.<> c.<> 9__5_1 = new Func<Settlement, bool>(KingdomClanItemVM.<> c.<> 9.< Refresh > b__5_1));
			}
			foreach (Settlement current2 in arg_100_0.Where(arg_100_1))
			{
				this.Fiefs.Add(new KingdomClanFiefItemVM(current2));
			}*/
            this.NumOfFiefs = this.Fiefs.Count;
            this.Influence = (int)this.Clan.Influence;
        }

    }
}
