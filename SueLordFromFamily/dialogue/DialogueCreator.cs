using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace SueLordFromFamily.dialogue
{
    class DialogueCreator
    {
        private String id;
        private String inputOrder;
        private String outOrder;
        private String text;
        private ConditionDelegate condition;
        private ResultDelegate result;
        private bool isPlayer;

        public DialogueCreator Id(String id)
        {
            this.id = id;
            return this;
        }


        public DialogueCreator IsPlayer(bool isPlayer)
        {
            this.isPlayer = isPlayer;
            return this;
        }

        public DialogueCreator InputOrder(String inputOrder)
        {
            this.inputOrder = inputOrder;
            return this;
        }

        public DialogueCreator OutOrder(String outOrder)
        {
            this.outOrder = outOrder;
            return this;
        }

        public DialogueCreator Text(String text)
        {
            this.text = text;
            return this;
        }

        public DialogueCreator Condition(ConditionDelegate condition)
        {
            this.condition = condition;
            return this;
        }

        public DialogueCreator Result(ResultDelegate result)
        {
            this.result = result;
            return this;
        }

        public void CreateAndAdd(CampaignGameStarter campaignGameStarter)
        {
            if (this.isPlayer)
            {
                campaignGameStarter.AddRepeatablePlayerLine(this.id, this.inputOrder, this.outOrder, this.text,  NewCondition(this.condition), NewResult(this.result), 100, null);
            }
            else
            {
                campaignGameStarter.AddDialogLine(this.id, this.inputOrder, this.outOrder, this.text, NewCondition(this.condition), NewResult(this.result), 100, null);
            }
        }

        public delegate bool ConditionDelegate();
        public delegate void ResultDelegate();


        public ConversationSentence.OnConsequenceDelegate NewResult(ResultDelegate action)
        {
            if (null != action)
            {
                return new ConversationSentence.OnConsequenceDelegate(action);
            }else
            {
                return null;
            }
           
        }

        public ConversationSentence.OnConditionDelegate NewCondition(ConditionDelegate action)
        {
            if (null != action)
            {
                return new ConversationSentence.OnConditionDelegate(action);
            }
            else
            {
                return null;
            }
          
        }
    }
}
