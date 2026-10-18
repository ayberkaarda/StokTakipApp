using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace StokTakip.Presenters
{
    internal static class ServisKapsami
    {
        public static async Task<TSonuc> CalistirAsync<TServis, TSonuc>(
            this IServiceScopeFactory scopeFactory, Func<TServis, Task<TSonuc>> islem)
            where TServis : class
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            return await islem(scope.ServiceProvider.GetRequiredService<TServis>());
        }

        public static async Task CalistirAsync<TServis>(
            this IServiceScopeFactory scopeFactory, Func<TServis, Task> islem)
            where TServis : class
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await islem(scope.ServiceProvider.GetRequiredService<TServis>());
        }
    }
}
