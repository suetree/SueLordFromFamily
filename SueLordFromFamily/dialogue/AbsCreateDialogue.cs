using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SueLordFromFamily.dialogue
{
    public abstract class AbsCreateDialogue
    {
        public CampaignGameStarter CampaignGameStarter { set; get; }

        public AbsCreateDialogue(CampaignGameStarter campaignGameStarter)
        {
            this.CampaignGameStarter = campaignGameStarter;
        }

        public abstract void GenerateDialogue();

        public String LoactionText(String idStr)
        {
            return GameTexts.FindText(idStr, null).ToString();
        }
    }
}
