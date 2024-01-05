using System;
using System.Collections.Generic;

namespace RottenTomatoesParser.Models;

public partial class Film
{
    public Guid FilmId { get; set; }

    public Guid DirectorId { get; set; }

    public int CriticsReviewRating { get; set; }

    public int CriticsReviewCount { get; set; }

    public int AudienceReviewRating { get; set; }

    public int AudienceReviewCount { get; set; }

    public double? BoxOffice { get; set; }

    public string Title { get; set; } = null!;

    public virtual Director Director { get; set; } = null!;

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
