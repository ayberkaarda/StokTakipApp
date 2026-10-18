using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data;

public sealed class UnitOfWork(StokTakipDbContext context) : IUnitOfWork
{
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return new UnitOfWorkTransaction(context, transaction);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Geri alınan (veya commit edilmeden kapatılan) transaction sonrası izlenen varlıklar temizlenir;
    /// aksi halde bellekte kalan değişiklikler (ör. düşülmüş stok) aynı kapsamdaki sonraki
    /// SaveChanges çağrısıyla yanlışlıkla kalıcı hale gelebilir.
    /// </summary>
    private sealed class UnitOfWorkTransaction(StokTakipDbContext context, IDbContextTransaction transaction)
        : IUnitOfWorkTransaction
    {
        private bool _committed;
        private bool _disposed;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await transaction.CommitAsync(cancellationToken);
            _committed = true;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                context.ChangeTracker.Clear();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                await transaction.DisposeAsync();
            }
            finally
            {
                if (!_committed)
                {
                    context.ChangeTracker.Clear();
                }
            }
        }
    }
}
