using System;
using NWN.Core;

namespace Anvil.API
{
  public sealed partial class Talent : EngineStructure
  {
    internal Talent(IntPtr handle) : base(handle) {}

    protected override int StructureId
    {
      get => NWScript.ENGINE_STRUCTURE_TALENT;
    }

    public static implicit operator Talent(IntPtr intPtr)
    {
      return new Talent(intPtr);
    }

    /// <summary>
    /// Gets the associated spell, if this talent is a spell.
    /// </summary>
    public Spell Spell
    {
      get => (Spell)TryGetId(TalentType.Spell);
    }

    /// <summary>
    /// Gets the associated feat, if this talent is a feat.
    /// </summary>
    public Feat Feat
    {
      get => (Feat)TryGetId(TalentType.Feat);
    }

    /// <summary>
    /// Gets the associated skill, if this talent is a skill.
    /// </summary>
    public Skill Skill
    {
      get => (Skill)TryGetId(TalentType.Skill);
    }

    /// <summary>
    /// Gets the type of this talent (Spell/Feat/Skill).
    /// </summary>
    public TalentType Type
    {
      get => (TalentType)NWScript.GetTypeFromTalent(this);
    }

    /// <summary>
    /// Gets a value indicating whether this talent is valid.
    /// </summary>
    public bool Valid
    {
      get => NWScript.GetIsTalentValid(this).ToBool();
    }

    private int TryGetId(TalentType expectedType)
    {
      if (Type != expectedType)
      {
        throw new Exception($"Expected talent to be {expectedType}, but it is {Type}!");
      }

      return NWScript.GetIdFromTalent(this);
    }
  }
}
