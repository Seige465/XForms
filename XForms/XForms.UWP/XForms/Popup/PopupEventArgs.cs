using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XForms.UWP.XForms.PopupControls
{
    class PopupEventArgs :EventArgs
    {
        public object Value { get; set; }
        public bool Save { get; set; } = true;
    }
}
