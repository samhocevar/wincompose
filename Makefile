
EXE = WinCompose.exe

all: $(EXE)

clean:
	rm -f $(EXE)

%.exe: %.ahk
	"c:\\Program Files\\AutoHotkey\\Compiler\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico

