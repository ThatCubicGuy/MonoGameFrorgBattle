using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.Ability;

namespace FrogBattle.Classes
{
    // rewrite #7 gazillion lmfao
    internal abstract class Ability
    {
        protected Ability(Character parent, Properties properties)
        {
            Parent = parent;
            Props = properties;
        }
        public Character Parent { get; }
        public Properties Props { get; }
        public Dictionary<Pools, Cost> Costs { get; }
        public record Properties
        (
            string Name,
            bool RepeatsTurn
        );
        public abstract bool Init();
        public abstract bool Use();
        protected Ability WithCost(Cost cost)
        {
            Costs[cost.Pool] = cost;
            return this;
        }
    }
    internal record Cost
    (
        Ability Parent,
        double Amount,
        Pools Pool,
        Operators Op
    );

    internal abstract class SingleTargetAttack : Ability
    {
        public SingleTargetAttack(Character source, Properties properties, Character target) : base(source, properties)
        {
            Target = target;
        }
        public Character Target { get; }
    }

    internal abstract class BlastAttack : Ability
    {
        public BlastAttack(Character source, Properties properties, Character mainTarget) : base(source, properties)
        {
            Target = mainTarget;
        }
        public Character Target { get; }
    }

    internal abstract class AoEAttack : Ability
    {
        public AoEAttack(Character source, Properties properties, params Character[] targets) : base(source, properties)
        {
            Targets = [.. targets];
        }
        public List<Character> Targets { get; }
    }

    internal abstract class BuffSelf : Ability
    {
        public BuffSelf(Character source, Properties properties, StatusEffect buff) : base(source, properties)
        {
            Buff = buff;
        }
        public StatusEffect Buff { get; }
    }
}
