using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
	class SaveData
	{
		public List<Lore> PlayerList
		{
			get;
			set;
		}

		public LorePlayer Party
		{
			get;
			set;
		}

		public Map Map
		{
			get;
			set;
		}

		public int Encounter {
			get;
			set;
		}

		public int MaxEnemy {
			get;
			set;
		}

		public long SaveTime {
			get;
			set;
		}
	}
}
