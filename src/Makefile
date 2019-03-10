
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

MAIN_EXE = bin/$(CONFIG)/wincompose.exe
EXE = $(MAIN_EXE) \
      bin/$(CONFIG)/wincompose-sequences.exe \
      bin/$(CONFIG)/wincompose-settings.exe
DLL = bin/$(CONFIG)/language.dll \
      bin/$(CONFIG)/Emoji.Wpf.dll \
      bin/$(CONFIG)/Typography.OpenFont.dll \
      bin/$(CONFIG)/Typography.GlyphLayout.dll
LANG = $(sort $(subst ., , $(suffix $(basename $(wildcard i18n/*.*.resx unicode/*.*.resx)))))

PORTABLE = WinCompose-NoInstall-$(VERSION).zip

VSWHERE = "${ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
DEVENV = "$(shell $(VSWHERE) | sed -ne 's/^productPath: //p' | sed 's/devenv.exe/devenv.com/' | head -n 1)"
MSGMERGE = msgmerge

all: installer portable
	@echo
	@echo Latest: $(VERSION)
	@echo Installer: https://github.com/samhocevar/wincompose/releases/download/v$(VERSION)/WinCompose-Setup-$(VERSION).exe
	@echo InstallerSHA256: $(shell sha256sum WinCompose-Setup-$(VERSION).exe)
	@echo InstallerMD5: $(shell md5sum WinCompose-Setup-$(VERSION).exe)
	@echo Portable: https://github.com/samhocevar/wincompose/releases/download/v$(VERSION)/WinCompose-NoInstall-$(VERSION).zip
	@echo PortableSHA256: $(shell sha256sum $(PORTABLE))
	@echo PortableMD5: $(shell md5sum $(PORTABLE))

clean:
	$(foreach x, $(EXE), $(DEVENV) wincompose.sln //clean $(CONFIG) //project $(basename $(notdir $x)).csproj;)
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project language.csproj
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project installer.csproj
	$(DEVENV) wincompose.sln //clean $(CONFIG) //project trampoline.vcxproj
	rm -f $(PORTABLE)
	rm -rf obj bin || true

installer: $(EXE) $(ISS) $(PAS)
	$(DEVENV) wincompose.sln //build $(CONFIG) //project installer.csproj

portable: $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir -p $(PORTABLE:.zip=)/rules
	mkdir -p $(addprefix $(PORTABLE:.zip=)/, $(LANG))
	for L in $(LANG); do cp bin/$(CONFIG)/$${L}/language.resources.dll $(PORTABLE:.zip=)/$${L}/; done
	cp $(EXE) $(DLL) $(PORTABLE:.zip=)
	cp $(MAIN_EXE).config $(DLL) $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)/rules
	zip -r $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)

# Sometimes Visual Studio does not rebuild this dependency
$(MAIN_EXE): bin/$(CONFIG)/language.dll

%.exe %.dll:
	@mkdir -p bin/$(CONFIG)
	@rm -f $@
	$(DEVENV) wincompose.sln //build $(CONFIG) //project $(basename $(notdir $@)).csproj

