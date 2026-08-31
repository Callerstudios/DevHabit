using DevHabit.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.API.Database.Configurations;

public class GitHubAccessTokenConfiguration : IEntityTypeConfiguration<GitHubAccessToken>
{
    public void Configure(EntityTypeBuilder<GitHubAccessToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Token)
            .IsRequired().HasMaxLength(1000);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
