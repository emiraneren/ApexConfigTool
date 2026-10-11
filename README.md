# Apex Config Tool

Apex Legends ayarlarını (grafik, gölge, FOV, FPS limiti, hassasiyet vb.) arayüzden düzenleyen araç. Her değişiklikten önce otomatik yedek alır. Arayüz: Türkçe, İngilizce, Çince, Rusça.

## Kullanım

1. **[ApexConfigTool.exe'yi indir](https://github.com/emiraneren/ApexConfigTool/releases/latest/download/ApexConfigTool.exe)** ve çalıştır. (Diğer sürümler: [Releases](https://github.com/emiraneren/ApexConfigTool/releases))
2. Apex'i kapat, ayarları seç, uygula.

Kurulum gerekmez: .NET çalışma zamanı `.exe`'nin içine gömülüdür, bu yüzden dosya yaklaşık 60 MB'tır. Windows "tanınmayan uygulama" uyarısı verirse "Daha fazla bilgi → Yine de çalıştır" deyin (dosya imzasız olduğu için çıkar).

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
