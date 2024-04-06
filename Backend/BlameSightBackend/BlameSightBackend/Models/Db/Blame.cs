using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

[Table("blames")]
public partial class Blame
{
    [Key]
    [Column("blame_id")]
    public int BlameId { get; set; }

    [Column("blamer_id")]
    public int? BlamerId { get; set; }

    [Column("blamed_id")]
    public int? BlamedId { get; set; }

    [Column("urgency_descriptor_id")]
    public int? UrgencyDescriptorId { get; set; }

    [Column("repo_id")]
    public int? RepoId { get; set; }

    [Column("blame_path")]
    [StringLength(4096)]
    public string BlamePath { get; set; } = null!;

    [Column("blame_line")]
    public int? BlameLine { get; set; }

    [Column("blame_message")]
    [StringLength(256)]
    public string? BlameMessage { get; set; }

    [Column("blame_accepted")]
    public bool? BlameAccepted { get; set; }

    [Column("blame_complete")]
    public bool? BlameComplete { get; set; }

    [Column("blame_timestamp", TypeName = "timestamp without time zone")]
    public DateTime BlameTimestamp { get; set; }

    [ForeignKey("BlamedId")]
    [InverseProperty("BlameBlameds")]
    public virtual User? Blamed { get; set; }

    [ForeignKey("BlamerId")]
    [InverseProperty("BlameBlamers")]
    public virtual User? Blamer { get; set; }

    [ForeignKey("RepoId")]
    [InverseProperty("Blames")]
    public virtual Repo? Repo { get; set; }

    [ForeignKey("UrgencyDescriptorId")]
    [InverseProperty("Blames")]
    public virtual Urgencydescriptor? UrgencyDescriptor { get; set; }
}
