#define NAME "WinCompose"
#define EXE "wincompose.exe"
#define VERSION GetEnv('VERSION')
#define CONFIG GetEnv('CONFIG')

[Setup]
AppName = {#NAME}
AppVersion = {#VERSION}
AppPublisher = Sam Hocevar
AppPublisherURL = http://sam.hocevar.net/
OutputBaseFilename = "{#NAME}-Setup-{#VERSION}"
ArchitecturesInstallIn64BitMode = x64
DefaultDirName = {pf}\{#NAME}
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
Source: "bin\{#CONFIG}\trampoline.dll"; DestDir: "{tmp}"; Flags: dontcopy

Source: "bin\{#CONFIG}\{#EXE}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Emoji.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\Typography.OpenFont.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\am\*.dll"; DestDir: "{app}\am"; Flags: ignoreversion
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
Source: "bin\{#CONFIG}\hu\*.dll"; DestDir: "{app}\hu"; Flags: ignoreversion
Source: "bin\{#CONFIG}\id\*.dll"; DestDir: "{app}\id"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it\*.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it-CH\*.dll"; DestDir: "{app}\it-CH"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ja\*.dll"; DestDir: "{app}\ja"; Flags: ignoreversion
Source: "bin\{#CONFIG}\nl\*.dll"; DestDir: "{app}\nl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pl\*.dll"; DestDir: "{app}\pl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pt-BR\*.dll"; DestDir: "{app}\pt-BR"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ro\*.dll"; DestDir: "{app}\ro"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ru\*.dll"; DestDir: "{app}\ru"; Flags: ignoreversion
Source: "bin\{#CONFIG}\rw\*.dll"; DestDir: "{app}\rw"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sk\*.dll"; DestDir: "{app}\sk"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sl\*.dll"; DestDir: "{app}\sl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sq\*.dll"; DestDir: "{app}\sq"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sr\*.dll"; DestDir: "{app}\sr"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sv\*.dll"; DestDir: "{app}\sv"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHS\*.dll"; DestDir: "{app}\zh-CHS"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHT\*.dll"; DestDir: "{app}\zh-CHT"; Flags: ignoreversion
Source: "rules\Xorg.txt"; DestDir: "{app}\res"
Source: "rules\Xcompose.txt"; DestDir: "{app}\res"
Source: "rules\DefaultUserSequences.txt"; DestDir: "{app}\res"
Source: "rules\Emoji.txt"; DestDir: "{app}\res"
Source: "rules\WinCompose.txt"; DestDir: "{app}\res"

[Languages]
; Name: "hy"; MessagesFile: "compiler:Languages/Armenian.islu"
Name: "pt_BR"; MessagesFile: "compiler:Languages/BrazilianPortuguese.isl"
; Name: "ca"; MessagesFile: "compiler:Languages/Catalan.isl"
; Name: "??"; MessagesFile: "compiler:Languages/Corsican.isl"
Name: "cs"; MessagesFile: "compiler:Languages/Czech.isl"
Name: "da"; MessagesFile: "compiler:Languages/Danish.isl"
Name: "nl"; MessagesFile: "compiler:Languages/Dutch.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "fi"; MessagesFile: "compiler:Languages/Finnish.isl"
Name: "fr"; MessagesFile: "compiler:Languages/French.isl"
Name: "de"; MessagesFile: "compiler:Languages/German.isl"
Name: "el"; MessagesFile: "compiler:Languages/Greek.isl"
; Name: "he"; MessagesFile: "compiler:Languages/Hebrew.isl"
Name: "hu"; MessagesFile: "compiler:Languages/Hungarian.isl"
Name: "it"; MessagesFile: "compiler:Languages/Italian.isl"
Name: "ja"; MessagesFile: "compiler:Languages/Japanese.isl"
; Name: "??"; MessagesFile: "compiler:Languages/Nepali.islu"
; Name: "no"; MessagesFile: "compiler:Languages/Norwegian.isl"
Name: "pl"; MessagesFile: "compiler:Languages/Polish.isl"
; Name: "pt"; MessagesFile: "compiler:Languages/Portuguese.isl"
Name: "ru"; MessagesFile: "compiler:Languages/Russian.isl"
; Name: "??"; MessagesFile: "compiler:Languages/ScottishGaelic.isl"
; Name: "Cy-sr-SP"; MessagesFile: "compiler:Languages/SerbianCyrillic.isl"
; Name: "Lt-sr-SP"; MessagesFile: "compiler:Languages/SerbianLatin.isl"
Name: "sl"; MessagesFile: "compiler:Languages/Slovenian.isl"
Name: "es"; MessagesFile: "compiler:Languages/Spanish.isl"
; Name: "tr"; MessagesFile: "compiler:Languages/Turkish.isl"
; Name: "uk"; MessagesFile: "compiler:Languages/Ukrainian.isl"

[Icons]
Name: "{userstartup}\{#NAME}"; Filename: "{app}\{#EXE}"; WorkingDir: "{app}"
; FIXME: IconIndex: 1 should work, but we don’t have a way (yet?) to put several icons in our .exe
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#EXE}"; IconIndex: 1
Name: "{group}\{#NAME}"; Filename: "{app}\{#EXE}"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#EXE}"; Flags: nowait

[InstallDelete]
; We used to be installed in c:\Program Files (x86)
Type: filesandordirs; Name: "{pf32}\{#NAME}"
; Legacy stuff that we need to remove
Type: files; Name: "{app}\res\resources.dll"
Type: files; Name: "{app}\res\wc.ico"
Type: files; Name: "{app}\res\wca.ico"
Type: files; Name: "{app}\res\wcd.ico"
Type: files; Name: "{app}\res\Compose.txt"
Type: files; Name: "{app}\res\Keys.txt"
Type: dirifempty; Name: "{app}\res"
Type: files; Name: "{app}\locale\default.ini"
Type: files; Name: "{app}\locale\fr.ini"
Type: dirifempty; Name: "{app}\locale"
Type: files; Name: "{app}\po\*.po"
Type: dirifempty; Name: "{app}\po"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#EXE}"; Flags: runhidden

[UninstallDelete]
Type: dirifempty; Name: "{app}\rules"
Type: dirifempty; Name: "{app}\res"
Type: dirifempty; Name: "{app}\po"
Type: dirifempty; Name: "{app}"

[Code]
#include "installer.pas"

