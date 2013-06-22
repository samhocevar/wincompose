
EXE = WinCompose.exe
ISS = WinCompose.iss

all: installer

clean:
	rm -f $(EXE)

installer: $(EXE) $(ISS)
	rm -f $@
	"c:\\Program Files (x86)\\Inno Setup 5\\ISCC.exe" $(ISS)
	rm -f $(EXE)

%.exe: %.ahk
	rm -f $@
	"c:\\Program Files\\AutoHotkey\\Compiler\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico

