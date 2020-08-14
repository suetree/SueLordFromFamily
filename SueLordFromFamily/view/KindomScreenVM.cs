using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.GameState;
namespace SueLordFromFamily.view
{
    class KindomScreenVM : ViewModel
    {

		GauntletKingdomScreen _parentScreen;

		private GauntletMovie _currentMovie;
		GauntletLayer _serviceLayer;

		VassalServiceVM clanServiceVM;

		[DataSourceProperty]
		public string BtnName
		{
			get
			{
				return new TextObject("{=sue_clan_create_from_family_clan_service}Clan Service", null).ToString();
			}
		}

		public KindomScreenVM(GauntletKingdomScreen gauntletClanScreen)
		{
			this._parentScreen = gauntletClanScreen;
		}

		public void ShowClanServiceView()
		{

			bool flag = this._serviceLayer == null;
			if (flag)
			{
				this._serviceLayer = new GauntletLayer(200, "GauntletLayer");
				this.clanServiceVM = new VassalServiceVM(this, this._parentScreen, new Action(OpenBannerEditorWithPlayerClan));
				this._currentMovie = this._serviceLayer.LoadMovie("VassalService", this.clanServiceVM);
				this._serviceLayer.IsFocusLayer = true;
				ScreenManager.TrySetFocus(this._serviceLayer);
				this._serviceLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
				this._parentScreen.AddLayer(this._serviceLayer);
				this._serviceLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
			}

		}

		private void OpenBannerEditorWithPlayerClan()
		{
			Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
		}

		public override void RefreshValues()
		{
			if (null != this.clanServiceVM)
			{
				clanServiceVM.RefreshValues();
			}
		}

		public void CloseSettingView()
		{
			bool flag = this._serviceLayer != null;
			if (flag)
			{
				this._serviceLayer.ReleaseMovie(this._currentMovie);
				this._parentScreen.RemoveLayer(this._serviceLayer);
				this._serviceLayer.InputRestrictions.ResetInputRestrictions();
				this._serviceLayer = null;
				this.clanServiceVM = null;
				this.RefreshValues();
			}
		}

		public bool IsHotKeyPressed(string hotkey)
		{
			bool flag = this._serviceLayer != null;
			return flag && this._serviceLayer.Input.IsHotKeyReleased(hotkey);
		}

	}
}
