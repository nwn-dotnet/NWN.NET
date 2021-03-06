using System;

namespace Anvil.API
{
  public sealed class VisualTransformLerpSettings
  {
    public VisualTransformLerpType LerpType { get; set; } = VisualTransformLerpType.Linear;

    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(2);

    public bool PauseWithGame { get; set; } = true;

    public bool ReturnDestinationTransform { get; set; } = false;
  }
}
