using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
    class Common
    {
        public static string GetGenderStr(Lore player)
        {
            if (player.Gender == "male")
                return "남성";
            else
                return "여성";
        }

        public static string GetClassStr(Lore player)
        {
            switch (player.Class) {
                case 1:
                    return "기사";
                case 2:
                    return "마법사";
                case 3:
                    return "에스퍼";
                case 4:
                    return "전사";
                case 5:
                    return "전투승";
                case 6:
                    return "닌자";
                case 7:
                    return "사냥꾼";
                case 8:
                    return "떠돌이";
                case 9:
                    return "혼령";
                case 10:
                    return "반신반인";
                default:
                    return "불확실함";
            }
        }

        public static string GetWeaponStr(int weapon)
        {
            switch (weapon)
            {
                case 0:
                    return "맨손";
                case 1:
                    return "단도";
                case 2:
                    return "곤봉";
                case 3:
                    return "미늘창";
                case 4:
                    return "장검";
                case 5:
                    return "철퇴";
                case 6:
                    return "기병창";
                case 7:
                    return "도끼창";
                case 8:
                    return "삼지창";
                case 9:
                    return "화염검";
                default:
                    return "불확실한 무기";
            }
        }

        public static string GetDefenceStr(int shield)
        {
            switch (shield)
            {
                case 0:
                    return "없음";
                case 1:
                    return "가죽";
                case 2:
                    return "청동";
                case 3:
                    return "강철";
                case 4:
                    return "은제";
                case 5:
                    return "금제";
                default:
                    return "불확실한";
            }
        }
    }
}
