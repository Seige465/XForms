using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace XForms.UWP.XForms
{
    public static class Keys
    {
        public static List<VirtualKey> Alpha = new List<VirtualKey>()
        {
             VirtualKey.A, VirtualKey.B, VirtualKey.C, VirtualKey.D, VirtualKey.E, VirtualKey.F, VirtualKey.G, VirtualKey.H, VirtualKey.I, VirtualKey.J, VirtualKey.K, VirtualKey.L, VirtualKey.M
            , VirtualKey.N, VirtualKey.O, VirtualKey.P, VirtualKey.Q, VirtualKey.R, VirtualKey.S, VirtualKey.T, VirtualKey.V, VirtualKey.W, VirtualKey.X, VirtualKey.Y, VirtualKey.Z, VirtualKey.Space
            
        };
        public static List<VirtualKey> Numeric = new List<VirtualKey>()
        {   VirtualKey.Number0, VirtualKey.Number1, VirtualKey.Number2, VirtualKey.Number3, VirtualKey.Number4, VirtualKey.Number5, VirtualKey.Number6, VirtualKey.Number7
            , VirtualKey.Number8, VirtualKey.Number9, VirtualKey.NumberPad0, VirtualKey.NumberPad1, VirtualKey.NumberPad2, VirtualKey.NumberPad3, VirtualKey.NumberPad4, VirtualKey.NumberPad5
            , VirtualKey.NumberPad6, VirtualKey.NumberPad7, VirtualKey.NumberPad8, VirtualKey.NumberPad9
        };
        public static List<VirtualKey> Decimal = new List<VirtualKey>()
        {
            VirtualKey.Decimal
        };
        public static List<VirtualKey> Override = new List<VirtualKey>()
        {
            VirtualKey.Tab
        };
    }
}
