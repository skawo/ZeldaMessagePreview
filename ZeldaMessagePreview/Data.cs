﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeldaMessage
{
    public static class Data
    {
        public enum MsgIcon
        {
            DEKU_STICK,
            DEKU_NUT,
            BOMBS,
            BOW,
            FIRE_ARROWS,
            DINS_FIRE,
            SLINGSHOT,
            FAIRY_OCARINA,
            OCARINA_OF_TIME,
            BOMBCHUS,
            HOOKSHOT,
            LONGSHOT,
            ICE_ARROWS,
            FARORES_WIND,
            BOOMERANG,
            LENS_OF_TRUTH,
            BEANS,
            MEGATON_HAMMER,
            LIGHT_ARROWS,
            NAYRUS_LOVE,
            EMPTY_BOTTLE,
            RED_POTION,
            GREEN_POTION,
            BLUE_POTION,
            FAIRY,
            FISH,
            MILK,
            RUTOS_LETTER,
            BLUE_FIRE,
            BOTTLE_BUG,
            BOTTLE_POE,
            HALF_MILK,
            BOTTLE_BIGPOE,
            WEIRD_EGG,
            CHICKEN,
            ZELDAS_LETTER,
            KEATON_MASK,
            SKULL_MASK,
            SPOOKY_MASK,
            BUNNY_HOOD,
            GORON_MASK,
            ZORA_MASK,
            GERUDO_MASK,
            MASK_OF_TRUTH,
            SOLD_OUT,
            POCKET_EGG,
            POCKET_CUCCO,
            COJIRO,
            ODD_MUSHROOM,
            ODD_POTION,
            POACHERS_SAW,
            BROKEN_SWORD,
            PRESCRIPTION,
            EYEBALL_FROG,
            EYEDROPS,
            CLAIM_CHECK,
            BOW_FIRE,
            BOW_ICE,
            BOW_LIGHT,
            KOKIRI_SWORD,
            MASTER_SWORD,
            BIGGORON_SWORD,
            DEKU_SHIELD,
            HYLIAN_SHIELD,
            MIRROR_SHIELD,
            KOKIRI_TUNIC,
            GORON_TUNIC,
            ZORA_TUNIC,
            BOOTS,
            IRON_BOOTS,
            PEGASUS_BOOTS,
            BULLET_BAG,
            BIGGER_BULLET_BAG,
            BIGGEST_BULLET_BAG,
            QUIVER,
            BIG_QUIVER,
            BIGGEST_QUIVER,
            BOMB_BAG,
            BIGGER_BOMB_BAG,
            BIGGEST_BOMB_BAG,
            GORON_BRACELET,
            SILVER_GAUNTLETS,
            GOLDEN_GAUNTLETS,
            ZORA_SCALE,
            GOLDEN_SCALE,
            BROKEN_KNIFE,
            WALLET,
            ADULTS_WALLET,
            GIANTS_WALLET,
            DEKU_SEEDS,
            FISHING_ROD,
            NOTHING_1,
            NOTHING_2,
            NOTHING_3,
            NOTHING_4,
            NOTHING_5,
            NOTHING_6,
            NOTHING_7,
            NOTHING_9,
            NOTHING_10,
            NOTHING_11,
            NOTHING_12,
            FOREST_MEDALLION,
            FIRE_MEDALLION,
            WATER_MEDALLION,
            SPIRIT_MEDALLION,
            SHADOW_MEDALLION,
            LIGHT_MEDALLION,
            KOKIRI_EMERALD,
            GORON_RUBY,
            ZORA_SAPPHIRE,
            STONE_OF_AGONY,
            GERUDO_PASS,
            GOLDEN_SKULLTULA,
            HEART_CONTAINER,
            HEART_PIECE,
            BOSS_KEY,
            COMPASS,
            DUNGEON_MAP,
            SMALL_KEY,
            MAGIC_JAR,
            BIG_MAGIC_JAR,
        }

        public enum MsgControlCode
        {
            LINE_BREAK = 0x01,
            END = 0x02,
            NEW_BOX = 0x04,
            COLOR = 0x05,
            SHIFT = 0x06,
            JUMP = 0x07,
            DI = 0x08,
            DC = 0x09,
            SHOP_DESCRIPTION = 0x0A,
            EVENT = 0x0B,
            DELAY = 0x0C,
            AWAIT_BUTTON = 0x0D,
            FADE = 0x0E,
            PLAYER = 0x0F,
            OCARINA = 0x10,
            FADE2 = 0x11,
            SOUND = 0x12,
            ICON = 0x13,
            SPEED = 0x14,
            BACKGROUND = 0x15,
            MARATHON_TIME = 0x16,
            RACE_TIME = 0x17,
            POINTS = 0x18,
            GOLD_SKULLTULAS = 0x19,
            NS = 0x1A,
            TWO_CHOICES = 0x1B,
            THREE_CHOICES = 0x1C,
            FISH_WEIGHT = 0x1D,
            HIGH_SCORE = 0x1E,
            TIME = 0x1F,

            DASH = 0x7F,
            À = 0x80,
            Î = 0x81,
            Â = 0x82,
            Ä = 0x83,
            Ç = 0x84,
            È = 0x85,
            É = 0x86,
            Ê = 0x87,
            Ë = 0x88,
            Ï = 0x89,
            Ô = 0x8A,
            Ö = 0x8B,
            Ù = 0x8C,
            Û = 0x8D,
            Ü = 0x8E,
            ß = 0x8F,
            à = 0x90,
            á = 0x91,
            â = 0x92,
            ä = 0x93,
            ç = 0x94,
            è = 0x95,
            é = 0x96,
            ê = 0x97,
            ë = 0x98,
            ï = 0x99,
            ô = 0x9A,
            ö = 0x9B,
            ù = 0x9C,
            û = 0x9D,
            ü = 0x9E,

            A_BUTTON = 0x9F,
            B_BUTTON = 0xA0,
            C_BUTTON = 0xA1,
            L_BUTTON = 0xA2,
            R_BUTTON = 0xA3,
            Z_BUTTON = 0xA4,
            C_UP = 0xA5,
            C_DOWN = 0xA6,
            C_LEFT = 0xA7,
            C_RIGHT = 0xA8,
            TRIANGLE = 0xA9,
            CONTROL_STICK = 0xAA,
            D_PAD = 0xAB
        }

        public enum MsgColor
        {
            W = 0x40,
            R = 0x41,
            G = 0x42,
            B = 0x43,
            C = 0x44,
            M = 0x45,
            Y = 0x46,
            BLK = 0x47
        }

        public enum MsgHighScore
        {
            ARCHERY = 0x00,
            POE_POINTS = 0x01,
            FISHING = 0x02,
            HORSE_RACE = 0x03,
            MARATHON = 0x04,
            DAMPE_RACE = 0x06
        }

        public enum BoxType
        {
            Black,
            Wooden,
            Blue,
            Ocarina,
            None_White,
            None_Black,
        }

        public static readonly Dictionary<MsgControlCode, List<char>> ControlCharPresets = new Dictionary<MsgControlCode, List<char>>()
        {
            {MsgControlCode.PLAYER,             new List<char>() { 'L', 'i', 'n', 'k' } },
            {MsgControlCode.POINTS,             new List<char>() { '1', '0', '0', '0' } },
            {MsgControlCode.FISH_WEIGHT,        new List<char>() { '1', '0' } },
            {MsgControlCode.GOLD_SKULLTULAS,    new List<char>() { '1', '0', '0' } },
            {MsgControlCode.MARATHON_TIME,      new List<char>() { '0', '0', '"', '0', '0', '"' } },
            {MsgControlCode.RACE_TIME,          new List<char>() { '0', '0', '"', '0', '0', '"' } },
        };

        public static readonly List<List<RGB>> CharColors = new List<List<RGB>>()
        {
            new List<RGB>() { new RGB(255, 60, 60), new RGB(255, 120, 0)},
            new List<RGB>() { new RGB(70, 255, 80), new RGB(70, 255, 80)},
            new List<RGB>() { new RGB(80, 110, 255), new RGB(80, 90, 255)},
            new List<RGB>() { new RGB(90, 180, 255), new RGB(100, 180, 255)},
            new List<RGB>() { new RGB(210, 100, 255), new RGB(255, 150, 180)},
            new List<RGB>() { new RGB(255, 255, 30), new RGB(255, 255, 50)},
            new List<RGB>() { new RGB(0, 0, 0), new RGB(0, 0, 0)},
            new List<RGB>() { new RGB(255, 255, 255), new RGB(0, 0, 0)},
        };

        public static readonly int OUTPUT_IMAGE_X = 256;
        public static readonly int OUTPUT_IMAGE_Y = 64 + (Properties.Resources.Box_End.Width / 2);
        public static readonly int XPOS_DEFAULT = 0x20;
        public static readonly int LINEBREAK_SIZE = 12;
        public static readonly int YPOS_DEFAULT = 8;
        public static readonly int CHOICE_OFFSET = 0x20;
        public static readonly float SCALE_DEFAULT = 0.75f;

        public static readonly float[] FontWidths =
        {
            /* */ 8.0f,
            /* !*/ 8.0f,
            /* "*/ 6.0f,
            /* #*/ 9.0f,
            /* $*/ 9.0f,
            /* %*/ 14.0f,
            /* &*/ 12.0f,
            /* '*/ 3.0f,
            /* (*/ 7.0f,
            /* )*/ 7.0f,
            /* **/ 7.0f,
            /* +*/ 9.0f,
            /* ,*/ 4.0f,
            /* -*/ 6.0f,
            /* .*/ 4.0f,
            /* /*/ 9.0f,
            /* 0*/ 10.0f,
            /* 1*/ 5.0f,
            /* 2*/ 9.0f,
            /* 3*/ 9.0f,
            /* 4*/ 10.0f,
            /* 5*/ 9.0f,
            /* 6*/ 9.0f,
            /* 7*/ 9.0f,
            /* 8*/ 9.0f,
            /* 9*/ 9.0f,
            /* :*/ 6.0f,
            /* ;*/ 6.0f,
            /* <*/ 9.0f,
            /* =*/ 11.0f,
            /* >*/ 9.0f,
            /* ?*/ 11.0f,
            /* @*/ 13.0f,
            /* A*/ 12.0f,
            /* B*/ 9.0f,
            /* C*/ 11.0f,
            /* D*/ 11.0f,
            /* E*/ 8.0f,
            /* F*/ 8.0f,
            /* G*/ 12.0f,
            /* H*/ 10.0f,
            /* I*/ 4.0f,
            /* J*/ 8.0f,
            /* K*/ 10.0f,
            /* L*/ 8.0f,
            /* M*/ 13.0f,
            /* N*/ 11.0f,
            /* O*/ 13.0f,
            /* P*/ 9.0f,
            /* Q*/ 13.0f,
            /* R*/ 10.0f,
            /* S*/ 10.0f,
            /* T*/ 9.0f,
            /* U*/ 10.0f,
            /* V*/ 11.0f,
            /* W*/ 15.0f,
            /* X*/ 11.0f,
            /* Y*/ 10.0f,
            /* Z*/ 10.0f,
            /* [*/ 7.0f,
            /* ¥*/ 10.0f,
            /* ]*/ 7.0f,
            /* ^*/ 10.0f,
            /* _*/ 9.0f,
            /* `*/ 5.0f,
            /* a*/ 8.0f,
            /* b*/ 9.0f,
            /* c*/ 8.0f,
            /* d*/ 9.0f,
            /* e*/ 9.0f,
            /* f*/ 6.0f,
            /* g*/ 9.0f,
            /* h*/ 8.0f,
            /* i*/ 4.0f,
            /* j*/ 6.0f,
            /* k*/ 8.0f,
            /* l*/ 4.0f,
            /* m*/ 12.0f,
            /* n*/ 9.0f,
            /* o*/ 9.0f,
            /* p*/ 9.0f,
            /* q*/ 9.0f,
            /* r*/ 7.0f,
            /* s*/ 8.0f,
            /* t*/ 7.0f,
            /* u*/ 8.0f,
            /* v*/ 9.0f,
            /* w*/ 12.0f,
            /* x*/ 8.0f,
            /* y*/ 9.0f,
            /* z*/ 8.0f,
            /* {*/ 7.0f,
            /* |*/ 5.0f,
            /* }*/ 7.0f,
            /* ~*/ 10.0f,
            /* ‾*/ 10.0f,
            /* À*/ 12.0f,
            /* î*/ 6.0f,
            /* Â*/ 12.0f,
            /* Ä*/ 12.0f,
            /* Ç*/ 11.0f,
            /* È*/ 8.0f,
            /* É*/ 8.0f,
            /* Ê*/ 8.0f,
            /* Ë*/ 6.0f,
            /* Ï*/ 6.0f,
            /* Ô*/ 13.0f,
            /* Ö*/ 13.0f,
            /* Ù*/ 10.0f,
            /* Û*/ 10.0f,
            /* Ü*/ 10.0f,
            /* ß*/ 9.0f,
            /* à*/ 8.0f,
            /* á*/ 8.0f,
            /* â*/ 8.0f,
            /* ä*/ 8.0f,
            /* ç*/ 8.0f,
            /* è*/ 9.0f,
            /* é*/ 9.0f,
            /* ê*/ 9.0f,
            /* ë*/ 9.0f,
            /* ï*/ 6.0f,
            /* ô*/ 9.0f,
            /* ö*/ 9.0f,
            /* ù*/ 9.0f,
            /* û*/ 9.0f,
            /* ü*/ 9.0f,
            /* [A] */ 14.0f,
            /* [B] */ 14.0f,
            /* [C] */ 14.0f,
            /* [L] */ 14.0f,
            /* [R] */ 14.0f,
            /* [Z] */ 14.0f,
            /* [C-Up] */ 14.0f,
            /* [C-Down] */ 14.0f,
            /* [C-Left] */ 14.0f,
            /* [C-Right] */ 14.0f,
            /* ▼*/ 14.0f,
            /* [Analog-Stick] */ 14.0f,
            /* [D-Pad] */ 14.0f,
            /* ?*/ 14.0f,
            /* ?*/ 14.0f,
            /* ?*/ 14.0f,
            /* ?*/ 14.0f,
        };
    }
}
