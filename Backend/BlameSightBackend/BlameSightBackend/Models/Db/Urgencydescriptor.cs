﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Models.Db;

[Table("urgency_descriptors")]
[Index("UrgencyDescriptorName", Name = "urgencydescriptors_urgency_descriptor_name_key", IsUnique = true)]
public partial class Urgencydescriptor
{
    [Key]
    [Column("urgency_descriptor_id")]
    public int UrgencyDescriptorId { get; set; }

    [Column("urgency_descriptor_name")]
    [StringLength(30)]
    public string UrgencyDescriptorName { get; set; } = null!;

    [InverseProperty("UrgencyDescriptor")]
    public virtual ICollection<Blame> Blames { get; set; } = new List<Blame>();
}
