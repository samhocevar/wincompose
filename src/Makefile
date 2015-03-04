
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta20150303

ISS = installer.iss
PO = $(wildcard po/*.po)

TXT = rules/Xorg.txt \
      rules/Xcompose.txt \
      rules/Emoji.txt \
      rules/WinCompose.txt

EXE = bin/Release/WinCompose.exe
INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5

ISCC = $(ISCCDIR)\\ISCC.exe
XGETTEXT = xgettext
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
	cp $(PO) $(PORTABLE:.zip=)/po
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/Release/WinCompose.exe:
	@mkdir -p bin/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.csproj

update-po:
	#$(XGETTEXT) -LC -opo/wincompose.pot --from-code=utf-8 -k_ *.cs
	#for l in $(PO); do printf %s $$l && msgmerge -U po/$$l.po po/wincompose.pot; done
	#rm -f po/*~
	#X=true; for l in $(PO); do if grep -q '"po.'$$l'[.]po"' $(ISS); then : ; else echo "po/$$l.po" not in $(ISS); X=false; fi; done; $$X || exit 1

resx2pot:
	cat i18n/Text.resx | awk '\
	    /<!--/      { off=1 } \
	    /-->/       { off=0 } \
	    /^ *<data / { split($$0, a, "\""); id=a[2]; comment=""; } \
	    /<value>/   { split ($$0, a, /[<>]/); value=a[3]; } \
	    /<comment>/ { split ($$0, a, /[<>]/); comment=a[3]; } \
	    /<\/data>/  { if (!off) { print "#: ID:" id; if (comment) { print "#: " comment; } print "msgid \"" value "\""; print "msgstr \"\""; print ""; } }' \
	| sed 's/&amp;/\&/g; s/&lt;/</g; s/&gt;/>/g' \
	> po/wincompose.pot
	for l in $(PO); do printf %s $$l && msgmerge -U $$l po/wincompose.pot; done
	rm -f po/*~

