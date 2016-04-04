#define NAME "WinCompose"
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
Compression = lzma2
SolidCompression = yes
OutputDir = .
ShowLanguageDialog = auto
; We only install stuff in {userstartup} and in {app}. Since the latter
; can be overridden, we do not necessarily need elevated privileges, so
; let the user decide.
PrivilegesRequired = lowest

[Files]
Source: "bin\{#CONFIG}\{#NAME}.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#CONFIG}\am\*.dll"; DestDir: "{app}\am"; Flags: ignoreversion
Source: "bin\{#CONFIG}\be\*.dll"; DestDir: "{app}\be"; Flags: ignoreversion
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
Source: "bin\{#CONFIG}\id\*.dll"; DestDir: "{app}\id"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it\*.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "bin\{#CONFIG}\it-CH\*.dll"; DestDir: "{app}\it-CH"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ja\*.dll"; DestDir: "{app}\ja"; Flags: ignoreversion
Source: "bin\{#CONFIG}\nl\*.dll"; DestDir: "{app}\nl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\pl\*.dll"; DestDir: "{app}\pl"; Flags: ignoreversion
Source: "bin\{#CONFIG}\ru\*.dll"; DestDir: "{app}\ru"; Flags: ignoreversion
Source: "bin\{#CONFIG}\rw\*.dll"; DestDir: "{app}\rw"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sk\*.dll"; DestDir: "{app}\sk"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sr\*.dll"; DestDir: "{app}\sr"; Flags: ignoreversion
Source: "bin\{#CONFIG}\sv\*.dll"; DestDir: "{app}\sv"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHS\*.dll"; DestDir: "{app}\zh-CHS"; Flags: ignoreversion
Source: "bin\{#CONFIG}\zh-CHT\*.dll"; DestDir: "{app}\zh-CHT"; Flags: ignoreversion
Source: "rules\Xorg.txt"; DestDir: "{app}\res"
Source: "rules\Xcompose.txt"; DestDir: "{app}\res"
Source: "rules\Emoji.txt"; DestDir: "{app}\res"
Source: "rules\WinCompose.txt"; DestDir: "{app}\res"

[Languages]
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
; Name: "hu"; MessagesFile: "compiler:Languages/Hungarian.isl"
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
; Name: "sl"; MessagesFile: "compiler:Languages/Slovenian.isl"
Name: "es"; MessagesFile: "compiler:Languages/Spanish.isl"
; Name: "tr"; MessagesFile: "compiler:Languages/Turkish.isl"
; Name: "uk"; MessagesFile: "compiler:Languages/Ukrainian.isl"

[Icons]
Name: "{userstartup}\{#NAME}"; Filename: "{app}\{#NAME}.exe"; WorkingDir: "{app}"
; FIXME: IconIndex: 1 should work, but we don’t have a way (yet?) to put several icons in our .exe
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

[Code]
//
// Some Win32 API hooks
//
procedure exit_process(uExitCode: uint);
    external 'ExitProcess@kernel32.dll stdcall';

function reexec(hwnd: hwnd; lpOperation: string; lpFile: string;
                lpParameters: string; lpDirectory: string;
                nShowCmd: integer): thandle;
    external 'ShellExecuteW@shell32.dll stdcall';

//
// Installer state
// s_run_1: first run
// s_run_2: second run, with elevated privileges
// s_skipped: second run, with all pages automatically skipped
//
var
    state: (s_run_1, s_run_2, s_skipped);

//
// Check for outdated .NET versions and exit the installer with a
// message if it is not the expected version.
//
procedure check_dot_net;
var
    names: tarrayofstring;
    path, version: string;
    ret: integer;
begin
    path := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\';
    if not(reggetsubkeynames(HKLM, path, names)) or (getarraylength(names) = 0) then
    begin
        msgbox('WinCompose requires the .NET Framework, which does not seem to be installed.', mbInformation, MB_OK);
        shellexec('open', 'https://www.microsoft.com/en-us/download/details.aspx?id=21', '', '', sw_show, ewnowait, ret);
        exit_process(0);
    end;
    if regquerystringvalue(HKLM, path + '\v3.5', 'Version', version) and (pos('3.5.21022.08', version) = 1) then
    begin
        msgbox('WinCompose requires the .NET Framework 3.5 Service Pack 1, which does not seem to be installed.', mbInformation, MB_OK);
        shellexec('open', 'https://www.microsoft.com/en-us/download/details.aspx?id=22', '', '', sw_show, ewnowait, ret);
        exit_process(0);
    end;
end;

//
// Visit the project homepage
//
procedure visit_homepage(sender: tobject);
var
    ret: integer;
begin
    shellexec('open', 'http://wincompose.info/', '', '', sw_show, ewnowait, ret);
end;

//
// Initialize installer state.
//
procedure InitializeWizard;
var
    i: integer;
    homepage: tnewstatictext;
begin
    state := s_run_1;
    for i := 1 to paramcount do
        if comparetext(paramstr(i), '/elevate') = 0 then state := s_run_2;

    homepage := tnewstatictext.create(wizardform);
    homepage.caption := 'http://wincompose.info/';
    homepage.cursor := crhand;
    homepage.onclick := @visit_homepage;
    homepage.parent := wizardform;
    homepage.font.style := homepage.font.style + [fsunderline];
    homepage.font.color := clblue;
    homepage.top := wizardform.cancelbutton.top
                  + wizardform.cancelbutton.height
                  - homepage.height - 2;
    homepage.left := scalex(20);

    check_dot_net();
end;

//
// If we're in the target directory selection page, check that we
// can actually install files in that directory. Otherwise, we hijack
// NextButtonClick() to re-execute ourselves in admin mode.
//
function NextButtonClick(page_id: integer): boolean;
var
    e1: boolean;
    e2: thandle;
begin
    result := true;
    if (state = s_run_1) and (page_id = wpselectdir) then
    begin
        createdir(expandconstant('{app}'));
        e1 := savestringtofile(expandconstant('{app}/.stamp'), '', false);
        deletefile(expandconstant('{app}/.stamp'));
        deltree(expandconstant('{app}'), true, false, false);
        if e1 then exit;

        e2 := reexec(wizardform.handle, 'runas', expandconstant('{srcexe}'),
                     expandconstant('/dir="{app}" /elevate'), '', sw_show);
        if e2 > 32 then exit_process(0);

        result := false;
        msgbox(format('Administrator rights are required. Code: %d', [e2]),
               mberror, MB_OK);
    end;
end;

//
// Broadcast the WM_WINCOMPOSE_EXIT message for all WinCompose instances
// to shutdown themselves.
//
function PrepareToInstall(var needsrestart: boolean): string;
var
    dummy: integer;
begin
    postbroadcastmessage(registerwindowmessage('WM_WINCOMPOSE_EXIT'), 0, 0);
    sleep(1000);
    exec('>', 'cmd.exe /c taskkill /f /im {#NAME}.exe', '',
         SW_HIDE, ewwaituntilterminated, dummy);
end;

//
// If running elevated and we haven't reached the directory selection page,
// skip all pages, including the directory selection page.
//
function ShouldSkipPage(page_id: integer): boolean;
begin
    result := (state = s_run_2) and (page_id <= wpselectdir);
    if (state = s_run_2) and (page_id = wpselectdir) then
        state := s_skipped;
end;

