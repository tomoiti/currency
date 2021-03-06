﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Currency.Models
{
    public class Currency
    {
        public long Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required, MaxLength(3)]
        public string Code { get; set; }
        [Required]
        public decimal Rate { get; set; }
    }
}