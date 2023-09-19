using System;
using System.Collections.Generic;

namespace TelegramBot.Entities;

public partial class Amei
{
    public int Usrid { get; set; }

    public int Pernr { get; set; }

    public string Nachn { get; set; } = null!;

    public string Vorna { get; set; } = null!;

    public string? Midnm { get; set; }

    public string? Perid { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public string? Zznachn { get; set; }

    public string? Zzvorna { get; set; }

    public string? Bukrs { get; set; }

    public int? PernrOld { get; set; }

    public byte? Rodjd { get; set; }

    public byte? Robuv { get; set; }

    public byte? Rgubr { get; set; }

    public byte? Rrost { get; set; }

    public int? Plans { get; set; }

    public string? Vdsk1 { get; set; }

    public string? Persg { get; set; }

    public int? AmeiChief { get; set; }
}
