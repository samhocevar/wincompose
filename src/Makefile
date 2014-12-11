
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.12

ISS = installer.iss
SRC_AHK = main.ahk
SRC_INC = constants.ahk ui.ahk utils.ahk
RC = resources.rc
PO = $(filter-out en, $(shell sed -ne '/available languages/,/}/p' < constants.ahk  | sed -ne 's/.*"\(..\)".*/\1/p'))

TXT = res/Keys.txt res/Xorg.txt res/Xcompose.txt res/WinCompose.txt
ICONS = res/icon_normal.ico res/icon_active.ico res/icon_disabled.ico res/icon_empty.ico

EXE = obj/WinCompose.exe
DLL = res/resources.dll
INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKDIR = ./ahk

AHK = $(AHKDIR)/bin/Win32w/AutoHotkey.exe
ISCC = $(ISCCDIR)\\ISCC.exe
WINDRES = windres
XGETTEXT = xgettext
MSGMERGE = msgmerge

all: installer portable

clean:
	rm -f $(INSTALLER) $(PORTABLE)
	rm -f $(DLL) $(DLL:%.dll:%.o) $(EXE)
	@rmdir obj >/dev/null 2>&1 || true

run:
	$(AHK) main.ahk

check:
	$(AHK) tests.ahk

installer: update-source update-po $(DLL) $(EXE) $(ISS)
	rm -f $(INSTALLER)
	"$(ISCC)" $(ISS)

portable: update-source update-po $(DLL) $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/po $(PORTABLE:.zip=)/res
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(PO:%=po/%.po) $(PORTABLE:.zip=)/po
	cp res/resources.dll $(TXT) $(PORTABLE:.zip=)/res
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

obj/%.exe: update-ahk $(SRC_AHK) $(SRC_INC)
	@mkdir -p obj
	@rm -f $@
	$(AHK) 'ahk2exe\\ahk2exe.ahk' //in $(SRC_AHK) //out $@ //icon res/icon_normal.ico //bin $(AHKDIR)/bin/Win32w/AutoHotkeySC.bin

res/%.dll: obj/%.o
	$(CC) -shared $^ -o $@

obj/%.o: $(RC) $(ICONS)
	mkdir -p obj
	rm -f $@
	$(WINDRES) $(RC) -o $@

update-source:
	sed -i 's/\(#define *VERSION *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(ISS)
	sed -i 's/\(global *version *:= *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(SRC_AHK)

update-po:
	$(XGETTEXT) -LC -opo/wincompose.pot --from-code=utf-8 -k_ $(SRC_AHK) $(SRC_INC)
	for l in $(PO); do printf %s $$l && msgmerge -U po/$$l.po po/wincompose.pot; done
	rm -f po/*~
	X=true; for l in $(PO); do if grep -q '"po.'$$l'[.]po"' $(ISS); then : ; else echo "po/$$l.po" not in $(ISS); X=false; fi; done; $$X || exit 1

update-ahk:
	cd $(AHKDIR) && devenv AutoHotkeyx.sln //build Release //project AutoHotkeyx.vcxproj
	cd $(AHKDIR) && devenv AutoHotkeyx.sln //build Self-contained //project AutoHotkeyx.vcxproj

