using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
    class BattleEnemyData
    {
        public int ENumber
        {
            get;
            set;
        }

        public EnemyData Stats
        {
            get;
            set;
        }

        public int HP
        {
            get;
            set;
        }

        public bool Posion
        {
            get;
            set;
        }

        public bool Unconscious
        {
            get;
            set;
        }

        public bool Dead
        {
            get;
            set;
        }
    }
}
