using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace SueLordFromFamily
{
    class NewClanBannerEditorState : GameState
	{
		private IBannerEditorStateHandler _handler;

		public CharacterObject EditCharacter { set; get; }
		public Clan EditClan { set; get; }


		public NewClanBannerEditorState(CharacterObject character, Clan clan)
		{
			this.EditCharacter = character;
			this.EditClan = clan;
		}

		public override bool IsMenuState
		{
			get
			{
				return true;
			}
		}

		public IBannerEditorStateHandler Handler
		{
			get
			{
				return this._handler;
			}
			set
			{
				this._handler = value;
			}
		}

		public new Clan GetClan()
		{
			return this.EditClan;
		}

		public new CharacterObject GetCharacter()
		{
			return this.EditCharacter;
		}
	}
}
