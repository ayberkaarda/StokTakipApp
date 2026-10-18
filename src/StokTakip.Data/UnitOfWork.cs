using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StokTakip.Domain.Exceptions;
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            TransactionDisindaysaTemizle();
            throw new EszamanlilikException();
        }
        catch (DbUpdateException ex) when (BenzersizlikIhlali(ex.InnerException))
        {
            TransactionDisindaysaTemizle();
            throw new KayitZatenVarException();
        }
        catch
        {
            TransactionDisindaysaTemizle();
            throw;
        }
    }

    private static bool BenzersizlikIhlali(Exception? exception)
    {
        if (exception is SqlException sqlException)
            return sqlException.Number is 2601 or 2627;

        // SQLite yalnızca testlerde yüklüdür; sağlayıcıya derleme bağımlılığı eklenmez.
        return exception?.GetType().FullName == "Microsoft.Data.Sqlite.SqliteException"
            && exception.GetType().GetProperty("SqliteExtendedErrorCode")?.GetValue(exception) is int kod
            && kod is 2067 or 1555;
    }

    /// <summary>
    /// Transaction dışındaki başarısız SaveChanges hiçbir şey yazmamıştır; izlenen değişiklikler
    /// bırakılırsa aynı kapsamdaki sonraki SaveChanges onları yeniden gönderir. Transaction içindeyken
    /// temizlik geri alma/kapatma sırasında yapılır.
    /// </summary>
    private void TransactionDisindaysaTemizle()
    {
        if (context.Database.CurrentTransaction is null)
        {
            context.ChangeTracker.Clear();
        }
    }

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
