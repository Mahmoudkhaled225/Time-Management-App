using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Entities;

namespace src.EntitesConfig;



public class UserConfig: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)")
            .HasColumnName("username");;

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnType("varchar(256)")
            .HasConversion(v => v.ToLower(), v => v)
            .HasColumnName("email");;

        builder.Property(u => u.Password)
            .IsRequired()
            // .HasColumnType("nvarchar(256)")
            .HasColumnName("password");;
        
        builder.Property(u => u.Dob)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasColumnType("varchar(MAX)")
            .IsRequired()
            .HasColumnName("phone");;

        builder.Property(u => u.IsConfirmed)            
            .IsRequired(false);
        

        builder.Property(u => u.IsDeleted)
            .IsRequired(false);


        builder.Property(u => u.UndoIsDeletedCode)
            .HasColumnType("varchar(MAX)")
            .HasMaxLength(6)
            .IsRequired(false);

        builder.Property(u => u.ImgUrl)
            .HasColumnType("varchar(MAX)")
            .IsRequired(false);
        
        builder.Property(u => u.PublicId)
            .HasColumnType("varchar(MAX)")
            .IsRequired(false);


        // Indexes            
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
    
}
