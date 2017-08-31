//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.Globalization;

namespace WinCompose
{

/// <summary>
/// The Key class describes anything that can be hit on the keyboard,
/// resulting in either a printable string or a virtual key code.
/// </summary>
public class Key
{
    /// <summary>
    /// A dictionary of symbols that we use for some non-printable key labels.
    /// </summary>
    private static readonly Dictionary<VK, string> m_key_labels
        = new Dictionary<VK, string>
    {
        { VK.COMPOSE, "♦" },
        { VK.UP,      "▲" },
        { VK.DOWN,    "▼" },
        { VK.LEFT,    "◀" },
        { VK.RIGHT,   "▶" },
    };

    /// <summary>
    /// A dictionary of non-trivial keysyms and the corresponding
    /// Key object. Trivial (one-character) keysyms are not needed.
    /// </summary>
    private static readonly Dictionary<string, Key> m_keysyms
        = new Dictionary<string, Key>
    {
        /* ASCII-mapped keysyms, automatically generated:
           wget -qO- https://cgit.freedesktop.org/xorg/proto/x11proto/plain/keysymdef.h \
            | sed -ne '/XK_LATIN/,/XK_LATIN/p' | sort -k5 \
            | grep '#define XK_.[^ ].* U+' | while read _ a _ _ b _ ; do \
                b=$(echo ${b##U+} | tr A-Z a-z); \
                echo -e '        { "'${a##XK_}'",♥new Key("\u'$b'") }, // 0x'$b; done \
            | column -t -s ♥ | sed 's/"\(["\\]"\)/"\\\1/'
         */
        { "space",           new Key(" ") }, // 0x0020
        { "exclam",          new Key("!") }, // 0x0021
        { "quotedbl",        new Key("\"") }, // 0x0022
        { "numbersign",      new Key("#") }, // 0x0023
        { "dollar",          new Key("$") }, // 0x0024
        { "percent",         new Key("%") }, // 0x0025
        { "ampersand",       new Key("&") }, // 0x0026
        { "apostrophe",      new Key("'") }, // 0x0027
        { "parenleft",       new Key("(") }, // 0x0028
        { "parenright",      new Key(")") }, // 0x0029
        { "asterisk",        new Key("*") }, // 0x002a
        { "plus",            new Key("+") }, // 0x002b
        { "comma",           new Key(",") }, // 0x002c
        { "minus",           new Key("-") }, // 0x002d
        { "period",          new Key(".") }, // 0x002e
        { "slash",           new Key("/") }, // 0x002f
        { "colon",           new Key(":") }, // 0x003a
        { "semicolon",       new Key(";") }, // 0x003b
        { "less",            new Key("<") }, // 0x003c
        { "equal",           new Key("=") }, // 0x003d
        { "greater",         new Key(">") }, // 0x003e
        { "question",        new Key("?") }, // 0x003f
        { "at",              new Key("@") }, // 0x0040
        { "bracketleft",     new Key("[") }, // 0x005b
        { "backslash",       new Key("\\") }, // 0x005c
        { "bracketright",    new Key("]") }, // 0x005d
        { "asciicircum",     new Key("^") }, // 0x005e
        { "underscore",      new Key("_") }, // 0x005f
        { "grave",           new Key("`") }, // 0x0060
        { "braceleft",       new Key("{") }, // 0x007b
        { "bar",             new Key("|") }, // 0x007c
        { "braceright",      new Key("}") }, // 0x007d
        { "asciitilde",      new Key("~") }, // 0x007e
        { "nobreakspace",    new Key(" ") }, // 0x00a0
        { "exclamdown",      new Key("¡") }, // 0x00a1
        { "cent",            new Key("¢") }, // 0x00a2
        { "sterling",        new Key("£") }, // 0x00a3
        { "currency",        new Key("¤") }, // 0x00a4
        { "yen",             new Key("¥") }, // 0x00a5
        { "brokenbar",       new Key("¦") }, // 0x00a6
        { "section",         new Key("§") }, // 0x00a7
        { "diaeresis",       new Key("¨") }, // 0x00a8
        { "copyright",       new Key("©") }, // 0x00a9
        { "ordfeminine",     new Key("ª") }, // 0x00aa
        { "guillemotleft",   new Key("«") }, // 0x00ab
        { "notsign",         new Key("¬") }, // 0x00ac
        { "hyphen",          new Key("­") }, // 0x00ad
        { "registered",      new Key("®") }, // 0x00ae
        { "macron",          new Key("¯") }, // 0x00af
        { "degree",          new Key("°") }, // 0x00b0
        { "plusminus",       new Key("±") }, // 0x00b1
        { "twosuperior",     new Key("²") }, // 0x00b2
        { "threesuperior",   new Key("³") }, // 0x00b3
        { "acute",           new Key("´") }, // 0x00b4
        { "mu",              new Key("µ") }, // 0x00b5
        { "paragraph",       new Key("¶") }, // 0x00b6
        { "periodcentered",  new Key("·") }, // 0x00b7
        { "cedilla",         new Key("¸") }, // 0x00b8
        { "onesuperior",     new Key("¹") }, // 0x00b9
        { "masculine",       new Key("º") }, // 0x00ba
        { "guillemotright",  new Key("»") }, // 0x00bb
        { "onequarter",      new Key("¼") }, // 0x00bc
        { "onehalf",         new Key("½") }, // 0x00bd
        { "threequarters",   new Key("¾") }, // 0x00be
        { "questiondown",    new Key("¿") }, // 0x00bf
        { "Agrave",          new Key("À") }, // 0x00c0
        { "Aacute",          new Key("Á") }, // 0x00c1
        { "Acircumflex",     new Key("Â") }, // 0x00c2
        { "Atilde",          new Key("Ã") }, // 0x00c3
        { "Adiaeresis",      new Key("Ä") }, // 0x00c4
        { "Aring",           new Key("Å") }, // 0x00c5
        { "AE",              new Key("Æ") }, // 0x00c6
        { "Ccedilla",        new Key("Ç") }, // 0x00c7
        { "Egrave",          new Key("È") }, // 0x00c8
        { "Eacute",          new Key("É") }, // 0x00c9
        { "Ecircumflex",     new Key("Ê") }, // 0x00ca
        { "Ediaeresis",      new Key("Ë") }, // 0x00cb
        { "Igrave",          new Key("Ì") }, // 0x00cc
        { "Iacute",          new Key("Í") }, // 0x00cd
        { "Icircumflex",     new Key("Î") }, // 0x00ce
        { "Idiaeresis",      new Key("Ï") }, // 0x00cf
        { "ETH",             new Key("Ð") }, // 0x00d0
        { "Ntilde",          new Key("Ñ") }, // 0x00d1
        { "Ograve",          new Key("Ò") }, // 0x00d2
        { "Oacute",          new Key("Ó") }, // 0x00d3
        { "Ocircumflex",     new Key("Ô") }, // 0x00d4
        { "Otilde",          new Key("Õ") }, // 0x00d5
        { "Odiaeresis",      new Key("Ö") }, // 0x00d6
        { "multiply",        new Key("×") }, // 0x00d7
        { "Ooblique",        new Key("Ø") }, // 0x00d8
        { "Oslash",          new Key("Ø") }, // 0x00d8
        { "Ugrave",          new Key("Ù") }, // 0x00d9
        { "Uacute",          new Key("Ú") }, // 0x00da
        { "Ucircumflex",     new Key("Û") }, // 0x00db
        { "Udiaeresis",      new Key("Ü") }, // 0x00dc
        { "Yacute",          new Key("Ý") }, // 0x00dd
        { "THORN",           new Key("Þ") }, // 0x00de
        { "ssharp",          new Key("ß") }, // 0x00df
        { "agrave",          new Key("à") }, // 0x00e0
        { "aacute",          new Key("á") }, // 0x00e1
        { "acircumflex",     new Key("â") }, // 0x00e2
        { "atilde",          new Key("ã") }, // 0x00e3
        { "adiaeresis",      new Key("ä") }, // 0x00e4
        { "aring",           new Key("å") }, // 0x00e5
        { "ae",              new Key("æ") }, // 0x00e6
        { "ccedilla",        new Key("ç") }, // 0x00e7
        { "egrave",          new Key("è") }, // 0x00e8
        { "eacute",          new Key("é") }, // 0x00e9
        { "ecircumflex",     new Key("ê") }, // 0x00ea
        { "ediaeresis",      new Key("ë") }, // 0x00eb
        { "igrave",          new Key("ì") }, // 0x00ec
        { "iacute",          new Key("í") }, // 0x00ed
        { "icircumflex",     new Key("î") }, // 0x00ee
        { "idiaeresis",      new Key("ï") }, // 0x00ef
        { "eth",             new Key("ð") }, // 0x00f0
        { "ntilde",          new Key("ñ") }, // 0x00f1
        { "ograve",          new Key("ò") }, // 0x00f2
        { "oacute",          new Key("ó") }, // 0x00f3
        { "ocircumflex",     new Key("ô") }, // 0x00f4
        { "otilde",          new Key("õ") }, // 0x00f5
        { "odiaeresis",      new Key("ö") }, // 0x00f6
        { "division",        new Key("÷") }, // 0x00f7
        { "ooblique",        new Key("ø") }, // 0x00f8
        { "oslash",          new Key("ø") }, // 0x00f8
        { "ugrave",          new Key("ù") }, // 0x00f9
        { "uacute",          new Key("ú") }, // 0x00fa
        { "ucircumflex",     new Key("û") }, // 0x00fb
        { "udiaeresis",      new Key("ü") }, // 0x00fc
        { "yacute",          new Key("ý") }, // 0x00fd
        { "thorn",           new Key("þ") }, // 0x00fe
        { "ydiaeresis",      new Key("ÿ") }, // 0x00ff
        { "Amacron",         new Key("Ā") }, // 0x0100
        { "amacron",         new Key("ā") }, // 0x0101
        { "Abreve",          new Key("Ă") }, // 0x0102
        { "abreve",          new Key("ă") }, // 0x0103
        { "Aogonek",         new Key("Ą") }, // 0x0104
        { "aogonek",         new Key("ą") }, // 0x0105
        { "Cacute",          new Key("Ć") }, // 0x0106
        { "cacute",          new Key("ć") }, // 0x0107
        { "Ccircumflex",     new Key("Ĉ") }, // 0x0108
        { "ccircumflex",     new Key("ĉ") }, // 0x0109
        { "Cabovedot",       new Key("Ċ") }, // 0x010a
        { "cabovedot",       new Key("ċ") }, // 0x010b
        { "Ccaron",          new Key("Č") }, // 0x010c
        { "ccaron",          new Key("č") }, // 0x010d
        { "Dcaron",          new Key("Ď") }, // 0x010e
        { "dcaron",          new Key("ď") }, // 0x010f
        { "Dstroke",         new Key("Đ") }, // 0x0110
        { "dstroke",         new Key("đ") }, // 0x0111
        { "Emacron",         new Key("Ē") }, // 0x0112
        { "emacron",         new Key("ē") }, // 0x0113
        { "Eabovedot",       new Key("Ė") }, // 0x0116
        { "eabovedot",       new Key("ė") }, // 0x0117
        { "Eogonek",         new Key("Ę") }, // 0x0118
        { "eogonek",         new Key("ę") }, // 0x0119
        { "Ecaron",          new Key("Ě") }, // 0x011a
        { "ecaron",          new Key("ě") }, // 0x011b
        { "Gcircumflex",     new Key("Ĝ") }, // 0x011c
        { "gcircumflex",     new Key("ĝ") }, // 0x011d
        { "Gbreve",          new Key("Ğ") }, // 0x011e
        { "gbreve",          new Key("ğ") }, // 0x011f
        { "Gabovedot",       new Key("Ġ") }, // 0x0120
        { "gabovedot",       new Key("ġ") }, // 0x0121
        { "Gcedilla",        new Key("Ģ") }, // 0x0122
        { "gcedilla",        new Key("ģ") }, // 0x0123
        { "Hcircumflex",     new Key("Ĥ") }, // 0x0124
        { "hcircumflex",     new Key("ĥ") }, // 0x0125
        { "Hstroke",         new Key("Ħ") }, // 0x0126
        { "hstroke",         new Key("ħ") }, // 0x0127
        { "Itilde",          new Key("Ĩ") }, // 0x0128
        { "itilde",          new Key("ĩ") }, // 0x0129
        { "Imacron",         new Key("Ī") }, // 0x012a
        { "imacron",         new Key("ī") }, // 0x012b
        { "Iogonek",         new Key("Į") }, // 0x012e
        { "iogonek",         new Key("į") }, // 0x012f
        { "Iabovedot",       new Key("İ") }, // 0x0130
        { "idotless",        new Key("ı") }, // 0x0131
        { "Jcircumflex",     new Key("Ĵ") }, // 0x0134
        { "jcircumflex",     new Key("ĵ") }, // 0x0135
        { "Kcedilla",        new Key("Ķ") }, // 0x0136

        // Non-printing keys
        { "Multi_key", new Key(VK.COMPOSE) },
        { "Up",        new Key(VK.UP) },
        { "Down",      new Key(VK.DOWN) },
        { "Left",      new Key(VK.LEFT) },
        { "Right",     new Key(VK.RIGHT) },

        /* Greek and cyrillic keys, automatically generated:
           for pre in Greek Cyrillic; do wget -qO- https://cgit.freedesktop.org/xorg/proto/x11proto/plain/keysymdef.h \
            | grep "#define XK_${pre}.* U+" | sort -k5 \
            | while read _ a _ _ b _ ; do \
                b=$(echo ${b##U+} | tr A-Z a-z); \
                echo -e '        { "'${a##XK_}'",♥new Key("\u'$b'") }, // 0x'$b; done \
            | column -t -s ♥ | sed 's/"\(["\\]"\)/"\\\1/'; done
         */
        { "Greek_accentdieresis",         new Key("΅") }, // 0x0385
        { "Greek_ALPHAaccent",            new Key("Ά") }, // 0x0386
        { "Greek_EPSILONaccent",          new Key("Έ") }, // 0x0388
        { "Greek_ETAaccent",              new Key("Ή") }, // 0x0389
        { "Greek_IOTAaccent",             new Key("Ί") }, // 0x038a
        { "Greek_OMICRONaccent",          new Key("Ό") }, // 0x038c
        { "Greek_UPSILONaccent",          new Key("Ύ") }, // 0x038e
        { "Greek_OMEGAaccent",            new Key("Ώ") }, // 0x038f
        { "Greek_iotaaccentdieresis",     new Key("ΐ") }, // 0x0390
        { "Greek_ALPHA",                  new Key("Α") }, // 0x0391
        { "Greek_BETA",                   new Key("Β") }, // 0x0392
        { "Greek_GAMMA",                  new Key("Γ") }, // 0x0393
        { "Greek_DELTA",                  new Key("Δ") }, // 0x0394
        { "Greek_EPSILON",                new Key("Ε") }, // 0x0395
        { "Greek_ZETA",                   new Key("Ζ") }, // 0x0396
        { "Greek_ETA",                    new Key("Η") }, // 0x0397
        { "Greek_THETA",                  new Key("Θ") }, // 0x0398
        { "Greek_IOTA",                   new Key("Ι") }, // 0x0399
        { "Greek_KAPPA",                  new Key("Κ") }, // 0x039a
        { "Greek_LAMBDA",                 new Key("Λ") }, // 0x039b
        { "Greek_LAMDA",                  new Key("Λ") }, // 0x039b
        { "Greek_MU",                     new Key("Μ") }, // 0x039c
        { "Greek_NU",                     new Key("Ν") }, // 0x039d
        { "Greek_XI",                     new Key("Ξ") }, // 0x039e
        { "Greek_OMICRON",                new Key("Ο") }, // 0x039f
        { "Greek_PI",                     new Key("Π") }, // 0x03a0
        { "Greek_RHO",                    new Key("Ρ") }, // 0x03a1
        { "Greek_SIGMA",                  new Key("Σ") }, // 0x03a3
        { "Greek_TAU",                    new Key("Τ") }, // 0x03a4
        { "Greek_UPSILON",                new Key("Υ") }, // 0x03a5
        { "Greek_PHI",                    new Key("Φ") }, // 0x03a6
        { "Greek_CHI",                    new Key("Χ") }, // 0x03a7
        { "Greek_PSI",                    new Key("Ψ") }, // 0x03a8
        { "Greek_OMEGA",                  new Key("Ω") }, // 0x03a9
        { "Greek_IOTAdieresis",           new Key("Ϊ") }, // 0x03aa
        { "Greek_UPSILONdieresis",        new Key("Ϋ") }, // 0x03ab
        { "Greek_alphaaccent",            new Key("ά") }, // 0x03ac
        { "Greek_epsilonaccent",          new Key("έ") }, // 0x03ad
        { "Greek_etaaccent",              new Key("ή") }, // 0x03ae
        { "Greek_iotaaccent",             new Key("ί") }, // 0x03af
        { "Greek_upsilonaccentdieresis",  new Key("ΰ") }, // 0x03b0
        { "Greek_alpha",                  new Key("α") }, // 0x03b1
        { "Greek_beta",                   new Key("β") }, // 0x03b2
        { "Greek_gamma",                  new Key("γ") }, // 0x03b3
        { "Greek_delta",                  new Key("δ") }, // 0x03b4
        { "Greek_epsilon",                new Key("ε") }, // 0x03b5
        { "Greek_zeta",                   new Key("ζ") }, // 0x03b6
        { "Greek_eta",                    new Key("η") }, // 0x03b7
        { "Greek_theta",                  new Key("θ") }, // 0x03b8
        { "Greek_iota",                   new Key("ι") }, // 0x03b9
        { "Greek_kappa",                  new Key("κ") }, // 0x03ba
        { "Greek_lambda",                 new Key("λ") }, // 0x03bb
        { "Greek_lamda",                  new Key("λ") }, // 0x03bb
        { "Greek_mu",                     new Key("μ") }, // 0x03bc
        { "Greek_nu",                     new Key("ν") }, // 0x03bd
        { "Greek_xi",                     new Key("ξ") }, // 0x03be
        { "Greek_omicron",                new Key("ο") }, // 0x03bf
        { "Greek_pi",                     new Key("π") }, // 0x03c0
        { "Greek_rho",                    new Key("ρ") }, // 0x03c1
        { "Greek_finalsmallsigma",        new Key("ς") }, // 0x03c2
        { "Greek_sigma",                  new Key("σ") }, // 0x03c3
        { "Greek_tau",                    new Key("τ") }, // 0x03c4
        { "Greek_upsilon",                new Key("υ") }, // 0x03c5
        { "Greek_phi",                    new Key("φ") }, // 0x03c6
        { "Greek_chi",                    new Key("χ") }, // 0x03c7
        { "Greek_psi",                    new Key("ψ") }, // 0x03c8
        { "Greek_omega",                  new Key("ω") }, // 0x03c9
        { "Greek_iotadieresis",           new Key("ϊ") }, // 0x03ca
        { "Greek_upsilondieresis",        new Key("ϋ") }, // 0x03cb
        { "Greek_omicronaccent",          new Key("ό") }, // 0x03cc
        { "Greek_upsilonaccent",          new Key("ύ") }, // 0x03cd
        { "Greek_omegaaccent",            new Key("ώ") }, // 0x03ce
        { "Greek_horizbar",               new Key("―") }, // 0x2015

        { "Cyrillic_IO",              new Key("Ё") }, // 0x0401
        { "Cyrillic_JE",              new Key("Ј") }, // 0x0408
        { "Cyrillic_LJE",             new Key("Љ") }, // 0x0409
        { "Cyrillic_NJE",             new Key("Њ") }, // 0x040a
        { "Cyrillic_DZHE",            new Key("Џ") }, // 0x040f
        { "Cyrillic_A",               new Key("А") }, // 0x0410
        { "Cyrillic_BE",              new Key("Б") }, // 0x0411
        { "Cyrillic_VE",              new Key("В") }, // 0x0412
        { "Cyrillic_GHE",             new Key("Г") }, // 0x0413
        { "Cyrillic_DE",              new Key("Д") }, // 0x0414
        { "Cyrillic_IE",              new Key("Е") }, // 0x0415
        { "Cyrillic_ZHE",             new Key("Ж") }, // 0x0416
        { "Cyrillic_ZE",              new Key("З") }, // 0x0417
        { "Cyrillic_I",               new Key("И") }, // 0x0418
        { "Cyrillic_SHORTI",          new Key("Й") }, // 0x0419
        { "Cyrillic_KA",              new Key("К") }, // 0x041a
        { "Cyrillic_EL",              new Key("Л") }, // 0x041b
        { "Cyrillic_EM",              new Key("М") }, // 0x041c
        { "Cyrillic_EN",              new Key("Н") }, // 0x041d
        { "Cyrillic_O",               new Key("О") }, // 0x041e
        { "Cyrillic_PE",              new Key("П") }, // 0x041f
        { "Cyrillic_ER",              new Key("Р") }, // 0x0420
        { "Cyrillic_ES",              new Key("С") }, // 0x0421
        { "Cyrillic_TE",              new Key("Т") }, // 0x0422
        { "Cyrillic_U",               new Key("У") }, // 0x0423
        { "Cyrillic_EF",              new Key("Ф") }, // 0x0424
        { "Cyrillic_HA",              new Key("Х") }, // 0x0425
        { "Cyrillic_TSE",             new Key("Ц") }, // 0x0426
        { "Cyrillic_CHE",             new Key("Ч") }, // 0x0427
        { "Cyrillic_SHA",             new Key("Ш") }, // 0x0428
        { "Cyrillic_SHCHA",           new Key("Щ") }, // 0x0429
        { "Cyrillic_HARDSIGN",        new Key("Ъ") }, // 0x042a
        { "Cyrillic_YERU",            new Key("Ы") }, // 0x042b
        { "Cyrillic_SOFTSIGN",        new Key("Ь") }, // 0x042c
        { "Cyrillic_E",               new Key("Э") }, // 0x042d
        { "Cyrillic_YU",              new Key("Ю") }, // 0x042e
        { "Cyrillic_YA",              new Key("Я") }, // 0x042f
        { "Cyrillic_a",               new Key("а") }, // 0x0430
        { "Cyrillic_be",              new Key("б") }, // 0x0431
        { "Cyrillic_ve",              new Key("в") }, // 0x0432
        { "Cyrillic_ghe",             new Key("г") }, // 0x0433
        { "Cyrillic_de",              new Key("д") }, // 0x0434
        { "Cyrillic_ie",              new Key("е") }, // 0x0435
        { "Cyrillic_zhe",             new Key("ж") }, // 0x0436
        { "Cyrillic_ze",              new Key("з") }, // 0x0437
        { "Cyrillic_i",               new Key("и") }, // 0x0438
        { "Cyrillic_shorti",          new Key("й") }, // 0x0439
        { "Cyrillic_ka",              new Key("к") }, // 0x043a
        { "Cyrillic_el",              new Key("л") }, // 0x043b
        { "Cyrillic_em",              new Key("м") }, // 0x043c
        { "Cyrillic_en",              new Key("н") }, // 0x043d
        { "Cyrillic_o",               new Key("о") }, // 0x043e
        { "Cyrillic_pe",              new Key("п") }, // 0x043f
        { "Cyrillic_er",              new Key("р") }, // 0x0440
        { "Cyrillic_es",              new Key("с") }, // 0x0441
        { "Cyrillic_te",              new Key("т") }, // 0x0442
        { "Cyrillic_u",               new Key("у") }, // 0x0443
        { "Cyrillic_ef",              new Key("ф") }, // 0x0444
        { "Cyrillic_ha",              new Key("х") }, // 0x0445
        { "Cyrillic_tse",             new Key("ц") }, // 0x0446
        { "Cyrillic_che",             new Key("ч") }, // 0x0447
        { "Cyrillic_sha",             new Key("ш") }, // 0x0448
        { "Cyrillic_shcha",           new Key("щ") }, // 0x0449
        { "Cyrillic_hardsign",        new Key("ъ") }, // 0x044a
        { "Cyrillic_yeru",            new Key("ы") }, // 0x044b
        { "Cyrillic_softsign",        new Key("ь") }, // 0x044c
        { "Cyrillic_e",               new Key("э") }, // 0x044d
        { "Cyrillic_yu",              new Key("ю") }, // 0x044e
        { "Cyrillic_ya",              new Key("я") }, // 0x044f
        { "Cyrillic_io",              new Key("ё") }, // 0x0451
        { "Cyrillic_je",              new Key("ј") }, // 0x0458
        { "Cyrillic_lje",             new Key("љ") }, // 0x0459
        { "Cyrillic_nje",             new Key("њ") }, // 0x045a
        { "Cyrillic_dzhe",            new Key("џ") }, // 0x045f
        { "Cyrillic_GHE_bar",         new Key("Ғ") }, // 0x0492
        { "Cyrillic_ghe_bar",         new Key("ғ") }, // 0x0493
        { "Cyrillic_ZHE_descender",   new Key("Җ") }, // 0x0496
        { "Cyrillic_zhe_descender",   new Key("җ") }, // 0x0497
        { "Cyrillic_KA_descender",    new Key("Қ") }, // 0x049a
        { "Cyrillic_ka_descender",    new Key("қ") }, // 0x049b
        { "Cyrillic_KA_vertstroke",   new Key("Ҝ") }, // 0x049c
        { "Cyrillic_ka_vertstroke",   new Key("ҝ") }, // 0x049d
        { "Cyrillic_EN_descender",    new Key("Ң") }, // 0x04a2
        { "Cyrillic_en_descender",    new Key("ң") }, // 0x04a3
        { "Cyrillic_U_straight",      new Key("Ү") }, // 0x04ae
        { "Cyrillic_u_straight",      new Key("ү") }, // 0x04af
        { "Cyrillic_U_straight_bar",  new Key("Ұ") }, // 0x04b0
        { "Cyrillic_u_straight_bar",  new Key("ұ") }, // 0x04b1
        { "Cyrillic_HA_descender",    new Key("Ҳ") }, // 0x04b2
        { "Cyrillic_ha_descender",    new Key("ҳ") }, // 0x04b3
        { "Cyrillic_CHE_descender",   new Key("Ҷ") }, // 0x04b6
        { "Cyrillic_che_descender",   new Key("ҷ") }, // 0x04b7
        { "Cyrillic_CHE_vertstroke",  new Key("Ҹ") }, // 0x04b8
        { "Cyrillic_che_vertstroke",  new Key("ҹ") }, // 0x04b9
        { "Cyrillic_SHHA",            new Key("Һ") }, // 0x04ba
        { "Cyrillic_shha",            new Key("һ") }, // 0x04bb
        { "Cyrillic_SCHWA",           new Key("Ә") }, // 0x04d8
        { "Cyrillic_schwa",           new Key("ә") }, // 0x04d9
        { "Cyrillic_I_macron",        new Key("Ӣ") }, // 0x04e2
        { "Cyrillic_i_macron",        new Key("ӣ") }, // 0x04e3
        { "Cyrillic_O_bar",           new Key("Ө") }, // 0x04e8
        { "Cyrillic_o_bar",           new Key("ө") }, // 0x04e9
        { "Cyrillic_U_macron",        new Key("Ӯ") }, // 0x04ee
        { "Cyrillic_u_macron",        new Key("ӯ") }, // 0x04ef
    };

