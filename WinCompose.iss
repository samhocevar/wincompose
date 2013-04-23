#define NAME "WinCompose"
#define VERSION "0.1"

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
Source: "{#NAME}.exe"; DestDir: "{app}"
Source: "res\Compose.txt"; DestDir: "{app}\res"
Source: "res\Keys.txt"; DestDir: "{app}\res"
Source: "res\wc.ico"; DestDir: "{app}\res"
Source: "res\wca.ico"; DestDir: "{app}\res"
Source: "res\wcd.ico"; DestDir: "{app}\res"

[Icons]
Name: "{commonstartup}\{#NAME}"; Filename: "{app}\{#NAME}.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; IconFilename: "{app}\res\wcd.ico"
Name: "{group}\{#NAME}"; Filename: "{app}\{#NAME}.exe"

[Run]
Filename: "{app}\{#NAME}.exe"; Flags: nowait

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#NAME}.exe"; Flags: runhidden

[UninstallDelete]
Type: dirifempty; Name: "{app}"

