; Inno Setup szkript az IP Changer telepítőjéhez.
; Fordítás: Inno Setup 6 (ISCC.exe). A telepítő a "publish" mappa tartalmát
; csomagolja, amelyet előbb a `dotnet publish` hoz létre (lásd docs/PACKAGING.md).
;
; A verzió felülírható a parancssorból:  ISCC.exe /DMyAppVersion=1.2.3 IpChanger.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "IP Changer"
#define MyAppPublisher "BDominik2001"
#define MyAppExeName "IpChanger.exe"
#define MyAppUrl "https://github.com/BDominik2001/IPCHANGER"

[Setup]
; Egyedi, állandó azonosító – frissítéskor NE változtasd meg.
AppId={{7F3A2C10-9B4E-4D2A-8E1F-2C5B7A9D0E31}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\dist
OutputBaseFilename=IpChanger-Setup-{#MyAppVersion}
SetupIconFile=..\src\IpChanger\Assets\app.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Az alkalmazás rendszergazdai jogot igényel; a telepítő is emelt joggal fut,
; és a Program Files mappába települ.
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; A teljes publish-mappa tartalma (self-contained build – nincs külön .NET runtime igény).
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
