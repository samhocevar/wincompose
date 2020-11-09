#define NAME "WinCompose"
#define AUTHOR "Sam Hocevar"
#define EXE "wincompose.exe"
#ifndef VERSION
#   define VERSION GetEnv('VERSION')
#endif
#ifndef CONFIG
#   define CONFIG GetEnv('CONFIG')
#endif

[Setup]
AppName = {#NAME}
AppVersion = {#VERSION}
AppPublisher = {#AUTHOR}
AppPublisherURL = http://sam.hocevar.net/
OutputBaseFilename = "{#NAME}-Setup-{#VERSION}"
ArchitecturesInstallIn64BitMode = x64
DefaultDirName = {commonpf}\{#NAME}
DefaultGroupName = {#NAME}
SetupIconFile = "res\icon_normal.ico"
UninstallDisplayIcon = "{app}\{#EXE}"
Compression = lzma2
SolidCompression = yes
OutputDir = .
ShowLanguageDialog = auto
; We only install stuff in {userstartup} and in {app}. Since the latter
; can be overridden, we do not necessarily need elevated privileges, so
; let the user decide.
PrivilegesRequired = lowest

[Files]
; We put this at the beginning so that it’s easier to decompress
Source: "bin\{#CONFIG}\installer-helper.dll"; DestDir: "{tmp}"; Flags: dontcopy

Source: "bin\{#CONFIG}\{#EXE}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\{#EXE}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\language.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Emoji.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Typography.OpenFont.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Typography.GlyphLayout.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Hardcodet.Wpf.TaskbarNotification.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\am\*.dll"; DestDir: "{app}\am"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ar\*.dll"; DestDir: "{app}\ar"; Flags: ignoreversion
Source: "bin\{#CONFIG}\be\*.dll"; DestDir: "{app}\be"; Flags: ignoreversion
Source: "bin\{#CONFIG}\be-BY\*.dll"; DestDir: "{app}\be-BY"; Flags: ignoreversion
Source: "bin\{#CONFIG}\cs\*.dll"; DestDir: "{app}\cs"; Flags: ignoreversion
Source: "bin\{#CONFIG}\da\*.dll"; DestDir: "{app}\da"; Flags: ignoreversion
Source: "bin\{#CONFIG}\de\*.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "bin\{#CONFIG}\de-CH\*.dll"; DestDir: "{app}\de-CH"; Flags: ignoreversion
Source: "bin\{#CONFIG}\el\*.dll"; DestDir: "{app}\el"; Flags: ignoreversion
Source: "bin\{#CONFIG}\es\*.dll"; DestDir: "{app}\es"; Flags: ignoreversion
Source: "bin\{#CONFIG}\et\*.dll"; DestDir: "{app}\et"; Flags: ignoreversion
Source: "bin\{#CONFIG}\fi\*.dll"; DestDir: "{app}\fi"; Flags: ignoreversion
Source: "bin\{#CONFIG}\fr\*.dll"; DestDir: "{app}\fr"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ga\*.dll"; DestDir: "{app}\ga"; Flags: ignoreversion
Source: "bin\{#CONFIG}\hr\*.dll"; DestDir: "{app}\hr"; Flags: ignoreversion
Source: "bin\{#CONFIG}\hu\*.dll"; DestDir: "{app}\hu"; Flags: ignoreversion
Source: "bin\{#CONFIG}\id\*.dll"; DestDir: "{app}\id"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it\*.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it-CH\*.dll"; DestDir: "{app}\it-CH"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ja\*.dll"; DestDir: "{app}\ja"; Flags: ignoreversion
Source: "bin\{#CONFIG}\lt\*.dll"; DestDir: "{app}\lt"; Flags: ignoreversion
Source: "bin\{#CONFIG}\nl\*.dll"; DestDir: "{app}\nl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\no\*.dll"; DestDir: "{app}\no"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pl\*.dll"; DestDir: "{app}\pl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pt\*.dll"; DestDir: "{app}\pt"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pt-BR\*.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ro\*.dll"; DestDir: "{app}\ro"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ru\*.dll"; DestDir: "{app}\ru"; Flags: ignoreversion
Source: "bin\{#CONFIG}\rw\*.dll"; DestDir: "{app}\rw"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sk\*.dll"; DestDir: "{app}\sk"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sl\*.dll"; DestDir: "{app}\sl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sq\*.dll"; DestDir: "{app}\sq"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sr\*.dll"; DestDir: "{app}\sr"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sv\*.dll"; DestDir: "{app}\sv"; Flags: ignoreversion
Source: "bin\{#CONFIG}\uk\*.dll"; DestDir: "{app}\uk"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHS\*.dll"; DestDir: "{app}\zh-CHS"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHT\*.dll"; DestDir: "{app}\zh-CHT"; Flags: ignoreversion
Source: "rules\DefaultUserSequences.txt"; DestDir: "{app}\res"
Source: "rules\Emoji.txt"; DestDir: "{app}\res"
Source: "rules\WinCompose.txt"; DestDir: "{app}\res"

[Languages]
; Put English first, because Inno Setup will apparently fall back to the first
; language specified in this section when the current Windows UI language is
; not supported.
Name: "en"; MessagesFile: "compiler:Default.isl"

Name: "ar"; MessagesFile: "3rdparty/innosetup/Files/Languages/Unofficial/Arabic.isl"
; Name: "ca"; MessagesFile: "compiler:Languages/Catalan.isl"
Name: "cs"; MessagesFile: "compiler:Languages/Czech.isl"
Name: "da"; MessagesFile: "compiler:Languages/Danish.isl"
Name: "fi"; MessagesFile: "compiler:Languages/Finnish.isl"
Name: "fr"; MessagesFile: "compiler:Languages/French.isl"
Name: "de"; MessagesFile: "compiler:Languages/German.isl"
Name: "es"; MessagesFile: "compiler:Languages/Spanish.isl"
; Name: "he"; MessagesFile: "compiler:Languages/Hebrew.isl"
Name: "hr"; MessagesFile: "3rdparty/innosetup/Files/Languages/Unofficial/Croatian.isl"
; Name: "hy"; MessagesFile: "compiler:Languages/Armenian.islu"
Name: "id"; MessagesFile: "3rdparty/innosetup/Files/Languages/Unofficial/Indonesian.isl"
; Name: "is"; MessagesFile: "compiler:Languages/Icelandic.isl"
Name: "it"; MessagesFile: "compiler:Languages/Italian.isl"
Name: "ja"; MessagesFile: "compiler:Languages/Japanese.isl"
Name: "lt"; MessagesFile: "3rdparty/innosetup/Files/Languages/Unofficial/Lithuanian.isl"
Name: "nl"; MessagesFile: "compiler:Languages/Dutch.isl"
Name: "no"; MessagesFile: "compiler:Languages/Norwegian.isl"
Name: "pl"; MessagesFile: "compiler:Languages/Polish.isl"
Name: "pt"; MessagesFile: "compiler:Languages/Portuguese.isl"
Name: "pt_BR"; MessagesFile: "compiler:Languages/BrazilianPortuguese.isl"
Name: "ru"; MessagesFile: "compiler:Languages/Russian.isl"
; Name: "Cy-sr-SP"; MessagesFile: "compiler:Languages/SerbianCyrillic.isl"
; Name: "Lt-sr-SP"; MessagesFile: "compiler:Languages/SerbianLatin.isl"
Name: "sk"; MessagesFile: "compiler:Languages/Slovak.isl"
Name: "sl"; MessagesFile: "compiler:Languages/Slovenian.isl"
Name: "sv"; MessagesFile: "3rdparty/innosetup/Files/Languages/Unofficial/Swedish.isl"
; Name: "tr"; MessagesFile: "compiler:Languages/Turkish.isl"
Name: "uk"; MessagesFile: "compiler:Languages/Ukrainian.isl"

; Name: "??"; MessagesFile: "compiler:Languages/Corsican.isl"
; Name: "??"; MessagesFile: "compiler:Languages/Nepali.islu"
; Name: "??"; MessagesFile: "compiler:Languages/ScottishGaelic.isl"

; FIXME: these languages are present as unofficial translations but contain errors
; be / Belarusian.isl
; eo / Esperanto.isl
; et / Estonian.isl
; ro / Romanian.isl
; sq / Albanian.isl

; FIXME: these languages used to be in Inno Setup 5 but are no longer here
; el / Greek.isl
; hu / Hungarian.isl

[Icons]
Name: "{group}\Uninstall {#NAME}"; Filename: "{uninstallexe}";
; Set the elevation bit on wincompose.exe.lnk because it launches an elevated task
Name: "{group}\{#NAME}"; Filename: "{sys}\schtasks"; Parameters: "/tn ""{#NAME}"" /run"; \
    IconFilename: "{app}\{#EXE}"; AfterInstall: set_elevation_bit('{group}\{#NAME}.lnk')
; We provide these shortcuts in case the notification area icon is disabled
Name: "{group}\{#NAME} Sequences"; Filename: "{app}\{#EXE}"; IconIndex: 1; Parameters: "-sequences"
Name: "{group}\{#NAME} Settings"; Filename: "{app}\{#EXE}"; IconIndex: 2; Parameters: "-settings"

[Run]
; After installation is finished, add an event-triggered scheduled task, then
; try to re-add the same task with elevated privileges and various tweaks that
; schtasks.exe does not support.
; This should allow to run WinCompose from the startup menu without triggering
; the UAC window. Running with high privileges is necessary to inject keyboard
; events into other high level processes, such as cmd.exe run as Administrator.
; FIXME: On Windows XP, tasks are in {win}\Tasks instead of {sys}\Tasks and
; their format is .job (binary) instead of .xml!
Filename: "{sys}\schtasks"; Parameters: "/tn ""{#NAME}"" /f /create /sc onlogon /tr ""\""{app}\{#EXE}\"" /fromtask"""; Flags: runhidden
Filename: "{sys}\schtasks"; Parameters: "/tn ""{#NAME}"" /f /create /xml ""{sys}\Tasks\{#NAME}"""; \
    BeforeInstall: fix_scheduled_task('{sys}\Tasks\{#NAME}'); Flags: runhidden
; Launch the task just after installation
Filename: "{sys}\schtasks"; Parameters: "/tn ""{#NAME}"" /run"; Flags: runhidden

[InstallDelete]
; We used to be installed in c:\Program Files (x86)
Type: filesandordirs; Name: "{commonpf32}\{#NAME}"
; We used to call our uninstaller shortcut “Uninstall”
Type: files; Name: "{group}\Uninstall.lnk"
; We used to add ourselves to user startup
Type: files; Name: "{userstartup}\{#NAME}.lnk"
; We moved translations into a separate language.dll project
Type: files; Name: "{app}\am\wincompose.resources.dll"
Type: files; Name: "{app}\be\wincompose.resources.dll"
Type: files; Name: "{app}\be-BY\wincompose.resources.dll"
Type: files; Name: "{app}\cs\wincompose.resources.dll"
Type: files; Name: "{app}\da\wincompose.resources.dll"
Type: files; Name: "{app}\de\wincompose.resources.dll"
Type: files; Name: "{app}\de-CH\wincompose.resources.dll"
Type: files; Name: "{app}\el\wincompose.resources.dll"
Type: files; Name: "{app}\es\wincompose.resources.dll"
Type: files; Name: "{app}\et\wincompose.resources.dll"
Type: files; Name: "{app}\fi\wincompose.resources.dll"
Type: files; Name: "{app}\fr\wincompose.resources.dll"
Type: files; Name: "{app}\ga\wincompose.resources.dll"
Type: files; Name: "{app}\hu\wincompose.resources.dll"
Type: files; Name: "{app}\id\wincompose.resources.dll"
Type: files; Name: "{app}\it\wincompose.resources.dll"
Type: files; Name: "{app}\it-CH\wincompose.resources.dll"
Type: files; Name: "{app}\ja\wincompose.resources.dll"
Type: files; Name: "{app}\nl\wincompose.resources.dll"
Type: files; Name: "{app}\pl\wincompose.resources.dll"
Type: files; Name: "{app}\pt-BR\wincompose.resources.dll"
Type: files; Name: "{app}\ro\wincompose.resources.dll"
Type: files; Name: "{app}\ru\wincompose.resources.dll"
Type: files; Name: "{app}\rw\wincompose.resources.dll"
Type: files; Name: "{app}\sk\wincompose.resources.dll"
Type: files; Name: "{app}\sl\wincompose.resources.dll"
Type: files; Name: "{app}\sq\wincompose.resources.dll"
Type: files; Name: "{app}\sr\wincompose.resources.dll"
Type: files; Name: "{app}\sv\wincompose.resources.dll"
Type: files; Name: "{app}\zh-CHS\wincompose.resources.dll"
Type: files; Name: "{app}\zh-CHT\wincompose.resources.dll"
; For some reason this was present on my computer, with a .suo file in
; it (dated from Sep 2017). I don’t know whether any real users are
; affected, but let’s err on the side of caution.
Type: filesandordirs; Name: "{app}\.vs"
; Legacy stuff that we need to remove
Type: files; Name: "{app}\rules\Xcompose.txt"
Type: files; Name: "{app}\rules\Xorg.txt"
Type: dirifempty; Name: "{app}\rules"
Type: files; Name: "{app}\res\resources.dll"
Type: files; Name: "{app}\res\wc.ico"
Type: files; Name: "{app}\res\wca.ico"
Type: files; Name: "{app}\res\wcd.ico"
Type: files; Name: "{app}\res\Compose.txt"
Type: files; Name: "{app}\res\Keys.txt"
Type: files; Name: "{app}\res\Xcompose.txt"
Type: files; Name: "{app}\res\Xorg.txt"
Type: dirifempty; Name: "{app}\res"
Type: files; Name: "{app}\locale\default.ini"
Type: files; Name: "{app}\locale\fr.ini"
Type: dirifempty; Name: "{app}\locale"
Type: files; Name: "{app}\po\*.po"
Type: dirifempty; Name: "{app}\po"
; Those were just tests and only a few beta releases had them
Type: files; Name: "{app}\{#NAME}-sequences.exe"
Type: files; Name: "{app}\{#NAME}-settings.exe"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#EXE}"; Flags: runhidden
; Use "nowait" because /f does not exist on XP / 2003
Filename: "{sys}\schtasks"; Parameters: "/delete /f /tn ""{#NAME}"""; Flags: runhidden nowait

[UninstallDelete]
Type: dirifempty; Name: "{app}\rules"
Type: dirifempty; Name: "{app}\res"
Type: dirifempty; Name: "{app}\po"
Type: dirifempty; Name: "{app}"

[Code]
#include "installer.pas"

