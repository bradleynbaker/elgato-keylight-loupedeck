using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ElgatoKeyLight
{
    internal record KeyLightState(int On, int Brightness);

    internal static class KeyLightService
    {
        private const string BaseUrl = "http://192.168.1.189:9123";
        private const int PollIntervalMs = 2000;

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(1) };
        private static Timer? _timer;

        public static KeyLightState? State { get; private set; }
        public static bool IsReachable { get; private set; }

        public static event EventHandler? StateChanged;

        public static void Start()
        {
            _timer = new Timer(_ => _ = PollAsync(), null, 0, PollIntervalMs);
        }

        public static void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public static async Task<KeyLightState?> FetchAsync()
        {
            var json = await Http.GetStringAsync($"{BaseUrl}/elgato/lights");
            var node = JsonNode.Parse(json)?["lights"]?[0];
            if (node is null) return null;
            return new KeyLightState(
                node["on"]!.GetValue<int>(),
                node["brightness"]!.GetValue<int>()
            );
        }

        public static async Task SetAsync(int? on = null, int? brightness = null)
        {
            var light = new JsonObject();
            if (on.HasValue) light["on"] = on.Value;
            if (brightness.HasValue) light["brightness"] = brightness.Value;

            var payload = new JsonObject
            {
                ["numberOfLights"] = 1,
                ["lights"] = new JsonArray { light }
            };

            var content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
            await Http.PutAsync($"{BaseUrl}/elgato/lights", content);

            // Immediately sync so callers see the new state without waiting for next poll
            State = await FetchAsync();
            IsReachable = true;
            StateChanged?.Invoke(null, EventArgs.Empty);
        }

        private static async Task PollAsync()
        {
            try
            {
                State = await FetchAsync();
                IsReachable = true;
            }
            catch
            {
                IsReachable = false;
            }

            StateChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
