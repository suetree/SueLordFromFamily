using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SueLordFromFamily.view
{
    class VassalServiceVM : ViewModel
    {

        KindomScreenVM parentView;
        GauntletKingdomScreen parentScreen;

        Action editClanBanner;

        MBBindingList<VassalClanVM> _clans;

        MBBindingList<MemberItemVM> _members;

        [DataSourceProperty]
        public MBBindingList<VassalClanVM> Clans
        {
            get
            {
                return this._clans;
            }
        }

        [DataSourceProperty]
        public MBBindingList<MemberItemVM> Members
        {
            get
            {
                return this._members;
            }
        }


        [DataSourceProperty]
        public string DisplayName
        {
            get
            {
                return new TextObject("{=sue_clan_create_from_family_clan_service}Clan Service", null).ToString();
            }
        }

        public VassalServiceVM(KindomScreenVM parent, GauntletKingdomScreen parentScreen, Action editClanBanner)
        {
            this.parentView = parent;
            this.parentScreen = parentScreen;
            this.editClanBanner = editClanBanner;

            this._clans = new MBBindingList<VassalClanVM>();
            this._members = new MBBindingList<MemberItemVM>();
            Kingdom kingdom = Hero.MainHero.MapFaction as Kingdom;
            if (kingdom.Clans.Count > 1)
            {
                IEnumerable<Clan> list = kingdom.Clans.Where(obj => obj != Clan.PlayerClan);
                list.ToList().ForEach(obj => this._clans.Add(new VassalClanVM(obj, new Action<VassalClanVM>(OnSelectVassal))));
                Clan clan = list.First();
                IEnumerable<Hero> heros = clan.Heroes;
                heros.ToList().ForEach(obj => this._members.Add(new MemberItemVM(obj, new Action<MemberItemVM>(OnSelectMember))));
            }
            

            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            if (null != this.Clans)
            {
                this.Clans.ToList().ForEach(obj => obj.RefreshValues());
            }
        }

        public void OnSelectVassal(VassalClanVM vassalItem)
        {

        }

        public void OnSelectMember(MemberItemVM vassalItem)
        {

        }

        public void EditClanBannar()
        {
            InformationManager.DisplayMessage(new InformationMessage("测试修改封臣"));

            Kingdom kingdom = Hero.MainHero.MapFaction as Kingdom;
            if(kingdom.Clans.Count > 1)
            {
                Clan clan = kingdom.Clans.Where(obj => obj != Clan.PlayerClan).First();
                if (null != clan)
                {
                    OpenBannerSelectionScreen(clan, clan.Leader);
                    //this.editClanBanner();
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("没有封臣"));
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("没有封臣"));
            }
           
           
        }

        private void OpenBannerSelectionScreen(Clan clan, Hero hero)
        {
            NewClanBannerEditorState state = new NewClanBannerEditorState(hero.CharacterObject, clan);

            //getClan.Invoke(editorState, BindingFlags.Public | BindingFlags.Instance, new Object[]);
            FieldInfo fieldInfoId = hero.CharacterObject.GetType().GetField("GameStateManager", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (null != fieldInfoId)
            {
                fieldInfoId.SetValue(state, Game.Current.GameStateManager);
            }
            if (null != Game.Current.GameStateManager.GameStateManagerListener)
            {
                Game.Current.GameStateManager.GameStateManagerListener.OnCreateState(state);
              
                // state.GameStateManager = Game.Current.GameStateManager;
            }
         
            Game.Current.GameStateManager.PushState(state, 0);
           // Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
        }

        public void ExecuteCloseWindow()
        {
            this.parentView.CloseSettingView();
            this.OnFinalize();
        }

        public new void OnFinalize()
        {
            base.OnFinalize();
            bool flag = Game.Current != null;
            if (flag)
            {
                Game.Current.AfterTick = (Action<float>)Delegate.Remove(Game.Current.AfterTick, new Action<float>(this.AfterTick));
            }
            this.parentView = null;

        }

        public void AfterTick(float dt)
        {
            bool flag = this.parentView.IsHotKeyPressed("Exit");
            if (flag)
            {
                this.ExecuteCloseWindow();
            }
        }
    }
}
