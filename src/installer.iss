#define NAME "WinCompose"
#define VERSION "0.7.0"

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
Compression = lzma2
SolidCompression = yes
OutputDir = .
ShowLanguageDialog = auto

[Files]
Source: "bin\Release\{#NAME}.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\am\*.dll"; DestDir: "{app}\am"; Flags: ignoreversion
Source: "bin\Release\be\*.dll"; DestDir: "{app}\be"; Flags: ignoreversion
Source: "bin\Release\cs\*.dll"; DestDir: "{app}\cs"; Flags: ignoreversion
Source: "bin\Release\da\*.dll"; DestDir: "{app}\da"; Flags: ignoreversion
Source: "bin\Release\de\*.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "bin\Release\el\*.dll"; DestDir: "{app}\el"; Flags: ignoreversion
Source: "bin\Release\es\*.dll"; DestDir: "{app}\es"; Flags: ignoreversion
Source: "bin\Release\et\*.dll"; DestDir: "{app}\et"; Flags: ignoreversion
Source: "bin\Release\fi\*.dll"; DestDir: "{app}\fi"; Flags: ignoreversion
Source: "bin\Release\fr\*.dll"; DestDir: "{app}\fr"; Flags: ignoreversion
Source: "bin\Release\ga\*.dll"; DestDir: "{app}\ga"; Flags: ignoreversion
Source: "bin\Release\id\*.dll"; DestDir: "{app}\id"; Flags: ignoreversion
Source: "bin\Release\it\*.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "bin\Release\nl\*.dll"; DestDir: "{app}\nl"; Flags: ignoreversion
Source: "bin\Release\pl\*.dll"; DestDir: "{app}\pl"; Flags: ignoreversion
Source: "bin\Release\ru\*.dll"; DestDir: "{app}\ru"; Flags: ignoreversion
Source: "bin\Release\rw\*.dll"; DestDir: "{app}\rw"; Flags: ignoreversion
Source: "bin\Release\sk\*.dll"; DestDir: "{app}\sk"; Flags: ignoreversion
Source: "bin\Release\sr\*.dll"; DestDir: "{app}\sr"; Flags: ignoreversion
Source: "bin\Release\sv\*.dll"; DestDir: "{app}\sv"; Flags: ignoreversion
Source: "bin\Release\zh-CHS\*.dll"; DestDir: "{app}\zh-CHS"; Flags: ignoreversion
Source: "bin\Release\zh-CHT\*.dll"; DestDir: "{app}\zh-CHT"; Flags: ignoreversion
Source: "rules\Xorg.txt"; DestDir: "{app}\res"
Source: "rules\Xcompose.txt"; DestDir: "{app}\res"
Source: "rules\Emoji.txt"; DestDir: "{app}\res"
Source: "rules\WinCompose.txt"; DestDir: "{app}\res"

[Languages]
; Name: "pt-BR"; MessagesFile: "compiler:Languages/BrazilianPortuguese.isl"
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
; Name: "hu"; MessagesFile: "compiler:Languages/Hungarian.isl"
Name: "it"; MessagesFile: "compiler:Languages/Italian.isl"
; Name: "ja"; MessagesFile: "compiler:Languages/Japanese.isl"
; Name: "??"; MessagesFile: "compiler:Languages/Nepali.isl"
; Name: "no"; MessagesFile: "compiler:Languages/Norwegian.isl"
Name: "pl"; MessagesFile: "compiler:Languages/Polish.isl"
; Name: "pt"; MessagesFile: "compiler:Languages/Portuguese.isl"
Name: "ru"; MessagesFile: "compiler:Languages/Russian.isl"
; Name: "??"; MessagesFile: "compiler:Languages/ScottishGaelic.isl"
; Name: "Cy-sr-SP"; MessagesFile: "compiler:Languages/SerbianCyrillic.isl"
; Name: "Lt-sr-SP"; MessagesFile: "compiler:Languages/SerbianLatin.isl"
; Name: "sl"; MessagesFile: "compiler:Languages/Slovenian.isl"
Name: "es"; MessagesFile: "compiler:Languages/Spanish.isl"
; Name: "tr"; MessagesFile: "compiler:Languages/Turkish.isl"
; Name: "uk"; MessagesFile: "compiler:Languages/Ukrainian.isl"

[Icons]
Name: "{userstartup}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#NAME}.exe"; IconIndex: 1
Name: "{group}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#NAME}.exe"; Flags: nowait

[InstallDelete]
; Legacy stuff that we need to remove
Type: files; Name: "{app}\res\resources.dll"
Type: files; Name: "{app}\res\wc.ico"
Type: files; Name: "{app}\res\wca.ico"
Type: files; Name: "{app}\res\wcd.ico"
Type: files; Name: "{app}\res\Compose.txt"
Type: files; Name: "{app}\res\Keys.txt"
Type: files; Name: "{app}\locale\default.ini"
Type: files; Name: "{app}\locale\fr.ini"
Type: dirifempty; Name: "{app}\locale"
Type: files; Name: "{app}\po\*.po"
Type: dirifempty; Name: "{app}\po"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /f /im {#NAME}.exe"; Flags: runhidden

[UninstallDelete]
Type: dirifempty; Name: "{app}\rules"
Type: dirifempty; Name: "{app}\res"
Type: dirifempty; Name: "{app}\po"
Type: dirifempty; Name: "{app}"

