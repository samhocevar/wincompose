
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = 0.6.99beta20150306

ISS = installer.iss

TXT = rules/Xorg.txt \
      rules/Xcompose.txt \
      rules/Emoji.txt \
      rules/WinCompose.txt

EXE = bin/Release/WinCompose.exe
INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5

ISCC = $(ISCCDIR)\\ISCC.exe
MSGMERGE = msgmerge

all: installer portable

clean:
	rm -f $(INSTALLER) $(PORTABLE)
	rm -rf obj wincompose/obj wincompose.gui/obj || true
	rm -rf bin wincompose/bin wincompose.gui/bin || true

installer: $(EXE) $(ISS)
	rm -f $(INSTALLER)
	"$(ISCC)" $(ISS)

portable: $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/rules
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/Release/WinCompose.exe:
	@mkdir -p bin/Release
	@rm -f $@
	devenv wincompose.sln //build Release //project wincompose.csproj

