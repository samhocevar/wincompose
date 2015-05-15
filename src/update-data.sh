#!/bin/sh

set -e

STEPS=5
CACHE=unicode/cache
mkdir -p ${CACHE}

#
# Rebuild po/wincompose.pot from our master translation file Text.resx
# then update all .po files
#

echo "[1/${STEPS}] Rebuild potfiles…"
DEST=po/wincompose.pot
# Update POT-Creation-Date with: date +'%Y-%m-%d %R%z'
cat > ${DEST} << EOF
# SOME DESCRIPTIVE TITLE.
# Copyright (C) YEAR THE PACKAGE'S COPYRIGHT HOLDER
# This file is distributed under the same license as the PACKAGE package.
# FIRST AUTHOR <EMAIL@ADDRESS>, YEAR.
#
#, fuzzy
msgid ""
msgstr ""
"Project-Id-Version: WinCompose $(sed -ne 's/.*<ApplicationVersion>\([^<]*\).*/\1/p' build.xml)\n"
"Report-Msgid-Bugs-To: Sam Hocevar <sam@hocevar.net>\n"
"POT-Creation-Date: 2015-03-23 15:27+0100\n"
"PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\n"
"Last-Translator: FULL NAME <EMAIL@ADDRESS>\n"
"Language-Team: LANGUAGE <LL@li.org>\n"
"Language: \n"
"MIME-Version: 1.0\n"
"Content-Type: text/plain; charset=UTF-8\n"
"Content-Transfer-Encoding: 8bit\n"

EOF
for FILE in i18n/Text.resx unicode/Category.resx; do
    awk < ${FILE} '
    /<!--/      { off=1 }
    /-->/       { off=0 }
    /<data /    { split($0, a, "\""); id=a[2]; comment=""; obsolete=0 }
    /"Obsolete/ { obsolete=1 }
    /<value>/   { split ($0, a, /[<>]/); value=a[3]; }
    /<comment>/ { split ($0, a, /[<>]/); comment=a[3]; }
    /<\/data>/  { if (!off) {
                      print "#: '${FILE}':" (NR - 1) " ID:" id;
                      if (comment) { print "#. " comment }
                      if (obsolete) { print "#. This string is obsolete but might be reused in the future" }
                      print "msgid \"" value "\"";
                      print "msgstr \"\""; print "";
                  } }' \
  | sed 's/&amp;/\&/g; s/&lt;/</g; s/&gt;/>/g' \
  >> ${DEST}
done

for POFILE in po/*.po; do
     printf %s ${POFILE}
     msgmerge -U ${POFILE} po/wincompose.pot
done
rm -f po/*~

#
# Update each Text.*.resx with contents from *.po, i.e. the
# work from Weblate translators
#

echo "[2/${STEPS}] Rebuild resx files…"
for POFILE in po/*.po; do
    L=$(basename ${POFILE} .po)
    case $L in
        zh_CN) L=zh-CHS ;;
        zh) L=zh-CHT ;;
        sc) L=it-CH ;;
        *@*) continue ;;
    esac

    for FILE in i18n/Text.resx unicode/Category.resx; do
        DEST=${FILE%%.resx}.${L}.resx
        sed -e '/^  <data/,$d' < ${FILE} > ${DEST}
        cat ${POFILE} \
          | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
          | awk 'function f() {
                     if (good && id && msgstr) {
                         print "  <data name=\"" id "\" xml:space=\"preserve\">";
                         print "    <value>" msgstr "</value>";
                         if (0 && comment) { print "    <comment>" comment "</comment>"; }
                         print "  </data>";
                     }
                     reset();
                 }
                 function reset() { good=0; id=""; comment=""; }
                 /^$/        { f(); }
                 END         { f(); }
                 /^#[.] /    { split($0, a, "#[.] "); comment=a[2]; }
                 /^#:.*ID:/  { split($0, a, "ID:"); id=a[2]; }
                 /^#: .*\/'${FILE##*/}':/ { good=1 }
                 /^#, fuzzy/ { reset(); }
                 /^ *"/      { split($0, a, "\""); msgstr=msgstr a[2]; }
                 /^msgstr/   { split($0, a, "\""); msgstr=a[2]; }' \
          >> ${DEST}
        echo "</root>" >> ${DEST}
        touch ${DEST%%.resx}.Designer.cs
    done
