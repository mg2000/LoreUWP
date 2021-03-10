using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
    class LorePlayer
    {
        public int Map
        {
            get;
            set;
        }

        public int XAxis
        {
            get;
            set;
        }

        public int YAxis
        {
            get;
            set;
        }

        public int Food
        {
            get;
            set;
        }

        public int Gold
        {
            get;
            set;
        }

        private int[] mEtc = new int[100];
        public int[] Etc
        {
            get
            {
                return mEtc;
            }
        }
    }
}
