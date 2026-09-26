# Apex Config Tool

Apex Legends için çalışan bir yapılandırma aracı. Ekran, grafik, gölge, performans önayarları,
FOV/FPS/mouse gibi autoexec ayarlarını arayüzden düzenler; her değişiklikten önce otomatik
yedek alır ve uygulamadan önce bir önizleme ekranı gösterir.

Arayüz **Türkçe, İngilizce, Çince ve Rusça**dır; sol alt köşedeki **TR / EN / 中文 / RU** düğmeleriyle anında değiştirilebilir.

**Geliştirici:** emirhaneren34@gmail.com

## Kullanım (en hızlı yol)

Kaynak koduyla uğraşmadan doğrudan çalıştırmak isterseniz, `Program\ApexConfigTool.exe`
dosyasına çift tıklamanız yeterli — .NET 9 masaüstü çalışma zamanı (runtime) kurulu olmalı
(SDK değil, sadece runtime yeter): https://dotnet.microsoft.com/download/dotnet/9.0

`Program` klasörü kaynak kontrolüne dahil değildir (`.gitignore`); her derlemede
aşağıdaki komutla yeniden üretilir.

## Gereksinimler (kaynaktan çalıştırmak için)

- Windows
- .NET 9 SDK (bu makinede kurulu: `winget install Microsoft.DotNet.SDK.9`)

## Çalıştırma

```
dotnet run
```

veya tek klasörde toplanmış çalıştırılabilir bir sürüm üretmek için:

```
dotnet publish -c Release -r win-x64 --self-contained false -o Program
```

Bu komut `Program\ApexConfigTool.exe` dosyasını (ve yanındaki birkaç `.dll` dosyasını) üretir.

## Neler yapıyor?

- **Otomatik tespit**: Steam kütüphanelerini (`libraryfolders.vdf`) ve EA App / Origin kurulumlarını
  (kayıt defteri + yaygın klasörler) tarayarak Apex'i bulur. Kullanıcı config'i her zaman
  `%UserProfile%\Saved Games\Respawn\Apex\local\videoconfig.txt` içinde aranır.
- **Ekran / Grafik / Gölgeler**: `videoconfig.txt` içindeki `setting.*` anahtarlarını düzenler.
  Bir ayar dosyada zaten yoksa dokunulmaz (sürümler arası farkları güvenle atlar).
- **Performans önayarları**: Rekabetçi / Maksimum FPS / Dengeli / Kalite; grafik ve gölge
  sayfalarını tek tıkla ayarlar.
- **Autoexec**: FOV, FPS limiti (oyun içi + lobi), ham fare girişi, mouse hızlanması, film
  grenliliği gibi ayarları `cfg\autoexec.cfg` içine, yalnızca kendi işaretli bloğu
  (`// ===== ApexConfigTool BASLANGIC/BITIS =====`) üzerinden yazar; elle eklenen diğer satırlara
  dokunmaz.
- **Profiller**: Tüm yapılandırmayı adlandırılmış bir JSON dosyası olarak kaydedip geri yükler
  (`%LocalAppData%\ApexConfigTool\Profiles`).
- **Yedekler**: Her yazmadan önce otomatik yedek alınır; Yedekler sayfasından geri yüklenebilir
  veya silinebilir.
- **Gelişmiş**: `videoconfig.txt` içindeki tüm anahtarları arama/filtreleme ile inceleme ve ham
  metin düzenleyici.

Apex çalışırken dosyalara yazma işlemi engellenir; önce oyunun kapatılması istenir.

## Proje yapısı

```
Models/     Veri sınıfları (kurulum bilgisi, yedek girişi, profil, vb.)
Services/   İş mantığı (tespit, dosya okuma/yazma, diff, lokalizasyon)
Views/      Her sayfa için UserControl (XAML + code-behind)
```
