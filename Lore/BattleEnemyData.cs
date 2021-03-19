using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
	class BattleEnemyData
	{
		public BattleEnemyData(int enemyID, EnemyData enemy){
			ENumber = enemyID;
			Name = enemy.Name;
			Strength = enemy.Strength;
			Mentality = enemy.Mentality;
			Endurance = enemy.Endurance;
			Resistance = enemy.Resistance;
			Agility = enemy.Agility;

			Accuracy = new int[enemy.Accuracy.Length];
			for (var i = 0; i < Accuracy.Length; i++)
				Accuracy[i] = enemy.Accuracy[i];

			AC = enemy.AC;
			Special = enemy.Special;
			CastLevel = enemy.CastLevel;
			SpecialCastLevel = enemy.SpecialCastLevel;
			Level = enemy.Level;

			HP = Endurance * Level;
			Posion = false;
			Unconscious = false;
			Dead = false;
		}

		public int ENumber
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public int Strength
		{
			get;
			set;
		}

		public int Mentality
		{
			get;
			set;
		}

		public int Endurance
		{
			get;
			set;
		}

		public int Resistance
		{
			get;
			set;
		}

		public int Agility
		{
			get;
			set;
		}

		public int[] Accuracy
		{
			get;
			set;
		}

		public int AC
		{
			get;
			set;
		}

		public int Special
		{
			get;
			set;
		}

		public int CastLevel
		{
			get;
			set;
		}

		public int SpecialCastLevel
		{
			get;
			set;
		}

		public int Level
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
