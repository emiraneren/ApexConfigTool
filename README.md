# Apex Config Tool

Apex Legends ayarlarını (grafik, gölge, FOV, FPS limiti, hassasiyet vb.) arayüzden düzenleyen araç. Her değişiklikten önce otomatik yedek alır. Arayüz: Türkçe, İngilizce, Çince, Rusça.

## Kullanım

1. `ApexConfigTool.exe` dosyasını indir ve çalıştır.
2. Apex'i kapat, ayarları seç, uygula.

.NET 10 Desktop Runtime kurulu olmalı: https://dotnet.microsoft.com/download/dotnet/10.0

## Neler yapıyor?

- Grafik ve gölge ayarları, hazır performans önayarları
- FOV, FPS limiti, hassasiyet, düşük gecikme (autoexec)
- Profiller ve otomatik yedekler

Yalnızca oyunun kendi ayar dosyalarını düzenler; oyuna müdahale etmez.

## Kaynaktan derleme

```
cd src
dotnet run
```
