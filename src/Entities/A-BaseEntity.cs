using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BaseEntity
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("_created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    
    [Column("_updated_at")]
    public DateTime? UpdatedAt { get; set; } = DateTime.Now;

}