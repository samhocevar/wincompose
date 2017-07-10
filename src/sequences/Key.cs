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
        // ASCII-mapped keysyms
        { "space",        new Key(" ") },  // 0x20
        { "exclam",       new Key("!") },  // 0x21
        { "quotedbl",     new Key("\"") }, // 0x22
        { "numbersign",   new Key("#") },  // 0x23
        { "dollar",       new Key("$") },  // 0x24
        { "percent",      new Key("%") },  // 0x25
        { "ampersand",    new Key("&") },  // 0x26
        { "apostrophe",   new Key("'") },  // 0x27
        { "parenleft",    new Key("(") },  // 0x28
        { "parenright",   new Key(")") },  // 0x29
        { "asterisk",     new Key("*") },  // 0x2a
        { "plus",         new Key("+") },  // 0x2b
        { "comma",        new Key(",") },  // 0x2c
        { "minus",        new Key("-") },  // 0x2d
        { "period",       new Key(".") },  // 0x2e
        { "slash",        new Key("/") },  // 0x2f
        { "colon",        new Key(":") },  // 0x3a
        { "semicolon",    new Key(";") },  // 0x3b
        { "less",         new Key("<") },  // 0x3c
        { "equal",        new Key("=") },  // 0x3d
        { "greater",      new Key(">") },  // 0x3e
        { "question",     new Key("?") },  // 0x3f
        { "at",           new Key("@") },  // 0x40
        { "bracketleft",  new Key("[") },  // 0x5b
        { "backslash",    new Key("\\") }, // 0x5c
        { "bracketright", new Key("]") },  // 0x5d
        { "asciicircum",  new Key("^") },  // 0x5e
        { "underscore",   new Key("_") },  // 0x5f
        { "grave",        new Key("`") },  // 0x60
        { "braceleft",    new Key("{") },  // 0x7b
        { "bar",          new Key("|") },  // 0x7c
        { "braceright",   new Key("}") },  // 0x7d
        { "asciitilde",   new Key("~") },  // 0x7e

        // Non-printing keys
        { "Multi_key", new Key(VK.COMPOSE) },
        { "Up",        new Key(VK.UP) },
        { "Down",      new Key(VK.DOWN) },
        { "Left",      new Key(VK.LEFT) },
        { "Right",     new Key(VK.RIGHT) },

        /* Greek keys, automatically generated:
           wget -qO- https://cgit.freedesktop.org/xorg/proto/x11proto/plain/keysymdef.h \
            | grep '#define XK_Greek.* U+' | while read _ a _ _ b _ ; do \
                echo -e '{ "'${a##XK_}'", new Key("\u'${b##U+}'") }, // 0x'${b##U+}; done \
            | sort -k6 | column -t | sed 's/ \( [^ ]\)/\1/g'
         */
        { "Greek_accentdieresis",        new Key("΅") }, // 0x0385
        { "Greek_ALPHAaccent",           new Key("Ά") }, // 0x0386
        { "Greek_EPSILONaccent",         new Key("Έ") }, // 0x0388
        { "Greek_ETAaccent",             new Key("Ή") }, // 0x0389
        { "Greek_IOTAaccent",            new Key("Ί") }, // 0x038A
        { "Greek_OMICRONaccent",         new Key("Ό") }, // 0x038C
        { "Greek_UPSILONaccent",         new Key("Ύ") }, // 0x038E
        { "Greek_OMEGAaccent",           new Key("Ώ") }, // 0x038F
        { "Greek_iotaaccentdieresis",    new Key("ΐ") }, // 0x0390
        { "Greek_ALPHA",                 new Key("Α") }, // 0x0391
        { "Greek_BETA",                  new Key("Β") }, // 0x0392
        { "Greek_GAMMA",                 new Key("Γ") }, // 0x0393
        { "Greek_DELTA",                 new Key("Δ") }, // 0x0394
        { "Greek_EPSILON",               new Key("Ε") }, // 0x0395
        { "Greek_ZETA",                  new Key("Ζ") }, // 0x0396
        { "Greek_ETA",                   new Key("Η") }, // 0x0397
        { "Greek_THETA",                 new Key("Θ") }, // 0x0398
        { "Greek_IOTA",                  new Key("Ι") }, // 0x0399
        { "Greek_KAPPA",                 new Key("Κ") }, // 0x039A
        { "Greek_LAMBDA",                new Key("Λ") }, // 0x039B
        { "Greek_LAMDA",                 new Key("Λ") }, // 0x039B
        { "Greek_MU",                    new Key("Μ") }, // 0x039C
        { "Greek_NU",                    new Key("Ν") }, // 0x039D
        { "Greek_XI",                    new Key("Ξ") }, // 0x039E
        { "Greek_OMICRON",               new Key("Ο") }, // 0x039F
        { "Greek_PI",                    new Key("Π") }, // 0x03A0
        { "Greek_RHO",                   new Key("Ρ") }, // 0x03A1
        { "Greek_SIGMA",                 new Key("Σ") }, // 0x03A3
        { "Greek_TAU",                   new Key("Τ") }, // 0x03A4
        { "Greek_UPSILON",               new Key("Υ") }, // 0x03A5
        { "Greek_PHI",                   new Key("Φ") }, // 0x03A6
        { "Greek_CHI",                   new Key("Χ") }, // 0x03A7
        { "Greek_PSI",                   new Key("Ψ") }, // 0x03A8
        { "Greek_OMEGA",                 new Key("Ω") }, // 0x03A9
        { "Greek_IOTAdieresis",          new Key("Ϊ") }, // 0x03AA
        { "Greek_UPSILONdieresis",       new Key("Ϋ") }, // 0x03AB
        { "Greek_alphaaccent",           new Key("ά") }, // 0x03AC
        { "Greek_epsilonaccent",         new Key("έ") }, // 0x03AD
        { "Greek_etaaccent",             new Key("ή") }, // 0x03AE
        { "Greek_iotaaccent",            new Key("ί") }, // 0x03AF
        { "Greek_upsilonaccentdieresis", new Key("ΰ") }, // 0x03B0
        { "Greek_alpha",                 new Key("α") }, // 0x03B1
        { "Greek_beta",                  new Key("β") }, // 0x03B2
        { "Greek_gamma",                 new Key("γ") }, // 0x03B3
        { "Greek_delta",                 new Key("δ") }, // 0x03B4
        { "Greek_epsilon",               new Key("ε") }, // 0x03B5
        { "Greek_zeta",                  new Key("ζ") }, // 0x03B6
        { "Greek_eta",                   new Key("η") }, // 0x03B7
        { "Greek_theta",                 new Key("θ") }, // 0x03B8
        { "Greek_iota",                  new Key("ι") }, // 0x03B9
        { "Greek_kappa",                 new Key("κ") }, // 0x03BA
        { "Greek_lambda",                new Key("λ") }, // 0x03BB
        { "Greek_lamda",                 new Key("λ") }, // 0x03BB
        { "Greek_mu",                    new Key("μ") }, // 0x03BC
        { "Greek_nu",                    new Key("ν") }, // 0x03BD
        { "Greek_xi",                    new Key("ξ") }, // 0x03BE
        { "Greek_omicron",               new Key("ο") }, // 0x03BF
        { "Greek_pi",                    new Key("π") }, // 0x03C0
        { "Greek_rho",                   new Key("ρ") }, // 0x03C1
        { "Greek_finalsmallsigma",       new Key("ς") }, // 0x03C2
        { "Greek_sigma",                 new Key("σ") }, // 0x03C3
        { "Greek_tau",                   new Key("τ") }, // 0x03C4
        { "Greek_upsilon",               new Key("υ") }, // 0x03C5
        { "Greek_phi",                   new Key("φ") }, // 0x03C6
        { "Greek_chi",                   new Key("χ") }, // 0x03C7
        { "Greek_psi",                   new Key("ψ") }, // 0x03C8
        { "Greek_omega",                 new Key("ω") }, // 0x03C9
        { "Greek_iotadieresis",          new Key("ϊ") }, // 0x03CA
        { "Greek_upsilondieresis",       new Key("ϋ") }, // 0x03CB
        { "Greek_omicronaccent",         new Key("ό") }, // 0x03CC
        { "Greek_upsilonaccent",         new Key("ύ") }, // 0x03CD
        { "Greek_omegaaccent",           new Key("ώ") }, // 0x03CE
        { "Greek_horizbar",              new Key("―") }, // 0x2015

        /* Cyrillic keys, automatically generated:
           wget -qO- https://cgit.freedesktop.org/xorg/proto/x11proto/plain/keysymdef.h \
            | grep '#define XK_Cyrillic.* U+' | while read _ a _ _ b _ ; do \
                echo -e '{ "'${a##XK_}'", new Key("\u'${b##U+}'") }, // 0x'${b##U+}; done \
            | sort -k6 | column -t | sed 's/ \( [^ ]\)/\1/g'
         */
        { "Cyrillic_IO",             new Key("Ё") }, // 0x0401
        { "Cyrillic_JE",             new Key("Ј") }, // 0x0408
        { "Cyrillic_LJE",            new Key("Љ") }, // 0x0409
        { "Cyrillic_NJE",            new Key("Њ") }, // 0x040A
        { "Cyrillic_DZHE",           new Key("Џ") }, // 0x040F
        { "Cyrillic_A",              new Key("А") }, // 0x0410
        { "Cyrillic_BE",             new Key("Б") }, // 0x0411
        { "Cyrillic_VE",             new Key("В") }, // 0x0412
        { "Cyrillic_GHE",            new Key("Г") }, // 0x0413
        { "Cyrillic_DE",             new Key("Д") }, // 0x0414
        { "Cyrillic_IE",             new Key("Е") }, // 0x0415
        { "Cyrillic_ZHE",            new Key("Ж") }, // 0x0416
        { "Cyrillic_ZE",             new Key("З") }, // 0x0417
        { "Cyrillic_I",              new Key("И") }, // 0x0418
        { "Cyrillic_SHORTI",         new Key("Й") }, // 0x0419
        { "Cyrillic_KA",             new Key("К") }, // 0x041A
        { "Cyrillic_EL",             new Key("Л") }, // 0x041B
        { "Cyrillic_EM",             new Key("М") }, // 0x041C
        { "Cyrillic_EN",             new Key("Н") }, // 0x041D
        { "Cyrillic_O",              new Key("О") }, // 0x041E
        { "Cyrillic_PE",             new Key("П") }, // 0x041F
        { "Cyrillic_ER",             new Key("Р") }, // 0x0420
        { "Cyrillic_ES",             new Key("С") }, // 0x0421
        { "Cyrillic_TE",             new Key("Т") }, // 0x0422
        { "Cyrillic_U",              new Key("У") }, // 0x0423
        { "Cyrillic_EF",             new Key("Ф") }, // 0x0424
        { "Cyrillic_HA",             new Key("Х") }, // 0x0425
        { "Cyrillic_TSE",            new Key("Ц") }, // 0x0426
        { "Cyrillic_CHE",            new Key("Ч") }, // 0x0427
        { "Cyrillic_SHA",            new Key("Ш") }, // 0x0428
        { "Cyrillic_SHCHA",          new Key("Щ") }, // 0x0429
        { "Cyrillic_HARDSIGN",       new Key("Ъ") }, // 0x042A
        { "Cyrillic_YERU",           new Key("Ы") }, // 0x042B
        { "Cyrillic_SOFTSIGN",       new Key("Ь") }, // 0x042C
        { "Cyrillic_E",              new Key("Э") }, // 0x042D
        { "Cyrillic_YU",             new Key("Ю") }, // 0x042E
        { "Cyrillic_YA",             new Key("Я") }, // 0x042F
        { "Cyrillic_a",              new Key("а") }, // 0x0430
        { "Cyrillic_be",             new Key("б") }, // 0x0431
        { "Cyrillic_ve",             new Key("в") }, // 0x0432
        { "Cyrillic_ghe",            new Key("г") }, // 0x0433
        { "Cyrillic_de",             new Key("д") }, // 0x0434
        { "Cyrillic_ie",             new Key("е") }, // 0x0435
        { "Cyrillic_zhe",            new Key("ж") }, // 0x0436
        { "Cyrillic_ze",             new Key("з") }, // 0x0437
        { "Cyrillic_i",              new Key("и") }, // 0x0438
        { "Cyrillic_shorti",         new Key("й") }, // 0x0439
        { "Cyrillic_ka",             new Key("к") }, // 0x043A
        { "Cyrillic_el",             new Key("л") }, // 0x043B
        { "Cyrillic_em",             new Key("м") }, // 0x043C
        { "Cyrillic_en",             new Key("н") }, // 0x043D
        { "Cyrillic_o",              new Key("о") }, // 0x043E
        { "Cyrillic_pe",             new Key("п") }, // 0x043F
        { "Cyrillic_er",             new Key("р") }, // 0x0440
        { "Cyrillic_es",             new Key("с") }, // 0x0441
        { "Cyrillic_te",             new Key("т") }, // 0x0442
        { "Cyrillic_u",              new Key("у") }, // 0x0443
        { "Cyrillic_ef",             new Key("ф") }, // 0x0444
        { "Cyrillic_ha",             new Key("х") }, // 0x0445
        { "Cyrillic_tse",            new Key("ц") }, // 0x0446
        { "Cyrillic_che",            new Key("ч") }, // 0x0447
        { "Cyrillic_sha",            new Key("ш") }, // 0x0448
        { "Cyrillic_shcha",          new Key("щ") }, // 0x0449
        { "Cyrillic_hardsign",       new Key("ъ") }, // 0x044A
        { "Cyrillic_yeru",           new Key("ы") }, // 0x044B
        { "Cyrillic_softsign",       new Key("ь") }, // 0x044C
        { "Cyrillic_e",              new Key("э") }, // 0x044D
        { "Cyrillic_yu",             new Key("ю") }, // 0x044E
        { "Cyrillic_ya",             new Key("я") }, // 0x044F
        { "Cyrillic_io",             new Key("ё") }, // 0x0451
        { "Cyrillic_je",             new Key("ј") }, // 0x0458
        { "Cyrillic_lje",            new Key("љ") }, // 0x0459
        { "Cyrillic_nje",            new Key("њ") }, // 0x045A
        { "Cyrillic_dzhe",           new Key("џ") }, // 0x045F
        { "Cyrillic_GHE_bar",        new Key("Ғ") }, // 0x0492
        { "Cyrillic_ghe_bar",        new Key("ғ") }, // 0x0493
        { "Cyrillic_ZHE_descender",  new Key("Җ") }, // 0x0496
        { "Cyrillic_zhe_descender",  new Key("җ") }, // 0x0497
        { "Cyrillic_KA_descender",   new Key("Қ") }, // 0x049A
        { "Cyrillic_ka_descender",   new Key("қ") }, // 0x049B
        { "Cyrillic_KA_vertstroke",  new Key("Ҝ") }, // 0x049C
        { "Cyrillic_ka_vertstroke",  new Key("ҝ") }, // 0x049D
        { "Cyrillic_EN_descender",   new Key("Ң") }, // 0x04A2
        { "Cyrillic_en_descender",   new Key("ң") }, // 0x04A3
        { "Cyrillic_U_straight",     new Key("Ү") }, // 0x04AE
        { "Cyrillic_u_straight",     new Key("ү") }, // 0x04AF
        { "Cyrillic_U_straight_bar", new Key("Ұ") }, // 0x04B0
        { "Cyrillic_u_straight_bar", new Key("ұ") }, // 0x04B1
        { "Cyrillic_HA_descender",   new Key("Ҳ") }, // 0x04B2
        { "Cyrillic_ha_descender",   new Key("ҳ") }, // 0x04B3
        { "Cyrillic_CHE_descender",  new Key("Ҷ") }, // 0x04B6
        { "Cyrillic_che_descender",  new Key("ҷ") }, // 0x04B7
        { "Cyrillic_CHE_vertstroke", new Key("Ҹ") }, // 0x04B8
        { "Cyrillic_che_vertstroke", new Key("ҹ") }, // 0x04B9
        { "Cyrillic_SHHA",           new Key("Һ") }, // 0x04BA
        { "Cyrillic_shha",           new Key("һ") }, // 0x04BB
        { "Cyrillic_SCHWA",          new Key("Ә") }, // 0x04D8
        { "Cyrillic_schwa",          new Key("ә") }, // 0x04D9
        { "Cyrillic_I_macron",       new Key("Ӣ") }, // 0x04E2
        { "Cyrillic_i_macron",       new Key("ӣ") }, // 0x04E3
        { "Cyrillic_O_bar",          new Key("Ө") }, // 0x04E8
        { "Cyrillic_o_bar",          new Key("ө") }, // 0x04E9
        { "Cyrillic_U_macron",       new Key("Ӯ") }, // 0x04EE
        { "Cyrillic_u_macron",       new Key("ӯ") }, // 0x04EF
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
                    { new Key(VK.PRINT),      i18n.Text.KeyPrint},

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
