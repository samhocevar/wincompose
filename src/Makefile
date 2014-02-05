
VERSION = 0.4.5

ISS = WinCompose.iss
AHK = WinCompose.ahk
EXE = WinCompose.exe
INSTALLER = WinCompose-Setup-$(VERSION).exe

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKCDIR = c:\\Program Files\\AutoHotkey\\Compiler

all: fixsource installer

clean:
	rm -f $(EXE) $(INSTALLER)

fixsource:
	sed -i 's/\(#define *VERSION *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(ISS)
	sed -i 's/\(global *version *:= *"\)[^"]*\(".*\)/\1$(VERSION)\2/' $(AHK)

installer: $(EXE) $(ISS)
	rm -f $@
	"$(ISCCDIR)\\ISCC.exe" $(ISS)
	rm -f $(EXE)

%.exe: %.ahk
	rm -f $@
	"$(AHKCDIR)\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico //bin "$(AHKCDIR)\\Unicode 32-bit.bin"

