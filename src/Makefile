
VERSION = 0.4.5

ISS = installer.iss
AHK = WinCompose.ahk
RC = resources.rc

ICONS = res/wc.ico res/wca.ico res/wcd.ico

EXE = obj/WinCompose.exe
DLL = obj/resources.dll
INSTALLER = WinCompose-Setup-$(VERSION).exe

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKCDIR = c:\\Program Files\\AutoHotkey\\Compiler

all: installer

clean:
	rm -f $(DLL) $(DLL:%.dll:%.o) $(EXE) $(INSTALLER)
	@rmdir obj >/dev/null 2>&1 || true

fixsource:
	sed -i 's/\(#define *VERSION *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(ISS)
	sed -i 's/\(global *version *:= *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(AHK)

installer: fixsource $(DLL) $(EXE) $(ISS)
	rm -f $@
	"$(ISCCDIR)\\ISCC.exe" $(ISS)

obj/%.exe: %.ahk
	@mkdir -p obj
	@rm -f $@
	"$(AHKCDIR)\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico //bin "$(AHKCDIR)\\Unicode 32-bit.bin"

%.dll: %.o
	$(CC) -shared $^ -o $@

obj/%.o: $(RC) $(ICONS)
	mkdir -p obj
	rm -f $@
	windres $(RC) -o $@

