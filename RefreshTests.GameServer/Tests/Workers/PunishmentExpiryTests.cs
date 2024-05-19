using NotEnoughLogs;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Workers;
using RefreshTests.GameServer.Logging;
using static Refresh.GameServer.Types.Roles.GameUserRole;

namespace RefreshTests.GameServer.Tests.Workers;

public class PunishmentExpiryTests : GameServerTest
{
    [Test]
    public void BannedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        using Logger logger = new(new []{ new NUnitSink() });
        
        PunishmentExpiryWorker worker = new();
        GameUser user = context.CreateUser();
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetAllUsersWithRole(Banned).Items, Is.Empty);
            Assert.That(user.Role, Is.EqualTo(User));
        });
        
        context.Database.BanUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Banned));
            Assert.That(context.Database.GetAllUsersWithRole(Banned).Items, Contains.Item(user));
        });
        
        worker.DoWork(logger, null!, context.Database);
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Banned));
        });

        context.Time.TimestampMilliseconds = 2000;
        worker.DoWork(logger, null!, context.Database);
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }

    [Test]
    public void RestrictedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        using Logger logger = new(new []{ new NUnitSink() });
        
        PunishmentExpiryWorker worker = new();
        
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(User));
        
        context.Database.RestrictUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        Assert.That(user.Role, Is.EqualTo(Restricted));
        
        worker.DoWork(logger, null!, context.Database);
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Restricted));
        });

        context.Time.TimestampMilliseconds = 2000;
        worker.DoWork(logger, null!, context.Database);
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }
}