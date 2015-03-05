
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta20150303

ISS = installer.iss
PO = $(notdir $(basename $(wildcard po/*.po)))

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

installer: update-po $(EXE) $(ISS)
	rm -f $(INSTALLER)
	"$(ISCC)" $(ISS)

portable: update-po $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/po $(PORTABLE:.zip=)/rules
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	cp $(PO:%=po/%.po) $(PORTABLE:.zip=)/po
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/Release/WinCompose.exe:
	@mkdir -p bin/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.csproj

# Populate po/wincompose.pot with strings from Text.resx and update *.po
resx2pot:
	cat i18n/Text.resx | awk '\
	    /<!--/      { off=1 } \
	    /-->/       { off=0 } \
	    /<data /    { split($$0, a, "\""); id=a[2]; comment=""; } \
	    /<value>/   { split ($$0, a, /[<>]/); value=a[3]; } \
	    /<comment>/ { split ($$0, a, /[<>]/); id=id " -- " a[3]; } \
	    /<\/data>/  { if (!off) { print "#: ID:" id; print "msgid \"" value "\""; print "msgstr \"\""; print ""; } }' \
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
	  sed -e '/^  <data/,$$d' < i18n/Text.resx > i18n/Text.$$l.resx; \
	  cat $$src \
          | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' \
          | awk '\
	    function f() { if (id && msgstr) { print "  <data name=\"" id "\" xml:space=\"preserve\">"; print "    <value>" msgstr "</value>"; if (comment) { print "    <comment>" comment "</comment>"; } print "  </data>"; id="" } } \
	    /^$$/       { f(); } \
	    END         { f(); } \
            /^#: ID:/   { split($$0, a, "(ID:| -- )"); id=a[2]; comment=a[3]; } \
            /^#, fuzzy/ { id=""; } \
            /^ *"/      { split($$0, a, "\""); msgstr=msgstr a[2]; } \
	    /^msgstr/   { split($$0, a, "\""); msgstr=a[2]; }' \
	  >> i18n/Text.$$l.resx; \
	  echo "</root>" >> i18n/Text.$$l.resx; \
	done

