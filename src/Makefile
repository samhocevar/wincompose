# Requirements:
#  - Inno Setup 6 (download and install)
#  - GitVersion (choco install gitversion)
#  - zip/unzip (mingw-get install msys-zip msys-unzip)
#  - nuget
#  - Visual Studio (ensure vswhere.exe is in %PATH%)
#  - gettext (mingw-get install msys-gettext)

VERSION = $(shell gitversion -showvariable MajorMinorPatch)
FRAMEWORK = net481
CONFIG = Release

SRCDIR = wincompose
BINDIR = wincompose\bin\$(CONFIG)\$(FRAMEWORK)

SLN = wincompose.sln
ISS = installer/installer.iss
PAS = installer/installer.pas

TXT = $(SRCDIR)/rules/DefaultUserSequences.txt \
      $(SRCDIR)/rules/Emoji.txt \
      $(SRCDIR)/rules/WinCompose.txt

EXE = $(BINDIR)/wincompose.exe
DLL = $(BINDIR)/language.dll \
      $(BINDIR)/Emoji.Wpf.dll \
      $(BINDIR)/Hardcodet.NotifyIcon.Wpf.dll \
      $(BINDIR)/NLog.dll \
      $(BINDIR)/stfu.dll \
	  $(BINDIR)/Wpf.Ui.dll \
      $(BINDIR)/System.ValueTuple.dll \
      $(BINDIR)/Typography.OpenFont.dll \
      $(BINDIR)/Typography.GlyphLayout.dll
LANG = $(sort $(subst ., , $(suffix $(basename $(wildcard language/i18n/*.*.resx language/unicode/*.*.resx)))))

PORTABLE = WinCompose-NoInstall-$(VERSION).zip

VSWHERE = "${ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
MSBUILD = "$(shell $(VSWHERE) -latest -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe')"
MSGMERGE = msgmerge

all: check installer portable
	@echo
	@echo Latest: $(VERSION)
	@echo Installer: https://github.com/ell1010/wincompose/releases/download/v$(VERSION)/WinCompose-Setup-$(VERSION).exe
	@echo InstallerMD5: $(shell certutil -hashfile WinCompose-Setup-$(VERSION).exe MD5)
	@echo InstallerSHA256: $(shell certutil -hashfile WinCompose-Setup-$(VERSION).exe sha256)
	@echo Portable: https://github.com/ell1010/wincompose/releases/download/v$(VERSION)/WinCompose-NoInstall-$(VERSION).zip
	@echo PortableSHA256: $(shell certutil -hashfile $(PORTABLE) MD5)
	@echo PortableMD5: $(shell certutil -hashfile $(PORTABLE) sha256)

check:
	bash ./check-data.sh

clean:
	$(MSBUILD) $(SLN) -p:Configuration=$(CONFIG) -t:Clean
	rm -f $(PORTABLE)
	rm -rf obj bin || true
	rm -rf language/obj language/bin || true
	rm -rf wincompose/obj wincompose/bin || true

build:
	dotnet restore $(SLN)
	$(MSBUILD) $(SLN) -p:Configuration=$(CONFIG) -t:Build

installer: build $(EXE) $(ISS) $(PAS)

portable: build $(EXE)
	rm -f $(PORTABLE)
	rm -rf $(PORTABLE:.zip=)
	mkdir $(PORTABLE:.zip=)\rules
	mkdir $(addprefix $(PORTABLE:.zip=)\, $(LANG))
	for %%L in ($(LANG)) do cp $(BINDIR)\%%L\language.resources.dll $(PORTABLE:.zip=)\%%L\ 
	cp $(EXE) $(DLL) $(PORTABLE:.zip=)
	cp $(EXE).config $(PORTABLE:.zip=)
	cp $(TXT) $(PORTABLE:.zip=)\rules
	echo start wincompose.exe -settings > $(PORTABLE:.zip=)\wincompose-settings.bat
	echo start wincompose.exe -sequences > $(PORTABLE:.zip=)\wincompose-sequences.bat
	tar -a -c -f $(PORTABLE) $(PORTABLE:.zip=)
	rm -rf $(PORTABLE:.zip=)