using Avalonia;
using Avalonia.Markup.Xaml;

namespace SpockWallet
{
    public class App : Application
    {
        public const int FastSyncTime = 2000;
        public const int MediumSyncTime = 5000;
        public const int SlowSyncTime = 30000;

        public const string Version = "v1.1.0";
        public static int SyncTimeSpan = SlowSyncTime;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
