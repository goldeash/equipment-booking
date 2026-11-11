using EquipmentBooking.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", table =>
        {
            table.HasComment("Groups of items with the same loan limit, usage guidelines and approval settings.");
            table.HasCheckConstraint("ck_categories_max_loan_days", "max_loan_days > 0");
        });
        builder.Property(category => category.Id).HasComment("Surrogate key.");
        builder.Property(category => category.Name).HasMaxLength(100).IsRequired().HasComment("Unique category name.");
        builder.HasIndex(category => category.Name).IsUnique().HasDatabaseName("ux_categories_name");
        builder.Property(category => category.Description).HasMaxLength(500).HasComment("Optional description.");
        builder.Property(category => category.MaxLoanDays).HasComment("Longest loan in days that can be approved automatically.");
        builder.Property(category => category.UsageGuidelines).HasMaxLength(2000).IsRequired()
            .HasComment("What the items may be used for; the assistant compares the stated purpose with this text.");
        builder.Property(category => category.IsSpecialised).HasDefaultValue(false).HasComment("Requests for items of a specialised category always go to a manager.");
        builder.Property(category => category.AutoApprovalEnabled).HasDefaultValue(true).HasComment("Switch for automatic approval of this category.");
    }
}
