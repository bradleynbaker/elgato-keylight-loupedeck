using System;
using Loupedeck;

namespace ElgatoKeyLight
{
    /// <summary>
    /// Knob: rotate = brightness ±5, press = toggle on/off.
    /// Value ring shows current brightness % (or OFF / — ).
    /// </summary>
    internal class BrightnessAdjustment : PluginDynamicAdjustment
    {
        public BrightnessAdjustment()
            : base("Brightness", "Rotate to adjust brightness · Press to toggle on/off", "Key Light", hasReset: true)
        {
        }

        protected override bool OnLoad()
        {
            KeyLightService.StateChanged += OnStateChanged;
            return true;
        }

        protected override bool OnUnload()
        {
            KeyLightService.StateChanged -= OnStateChanged;
            return true;
        }

        // Rotate CW = positive ticks, CCW = negative ticks
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            var state = KeyLightService.State;
            if (state is null) return;

            var next = Math.Clamp(state.Brightness + ticks * 5, 3, 100);
            _ = KeyLightService.SetAsync(brightness: next);
            this.AdjustmentValueChanged();
        }

        // Press (hasReset = true routes knob press here)
        protected override void RunCommand(string actionParameter)
        {
            var state = KeyLightService.State;
            if (state is null) return;

            _ = KeyLightService.SetAsync(on: 1 - state.On);
            this.AdjustmentValueChanged();
        }

        // Text shown on the Loupedeck knob value display
        protected override string GetAdjustmentValue(string actionParameter)
        {
            if (!KeyLightService.IsReachable) return "—";
            var state = KeyLightService.State;
            if (state is null) return "—";
            return state.On == 0 ? "OFF" : $"{state.Brightness}%";
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            this.AdjustmentValueChanged();
        }
    }
}
