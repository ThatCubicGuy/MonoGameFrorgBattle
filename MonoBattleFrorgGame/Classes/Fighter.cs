using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    internal class Fighter
    {
        protected uint BaseHp;
        protected uint BaseAtk;
        protected uint BaseDef;
        protected uint BaseSpd;
        protected uint MaxMana;
        protected uint MaxEnergy;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        internal class TurnEvent(Fighter caster, Fighter target, Func<Fighter, Fighter, string> eventAction)
        {
            private uint TurnCount;
            Fighter Caster { get; } = caster;
            Fighter Target { get; } = target;
            Func<Fighter, Fighter, string> EventAction { get; } = eventAction;
            public string ActivateEvent()
            {
                return EventAction(Caster, Target);
            }

            public uint Expire(uint turns = 1)
            {
                return TurnCount -= turns;
            }
        }
        protected List<TurnEvent> StartOfTurnEvents = [];
        protected List<TurnEvent> EndOfTurnEvents = [];
        private void StartOfTurnChecks()
        {
            string output = string.Empty;
            foreach (var item in StartOfTurnEvents)
            {
                output += item.ActivateEvent() + '\n';
                item.Expire();
            }
        }
        private void EndOfTurnChecks()
        {
            string output = string.Empty;
            foreach (var item in EndOfTurnEvents)
            {
                output += item.ActivateEvent() + '\n';
                item.Expire();
            }
        }
    }
}
