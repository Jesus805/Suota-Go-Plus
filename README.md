# SUOTA Go+

YOU ACKNOWLEDGE THERE MAY BE RISKS USING THIS SOFTWARE, I AM NOT LIABLE FOR ANY BRICKED PGP.

Suota Go+ is an Android Client and DA14580 image that can extract the Device/Blob Key from any Pokemon Go Plus over-the-air. (Pokeball Plus is not supported!). The client performs the over-the-air installation process, extracts the device/blob key, and saves the keys as a *\*.json* file. So far I have successfully extracted keys from 3 different PGP (2 legitimate, 1 clone) on a Samsung Galaxy S8+ and a One Plus 5. Unfortunately iOS is not supported, if anyone is interested in implementing it please submit a pull request or create your own client and I will link it on this project. 

## Building the Client from source
Run this command
```
git clone https://github.com/Jesus805/pgp_suota
```
Open `pgp_suota\Client\suota_pgp\suota_pgp.sln` with Visual Studio

Ensure NuGet packages are installed by right clicking the `suota_pgp.Android` project and then selecting "Manage NuGet packages..."

Build The project

## Building the Firmware from source
I do not recommend this option unless you have a DA14580 development board to test it on. 

Instructions are listed in `\Firmware\README.md`

## Installation
1. Install the `Suota Go+.apk` on your Android Device.
2. Run `Suota Go+`
3. `Suota Go+` will generate a `SuotaPgp` folder, place `patch.img` in that folder.

## Running
1. Connect your Go+ to the Pokemon Go App.
2. Once connected, open `Suota Go+`.
3. Under the "Patch Device" tab, click the Refresh button.
4. Select the paired Go+.
5. Select `patch.img` under "Firmware File".
6. Once you have selected your device and the patch. Select "Start Patch".
7. The device will patch and will let you know when it has completed.
8. Wait about 60 seconds then go to the 'Key Extractor" tab
9. Select "Scan" and select the "PGP Key Extractor" device.
10. Select "Get Device Info" to read the device/blob key.
11. Select "Save" to save it as a .json file in the `SuotaPgp` folder.
12. Restore the device to it's original state.

## Tips

To maximize your success, please ensure that the Go+ is next to your mobile device. It is also recommended to use a new battery before beginning the process.

For my blog post about this project. Including all the technical information that I learned, please visit my website
<Domain>
