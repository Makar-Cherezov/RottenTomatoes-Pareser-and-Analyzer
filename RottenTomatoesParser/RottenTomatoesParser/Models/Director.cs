using System;
using System.Collections.Generic;

namespace RottenTomatoesParser.Models;

public partial class Director
{
    public Guid DirectorId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Birthplace { get; set; } = null!;

    public virtual ICollection<Film> Films { get; set; } = new List<Film>();
}
