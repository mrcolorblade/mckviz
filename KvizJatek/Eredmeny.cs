using System;

namespace KvizJatek
{
    public class Eredmeny
    {
        public int Id { get; set; }
        public string JatekosNev { get; set; } = string.Empty;
        public int HelyesValaszokSzama { get; set; }
        public DateTime Datum { get; set; }
    }
}