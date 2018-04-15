using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XForms.UWP.XForms
{
    class FormLoadArgs
    {
        public int? group { get; set; }
        public bool isReadonly { get; set; } = false;
    }
}
