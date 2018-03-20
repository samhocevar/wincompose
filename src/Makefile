
# Requirements:
#  - Inno Setup 5 (download and install)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - Visual Studio (ensure devenv.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = $(shell sed -ne 's/.*<ApplicationVersion>\([^<]*\).*/\1/p' build.config)
CONFIG = Release

ISS = installer.iss
PAS = installer.pas

TXT = rules/DefaultUserSequences.txt \
      rules/Emoji.txt \
      rules/WinCompose.txt

EXE = bin/$(CONFIG)/wincompose.exe
DLL = bin/$(CONFIG)/Emoji.Wpf.dll \
      bin/$(CONFIG)/Typography.OpenFont.dll \
      bin/$(CONFIG)/Typography.GlyphLayout.dll
LANG = $(sort $(subst ., , $(suffix $(basename $(wildcard i18n/*.*.resx unicode/*.*.resx)))))

PORTABLE = WinCompose-NoInstall-$(VERSION).zip

# XXX: this is necessary until vswhere.exe actually works
VS150COMNTOOLS = C:\\Program\ Files\ \(x86\)\\Microsoft\ Visual\ Studio\\2017\\Community\\Common7\\Tools
DEVENV = $(VS150COMNTOOLS)\\..\\..\\Common7\\IDE\\devenv.com
MSGMERGE = msgmerge

all: clean installer portable
	@echo
	@echo Latest: $(VERSION)
	@echo Installer: https://github.com/samhocevar/wincompose/releases/download/v$(VERSION)/WinCompose-Setup-$(VERSION).exe
	@echo InstallerSHA256: $(shell sha256sum WinCompose-Setup-$(VERSION).exe)
	@echo InstallerMD5: $(shell md5sum WinCompose-Setup-$(VERSION).exe)
	@echo Portable: https://github.com/samhocevar/wincompose/releases/download/v$(VERSION)/WinCompose-NoInstall-$(VERSION).zip
	@echo PortableSHA256: $(shell sha256sum $(PORTABLE))
	@echo PortableMD5: $(shell md5sum $(PORTABLE))

clean:
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project wincompose.csproj
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project installer.vcxproj
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project trampoline.vcxproj
	rm -f $(PORTABLE)
	rm -rf obj wincompose/obj wincompose.gui/obj || true
	rm -rf bin wincompose/bin wincompose.gui/bin || true

installer: $(EXE) $(ISS) $(PAS)
	$(DEVENV) wincompose.sln //rebuild $(CONFIG) //project installer.vcxproj

portable: $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/rules
	mkdir -p $(addprefix $(PORTABLE:.zip=)/, $(LANG))
	for L in $(LANG); do cp bin/$(CONFIG)/$${L}/WinCompose.resources.dll $(PORTABLE:.zip=)/$${L}/; done
	cp $(EXE) $(DLL) $(PORTABLE:.zip=)
	cp $(EXE).config $(DLL) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

bin/$(CONFIG)/wincompose.exe:
	@mkdir -p bin/$(CONFIG)
	@rm -f $@
	$(DEVENV) wincompose.sln //build $(CONFIG) //project wincompose.csproj

