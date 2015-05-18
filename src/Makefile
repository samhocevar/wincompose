
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = $(shell sed -ne 's/.*<ApplicationVersion>\([^<]*\).*/\1/p' build.xml)
CONFIG = Release

ISS = installer.iss

TXT = rules/Xorg.txt \
      rules/Xcompose.txt \
      rules/Emoji.txt \
      rules/WinCompose.txt

EXE = bin/$(CONFIG)/WinCompose.exe
LANG = $(sort $(subst ., , $(suffix $(basename $(wildcard i18n/*.*.resx unicode/*.*.resx)))))

INSTALLER = WinCompose-Setup-$(VERSION).exe
PORTABLE = WinCompose-NoInstall-$(VERSION).zip

DEVENVDIR = "$(VS110COMNTOOLS)\\..\\..\\Common7\\IDE"
ISCCDIR = c:\\Program Files (x86)\\Inno Setup 5

ISCC = $(ISCCDIR)\\ISCC.exe
DEVENV = $(DEVENVDIR)\\devenv.com
MSGMERGE = msgmerge

all: clean installer portable

clean:
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project wincompose.csproj
	rm -f $(INSTALLER) $(PORTABLE)
	rm -rf obj wincompose/obj wincompose.gui/obj || true
	rm -rf bin wincompose/bin wincompose.gui/bin || true

installer: $(EXE) $(ISS)
	rm -f $(INSTALLER)
	CONFIG=$(CONFIG) VERSION=$(VERSION) "$(ISCC)" $(ISS)

portable: $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/rules
	mkdir -p $(addprefix $(PORTABLE:.zip=)/, $(LANG))
	for L in $(LANG); do cp bin/$(CONFIG)/$${L}/WinCompose.resources.dll $(PORTABLE:.zip=)/$${L}/; done
	cp $(EXE) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/$(CONFIG)/WinCompose.exe:
	@mkdir -p bin/$(CONFIG)
	@rm -f $@
	$(DEVENV) wincompose.sln //build $(CONFIG) //project wincompose.csproj

