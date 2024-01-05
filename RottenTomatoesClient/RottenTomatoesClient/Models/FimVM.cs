using System;
using System.Collections.Generic;

namespace RottenTomatoesClient.Models;

public class FilmVM
{

    public string Title { get; set; } = null!;

    public string DirectorName { get; set; } = null!;

    public int CriticsReviewRating { get; set; }

    public int CriticsReviewCount { get; set; }

    public int AudienceReviewRating { get; set; }

    public int AudienceReviewCount { get; set; }

    public double? BoxOffice { get; set; }

    public string Genres { get; set; } = null!;
}
