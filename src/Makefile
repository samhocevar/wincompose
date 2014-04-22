
VERSION = 0.6.3

ISS = installer.iss
SRC_AHK = main.ahk
SRC_INC = constants.ahk ui.ahk utils.ahk
RC = resources.rc
PO = fr cs

ICONS = res/icon_normal.ico res/icon_active.ico res/icon_disabled.ico res/icon_empty.ico

EXE = obj/WinCompose.exe
DLL = obj/resources.dll
INSTALLER = WinCompose-Setup-$(VERSION).exe

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKDIR = ./ahk

AHK = $(AHKDIR)/bin/Win32w/AutoHotkey.exe
ISCC = $(ISCCDIR)\\ISCC.exe
WINDRES = windres
XGETTEXT = xgettext
MSGMERGE = msgmerge

all: installer

clean:
	rm -f $(DLL) $(DLL:%.dll:%.o) $(EXE) $(INSTALLER)
	@rmdir obj >/dev/null 2>&1 || true

run:
	$(AHK) main.ahk

check:
	$(AHK) tests.ahk

installer: update-source update-po $(DLL) $(EXE) $(ISS)
	rm -f $@
	"$(ISCC)" $(ISS)

obj/%.exe: update-ahk $(SRC_AHK) $(SRC_INC)
	@mkdir -p obj
	@rm -f $@
	$(AHK) 'ahk2exe\\ahk2exe.ahk' //in $(SRC_AHK) //out $@ //icon res/icon_normal.ico //bin $(AHKDIR)/bin/Win32w/AutoHotkeySC.bin

%.dll: %.o
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

update-ahk:
	cd $(AHKDIR) && devenv AutoHotkeyx.sln //build Release //project AutoHotkeyx
	cd $(AHKDIR) && devenv AutoHotkeyx.sln //build Self-contained //project AutoHotkeyx

