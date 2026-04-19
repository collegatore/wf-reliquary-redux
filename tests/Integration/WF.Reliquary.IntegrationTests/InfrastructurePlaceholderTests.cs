using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class InfrastructurePlaceholderTests
{
    [Test]
    public void SqliteStorageHealthCheck_ImplementsApplicationPortWithoutCreatingDatabase()
    {
        IStorageHealthCheck healthCheck = new SqliteStorageHealthCheck();

        Assert.Multiple(() =>
        {
            Assert.That(healthCheck.ProviderName, Is.EqualTo("SQLite"));
            Assert.That(healthCheck.CanCreateConnectionString(), Is.True);
        });
    }
}
