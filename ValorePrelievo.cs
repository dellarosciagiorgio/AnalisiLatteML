﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    public class ValorePrelievo
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Uom { get; set; }
        public double? Valore { get; set; }
        public bool? FuoriSoglia { get; set; }
        public string AnalisiId { get; set; }
    }
}