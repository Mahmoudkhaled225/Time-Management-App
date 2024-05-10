using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Entities;

namespace src.EntitesConfig;

public class ToDoConfig : IEntityTypeConfiguration<ToDo>
{
    public void Configure(EntityTypeBuilder<ToDo> builder)
    {
        builder.ToTable("tasks");

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)")
            .HasColumnName("title");

        builder.Property(t => t.Content)
            .IsRequired(false)
            .HasColumnType("varchar(MAX)")
            .HasColumnName("content");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasColumnName("userId");

        // Configure the relationship with User
        builder.HasOne(t => t.User)
            .WithMany(u => u.ToDos)
            .HasForeignKey(t => t.UserId)
            .IsRequired();
    }
}