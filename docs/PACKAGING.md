# Telepítő készítése (Windows)

Ez az útmutató leírja, hogyan készíthető az IP Changerből terjeszthető
**telepítő (`setup.exe`)**, amelyet más Windows gépekre is fel lehet rakni.

## Áttekintés

A folyamat két lépés:

1. **Publish** – a .NET a forráskódból önálló (self-contained) alkalmazást épít,
   amely a .NET futtatókörnyezetet is tartalmazza, így a cél-gépre **nem kell
   előre telepíteni a .NET-et**.
2. **Telepítő fordítása** – az [Inno Setup](https://jrsoftware.org/isdl.php) a
   publish mappából egy `setup.exe`-t készít (Start menü parancsikon, uninstaller,
   megjelenés a Programok listájában).

## Előfeltételek (a fejlesztő/build gépen)

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Inno Setup 6](https://jrsoftware.org/isdl.php)
- Windows (a WPF fordítása Windowst igényel)

## Leggyorsabb út: egyetlen szkript

A repó gyökeréből:

```powershell
cd installer
./build-installer.ps1 -Version 1.0.0
```

Ez elvégzi a publish-t és lefordítja a telepítőt. Az eredmény:

```
dist\IpChanger-Setup-1.0.0.exe
```

Ezt az egy fájlt kell átvinni a többi gépre és lefuttatni.

## Kézi lépések (ha nem a szkriptet használod)

### 1) Publish (self-contained, 64 bites)

```powershell
dotnet publish src/IpChanger/IpChanger.csproj -c Release -r win-x64 `
  --self-contained true -p:Version=1.0.0 -o publish
```

> **Ne** kapcsold be a trimmelést (`-p:PublishTrimmed=true`) – a WPF a XAML és a
> reflection miatt eltörhet tőle.

### 2) Telepítő fordítása

```powershell
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" /DMyAppVersion=1.0.0 installer\IpChanger.iss
```

A kész telepítő a `dist\` mappába kerül.

> **Más mappaszerkezet?** A `.iss` alapból a repó elrendezését feltételezi
> (`installer/` a `.iss`-nek, `..\publish` a build, `..\src\...\app.ico` az ikon).
> Ha máshonnan fordítasz (pl. az Inno Setup Compiler grafikus felületén, a publish
> egy tetszőleges mappában), az útvonalak felülírhatók a szkript tetején lévő
> `#define`-okkal, vagy a parancssorból:
>
> ```powershell
> ISCC.exe /DPublishDir="C:\Users\Én\Desktop\IPChanger" ^
>          /DIconFile="C:\Users\Én\Desktop\app.ico" ^
>          /DMyAppVersion=1.0.0 IpChanger.iss
> ```
>
> Ha az ikonfájl nincs meg, a fordítás **nem szakad meg** – a telepítő a beépített
> ikont használja (az alkalmazás saját ikonja az `IpChanger.exe`-ben marad).

## Amit a telepítő csinál

- A `C:\Program Files\IP Changer` mappába telepít (emelt jogot kér, mert az
  alkalmazás rendszergazdaként fut).
- Létrehoz egy **Start menü** parancsikont (és opcionálisan asztali ikont).
- Feltelepít egy **uninstaller**-t, és megjelenik a *Programok és szolgáltatások*
  listájában.
- A telepítő nyelve magyar vagy angol lehet.

## Egyéb kiadási formák

| Cél | Parancs / megoldás |
|-----|--------------------|
| **Egyetlen hordozható `.exe`** (nincs telepítés, csak másolás) | `dotnet publish ... -p:PublishSingleFile=true` – a `publish\IpChanger.exe` önmagában futtatható |
| **Kicsi csomag, de kell a .NET a gépen** | `--self-contained false` (a cél-gépre kell a [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| **ARM64 gépek** | `-r win-arm64` a publish-nál és a build szkriptnél |

## Kódaláírás és SmartScreen

Aláíratlan telepítőnél/alkalmazásnál a Windows **SmartScreen** első indításkor
figyelmeztethet („Ismeretlen kiadó”). Ez normális. Éles terjesztéshez érdemes a
`setup.exe`-t és a `IpChanger.exe`-t egy **kódaláíró tanúsítvánnyal** (Authenticode)
aláírni (`signtool`), ez megszünteti a figyelmeztetést.
