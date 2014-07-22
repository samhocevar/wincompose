#define NAME "WinCompose"
#define VERSION "0.6.11"

[Setup]
AppName = {#NAME}
AppVersion = {#VERSION}
OutputBaseFilename = "{#NAME}-Setup-{#VERSION}"
DefaultDirName = {pf}\{#NAME}
DefaultGroupName = {#NAME}
SetupIconFile = "res\icon_normal.ico"
Compression = lzma2
SolidCompression = yes
OutputDir = .
ShowLanguageDialog = auto

[Files]
Source: "obj\{#NAME}.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "res\resources.dll"; DestDir: "{app}\res"
Source: "res\Keys.txt"; DestDir: "{app}\res"
Source: "res\WinCompose.txt"; DestDir: "{app}\res"
Source: "res\Xorg.txt"; DestDir: "{app}\res"
Source: "res\Xcompose.txt"; DestDir: "{app}\res"
; This should match constants.ahk
Source: "po\cs.po"; DestDir: "{app}\po"
Source: "po\de.po"; DestDir: "{app}\po"
Source: "po\el.po"; DestDir: "{app}\po"
Source: "po\fr.po"; DestDir: "{app}\po"
Source: "po\id.po"; DestDir: "{app}\po"
Source: "po\nl.po"; DestDir: "{app}\po"
Source: "po\sv.po"; DestDir: "{app}\po"

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "cs"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "el"; MessagesFile: "compiler:Languages\Greek.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
; Name: "id" ; Unavailable
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl"
; Name: "sv" ; Unavailable

[Icons]
Name: "{userstartup}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; IconFilename: "{app}\res\resources.dll"; IconIndex: 2
Name: "{group}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#NAME}.exe"; Flags: nowait

[InstallDelete]
; Legacy stuff that we need to remove
Type: files; Name: "{app}\res\wc.ico"
Type: files; Name: "{app}\res\wca.ico"
Type: files; Name: "{app}\res\wcd.ico"
Type: files; Name: "{app}\res\Compose.txt"
Type: files; Name: "{app}\locale\default.ini"
Type: files; Name: "{app}\locale\fr.ini"
Type: dirifempty; Name: "{app}\locale"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#NAME}.exe"; Flags: runhidden

[UninstallDelete]
Type: dirifempty; Name: "{app}\res"
Type: dirifempty; Name: "{app}\po"
Type: dirifempty; Name: "{app}"

