using System;
using Loupedeck;

namespace ElgatoKeyLight
{
    /// <summary>
    /// Button tile: shows live brightness % (or OFF / —), tap toggles on/off.
    /// Polls via KeyLightService every 2s and redraws on state change.
    /// </summary>
    internal class StatusButton : PluginDynamicCommand
    {
        private static readonly BitmapColor ColorBg     = new(20,  20,  20);
        private static readonly BitmapColor ColorOn     = new(255, 200, 80);   // warm amber = on
        private static readonly BitmapColor ColorOff    = new(100, 100, 100);  // grey = off
        private static readonly BitmapColor ColorUnknown = new(60,  60,  60);

        public StatusButton()
            : base("Key Light Status", "Tap to toggle · Shows live brightness", "Key Light")
        {
        }

        protected override void OnLoad()
        {
            KeyLightService.StateChanged += OnStateChanged;
        }

        protected override void OnUnload()
        {
            KeyLightService.StateChanged -= OnStateChanged;
        }

        protected override void RunCommand(string actionParameter)
        {
            var state = KeyLightService.State;
            if (state is null) return;

            _ = KeyLightService.SetAsync(on: 1 - state.On);
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            var bb = new BitmapBuilder(imageSize);
            bb.FillRectangle(0, 0, bb.Width, bb.Height, ColorBg);

            if (!KeyLightService.IsReachable || KeyLightService.State is null)
            {
                bb.DrawText("—", 0, 0, bb.Width, bb.Height, ColorUnknown, fontSize: 28);
                return bb.ToImage();
            }

            var state = KeyLightService.State;
            var isOn  = state.On == 1;
            var label = isOn ? $"{state.Brightness}%" : "OFF";
            var color = isOn ? ColorOn : ColorOff;

            // Small indicator dot at top
            bb.FillRectangle(bb.Width / 2 - 4, 6, 8, 8, color);

            // Main label
            bb.DrawText(label, 0, bb.Height / 4, bb.Width, bb.Height / 2, color, fontSize: 22);

            // Sub-label
            bb.DrawText(isOn ? "ON" : "OFF", 0, bb.Height * 3 / 4 - 6, bb.Width, 20,
                new BitmapColor(150, 150, 150), fontSize: 11);

            return bb.ToImage();
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            this.ActionImageChanged();
        }
    }
}
