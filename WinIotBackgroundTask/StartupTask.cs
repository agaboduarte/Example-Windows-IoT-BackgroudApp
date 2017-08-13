using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace WinIotBackgroundTask
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral Deferral = null;
        bool Running = false;
        bool RequestStop = false;
        ThreadPoolTimer PoolTimer = null;
        object Lock = new object();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;

            Deferral = taskInstance.GetDeferral();

            await LogAsync($"Start {DateTime.Now}");

            PoolTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromSeconds(5));
        }

        private async void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            lock (Lock)
            {
                if (Running || RequestStop) return;

                Running = true;
            }

            await LogAsync($"Ticker {DateTime.Now}");

            lock (Lock)
            {
                Running = false;
            }
        }

        private async void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            lock (Lock)
            {
                RequestStop = true;
            }

            if (PoolTimer != null) PoolTimer.Cancel();

            await LogAsync($"Canceled {DateTime.Now}");
        }

        private async Task LogAsync(string text)
        {
            var log = await ApplicationData.Current.LocalFolder.CreateFileAsync("run.log", CreationCollisionOption.OpenIfExists);

            await FileIO.AppendLinesAsync(log, new[] { text });
        }
    }
}
