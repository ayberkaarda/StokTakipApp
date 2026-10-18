using NSubstitute;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class SkeletonTests
{
    [Fact]
    public void Skeleton_Calisir()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();

        Assert.NotNull(unitOfWork);
    }
}
