using EquipmentBooking.Application.Common.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class RequesterReturnHistoryConfiguration : IEntityTypeConfiguration<RequesterReturnHistory>
{
    public void Configure(EntityTypeBuilder<RequesterReturnHistory> builder)
    {
        builder.HasNoKey();
        builder.ToView("v_requester_return_history");
    }
}
