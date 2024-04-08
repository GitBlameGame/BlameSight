using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

[Table("repo_owners")]
[Index("RepoOwnerName", Name = "repo_owner_name_unique", IsUnique = true)]
public partial class Repoowner
{
    [Key]
    [Column("repo_owner_id")]
    public int RepoOwnerId { get; set; }

    [Column("repo_owner_name")]
    [StringLength(1000)]
    public string RepoOwnerName { get; set; } = null!;

    [InverseProperty("RepoOwner")]
    public virtual ICollection<Repo> Repos { get; set; } = new List<Repo>();
}
