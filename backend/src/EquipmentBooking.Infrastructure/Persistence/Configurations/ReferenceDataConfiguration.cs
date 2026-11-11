using EquipmentBooking.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

/// <summary>Shared mapping of fixed reference tables with a text code as primary key.</summary>
internal abstract class ReferenceDataConfiguration<T> : IEntityTypeConfiguration<T>
    where T : ReferenceData
{
    public const int CodeLength = 40;
    public const int NameLength = 100;

    private readonly string _table;
    private readonly string _comment;

    protected ReferenceDataConfiguration(string table, string comment)
    {
        _table = table;
        _comment = comment;
    }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(_table, table => table.HasComment(_comment));
        builder.HasKey(item => item.Code);
        builder.Property(item => item.Code).HasMaxLength(CodeLength).HasComment("Stable code used by the application and constraints.");
        builder.Property(item => item.Name).HasMaxLength(NameLength).IsRequired().HasComment("Display name in English.");
        builder.HasData(SeedData());
    }

    protected abstract IEnumerable<T> SeedData();
}
