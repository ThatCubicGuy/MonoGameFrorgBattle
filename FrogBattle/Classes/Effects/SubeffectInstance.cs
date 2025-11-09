using System;

namespace FrogBattle.Classes.Effects
{
    internal abstract class SubeffectInstance : ISubeffectInstance
    {
        public SubeffectInstance(IAttributeModifier ctx, ISubeffect definition)
        {
            Parent = ctx;
            Definition = definition;
        }
        public IAttributeModifier Parent { get; }
        public ISubeffect Definition { get; }

        double ISubeffectInstance.Amount => throw new NotImplementedException();

        public string GetLocalizedText() => Definition.GetLocalizedText(this);
        public object GetKey() => Definition.GetKey();
    }
    internal sealed class StaticSubeffectInstance : SubeffectInstance
    {
        public StaticSubeffectInstance(IAttributeModifier ctx, ISubeffect definition) : base(ctx, definition) { }

        public double Amount => Definition.GetAmount(this);

        // Use the following methods only after making sure that is a valid operation. They throw casting errors otherwise.
        public Damage GetDamage() => ((DamageOverTime)Definition).GetDamage(this);
        public Healing GetHealing() => ((HealingOverTime)Definition).GetHealing(this);
    }
    internal sealed class MutableSubeffectInstance : SubeffectInstance, IMutableSubeffectInstance
    {
        public MutableSubeffectInstance(IAttributeModifier ctx, IMutableEffect definition) : base(ctx, definition)
        {
            Amount = definition.GetAmount(this);
            MaxAmount = definition.GetMaxAmount(this);
        }

        public double Amount { get; set; }
        public double MaxAmount { get; }
    }
}
