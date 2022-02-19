using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public class GUI_UpdateEvent : EventArgs
    {
        public int Number { get; set; }

        public GUI_UpdateEvent(int n)
        {
            Number = n;
        }
    }
}
