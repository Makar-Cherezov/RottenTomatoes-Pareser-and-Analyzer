﻿using System;
using System.Collections.Generic;

namespace RottenTomatoesClient.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Genre1 { get; set; } = null!;

    public virtual ICollection<Film> Films { get; set; } = new List<Film>();

    internal ViewModel ViewModel
    {
        get => default;
        set
        {
        }
    }
}
