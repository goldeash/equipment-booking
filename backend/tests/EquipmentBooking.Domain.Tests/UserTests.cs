using EquipmentBooking.Domain.Users;
using Shouldly;

namespace EquipmentBooking.Domain.Tests;

public sealed class UserTests
{
    [Fact]
    public void Fifth_failed_sign_in_locks_the_account()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);
        var now = TestData.Now;

        for (var attempt = 0; attempt < User.MaxFailedSignIns - 1; attempt++)
        {
            user.RegisterFailedSignIn(now);
        }

        user.IsLockedOut(now).ShouldBeFalse();
        user.RegisterFailedSignIn(now);
        user.IsLockedOut(now).ShouldBeTrue();
        user.IsLockedOut(now.Add(User.LockoutDuration)).ShouldBeFalse();
    }

    [Fact]
    public void Successful_sign_in_resets_the_counter()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);
        user.RegisterFailedSignIn(TestData.Now);

        user.RegisterSuccessfulSignIn();

        user.FailedSignInCount.ShouldBe(0);
        user.LockoutEndsAt.ShouldBeNull();
    }

    [Fact]
    public void Role_change_is_reported_only_when_the_role_differs()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);

        user.ChangeRole(RoleCodes.Employee).ShouldBeFalse();
        user.ChangeRole(RoleCodes.Manager).ShouldBeTrue();
        user.RoleCode.ShouldBe(RoleCodes.Manager);
    }

    [Fact]
    public void Privacy_notice_acknowledgement_keeps_the_first_time()
    {
        var user = new User("a@company.example", "Ann Lee", RoleCodes.Employee, 1);

        user.AcknowledgePrivacyNotice(TestData.Now);
        user.AcknowledgePrivacyNotice(TestData.Now.AddDays(1));

        user.PrivacyNoticeAcknowledgedAt.ShouldBe(TestData.Now);
    }

    [Fact]
    public void Refresh_token_rotation_revokes_the_old_token()
    {
        var current = new RefreshToken(1, "hash-1", TestData.Now, TestData.Now.AddDays(7));
        var next = new RefreshToken(1, "hash-2", TestData.Now, TestData.Now.AddDays(7));

        current.IsActive(TestData.Now).ShouldBeTrue();
        current.ReplaceWith(next, TestData.Now);

        current.IsActive(TestData.Now).ShouldBeFalse();
        current.ReplacedByToken.ShouldBe(next);
        next.IsActive(TestData.Now.AddDays(8)).ShouldBeFalse();
    }
}
