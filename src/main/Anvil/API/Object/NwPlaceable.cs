using System;
using System.Numerics;
using System.Threading.Tasks;
using NWN.Core;
using NWN.Native.API;

namespace Anvil.API
{
  [NativeObjectInfo(ObjectTypes.Placeable, ObjectType.Placeable)]
  public sealed partial class NwPlaceable : NwStationary
  {
    internal readonly CNWSPlaceable Placeable;

    internal NwPlaceable(CNWSPlaceable placeable) : base(placeable)
    {
      Placeable = placeable;
      Inventory = new Inventory(this, placeable.m_pcItemRepository);
    }

    public static implicit operator CNWSPlaceable(NwPlaceable placeable)
    {
      return placeable?.Placeable;
    }

    public override float Rotation
    {
      get => (360 - NWScript.GetFacing(this)) % 360;
      set
      {
        float radians = (360 - value % 360) * NwMath.DegToRad;
        Vector3 orientation = new Vector3(MathF.Cos(radians), MathF.Sin(radians), 0.0f);
        Placeable.SetOrientation(orientation.ToNativeVector());
      }
    }

    public override bool KeyAutoRemoved
    {
      get => Placeable.m_bAutoRemoveKey.ToBool();
      set => Placeable.m_bAutoRemoveKey = value.ToInt();
    }

    public bool Occupied
    {
      get => NWScript.GetSittingCreature(this) != Invalid;
    }

    public NwCreature SittingCreature
    {
      get => NWScript.GetSittingCreature(this).ToNwObject<NwCreature>();
    }

    /// <summary>
    /// Gets the inventory of this placeable.
    /// </summary>
    public Inventory Inventory { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this placeable should illuminate.
    /// </summary>
    public bool Illumination
    {
      get => NWScript.GetPlaceableIllumination(this).ToBool();
      set => NWScript.SetPlaceableIllumination(this, value.ToInt());
    }

    /// <summary>
    /// Gets or sets a value indicating whether this placeable should be useable (clickable).
    /// </summary>
    public bool Useable
    {
      get => NWScript.GetUseableFlag(this).ToBool();
      set => NWScript.SetUseableFlag(this, value.ToInt());
    }

    public bool IsStatic
    {
      get => Placeable.m_bStaticObject.ToBool();
      set => Placeable.m_bStaticObject = value.ToInt();
    }

    /// <summary>
    /// Gets or sets a value indicating whether this placeable has an inventory.
    /// </summary>
    public bool HasInventory
    {
      get => NWScript.GetHasInventory(this).ToBool();
      set => Placeable.m_bHasInventory = value.ToInt();
    }

    /// <summary>
    /// Gets or sets the dialog ResRef for this placeable.
    /// </summary>
    public string DialogResRef
    {
      get => Placeable.GetDialogResref().ToString();
      set => Placeable.m_cDialog = new CResRef(value);
    }

    /// <summary>
    /// Moves the specified item/item stack to this placeable's inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public async Task GiveItem(NwItem item)
    {
      NwObject assignTarget;
      if (item.Possessor != null)
      {
        assignTarget = item.Possessor;
      }
      else
      {
        assignTarget = item.Area;
      }

      if (assignTarget != this)
      {
        await assignTarget.WaitForObjectContext();
        NWScript.ActionGiveItem(item, this);
      }
    }

    /// <summary>
    /// Moves a specified amount of items from an item stack to this placeable's inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The number of items from the item stack to take.</param>
    public async Task GiveItem(NwItem item, int amount)
    {
      if (amount > item.StackSize)
      {
        amount = item.StackSize;
      }

      if (amount == item.StackSize)
      {
        await GiveItem(item);
        return;
      }

      NwItem clone = item.Clone(this);
      clone.StackSize = amount;
      item.StackSize -= amount;
    }

    public static NwPlaceable Create(string template, Location location, bool useAppearAnim = false, string newTag = "")
    {
      location = Location.Create(location.Area, location.Position, location.FlippedRotation);
      return CreateInternal<NwPlaceable>(template, location, useAppearAnim, newTag);
    }

    /// <summary>
    /// Determines whether the specified action can be performed on this placeable.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <returns>true if the specified action can be performed, otherwise false.</returns>
    public bool IsPlaceableActionPossible(PlaceableAction action)
    {
      return NWScript.GetIsPlaceableObjectActionPossible(this, (int)action).ToBool();
    }

    public unsafe void AcquireItem(NwItem item, bool displayFeedback = true)
    {
      if (item == null)
      {
        throw new ArgumentNullException(nameof(item), "Item cannot be null.");
      }

      void* pItem = item.Item;
      Placeable.AcquireItem(&pItem, Invalid, 0xFF, 0xFF, displayFeedback.ToInt());
    }

    /// <summary>
    /// Gets this placeable's base save value for the specified saving throw.
    /// </summary>
    /// <param name="savingThrow">The type of saving throw.</param>
    /// <returns>The creature's base saving throw value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if savingThrow is not Fortitude, Reflex, or Will.</exception>
    public sbyte GetBaseSavingThrow(SavingThrow savingThrow)
    {
      return savingThrow switch
      {
        SavingThrow.Fortitude => Placeable.m_nFortSave.AsSByte(),
        SavingThrow.Reflex => Placeable.m_nReflexSave.AsSByte(),
        SavingThrow.Will => Placeable.m_nWillSave.AsSByte(),
        _ => throw new ArgumentOutOfRangeException(nameof(savingThrow), savingThrow, null),
      };
    }

    /// <summary>
    /// Sets this placeable's base save value for the specified saving throw.
    /// </summary>
    /// <param name="savingThrow">The type of saving throw.</param>
    /// <param name="newValue">The new base saving throw.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if savingThrow is not Fortitude, Reflex, or Will.</exception>
    public void SetBaseSavingThrow(SavingThrow savingThrow, sbyte newValue)
    {
      switch (savingThrow)
      {
        case SavingThrow.Fortitude:
          Placeable.m_nFortSave = newValue.AsByte();
          break;
        case SavingThrow.Reflex:
          Placeable.m_nReflexSave = newValue.AsByte();
          break;
        case SavingThrow.Will:
          Placeable.m_nWillSave = newValue.AsByte();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(savingThrow), savingThrow, null);
      }
    }

    public override byte[] Serialize()
    {
      return NativeUtils.SerializeGff("UTP", (resGff, resStruct) =>
      {
        Placeable.SaveObjectState(resGff, resStruct);
        return Placeable.SavePlaceable(resGff, resStruct, 0).ToBool();
      });
    }

    public static NwPlaceable Deserialize(byte[] serialized)
    {
      CNWSPlaceable placeable = null;

      bool result = NativeUtils.DeserializeGff(serialized, (resGff, resStruct) =>
      {
        if (!resGff.IsValidGff("UTP"))
        {
          return false;
        }

        placeable = new CNWSPlaceable(Invalid);
        if (placeable.LoadPlaceable(resGff, resStruct, null).ToBool())
        {
          placeable.LoadObjectState(resGff, resStruct);
          GC.SuppressFinalize(placeable);
          return true;
        }

        placeable.Dispose();
        return false;
      });

      return result && placeable != null ? placeable.ToNwObject<NwPlaceable>() : null;
    }

    private protected override void AddToArea(CNWSArea area, float x, float y, float z)
    {
      Placeable.AddToArea(area, x, y, z, true.ToInt());

      // If the placeable is trapped it needs to be added to the area's trap list for it to be detectable by players.
      if (IsTrapped)
      {
        area.m_pTrapList.Add(this);
      }
    }
  }
}
