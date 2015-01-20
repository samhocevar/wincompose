
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta

ISS = installer.iss
PO = be fr de

TXT = res/Xorg.txt res/Xcompose.txt res/WinCompose.txt

EXE = bin/Release/WinCompose.exe
GUIEXE = WinCompose.Gui/obj/Release/WinCompose.Gui.exe
INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5

ISCC = $(ISCCDIR)\\ISCC.exe
XGETTEXT = xgettext
MSGMERGE = msgmerge

all: installer portable

clean:
	rm -f $(INSTALLER) $(PORTABLE)
	@rmdir obj >/dev/null 2>&1 || true

installer: update-po $(EXE) $(GUIEXE) $(ISS)
	rm -f $(INSTALLER)
	"$(ISCC)" $(ISS)

portable: update-po $(EXE) $(GUIEXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/po $(PORTABLE:.zip=)/res
	cp $(EXE) $(GUIEXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/res
	cp $(PO:%=po/%.po) $(PORTABLE:.zip=)/po
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/Release/%.exe:
	@mkdir -p bin/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.csproj

WinCompose.Gui/obj/Release/%.exe:
	@mkdir -p WinCompose.Gui/obj/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.gui\\wincompose.gui.csproj

update-po:
	#$(XGETTEXT) -LC -opo/wincompose.pot --from-code=utf-8 -k_ *.cs
	#for l in $(PO); do printf %s $$l && msgmerge -U po/$$l.po po/wincompose.pot; done
	#rm -f po/*~
	#X=true; for l in $(PO); do if grep -q '"po.'$$l'[.]po"' $(ISS); then : ; else echo "po/$$l.po" not in $(ISS); X=false; fi; done; $$X || exit 1

