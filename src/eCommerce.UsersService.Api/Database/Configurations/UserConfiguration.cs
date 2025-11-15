using eCommerce.UsersService.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.UsersService.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.FirstName)
            .HasMaxLength(25);

        builder.Property(x => x.LastName)
            .HasMaxLength(25);

        builder.Property(x => x.Email)
            .HasMaxLength(100);
    }
}
