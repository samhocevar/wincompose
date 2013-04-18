
all: WinCompose.exe

WinCompose.exe: WinCompose.ahk
	"c:\\Program Files\\AutoHotkey\\Compiler\\Ahk2Exe.exe" //in $^ //out $@ //icon res/wc.ico

