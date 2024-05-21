using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoM.Entities;

public partial class collection
{
    [Key]
    public int collection_id { get; set; }

    [StringLength(100)]
    public string collection_name { get; set; } = null!;

    [InverseProperty("collection")]
    public virtual ICollection<photo> photos { get; set; } = new List<photo>();
}
