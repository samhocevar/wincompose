#define NAME "WinCompose"
#define VERSION "0.6.8"

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

