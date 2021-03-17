using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
	class Lore
	{
		public string Name
		{
			get;
			set;
		}

		public string Gender
		{
			get;
			set;
		}

		public int Class {
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

		public int Concentration
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

		public int Luck
		{
			get;
			set;
		}

		public int Poison {
			get;
			set;
		}

		public int Unconscious {
			get;
			set;
		}

		public int Dead {
			get;
			set;
		}

		public int HP {
			get;
			set;
		}

		public int SP {
			get;
			set;
		}

		public int ESP {
			get;
			set;
		}

		public int[] Level {
			get;
			set;
		}

		public int AC {
			get;
			set;
		}

		public long Experience {
			get;
			set;

		}

		public int Weapon {
			get;
			set;
		}
		
		public int Shield {
			get;
			set;
		}

		public int Armor {
			get;
			set;
		}

		public int WeaPower {
			get;
			set;
		}

		public int ShiPower {
			get;
			set;
		}

		public int ArmPower {
			get;
			set;
		}

		public bool IsAvailable {
			get
			{
				if (Unconscious == 0 && Dead == 0 && HP >= 0)
					return true;
				else
					return false;
			}
		}

		public string GenderName
		{
			get
			{
				if (Gender == "male")
					return "남성";
				else
					return "여성";
			}
		}

		public string GenderPronoun
		{
			get
			{
				if (Gender == "male")
					return "그";
				else
					return "그녀";
			}
		}
	}
}
