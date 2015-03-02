
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta20150302

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
	mkdir -p $(PORTABLE:.zip=)/po $(PORTABLE:.zip=)/res
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/res
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