done

#
# Use Unicode description files from the unicode translation project
# and create .resx translation files for our project
#

echo "[3/${STEPS}] Rebuild Unicode translation files…"
BASE=http://translationproject.org/latest/unicode-translation/
PO=$(wget -qO- $BASE | tr '<>' '\n' | sed -ne 's/^\(..\)[.]po$/\1/p')
for L in $PO; do
    printf "${L}... "
    SRC=${CACHE}/${L}.po
    # Get latest translation if new
    (cd ${CACHE} && wget -q -N ${BASE}/${L}.po)

    # Parse data and put it in the Char.*.resx and Block.*.resx files
    for FILE in Char Block; do
        case ${FILE} in
            #. CHARACTER NAME for U+007B
            Char) CODE='/^#[.] CHARACTER NAME for / { split($0, a, "+"); c="U" a[2]; }' ;;
            #. UNICODE BLOCK: U+0000..U+007F
            Block) CODE='/^#[.] UNICODE BLOCK: / { split($0, a, /[+.]/); c="U" a[3] "_U" a[6]; }' ;;
        esac
        DEST=unicode/${FILE}.${L}.resx
        sed -e '/^  <data/,$d' < unicode/${FILE}.resx > ${DEST}
        if uname | grep -qi mingw; then unix2dos; else cat; fi < ${SRC} \
          | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
          | awk 'function f() {
                     if (c && msgstr) {
                         print "  <data name=\"" c "\" xml:space=\"preserve\">";
                         print "    <value>" msgstr "</value>";
                         print "  </data>";
                     }
                     c=""; msgstr="";
                 }
                 '"${CODE}"'
                 /^msgstr/ { split($0, a, "\""); msgstr=a[2]; f(); }' \
          >> ${DEST}
        echo "</root>" >> ${DEST}
        touch ${DEST%%.resx}.Designer.cs
    done
done
echo "done."

#
# Check some wincompose.csproj consistency
#

echo "[4/${STEPS}] Check consistency…"
for x in unicode/*resx i18n/*resx; do
    lang="$(echo $x | cut -f2 -d.)"
    if ! grep -q '"'$(echo $x | tr / .)'"' wincompose.csproj; then
        echo "WARNING: $x not found in wincompose.csproj"
    fi
    if grep -q '^; Name: "'$lang'";' installer.iss; then
        echo "WARNING: $lang is commented out in installer.iss"
    fi
done

#
# Build translator list
#

echo "[5/${STEPS}] Update contributor list…"
printf '\xef\xbb\xbf' > res/.contributors.html
cat >> res/.contributors.html << EOF
<html>
<body style="font-family: verdana, sans-serif; font-size: .7em;">
<h3>Programming</h3>
<ul>
  <li>Sam Hocevar &lt;sam@hocevar.net&gt;</li>
  <li>Benlitz &lt;dev@benlitz.net&gt;</li>
  <li>gdow &lt;gdow@divroet.net&gt;</li>
</ul>
<h3>Translation</h3>
<ul>
EOF
git log po | sed -ne 's/^Author: //p' | LANG=C sort | uniq \
  | sed 's/</\&lt;/g' | sed 's/>/\&gt;/g' | sed 's,.*,<li>&</li>,' \
  >> res/.contributors.html
cat >> res/.contributors.html << EOF
</ul>
</body>
</html>
EOF
mv res/.contributors.html res/contributors.html

#
# Copy system files
#

if [ -f /usr/share/X11/locale/en_US.UTF-8/Compose ]; then
    cp /usr/share/X11/locale/en_US.UTF-8/Compose rules/Xorg.txt
fi

