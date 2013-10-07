using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AsyncOAuth;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Tools;
using VikingApi.ApiTools;
using VikingApi.AppClasses;

namespace UpdateTile
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private readonly UserBalance _balance = new UserBalance();
        private readonly Api _client = new Api();
        private ScheduledTask _task;

        protected CancellationTokenSource Cts = new CancellationTokenSource();

        public void CancelTask()
        {
            Cts.Cancel();
        }

        public void RenewToken()
        {
            Cts = new CancellationTokenSource();
        }

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            _task = task;
            await GetData((string)IsolatedStorageSettings.ApplicationSettings["sim"]);
        }

        public async Task<bool> GetData(string msisdn)
        {
            _client.RenewToken();
            _client.GetInfoFinished += client_GetDataFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await _client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), new KeyValuePair { name = "msisdn", content = msisdn });
            return true;
        }

        void client_GetDataFinished(object sender, GetInfoCompletedArgs args)
        {
            if (!args.Canceled)
            {
                if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                    return;
                _balance.Load(args.Json);
                if (_balance.Remaining == "expired")
                {
                    SetExpiredTile();
                }
                else
                {
                    SetTile();
                }
            }
            NotifyComplete();
        }

        private void SetTile()
        {
            var newTile = new FlipTileData
            {
                Count = (_balance.Remaining.Contains("day")) ? int.Parse(_balance.Remaining.Split(' ')[0]) : 1,
                BackContent = BuildInfoString()
            };
            ShellTile.ActiveTiles.FirstOrDefault().Update(newTile);
        }

        private void SetExpiredTile()
        {
            var newTile = new FlipTileData
            {
                Count = 0,
                BackContent = "expired"
            };
            ShellTile.ActiveTiles.FirstOrDefault().Update(newTile);
        }

        private string BuildInfoString()
        {
            var info = _balance.Credit + Environment.NewLine;
            info += _balance.Data.Split('/')[0].TrimEnd() + " MB" + Environment.NewLine;
            info += _balance.Sms.Split('/')[0].TrimEnd() + " SMS";
            return info;
        }
    }
}