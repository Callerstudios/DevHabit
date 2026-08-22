using DevHabit.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.API.Database.Configurations
{
    public class HabitConfiguration: IEntityTypeConfiguration<Habit>
    {
        public void Configure(EntityTypeBuilder<Habit> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(500);
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.OwnsOne(h => h.Frequency);
            builder.OwnsOne(h => h.Target, targetBuilder =>
            {
                targetBuilder.Property(t => t.Unit).HasMaxLength(100);
            });
            builder.OwnsOne(h => h.Milestone);
            builder.HasMany(h => h.Tags)
                .WithMany()
                .UsingEntity<HabitTag>();
        }
    }
}
