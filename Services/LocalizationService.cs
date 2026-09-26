using System.Collections.Generic;
using System.ComponentModel;

namespace ApexConfigTool.Services;

/// <summary>
/// Uygulama genelinde Türkçe / İngilizce metin çevirisi sağlayan tekil (singleton) servis.
/// XAML tarafında <c>{Binding Source={x:Static loc:LocalizationService.Instance}, Path=[AnahtarAdi]}</c>
/// şeklinde bağlanır; dil değiştiğinde indeksleyici tüm bağlı kontrolleri otomatik günceller.
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private AppLanguage language = AppLanguage.Turkish;

    public AppLanguage Language
    {
        get => language;
        set
        {
            if (language == value)
                return;

            language = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTurkish)));
        }
    }

    public bool IsTurkish => language == AppLanguage.Turkish;

    public void Toggle()
    {
        Language = language == AppLanguage.Turkish ? AppLanguage.English : AppLanguage.Turkish;
    }

    /// <summary>Indeksleyici — XAML binding'lerinde kullanılır.</summary>
    public string this[string key] => T(key);

    /// <summary>Kod tarafında çeviri almak için kullanılır.</summary>
    public string T(string key)
    {
        if (Translations.Map.TryGetValue(key, out var pair))
            return language == AppLanguage.Turkish ? pair.Tr : pair.En;

        return key;
    }

    /// <summary>Format dizesi içeren çeviriler için (örn. "{0} yedeği geri yüklendi.").</summary>
    public string F(string key, params object[] args)
    {
        return string.Format(T(key), args);
    }
}

