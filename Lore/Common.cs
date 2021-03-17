using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lore
{
    class Common
    {
        public static string GetClassStr(Lore player)
        {
            switch (player.Class)
            {
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

        public static string GetWeaponJosaStr(int weapon)
        {
            if (weapon == 0 || (2 <= weapon && weapon <= 4) || (6 <= weapon && weapon <= 9))
                return "으";
            else
                return "";
        }

        public static string GetDefenseStr(int shield)
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

        public static string GetMagicStr(int magic)
        {
            switch (magic)
            {
                case 1:
                    return "마법 화살";
                case 2:
                    return "마법 화구";
                case 3:
                    return "마법 단창";
                case 4:
                    return "독 바늘";
                case 5:
                    return "맥동 광선";
                case 6:
                    return "직격 뇌전";
                case 7:
                    return "공기 폭풍";
                case 8:
                    return "열선 파동";
                case 9:
                    return "초음파";
                case 10:
                    return "초냉기";
                case 11:
                    return "인공 지진";
                case 12:
                    return "차원 이탈";
                case 13:
                    return "독";
                case 14:
                    return "기술 무력화";
                case 15:
                    return "방어 무력화";
                case 16:
                    return "능력 저하";
                case 17:
                    return "마법 불능";
                case 18:
                    return "탈 초인화";
                case 19:
                    return "한명 치료";
                case 20:
                    return "한명 독 제거";
                case 21:
                    return "한명 치료와 독제거";
                case 22:
                    return "한명 의식 돌림";
                case 23:
                    return "한명 부활";
                case 24:
                    return "한명 치료와 독제거와 의식돌림";
                case 25:
                    return "한명 복합 치료";
                case 26:
                    return "모두 치료";
                case 27:
                    return "모두 독 제거";
                case 28:
                    return "모두 치료와 독제거";
                case 29:
                    return "모두 의식 돌림";
                case 30:
                    return "모두 치료와 독제거와 의식돌림";
                case 31:
                    return "모두 부활";
                case 32:
                    return "모두 복합 치료";
                case 33:
                    return "마법의 햇불";
                case 34:
                    return "공중 부상";
                case 35:
                    return "물위를 걸음";
                case 36:
                    return "늪위를 걸음";
                case 37:
                    return "기화 이동";
                case 38:
                    return "지형 변화";
                case 39:
                    return "공간 이동";
                case 40:
                    return "식량 제조";
                case 41:
                    return "투시";
                case 42:
                    return "예언";
                case 43:
                    return "독심";
                case 44:
                    return "천리안";
                case 45:
                    return "염력";
                default:
                    return "";
            }
        }

        public static string GetMagicJosaStr(int magic)
        {
            if (magic == 2 || magic == 9 || magic == 10 || (14 <= magic && magic <= 16) || (18 <= magic && magic <= 21) || (25 <= magic && magic <= 28) || magic == 32 || magic == 38 || magic == 40 || magic == 41)
                return "";
            else
                return "으";
        }

        public static string GetMagicMokjukStr(int magic)
        {
            if (magic == 2 || magic == 9 || magic == 10 || (14 <= magic && magic <= 16) || (18 <= magic && magic <= 21) || (25 <= magic && magic <= 28) || magic == 32 || magic == 38 || magic == 40 || magic == 41)
                return "를";
            else
                return "을";
        }
    }
}
