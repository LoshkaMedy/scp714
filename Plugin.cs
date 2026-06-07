using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using MEC;
using System;
using System.Collections.Generic;

namespace scp714
{
    public class SUB : Plugin<Config>
    {
        public static SUB Instance { get; private set; }
        public override string Name => "SCP-714";
        public override string Description => "SCP-714, something that steals and gifts.";
        public override string Author => "LoshkaMedy";
        public override string ConfigFileName { get; set; } = "config.yml";
       
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 6);

        public static HashSet<ushort> CustomItems = new();
        private CoroutineHandle _coroutine;

        public static Dictionary<int, float> RingDeathTimers = new();

        public Handler Events { get; } = new();
        public override void Enable()
        {
            Instance = this;

            CustomHandlersManager.RegisterEventsHandler(Events);
            _coroutine = Timing.RunCoroutine(Events.RingCoroutine());

            LabApi.Features.Console.Logger.Info("SCP-714 loaded");
        }

        public override void Disable()
        {
            CustomHandlersManager.UnregisterEventsHandler(Events);
            Timing.KillCoroutines(_coroutine);
            CustomItems.Clear();

            LabApi.Features.Console.Logger.Info("SCP-714 unloaded");
        }
    }
}