    /// <summary>
    /// A dictionary of keysyms and the corresponding Key object
    /// </summary>
    public static Key FromKeySym(string keysym)
    {
        if (m_keysyms.ContainsKey(keysym))
            return m_keysyms[keysym];

        if (keysym.Length == 1)
            return new Key(keysym);

        return null;
    }

    /// <summary>
    /// A list of keys for which we have a friendly name. This is used in
    /// the GUI, where the user can choose which key acts as the compose
    /// key. It needs to be lazy-initialised, because if we create Key objects
    /// before the application language is set, the descriptions will not be
    /// properly translated.
    /// </summary>
    private static Dictionary<Key, string> m_key_names = null;

    private static Dictionary<Key, string> KeyNames
    {
        get
        {
            // Lazy initialisation of m_key_names (see above)
            if (m_key_names == null)
            {
                m_key_names = new Dictionary<Key, string>
                {
                    { new Key(VK.LMENU),      i18n.Text.KeyLMenu },
                    { new Key(VK.RMENU),      i18n.Text.KeyRMenu },
                    { new Key(VK.LCONTROL),   i18n.Text.KeyLControl },
                    { new Key(VK.RCONTROL),   i18n.Text.KeyRControl },
                    { new Key(VK.LWIN),       i18n.Text.KeyLWin },
                    { new Key(VK.RWIN),       i18n.Text.KeyRWin },
                    { new Key(VK.CAPITAL),    i18n.Text.KeyCapital },
                    { new Key(VK.NUMLOCK),    i18n.Text.KeyNumLock },
                    { new Key(VK.PAUSE),      i18n.Text.KeyPause },
                    { new Key(VK.APPS),       i18n.Text.KeyApps },
                    { new Key(VK.ESCAPE),     i18n.Text.KeyEscape },
                    { new Key(VK.CONVERT),    i18n.Text.KeyConvert },
                    { new Key(VK.NONCONVERT), i18n.Text.KeyNonConvert },
                    { new Key(VK.SCROLL),     i18n.Text.KeyScroll },
                    { new Key(VK.INSERT),     i18n.Text.KeyInsert },
                    { new Key(VK.PRINT),      i18n.Text.KeyPrint },

                    { new Key(" "),    i18n.Text.KeySpace },
                    { new Key("\r"),   i18n.Text.KeyReturn },
                    { new Key("\x1b"), i18n.Text.KeyEscape },
                };

                /* Append F1—F24 */
                for (VK vk = VK.F1; vk <= VK.F24; ++vk)
                    m_key_names.Add(new Key(vk), vk.ToString());
            }

            return m_key_names;
        }
    }

