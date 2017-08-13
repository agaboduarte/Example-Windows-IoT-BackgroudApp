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
        bool RequestStop = false;
        BackgroundTaskDeferral Deferral = null;
        ThreadPoolTimer PoolTimer = null;
        TimerElapsedHandler Handler = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;

            Deferral = taskInstance.GetDeferral();

            await LogAsync($"Start {DateTime.Now}");

            Handler = new TimerElapsedHandler(PoolTimerCallback);
            PoolTimer = ThreadPoolTimer.CreateTimer(Handler, TimeSpan.FromSeconds(5));
        }

        private async void PoolTimerCallback(ThreadPoolTimer timer)
        {
            // If the task is very time-consuming, call CheckStop periodically to verify the cancellation of the work
            if (CheckStop()) return;

            await LogAsync($"Ticker {DateTime.Now}");

            // 
            // TODO: Insert code to perform background work
            //

            PoolTimer = ThreadPoolTimer.CreateTimer(Handler, timer.Delay);
        }

        private async void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            RequestStop = true;

            if (PoolTimer != null) PoolTimer.Cancel();

            await LogAsync($"Canceled {DateTime.Now}");
        }

        private async Task LogAsync(string text)
        {
            var log = await ApplicationData.Current.LocalFolder.CreateFileAsync("run.log", CreationCollisionOption.OpenIfExists);

            await FileIO.AppendLinesAsync(log, new[] { text });
        }

        private bool CheckStop()
        {
            if (!RequestStop) return false;

            // http://aka.ms/backgroundtaskdeferral
            Deferral.Complete();

            return true;
        }
    }
}
