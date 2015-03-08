#!/bin/sh

CACHE=i18n/cache
mkdir -p ${CACHE}

#
# Rebuild po/wincompose.pot from our master translation file Text.resx
# then update all .po files
#

echo "[1/3] Rebuild potfiles…"
awk < i18n/Text.resx '
    /<!--/      { off=1 }
    /-->/       { off=0 }
    /<data /    { split($0, a, "\""); id=a[2]; comment=""; obsolete=0 }
    /"Obsolete/ { obsolete=1 }
    /<value>/   { split ($0, a, /[<>]/); value=a[3]; }
    /<comment>/ { split ($0, a, /[<>]/); comment=a[3]; }
    /<\/data>/  { if (!off) {
                      print "#: i18n/Text.resx:" (NR - 1) " ID:" id;
                      if (comment) { print "#. " comment }
                      if (obsolete) { print "#. This string is obsolete but might be used again" }
                      print "msgid \"" value "\"";
                      print "msgstr \"\""; print "";
                  } }' \
  | sed 's/&amp;/\&/g; s/&lt;/</g; s/&gt;/>/g' \
  > po/wincompose.pot

rm -f po/*~
for POFILE in po/*.po; do
     printf %s ${POFILE}
     msgmerge -U ${POFILE} po/wincompose.pot
done

#
# Update each Text.*.resx with contents from *.po, i.e. the
# work from Weblate translators
#

echo "[2/3] Rebuild resx files…"
for POFILE in po/*.po; do
    LANG=$(basename -s.po ${POFILE})
    case $LANG in
        zh_CN) LANG=zh-CHS ;;
        zh) LANG=zh-CHT ;;
        *@*) continue ;;
    esac
    DST=i18n/Text.${LANG}.resx

    sed -e '/^  <data/,$d' < i18n/Text.resx > ${DST}
    cat ${POFILE} \
      | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
      | awk 'function f() {
                 if (id && msgstr) {
                     print "  <data name=\"" id "\" xml:space=\"preserve\">";
                     print "    <value>" msgstr "</value>";
                     if (0 && comment) { print "    <comment>" comment "</comment>"; }
                     print "  </data>"; reset();
                 }
             }
             function reset() { id=""; comment=""; }
             /^$/        { f(); }
             END         { f(); }
             /^#[.] /    { split($0, a, "#[.] "); comment=a[2]; }
             /^#:.*ID:/  { split($0, a, "ID:"); id=a[2]; }
             /^#, fuzzy/ { reset(); }
             /^ *"/      { split($0, a, "\""); msgstr=msgstr a[2]; }
             /^msgstr/   { split($0, a, "\""); msgstr=a[2]; }' \
      >> ${DST}
    echo "</root>" >> ${DST}
done

#
# Use Unicode description files from the unicode translation project
# and create .resx translation files for our project
#

echo "[3/3] Rebuild Unicode translation files…"
BASE=http://translationproject.org/latest/unicode-translation/
PO=$(wget -qO- $BASE | tr '<>' '\n' | sed -ne 's/^\(..\)[.]po$/\1/p')
for LANG in $PO; do
    printf "${LANG}... "
    SRC=${CACHE}/unicode-${LANG}.po
    DEST=i18n/Unicode.${LANG}.resx
    # Get latest translation if new
    (cd ${CACHE}
     mv unicode-${LANG}.po ${LANG}.po
     wget -q -N ${BASE}/${LANG}.po
     mv ${LANG}.po unicode-${LANG}.po)

    # Parse data and put it in the .resx file
    sed -e '/^  <data/,$d' < i18n/Unicode.resx > ${DEST}
    if uname | grep -qi mingw; then unix2dos; else cat; fi < ${SRC} \
      | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
      | awk 'function f() {
                 if (c && msgstr) {
                     print "  <data name=\"U" c "\" xml:space=\"preserve\">";
                     print "    <value>" msgstr "</value>";
                     print "  </data>";
                 }
                 c=""; msgstr="";
             }
             /^#[.] CHARACTER NAME for / { split($0, a, "+"); c=a[2]; }
             /^msgstr/ { split($0, a, "\""); msgstr=a[2]; f(); }' \
      >> ${DEST}
    echo "</root>" >> ${DEST}
done
echo "done."