    private readonly VK m_vk;

    private readonly string m_str;

    public Key(string str) { m_str = str; }

    public Key(VK vk) { m_vk = vk; }

    public VK VirtualKey { get { return m_vk; } }

    public bool IsPrintable()
    {
        return m_str != null;
    }

    /// <summary>
    /// Return whether a key is usable in a compose sequence
    /// </summary>
    public bool IsUsable()
    {
        return IsPrintable() || m_keysyms.ContainsValue(this);
    }

    /// <summary>
    /// Return whether a key is a modifier (shift, ctrl, alt)
    /// </summary>
    public bool IsModifier()
    {
        switch (m_vk)
        {
            case VK.LCONTROL:
            case VK.RCONTROL:
            case VK.CONTROL:
            case VK.LSHIFT:
            case VK.RSHIFT:
            case VK.SHIFT:
            case VK.LMENU:
            case VK.RMENU:
            case VK.MENU:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// A friendly name that we can put in e.g. a dropdown menu
    /// </summary>
    public string FriendlyName
    {
        get
        {
            string ret;
            if (KeyNames.TryGetValue(this, out ret))
                return ret;
            return ToString();
        }
    }

    /// <summary>
    /// A label that we can print on keycap icons
    /// </summary>
    public string KeyLabel
    {
        get
        {
            string ret;
            if (m_key_labels.TryGetValue(m_vk, out ret))
                return ret;
            return ToString();
        }
    }

    /// <summary>
    /// Serialize key to a printable string we can parse back into
    /// a <see cref="Key"/> object
    /// </summary>
    public override string ToString()
    {
        return m_str ?? string.Format("VK.{0}", m_vk);
    }

    public override bool Equals(object o)
    {
        return o is Key && this == (o as Key);
    }

    public static bool operator ==(Key a, Key b)
    {
        bool is_a_null = ReferenceEquals(a, null);
        bool is_b_null = ReferenceEquals(b, null);
        if (is_a_null || is_b_null)
            return is_a_null == is_b_null;
        return a.m_str != null ? a.m_str == b.m_str : a.m_vk == b.m_vk;
    }

    public static bool operator !=(Key a, Key b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Hash key by returning its printable representation’s hashcode or, if
    /// unavailable, its virtual key code’s hashcode.
    /// </summary>
    public override int GetHashCode()
    {
        return m_str != null ? m_str.GetHashCode() : ((int)m_vk).GetHashCode();
    }
};

}
