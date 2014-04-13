
VERSION = 0.6.0

ISS = installer.iss
AHK = main.ahk
INC = constants.ahk ui.ahk utils.ahk
RC = resources.rc
PO = fr cs

ICONS = res/icon_normal.ico res/icon_active.ico res/icon_disabled.ico res/icon_empty.ico

EXE = obj/WinCompose.exe
DLL = obj/resources.dll
INSTALLER = WinCompose-Setup-$(VERSION).exe

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKCDIR = c:\\Program Files\\AutoHotkey\\Compiler

ISCC = $(ISCCDIR)\\ISCC.exe
AHKC = $(AHKCDIR)\\Ahk2Exe.exe
WINDRES = windres
XGETTEXT = xgettext
MSGMERGE = msgmerge

all: installer

clean:
	rm -f $(DLL) $(DLL:%.dll:%.o) $(EXE) $(INSTALLER)
	@rmdir obj >/dev/null 2>&1 || true

check:
	start tests.ahk

fixsource:
	sed -i 's/\(#define *VERSION *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(ISS)
	sed -i 's/\(global *version *:= *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(AHK)

installer: fixsource update-po $(DLL) $(EXE) $(ISS)
	rm -f $@
	"$(ISCC)" $(ISS)

obj/%.exe: $(AHK) $(INC)
	@mkdir -p obj
	@rm -f $@
	"$(AHKC)" //in $(AHK) //out $@ //icon res/icon_normal.ico //bin "$(AHKCDIR)\\Unicode 32-bit.bin"

%.dll: %.o
	$(CC) -shared $^ -o $@

obj/%.o: $(RC) $(ICONS)
	mkdir -p obj
	rm -f $@
	$(WINDRES) $(RC) -o $@

update-po:
	$(XGETTEXT) -LC -opo/wincompose.pot --from-code=utf-8 -k_ $(AHK) $(INC)
	for l in $(PO); do msgmerge -U po/$$l.po po/wincompose.pot; done

