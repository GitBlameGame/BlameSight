using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

[Table("users")]
[Index("UserName", Name = "username_unique", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("user_name")]
    [StringLength(39)]
    public string UserName { get; set; } = null!;

    [InverseProperty("Blamed")]
    public virtual ICollection<Blame> BlameBlameds { get; set; } = new List<Blame>();

    [InverseProperty("Blamer")]
    public virtual ICollection<Blame> BlameBlamers { get; set; } = new List<Blame>();
}
