using System;
using System.Collections.Generic;
using System.Text;

namespace KvizJatek
{
    //1 sor / kerdes
    public class Kerdes
    {
        public int Id { get; set; }
        public string KerdesSzoveg { get; set; } = string.Empty;
        public string ValaszA { get; set; } = string.Empty;
        public string ValaszB { get; set; } = string.Empty;
        public string ValaszC { get; set; } = string.Empty;
        public string ValaszD { get; set; } = string.Empty;
        public string HelyesValasz { get; set; } = string.Empty;
        public string? KepNev { get; set; }
    }
}
