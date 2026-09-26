# Apex Config Tool

Apex Legends için çalışan bir yapılandırma aracı. Ekran, grafik, gölge ve performans önayarlarını; FOV, FPS limiti, hassasiyet, düşük gecikme gibi autoexec ayarlarını arayüzden düzenler. Her değişiklikten önce otomatik yedek alır ve uygulamadan önce bir önizleme ekranı gösterir.

Arayüz **Türkçe, İngilizce, Çince ve Rusça**dır; sol alt köşedeki **TR / EN / 中文 / RU** düğmeleriyle anında değiştirilebilir.

**Geliştirici:** emirhaneren34@gmail.com

## Kullanım (en hızlı yol)

1. Bu sayfadaki **`ApexConfigTool.exe`** dosyasını indirin.
2. Çift tıklayıp çalıştırın. Kurulum gerekmez, yanında başka dosya istemez.

> Bilgisayarınızda **.NET 10 Desktop Runtime** kurulu olmalı (SDK değil, sadece runtime yeter):
> https://dotnet.microsoft.com/download/dotnet/10.0
>
> Yoksa uygulama "runtime bulunamadı" hatası verir; yukarıdaki linkten "Desktop Runtime" (x64) indirip kurmanız yeterli.

Apex'i Steam veya EA App'ten kurduysanız oyun klasörünü ve config dosyalarını kendisi bulur. Apex açıkken dosyalara yazma işlemi engellenir; önce oyunu kapatmanız istenir.

## Neler yapıyor?

- **Ekran / Grafik / Gölgeler**: `videoconfig.txt` içindeki ayarları düzenler. Dosyada olmayan bir ayara dokunmaz.
- **Performans önayarları**: Rekabetçi / Maksimum FPS / Dengeli / Kalite; tek tıkla grafik ve gölge ayarlarını yükler.
- **Autoexec**: FOV, FPS limiti, ham fare girişi, hassasiyet (genel ve nişangaha özel), düşük gecikme, görsel sadelik gibi ayarları `cfg\autoexec.cfg` içine, yalnızca kendi işaretli bloğu üzerinden yazar; elle eklediğiniz diğer satırlara dokunmaz.
- **Profiller**: Tüm yapılandırmayı adlandırılmış bir profil olarak kaydedip geri yükler.
- **Yedekler**: Her yazmadan önce otomatik yedek alınır; Yedekler sayfasından geri yüklenebilir veya silinebilir.
- **Gelişmiş**: `videoconfig.txt` içindeki tüm anahtarları arama ile inceleme ve ham metin düzenleyici.

Yalnızca oyunun kendi client ayarlarını düzenler; oyuna müdahale eden, makro veya hile işlevi yoktur.

## Kaynak koddan derleme

Kaynak kod `src` klasöründedir. .NET 10 SDK gerekir.

```
cd src
dotnet run
```

Tek dosyalık `.exe` üretmek için:

```
cd src
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ..\out
```

## Klasör yapısı

```
ApexConfigTool.exe   Çalıştırılabilir uygulama
README.md            Bu dosya
src/                 Kaynak kod
  Models/            Veri sınıfları
  Services/          İş mantığı (tespit, dosya okuma/yazma, diff, çeviri)
  Views/             Her sayfa için UserControl (XAML + code-behind)
```
