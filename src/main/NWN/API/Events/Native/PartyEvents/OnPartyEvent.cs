using System;
using NWN.Native.API;
using NWN.Services;

namespace NWN.API.Events
{
  public class OnPartyEvent : IEvent
  {
    public bool PreventEvent { get; set; }

    public PartyEventType EventType { get; private init; }

    public NwPlayer Player { get; private init; }

    public NwPlayer Target { get; private init; }

    NwObject IEvent.Context => Player;

    [NativeFunction(NWNXLib.Functions._ZN11CNWSMessage25HandlePlayerToServerPartyEP10CNWSPlayerh)]
    internal delegate int HandlePartyMessageHook(IntPtr pMessage, IntPtr pPlayer, byte nMinor);

    internal class Factory : NativeEventFactory<HandlePartyMessageHook>
    {
      public Factory(Lazy<EventService> eventService, HookService hookService) : base(eventService, hookService) {}

      protected override FunctionHook<HandlePartyMessageHook> RequestHook(HookService hookService)
        => hookService.RequestHook<HandlePartyMessageHook>(OnHandlePartyMessage, HookOrder.Early);

      private int OnHandlePartyMessage(IntPtr pMessage, IntPtr pPlayer, byte nMinor)
      {
        PartyEventType eventType = (PartyEventType)nMinor;

        if (!Enum.IsDefined(eventType))
        {
          return Hook.CallOriginal(pMessage, pPlayer, nMinor);
        }

        CNWSMessage message = new CNWSMessage(pMessage, false);
        uint oidTarget = message.PeekMessage<uint>(0) & 0x7FFFFFFF;

        OnPartyEvent eventData = ProcessEvent(new OnPartyEvent
        {
          EventType = eventType,
          Player = pPlayer != IntPtr.Zero ? new NwPlayer(new CNWSPlayer(pPlayer, false)) : null,
          Target = oidTarget.ToNwObject<NwPlayer>()
        });

        return (!eventData.PreventEvent && Hook.CallOriginal(pMessage, pPlayer, nMinor).ToBool()).ToInt();
      }
    }
  }
}
