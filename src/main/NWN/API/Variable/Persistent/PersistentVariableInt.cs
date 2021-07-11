using NWN.Services;

namespace NWN.API
{
  public sealed class PersistentVariableInt : PersistentVariable<int>
  {
    public override bool HasValue
    {
      get => ObjectStorageService.TryGetObjectStorage(Object, out ObjectStorage objectStorage) && objectStorage.ContainsInt(Prefix, Name);
    }

    public override void Delete()
    {
      ObjectStorageService.GetObjectStorage(Object).Remove(Prefix, Name);
    }

    public override int Value
    {
      get => ObjectStorageService.GetObjectStorage(Object).GetInt(Prefix, Name).GetValueOrDefault();
      set => ObjectStorageService.GetObjectStorage(Object).Set(Prefix, Name, value);
    }
  }
}
