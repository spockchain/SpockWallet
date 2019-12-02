using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpockWallet.Services
{
    public class ScanLocationUpdate
    {
        public static ConcurrentQueue<WalletScanLocationUpdated> Queue { get; }

        private CancellationToken CancellationToken;

        static ScanLocationUpdate()
        {
            Queue = new ConcurrentQueue<WalletScanLocationUpdated>();
        }

        public void Start(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;

            while (true)
            {
                if (Queue.TryDequeue(out WalletScanLocationUpdated updated))
                {
                    using (DataContext dataContext = new DataContext())
                    {
                        var wallet = dataContext.Wallets.Where(p => p.Address == updated.Address).FirstOrDefault();
                        if (wallet != null)
                        {
                            wallet.ScanLocation = updated.ScanLocation;
                            dataContext.Update(wallet);
                            dataContext.SaveChanges();
                        }
                    }
                }
                else if (CancellationToken.IsCancellationRequested)
                {
                    RxApp.MainThreadScheduler.Schedule(new SyncShutdown(), (schedule, state) =>
                    {
                        MessageBus.Current.SendMessage(state);

                        return new ScanDisposable();
                    });
                    break;
                }
            }
        }
    }
}
