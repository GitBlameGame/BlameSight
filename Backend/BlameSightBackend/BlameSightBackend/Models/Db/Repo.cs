using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

[Table("repos")]
public partial class Repo
{
    [Key]
    [Column("repo_id")]
    public int RepoId { get; set; }

    [Column("repo_owner_id")]
    public int RepoOwnerId { get; set; }

    [Column("repo_name")]
    [StringLength(1000)]
    public string RepoName { get; set; } = null!;

    [InverseProperty("Repo")]
    public virtual ICollection<Blame> Blames { get; set; } = new List<Blame>();

    [ForeignKey("RepoOwnerId")]
    [InverseProperty("Repos")]
    public virtual Repoowner RepoOwner { get; set; } = null!;
}
