
EXE = WinCompose.exe
ISS = WinCompose.iss

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5
AHKCDIR = c:\\Program Files\\AutoHotkey\\Compiler

all: installer

clean:
	rm -f $(EXE)

installer: $(EXE) $(ISS)
	rm -f $@
	"$(ISCCDIR)\\ISCC.exe" $(ISS)
	rm -f $(EXE)

%.exe: %.ahk
	rm -f $@
	"$(AHKCDIR)\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico //bin "$(AHKCDIR)\\Unicode 32-bit.bin"

