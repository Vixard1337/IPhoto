using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoM.Entities;

[Index("collection_id", Name = "collection_id")]
public partial class photo
{
    [Key]
    public int photo_id { get; set; }

    [StringLength(100)]
    public string title { get; set; } = null!;

    public int? collection_id { get; set; }

    [Column("photo", TypeName = "blob")]
    public byte[]? photo1 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? added_date { get; set; }

    [StringLength(20)]
    public string format { get; set; } = null!;

    [ForeignKey("collection_id")]
    [InverseProperty("photos")]
    public virtual collection? collection { get; set; }
}
