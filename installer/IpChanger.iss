; Inno Setup szkript az IP Changer telepítőjéhez.
; Fordítás: Inno Setup 6 (ISCC.exe). A telepítő a publish-mappa tartalmát
; csomagolja, amelyet előbb a `dotnet publish` hoz létre (lásd docs/PACKAGING.md).
;
; --- Ha NEM a repó installer/ mappájából fordítasz, itt állítsd be az útvonalakat ---
;  PublishDir : a `dotnet publish` kimeneti mappája (ebből lesz a telepítő tartalma)
;  IconFile   : a telepítő exe ikonja (elhagyható)
; Ezek felülírhatók a parancssorból is, pl.:
;  ISCC.exe /DPublishDir="C:\...\IPChanger" /DMyAppVersion=1.2.3 IpChanger.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.1.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish"
#endif
#ifndef IconFile
  #define IconFile "..\src\IpChanger\Assets\app.ico"
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
; Csak akkor állítjuk be a telepítő ikonját, ha a fájl valóban létezik –
; így hiányzó ikon esetén a fordítás nem szakad meg (a beépített ikont használja).
#if FileExists(IconFile)
SetupIconFile={#IconFile}
#endif
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
; A teljes publish-mappa tartalma. Self-contained build esetén nincs külön .NET
; runtime igény a cél-gépen; framework-dependent build esetén kell a .NET 8 Desktop Runtime.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
