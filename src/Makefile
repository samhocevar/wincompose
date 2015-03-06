
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta20150306

ISS = installer.iss

TXT = rules/Xorg.txt \
      rules/Xcompose.txt \
      rules/Emoji.txt \
      rules/WinCompose.txt

EXE = bin/Release/WinCompose.exe
INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5

ISCC = $(ISCCDIR)\\ISCC.exe
MSGMERGE = msgmerge

all: installer portable

clean:
	rm -f $(INSTALLER) $(PORTABLE)
	rm -rf obj wincompose/obj wincompose.gui/obj || true
	rm -rf bin wincompose/bin wincompose.gui/bin || true

installer: $(EXE) $(ISS)
	rm -f $(INSTALLER)
	"$(ISCC)" $(ISS)

portable: $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/rules
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/Release/WinCompose.exe:
	@mkdir -p bin/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.csproj

#
# Translation stuff we need to deprecate one day
#

PO := $(notdir $(basename $(wildcard po/*.po)))

# Populate po/wincompose.pot with strings from Text.resx and update *.po
resx2pot:
	cat i18n/Text.resx | awk '\
	    /<!--/      { off=1 } \
	    /-->/       { off=0 } \
	    /<data /    { split($$0, a, "\""); id=a[2]; comment=""; } \
	    /<value>/   { split ($$0, a, /[<>]/); value=a[3]; } \
	    /<comment>/ { split ($$0, a, /[<>]/); comment=a[3]; } \
	    /<\/data>/  { if (!off) { if (comment) { print "#. " comment; } print "#: ID:" id; print "msgid \"" value "\""; print "msgstr \"\""; print ""; } }' \
	| sed 's/&amp;/\&/g; s/&lt;/</g; s/&gt;/>/g' \
	> po/wincompose.pot
	for l in $(PO); do printf %s $$l && msgmerge -U po/$$l.po po/wincompose.pot; done
	rm -f po/*~

# Update each Text.*.resx with contents from *.po
pot2resx:
	for l in $(PO); do \
	  src=po/$$l.po; \
          case $$l in \
	    zh) l=zh-CHT;; \
	    *@*) continue;; \
	  esac; \
	  dst=i18n/Text.$$l.resx; \
	  sed -e '/^  <data/,$$d' < i18n/Text.resx > $$dst; \
	  cat $$src \
          | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
          | awk '\
	    function f() { if (id && msgstr) { print "  <data name=\"" id "\" xml:space=\"preserve\">"; print "    <value>" msgstr "</value>"; if (0 && comment) { print "    <comment>" comment "</comment>"; } print "  </data>"; reset(); } } \
            function reset() { id=""; comment=""; } \
	    /^$$/       { f(); } \
	    END         { f(); } \
            /^#[.] /    { split($$0, a, "#[.] "); comment=a[2]; } \
            /^#: ID:/   { split($$0, a, ":"); id=a[3]; } \
            /^#, fuzzy/ { reset(); } \
            /^ *"/      { split($$0, a, "\""); msgstr=msgstr a[2]; } \
	    /^msgstr/   { split($$0, a, "\""); msgstr=a[2]; }' \
	  >> $$dst; \
	  echo "</root>" >> $$dst; \
	done

# Download Unicode description files from the unicode translation project
unicode2resx:
	for l in am da de fi fr ga nl pl rw sk sr; do \
	  src=translationproject.org/latest/unicode-translation/$$l.po; \
	  dst=i18n/Unicode.$$l.resx; \
	  sed -e '/^  <data/,$$d' < i18n/Unicode.resx > $$dst; \
	  wget -qO- $$src \
          | unix2dos \
          | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
	  | awk '\
            function f() { if (c && msgstr) { print "  <data name=\"U" c "\" xml:space=\"preserve\">"; print "    <value>" msgstr "</value>"; print "  </data>"; } c=""; msgstr=""; } \
	    /^#[.] CHARACTER NAME for /  { split($$0, a, "+"); c=a[2]; } \
            /^msgstr/ { split($$0, a, "\""); msgstr=a[2]; f(); } ' \
	  >> $$dst; \
	  echo "</root>" >> $$dst; \
	done

