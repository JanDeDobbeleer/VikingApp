using System.Threading;

namespace VikingApi.ApiTools
{
    public abstract class CancelAsyncTask
    {
        protected CancellationTokenSource Cts = new CancellationTokenSource();

        public void CancelTask()
        {
            Cts.Cancel();
        }

        public void RenewToken()
        {
            Cts = new CancellationTokenSource();
        }
    }
}