/// <summary>Tüm çeviri anahtarları ve karşılık gelen Türkçe/İngilizce metinler.</summary>
public static class Translations
{
    public static readonly Dictionary<string, (string Tr, string En)> Map = new()
    {
        // Genel
        ["AppTitle"] = ("Apex Config", "Apex Config"),
        ["VersionLabel"] = ("s1.0", "v1.0"),
        ["StatusReady"] = ("Hazır", "Ready"),
        ["ApexRunningPill"] = ("● Apex çalışıyor", "● Apex running"),
        ["ApexOffPill"] = ("● Apex kapalı", "● Apex off"),

        // Navigasyon
        ["NavDashboard"] = ("Panel", "Dashboard"),
        ["NavDisplay"] = ("Ekran", "Display"),
        ["NavGraphics"] = ("Grafik", "Graphics"),
        ["NavShadows"] = ("Gölgeler", "Shadows"),
        ["NavPerformance"] = ("Performans", "Performance"),
        ["NavAutoexec"] = ("Autoexec", "Autoexec"),
        ["NavProfiles"] = ("Profiller", "Profiles"),
        ["NavBackups"] = ("Yedekler", "Backups"),
        ["NavAdvanced"] = ("Gelişmiş", "Advanced"),

        // Alt bar butonları
        ["BtnBackup"] = ("Yedekle", "Backup"),
        ["BtnRestore"] = ("Geri yükle", "Restore"),
        ["BtnApply"] = ("Uygula", "Apply"),
        ["BtnApplyLaunch"] = ("Uygula ve başlat", "Apply & launch"),

        // Ortak değerler
        ["CommonKeepCurrent"] = ("Mevcudu koru", "Keep current"),
        ["CommonOn"] = ("Açık", "On"),
        ["CommonOff"] = ("Kapalı", "Off"),
        ["CommonEnabled"] = ("Etkin", "Enabled"),
        ["CommonDisabled"] = ("Devre dışı", "Disabled"),
        ["CommonDefault"] = ("Varsayılan", "Default"),
        ["CommonMinimal"] = ("Minimal", "Minimal"),
        ["CommonUnknown"] = ("Bilinmiyor", "Unknown"),
        ["CommonNotDetected"] = ("Tespit edilemedi", "Not detected"),

        // Panel (Dashboard)
        ["DashTitle"] = ("Panel", "Dashboard"),
        ["DashResolution"] = ("Çözünürlük", "Resolution"),
        ["DashFov"] = ("FOV", "FOV"),
        ["DashFpsLimit"] = ("FPS limiti", "FPS limit"),
        ["DashApex"] = ("Apex", "Apex"),
        ["DashLauncher"] = ("Başlatıcı", "Launcher"),
        ["DashProtection"] = ("Koruma", "Protection"),
        ["DashVideoConfig"] = ("Video config", "Video config"),
        ["DashRunning"] = ("Çalışıyor", "Running"),
        ["DashOff"] = ("Kapalı", "Off"),
        ["DashProtected"] = ("Korumalı", "Protected"),
        ["DashWritable"] = ("Yazılabilir", "Writable"),

        // Ekran (Display)
        ["DisplayTitle"] = ("Ekran", "Display"),
        ["DisplayResolution"] = ("Çözünürlük", "Resolution"),
        ["DisplayCustomResolution"] = ("Özel çözünürlük", "Custom resolution"),
        ["DisplayOptions"] = ("Seçenekler", "Options"),
        ["DisplayFullscreen"] = ("Tam ekran", "Fullscreen"),
        ["DisplayProtectFile"] = ("videoconfig.txt dosyasını koru", "Protect videoconfig.txt"),

        // Grafik (Graphics)
        ["GraphicsTitle"] = ("Grafik", "Graphics"),
        ["GraphicsTextures"] = ("Dokular", "Textures"),
        ["GraphicsStreamBudget"] = ("Akış bütçesi", "Streaming budget"),
        ["GraphicsFiltering"] = ("Filtreleme", "Filtering"),
        ["GraphicsResolution"] = ("Çözünürlük", "Resolution"),
        ["GraphicsDetail"] = ("Detay", "Detail"),
        ["GraphicsModelDetail"] = ("Model detayı", "Model detail"),
        ["GraphicsParticles"] = ("Parçacıklar", "Particles"),
        ["GraphicsRagdolls"] = ("Ragdoll'lar", "Ragdolls"),
        ["GraphicsWorldDecals"] = ("Dünya çıkartmaları", "World decals"),
        ["GraphicsRendering"] = ("Render", "Rendering"),
        ["GraphicsAntiAliasing"] = ("Kenar yumuşatma", "Anti-aliasing"),
        ["GraphicsVSync"] = ("Dikey senkron (VSync)", "VSync"),
        ["GraphicsVolumetricLighting"] = ("Hacimsel aydınlatma", "Volumetric lighting"),
        ["GraphicsVolumetricFog"] = ("Hacimsel sis", "Volumetric fog"),
        ["GraphicsAdvancedRendering"] = ("Gelişmiş render", "Advanced rendering"),
        ["GraphicsDynamicResolution"] = ("Dinamik çözünürlük", "Dynamic resolution"),
        ["GraphicsDynamicStreaming"] = ("Dinamik akış", "Dynamic streaming"),
        ["GraphicsAmbientOcclusion"] = ("Ortam gölgelemesi (AO)", "Ambient occlusion"),
        ["GraphicsMapDetail"] = ("Harita detayı", "Map detail"),

        ["OptNone"] = ("Yok", "None"),
        ["OptVeryLow"] = ("Çok düşük", "Very low"),
        ["OptLow"] = ("Düşük", "Low"),
        ["OptMedium"] = ("Orta", "Medium"),
        ["OptHigh"] = ("Yüksek", "High"),
        ["OptVeryHigh"] = ("Çok yüksek", "Very high"),
        ["OptUltra"] = ("Ultra", "Ultra"),
        ["OptLowest"] = ("En düşük", "Lowest"),
        ["OptMinimum"] = ("Minimum", "Minimum"),
        ["OptBilinear"] = ("Bilinear", "Bilinear"),
        ["OptTrilinear"] = ("Trilinear", "Trilinear"),
        ["OptAniso2"] = ("Anizotropik 2×", "Anisotropic 2×"),
        ["OptAniso4"] = ("Anizotropik 4×", "Anisotropic 4×"),
        ["OptAniso8"] = ("Anizotropik 8×", "Anisotropic 8×"),
        ["OptAniso16"] = ("Anizotropik 16×", "Anisotropic 16×"),
        ["OptTSAA"] = ("TSAA", "TSAA"),
        ["OptOptimized"] = ("Optimize", "Optimized"),
        ["OptNormal"] = ("Normal", "Normal"),

        // Gölgeler (Shadows)
        ["ShadowsTitle"] = ("Gölgeler", "Shadows"),
        ["ShadowsDisableAll"] = ("Tüm gölgeleri kapat", "Disable all shadows"),
        ["ShadowsRendering"] = ("Gölge render", "Shadow rendering"),
        ["ShadowsShadows"] = ("Gölgeler", "Shadows"),
        ["ShadowsDynamicShadows"] = ("Dinamik gölgeler", "Dynamic shadows"),
        ["ShadowsCascadeShadows"] = ("Kademeli (cascade) gölgeler", "Cascade shadows"),
        ["ShadowsNewShadowSystem"] = ("Yeni gölge sistemi", "New shadow system"),
        ["ShadowsCascadeQuality"] = ("Kademe kalitesi", "Cascade quality"),
        ["ShadowsCoverage"] = ("Kapsama", "Coverage"),
        ["ShadowsCascadeResolution"] = ("Kademe çözünürlüğü", "Cascade resolution"),
        ["ShadowsShadowDepth"] = ("Gölge derinliği", "Shadow depth"),
        ["ShadowsUpresFactor"] = ("Yükseltme (upres) çarpanı", "Upres factor"),

        // Performans (Performance)
        ["PerfTitle"] = ("Performans", "Performance"),
        ["PerfCompetitive"] = ("Rekabetçi", "Competitive"),
        ["PerfCompetitiveShort"] = ("Az karmaşa, hızlı render", "Low clutter, fast rendering"),
        ["PerfCompetitiveDesc"] = ("Orta düzey doku filtrelemesini korurken düşük maliyetli görsel ayarları yükler.", "Loads low-cost visual settings while keeping moderate texture filtering."),
        ["PerfMaxFps"] = ("Maksimum FPS", "Maximum FPS"),
        ["PerfMaxFpsShort"] = ("En düşük görsel maliyet", "Lowest visual cost"),
        ["PerfMaxFpsDesc"] = ("Araçtaki en agresif düşük grafik önayarını yükler.", "Loads the most aggressive low graphics preset available in the tool."),
        ["PerfBalanced"] = ("Dengeli", "Balanced"),
        ["PerfBalancedShort"] = ("Performans ve netlik", "Performance and clarity"),
        ["PerfBalancedDesc"] = ("Orta seviye bir grafik yapılandırması yükler.", "Loads a middle-ground graphics configuration."),
        ["PerfQuality"] = ("Kalite", "Quality"),
        ["PerfQualityShort"] = ("Daha yüksek görsel kalite", "Higher visual fidelity"),
        ["PerfQualityDesc"] = ("Daha yüksek görsel kaliteli ayarları yükler.", "Loads higher visual-quality settings."),
        ["PerfSelectedPreset"] = ("Seçili önayar", "Selected preset"),
        ["PerfCustom"] = ("Özel", "Custom"),
        ["StatusPresetLoaded"] = ("{0} önayarı yüklendi.", "{0} preset loaded."),

        // Autoexec
        ["AutoTitle"] = ("Autoexec", "Autoexec"),
        ["AutoInstallation"] = ("Kurulum", "Installation"),
        ["AutoLauncher"] = ("Başlatıcı", "Launcher"),
        ["AutoAutoexec"] = ("Autoexec", "Autoexec"),
        ["AutoGameFolder"] = ("Oyun klasörü", "Game folder"),
        ["AutoConfigPath"] = ("Config yolu", "Config path"),
        ["AutoOpenCfgFolder"] = ("Cfg klasörünü aç", "Open cfg folder"),
        ["AutoNotFound"] = ("Bulunamadı", "Not found"),
        ["AutoFound"] = ("Bulundu", "Found"),
        ["AutoNotCreated"] = ("Oluşturulmadı", "Not created"),
        ["AutoUnavailable"] = ("Kullanılamıyor", "Unavailable"),
        ["AutoFov"] = ("Görüş açısı", "Field of view"),
        ["AutoFovLabel"] = ("FOV", "FOV"),
        ["AutoFovAdvanced"] = ("120 · Gelişmiş", "120 · Advanced"),
        ["AutoAbilityFovScaling"] = ("Yetenek FOV ölçeklemesi", "Ability FOV scaling"),
        ["AutoFrameRate"] = ("Kare hızı", "Frame rate"),
        ["AutoGameLimit"] = ("Oyun içi limit", "Game limit"),
        ["AutoLobbyLimit"] = ("Lobi limiti", "Lobby limit"),
        ["AutoInput"] = ("Giriş", "Input"),
        ["AutoRawMouseInput"] = ("Ham fare girişi", "Raw mouse input"),
        ["AutoMouseAcceleration"] = ("Fare hızlanması", "Mouse acceleration"),
        ["AutoEffects"] = ("Efektler", "Effects"),
        ["AutoFilmGrain"] = ("Film grenliliği", "Film grain"),
        ["AutoAdsDof"] = ("Nişan alırken alan derinliği (ADS DoF)", "ADS depth of field"),
        ["AutoSprintViewShake"] = ("Koşarken görüş sarsıntısı", "Sprint view shake"),
        ["AutoModelDecals"] = ("Model çıkartmaları", "Model decals"),
        ["AutoCheapWater"] = ("Su render kalitesi", "Water rendering"),
        ["AutoAutoexecFile"] = ("Autoexec dosyası", "Autoexec file"),
        ["AutoReload"] = ("Yeniden yükle", "Reload"),
        ["AutoBackup"] = ("Yedekle", "Backup"),
        ["AutoLaunchOptions"] = ("Başlatma seçenekleri", "Launch options"),
        ["AutoCopy"] = ("Kopyala", "Copy"),
        ["AutoRawAutoexec"] = ("Ham autoexec", "Raw autoexec"),
        ["AutoSaveRawFile"] = ("Ham dosyayı kaydet", "Save raw file"),

        // Profiller (Profiles)
        ["ProfTitle"] = ("Profiller", "Profiles"),
        ["ProfSaveProfile"] = ("Profil kaydet", "Save profile"),
        ["ProfSaveCurrent"] = ("Mevcut yapılandırmayı kaydet", "Save current setup"),
        ["ProfSavedProfiles"] = ("Kayıtlı profiller", "Saved profiles"),
        ["ProfLoad"] = ("Yükle", "Load"),
        ["ProfDelete"] = ("Sil", "Delete"),
        ["ProfRefresh"] = ("Yenile", "Refresh"),
        ["ProfOpenFolder"] = ("Klasörü aç", "Open folder"),
        ["ProfNamePlaceholder"] = ("Apex Profilim", "My Apex Profile"),

        // Yedekler (Backups)
        ["BackupsTitle"] = ("Yedekler", "Backups"),
        ["BackupsVideoConfig"] = ("Video config", "Video config"),
        ["BackupsAutoexec"] = ("Autoexec", "Autoexec"),
        ["BackupsRestore"] = ("Geri yükle", "Restore"),
        ["BackupsDelete"] = ("Sil", "Delete"),
        ["BackupsOpenFolder"] = ("Klasörü aç", "Open folder"),
        ["BackupsRefresh"] = ("Yenile", "Refresh"),

        // Gelişmiş (Advanced)
        ["AdvTitle"] = ("Gelişmiş", "Advanced"),
        ["AdvInspectorTab"] = ("İnceleyici", "Inspector"),
        ["AdvRawEditorTab"] = ("Ham düzenleyici", "Raw editor"),
        ["AdvRefresh"] = ("Yenile", "Refresh"),
        ["AdvSettingsCount"] = ("{0} ayar", "{0} settings"),
        ["AdvReload"] = ("Yeniden yükle", "Reload"),
        ["AdvSave"] = ("Kaydet", "Save"),

        // Önizleme penceresi
        ["PreviewTitle"] = ("Bekleyen değişiklikler", "Pending changes"),
        ["PreviewSummary"] = ("{0} değişiklik · {1} video · {2} autoexec", "{0} changes · {1} video · {2} autoexec"),
        ["PreviewCancel"] = ("İptal", "Cancel"),
        ["PreviewApply"] = ("Uygula", "Apply"),
        ["PreviewApplyLaunch"] = ("Uygula ve başlat", "Apply & launch"),
        ["PreviewColSource"] = ("Kaynak", "Source"),
        ["PreviewColSetting"] = ("Ayar", "Setting"),
        ["PreviewColCurrent"] = ("Mevcut", "Current"),
        ["PreviewColNew"] = ("Yeni", "New"),

        // Diff — ayar dostça adları
        ["DiffResolutionWidth"] = ("Çözünürlük genişliği", "Resolution width"),
        ["DiffResolutionHeight"] = ("Çözünürlük yüksekliği", "Resolution height"),
        ["DiffFullscreen"] = ("Tam ekran", "Fullscreen"),
        ["DiffVSync"] = ("VSync", "VSync"),
        ["DiffAntiAliasing"] = ("Kenar yumuşatma", "Anti-aliasing"),
        ["DiffTextureFiltering"] = ("Doku filtreleme", "Texture filtering"),
        ["DiffTextureResolution"] = ("Doku çözünürlüğü", "Texture resolution"),
        ["DiffStreamingBudget"] = ("Akış bütçesi", "Streaming budget"),
        ["DiffModelDetail"] = ("Model detayı", "Model detail"),
        ["DiffParticleQuality"] = ("Parçacık kalitesi", "Particle quality"),
        ["DiffRagdolls"] = ("Ragdoll'lar", "Ragdolls"),
        ["DiffWorldDecals"] = ("Dünya çıkartmaları", "World decals"),
        ["DiffVolumetricLighting"] = ("Hacimsel aydınlatma", "Volumetric lighting"),
        ["DiffVolumetricFog"] = ("Hacimsel sis", "Volumetric fog"),
        ["DiffDynamicResolution"] = ("Dinamik çözünürlük", "Dynamic resolution"),
        ["DiffDynamicStreaming"] = ("Dinamik akış", "Dynamic streaming"),
        ["DiffAmbientOcclusion"] = ("Ortam gölgelemesi", "Ambient occlusion"),
        ["DiffMapDetail"] = ("Harita detayı", "Map detail"),
        ["DiffShadows"] = ("Gölgeler", "Shadows"),
        ["DiffDynamicShadows"] = ("Dinamik gölgeler", "Dynamic shadows"),
        ["DiffCascadeShadows"] = ("Kademeli gölgeler", "Cascade shadows"),
        ["DiffCsmCoverage"] = ("Kademe kapsamı", "CSM coverage"),
        ["DiffCsmResolution"] = ("Kademe çözünürlüğü", "CSM resolution"),
        ["DiffShadowSystem"] = ("Gölge sistemi", "Shadow system"),
        ["DiffFov"] = ("FOV", "FOV"),
        ["DiffAbilityFovScaling"] = ("Yetenek FOV ölçeklemesi", "Ability FOV scaling"),
        ["DiffFpsLimit"] = ("FPS limiti", "FPS limit"),
        ["DiffLobbyFpsLimit"] = ("Lobi FPS limiti", "Lobby FPS limit"),
        ["DiffRawMouseInput"] = ("Ham fare girişi", "Raw mouse input"),
        ["DiffMouseAcceleration"] = ("Fare hızlanması", "Mouse acceleration"),
        ["DiffCustomAcceleration"] = ("Özel hızlanma", "Custom acceleration"),
        ["DiffFilmGrain"] = ("Film grenliliği", "Film grain"),
        ["DiffAdsDof"] = ("ADS alan derinliği", "ADS depth of field"),
        ["DiffSprintViewShake"] = ("Koşu görüş sarsıntısı", "Sprint view shake"),
        ["DiffModelDecals"] = ("Model çıkartmaları", "Model decals"),
        ["DiffWaterRendering"] = ("Su render kalitesi", "Water rendering"),
        ["DiffDefault"] = ("(varsayılan)", "(default)"),
        ["DiffMissing"] = ("(eksik)", "(missing)"),

        // Ana pencere durum mesajları
        ["StatusDetectionError"] = ("Tespit hatası: {0}", "Detection error: {0}"),
        ["StatusConfigNotFound"] = ("Apex videoconfig dosyası bulunamadı.", "Apex videoconfig could not be found."),
        ["StatusInstallDetected"] = ("{0} kurulumu tespit edildi.", "{0} installation detected."),
        ["StatusConfigLoaded"] = ("Apex config yüklendi.", "Apex config loaded."),
        ["StatusCouldNotReadResolution"] = ("Apex çözünürlüğü okunamadı.", "Could not read Apex resolution."),
        ["StatusApexConfigNotLoaded"] = ("Apex config yüklenmedi.", "Apex config is not loaded."),
        ["StatusApexConfigNotFound"] = ("Apex config bulunamadı.", "Apex config not found."),
        ["StatusCloseApexBeforeApply"] = ("Değişiklikleri uygulamadan önce Apex'i kapatın.", "Close Apex before applying changes."),
        ["StatusInvalidDisplayConfig"] = ("Geçersiz ekran yapılandırması.", "Invalid display configuration."),
        ["StatusNoChangesPending"] = ("Bekleyen değişiklik yok.", "No changes are pending."),
        ["StatusApplyCancelled"] = ("Uygulama iptal edildi.", "Apply cancelled."),
        ["StatusVideoAndAutoexecApplied"] = ("Video config ve autoexec uygulandı.", "Video config and autoexec applied."),
        ["StatusVideoApplied"] = ("Video config uygulandı.", "Video config applied."),
        ["StatusAutoexecApplied"] = ("Autoexec uygulandı.", "Autoexec applied."),
        ["StatusLaunchError"] = ("Başlatma hatası: {0}", "Launch error: {0}"),
        ["StatusLaunching"] = ("Apex başlatılıyor...", "Launching Apex..."),
        ["StatusInstallationUnavailable"] = ("Apex kurulumu kullanılamıyor.", "Apex installation is unavailable."),
        ["StatusBackupCreated"] = ("Yedek oluşturuldu.", "Backup created."),
        ["StatusBackupError"] = ("Yedekleme hatası: {0}", "Backup error: {0}"),
        ["StatusNoBackupFound"] = ("Yedek bulunamadı.", "No backup found."),
        ["StatusLatestRestored"] = ("En son video config geri yüklendi.", "Latest video config restored."),
        ["StatusRestoreError"] = ("Geri yükleme hatası: {0}", "Restore error: {0}"),
        ["StatusCloseApexBeforeRestore"] = ("Geri yüklemeden önce Apex'i kapatın.", "Close Apex before restoring."),
        ["StatusProfileSaved"] = ("\"{0}\" profili kaydedildi.", "Profile \"{0}\" saved."),
        ["StatusProfileSaveError"] = ("Profil kaydetme hatası: {0}", "Profile save error: {0}"),
        ["StatusProfileLoaded"] = ("\"{0}\" profili yüklendi.", "Profile \"{0}\" loaded."),
        ["StatusProfileLoadError"] = ("Profil yükleme hatası: {0}", "Profile load error: {0}"),
        ["StatusCloseApexBeforeLoadProfile"] = ("Profil yüklemeden önce Apex'i kapatın.", "Close Apex before loading a profile."),
        ["StatusFpsRange"] = ("FPS limiti 30 ile 1000 arasında olmalıdır.", "FPS limit must be between 30 and 1000."),
        ["StatusLobbyFpsRange"] = ("Lobi FPS limiti 20 ile 300 arasında olmalıdır.", "Lobby FPS limit must be between 20 and 300."),
        ["StatusCouldNotOpenFolder"] = ("Klasör açılamadı: {0}", "Could not open folder: {0}"),

        // Autoexec görünümü durum mesajları
        ["AutoStatusNotDetected"] = ("Apex kurulumu tespit edilemedi.", "Apex installation was not detected."),
        ["AutoStatusCloseApexBeforeEdit"] = ("Autoexec'i düzenlemeden önce Apex'i kapatın.", "Close Apex before editing autoexec."),
        ["AutoStatusSaved"] = ("Autoexec kaydedildi.", "Autoexec saved."),
        ["AutoStatusSaveError"] = ("Autoexec kaydetme hatası: {0}", "Autoexec save error: {0}"),
        ["AutoStatusReloaded"] = ("Autoexec yeniden yüklendi.", "Autoexec reloaded."),
        ["AutoStatusUnavailable"] = ("Autoexec kullanılamıyor.", "Autoexec is unavailable."),
        ["AutoStatusNoFileYet"] = ("Henüz bir autoexec dosyası yok.", "No autoexec file exists yet."),
        ["AutoStatusBackupCreated"] = ("Autoexec yedeği oluşturuldu.", "Autoexec backup created."),
        ["AutoStatusBackupError"] = ("Autoexec yedekleme hatası: {0}", "Autoexec backup error: {0}"),
        ["AutoStatusLaunchOptionsCopied"] = ("Başlatma seçenekleri kopyalandı.", "Launch options copied."),

        // Yedekler görünümü durum mesajları
        ["BackupsStatusCloseApex"] = ("Yedek geri yüklemeden önce Apex Legends'ı kapatın.", "Close Apex Legends before restoring a backup."),
        ["BackupsStatusVideoUnavailable"] = ("Video config yedekleme servisi kullanılamıyor.", "VideoConfig backup service unavailable."),
        ["BackupsStatusSelectVideo"] = ("Önce bir videoconfig yedeği seçin.", "Select a videoconfig backup first."),
        ["BackupsStatusVideoRestored"] = ("Seçili videoconfig yedeği geri yüklendi.", "Selected videoconfig backup restored."),
        ["BackupsStatusVideoDeleted"] = ("Videoconfig yedeği silindi.", "Videoconfig backup deleted."),
        ["BackupsStatusAutoexecUnavailable"] = ("Autoexec yedekleme servisi kullanılamıyor.", "Autoexec backup service unavailable."),
        ["BackupsStatusSelectAutoexec"] = ("Önce bir autoexec yedeği seçin.", "Select an autoexec backup first."),
        ["BackupsStatusAutoexecRestored"] = ("Seçili autoexec yedeği geri yüklendi.", "Selected autoexec backup restored."),
        ["BackupsStatusAutoexecDeleted"] = ("Autoexec yedeği silindi.", "Autoexec backup deleted."),
        ["BackupsStatusDeleteError"] = ("Silme hatası: {0}", "Delete error: {0}"),
        ["BackupsStatusListRefreshed"] = ("Yedek listesi yenilendi.", "Backup list refreshed."),
        ["BackupsConfirmRestore"] = ("Bu yedeği geri yüklemek istiyor musunuz?", "Restore this backup?"),
        ["BackupsConfirmDelete"] = ("Bu yedeği kalıcı olarak silmek istiyor musunuz?", "Delete this backup permanently?"),
        ["BackupsRestoreTitle"] = ("Yedeği geri yükle", "Restore Backup"),
        ["BackupsDeleteTitle"] = ("Yedeği sil", "Delete Backup"),

        // Profiller görünümü durum mesajları
        ["ProfStatusEnterName"] = ("Önce bir profil adı girin.", "Enter a profile name first."),
        ["ProfStatusSelect"] = ("Önce bir profil seçin.", "Select a profile first."),
        ["ProfConfirmLoad"] = ("\"{0}\" profilini yüklemek istiyor musunuz?\n\nMevcut yapılandırmanız önce yedeklenecek.", "Load profile \"{0}\"?\n\nYour current config will be backed up first."),
        ["ProfConfirmDelete"] = ("\"{0}\" profilini silmek istiyor musunuz?", "Delete profile \"{0}\"?"),
        ["ProfLoadTitle"] = ("Profili yükle", "Load Profile"),
        ["ProfDeleteTitle"] = ("Profili sil", "Delete Profile"),
        ["ProfStatusDeleted"] = ("Profil silindi.", "Profile deleted."),
        ["ProfStatusDeleteError"] = ("Profil silme hatası: {0}", "Profile delete error: {0}"),
        ["ProfStatusRefreshed"] = ("Profiller yenilendi.", "Profiles refreshed."),

        // Gelişmiş görünüm durum mesajları
        ["AdvStatusRefreshed"] = ("Gelişmiş ayar inceleyicisi yenilendi.", "Advanced config inspector refreshed."),
        ["AdvStatusReloadedFromDisk"] = ("videoconfig.txt diskten yeniden yüklendi.", "videoconfig.txt reloaded from disk."),
        ["AdvStatusConfigNotLoaded"] = ("Video config yüklenmedi.", "Video config is not loaded."),
        ["AdvStatusCloseApexBeforeSave"] = ("Ham config kaydetmeden önce Apex'i kapatın.", "Close Apex before saving raw config."),
        ["AdvConfirmSaveRaw"] = ("Tüm videoconfig.txt dosyası kaydedilsin mi?\n\nÖnce bir yedek alınacak.", "Save the complete raw videoconfig.txt?\n\nA backup will be created first."),
        ["AdvSaveRawTitle"] = ("Ham config kaydet", "Save Raw Config"),
        ["AdvStatusSavedSuccessfully"] = ("Ham videoconfig.txt başarıyla kaydedildi.", "Raw videoconfig.txt saved successfully."),
        ["AdvStatusSaveError"] = ("Ham config kaydetme hatası: {0}", "Raw config save error: {0}"),
    };
}
