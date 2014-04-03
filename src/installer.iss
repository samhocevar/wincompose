#define NAME "WinCompose"
#define VERSION "0.5.0"

[Setup]
AppName = {#NAME}
AppVersion = {#VERSION}
OutputBaseFilename = "{#NAME}-Setup-{#VERSION}"
DefaultDirName = {pf}\{#NAME}
DefaultGroupName = {#NAME}
SetupIconFile = "res\wc.ico"
Compression = lzma2
SolidCompression = yes
OutputDir = .

[Files]
Source: "obj\{#NAME}.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "obj\resources.dll"; DestDir: "{app}\res"
Source: "res\Compose.txt"; DestDir: "{app}\res"
Source: "res\Keys.txt"; DestDir: "{app}\res"
Source: "locale\default.ini"; DestDir: "{app}\locale"
Source: "locale\fr.ini"; DestDir: "{app}\locale"

[Icons]
Name: "{userstartup}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; IconFilename: "{app}\res\resources.dll"; IconIndex: 2
Name: "{group}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#NAME}.exe"; Flags: nowait

[InstallDelete]
Type: files; Name: "{app}\res\wc.ico"
Type: files; Name: "{app}\res\wca.ico"
Type: files; Name: "{app}\res\wcd.ico"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#NAME}.exe"; Flags: runhidden

[UninstallDelete]
Type: dirifempty; Name: "{app}\res"
Type: dirifempty; Name: "{app}\locale"
Type: dirifempty; Name: "{app}"

