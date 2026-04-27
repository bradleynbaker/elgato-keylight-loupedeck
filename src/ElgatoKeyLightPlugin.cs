using Loupedeck;

namespace ElgatoKeyLight
{
    public class ElgatoKeyLightPlugin : Plugin
    {
        public override bool HasNoApplication => true;
        public override bool UsesApplicationApiOnly => true;

        public override void Load()
        {
            try
            {
                this.Info.Icon16x16   = EmbeddedResources.ReadImage(AssemblyHelper.GetPluginIconPath("PluginIcon16x16.png"));
                this.Info.Icon32x32   = EmbeddedResources.ReadImage(AssemblyHelper.GetPluginIconPath("PluginIcon32x32.png"));
                this.Info.Icon48x48   = EmbeddedResources.ReadImage(AssemblyHelper.GetPluginIconPath("PluginIcon48x48.png"));
                this.Info.Icon256x256 = EmbeddedResources.ReadImage(AssemblyHelper.GetPluginIconPath("PluginIcon256x256.png"));
            }
            catch { /* icons optional — plugin loads without them */ }

            KeyLightService.Start();
        }

        public override void Unload()
        {
            KeyLightService.Stop();
        }

        public override void RunCommand(string commandName, string parameter) { }
        public override void ApplyAdjustment(string adjustmentName, string parameter, int diff) { }
    }

    internal static class AssemblyHelper
    {
        private static readonly string Namespace = typeof(ElgatoKeyLightPlugin).Namespace!;
        public static string GetPluginIconPath(string filename) => $"{Namespace}.images.{filename}";
    }
}
