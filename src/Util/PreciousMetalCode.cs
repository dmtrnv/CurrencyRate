using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyRate.Util
{
    public enum PreciousMetalCode
    {
        [Display(Name = "Золото")]
        Gold = 1,
        [Display(Name = "Серебро")]
        Silver,
        [Display(Name = "Платина")]
        Platinum,
        [Display(Name = "Палладий")]
        Palladium
    }
}
