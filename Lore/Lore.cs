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

		private int[] mAccuracy = new int[3];
		public int[] Accuracy
		{
			get
			{
				return mAccuracy;
			}
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

		private int[] mLevel = new int[3];
		public int[] Level {
			get {
				return mLevel;
			}
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
	}
}
