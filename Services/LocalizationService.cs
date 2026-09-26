using System.Collections.Generic;
using System.ComponentModel;

namespace ApexConfigTool.Services;

/// <summary>
/// Uygulama genelinde Türkçe / İngilizce / Çince / Rusça metin çevirisi sağlayan
/// tekil (singleton) servis. XAML tarafında
/// <c>{Binding Source={x:Static loc:LocalizationService.Instance}, Path=[AnahtarAdi]}</c>
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
        }
    }

    /// <summary>Indeksleyici — XAML binding'lerinde kullanılır.</summary>
    public string this[string key] => T(key);

    /// <summary>Kod tarafında çeviri almak için kullanılır.</summary>
    public string T(string key)
    {
        if (!Translations.Map.TryGetValue(key, out var entry))
            return key;

        return language switch
        {
            AppLanguage.Turkish => entry.Tr,
            AppLanguage.Chinese => entry.Zh,
            AppLanguage.Russian => entry.Ru,
            _ => entry.En
        };
    }

    /// <summary>Format dizesi içeren çeviriler için (örn. "{0} yedeği geri yüklendi.").</summary>
    public string F(string key, params object[] args)
    {
        return string.Format(T(key), args);
    }
}

/// <summary>Tüm çeviri anahtarları ve karşılık gelen Türkçe/İngilizce/Çince/Rusça metinler.</summary>
public static class Translations
{
    public static readonly Dictionary<string, (string Tr, string En, string Zh, string Ru)> Map = new()
    {
        // Genel
        ["AppTitle"] = ("Apex Config", "Apex Config", "Apex Config", "Apex Config"),
        ["VersionLabel"] = ("s1.0", "v1.0", "v1.0", "в1.0"),
        ["StatusReady"] = ("Hazır", "Ready", "就绪", "Готово"),
        ["ApexRunningPill"] = ("● Apex çalışıyor", "● Apex running", "● Apex 运行中", "● Apex запущен"),
        ["ApexOffPill"] = ("● Apex kapalı", "● Apex off", "● Apex 未运行", "● Apex не запущен"),

        // Navigasyon
        ["NavDashboard"] = ("Panel", "Dashboard", "概览", "Панель"),
        ["NavDisplay"] = ("Ekran", "Display", "显示", "Экран"),
        ["NavGraphics"] = ("Grafik", "Graphics", "画质", "Графика"),
        ["NavShadows"] = ("Gölgeler", "Shadows", "阴影", "Тени"),
        ["NavPerformance"] = ("Performans", "Performance", "性能", "Производительность"),
        ["NavAutoexec"] = ("Autoexec", "Autoexec", "Autoexec", "Autoexec"),
        ["NavProfiles"] = ("Profiller", "Profiles", "配置文件", "Профили"),
        ["NavBackups"] = ("Yedekler", "Backups", "备份", "Резервные копии"),
        ["NavAdvanced"] = ("Gelişmiş", "Advanced", "高级", "Дополнительно"),

        // Navigasyon grup başlıkları
        ["NavGroupOverview"] = ("GENEL", "OVERVIEW", "概览", "ОБЗОР"),
        ["NavGroupSettings"] = ("AYARLAR", "SETTINGS", "设置", "НАСТРОЙКИ"),
        ["NavGroupGame"] = ("OYUN İÇİ", "IN-GAME", "游戏内", "В ИГРЕ"),
        ["NavGroupManage"] = ("YÖNETİM", "MANAGE", "管理", "УПРАВЛЕНИЕ"),

        // Sayfa alt başlıkları (PageHeader)
        ["DashSubtitle"] = ("Apex kurulumunuzun ve config dosyalarınızın genel durumu.", "Overview of your Apex installation and config files.", "您的 Apex 安装及配置文件的总体状态。", "Обзор состояния вашей установки Apex и конфигурационных файлов."),
        ["DisplaySubtitle"] = ("Çözünürlük ve pencere modu ayarları.", "Resolution and window mode settings.", "分辨率与窗口模式设置。", "Настройки разрешения и оконного режима."),
        ["GraphicsSubtitle"] = ("Doku, render ve detay kalitesi ayarları.", "Texture, rendering and detail quality settings.", "材质、渲染与细节质量设置。", "Настройки текстур, рендеринга и качества детализации."),
        ["ShadowsSubtitle"] = ("Gölge render kalitesi ile performans arasındaki dengeyi ayarlayın.", "Tune the balance between shadow rendering quality and performance.", "调节阴影渲染质量与性能之间的平衡。", "Настройте баланс между качеством теней и производительностью."),
        ["PerfSubtitle"] = ("Tek tıkla hazır performans önayarları yükleyin.", "Load ready-made performance presets with one click.", "一键加载现成的性能预设。", "Загружайте готовые пресеты производительности одним кликом."),
        ["AutoSubtitle"] = ("FOV, FPS limiti, giriş ve efekt ayarlarını autoexec.cfg üzerinden yönetin.", "Manage FOV, FPS limit, input and effect settings via autoexec.cfg.", "通过 autoexec.cfg 管理视野、帧率上限、输入与特效设置。", "Управляйте FOV, лимитом FPS, вводом и эффектами через autoexec.cfg."),
        ["ProfSubtitle"] = ("Tüm yapılandırmanızı adlandırılmış profiller olarak kaydedin ve geri yükleyin.", "Save and restore your entire configuration as named profiles.", "将整个配置保存为命名的配置文件并随时恢复。", "Сохраняйте и восстанавливайте всю конфигурацию как именованные профили."),
        ["BackupsSubtitle"] = ("Otomatik alınan yedekleri görüntüleyin, geri yükleyin veya silin.", "View, restore or delete your automatic backups.", "查看、恢复或删除自动生成的备份。", "Просматривайте, восстанавливайте или удаляйте автоматические резервные копии."),
        ["AdvSubtitle"] = ("Tüm ayarları arayın, inceleyin veya ham dosyayı doğrudan düzenleyin.", "Search, inspect every setting, or edit the raw file directly.", "搜索、查看所有设置，或直接编辑原始文件。", "Ищите и просматривайте любые настройки или редактируйте файл напрямую."),

        // Alt bar butonları
        ["BtnBackup"] = ("Yedekle", "Backup", "备份", "Резерв. копия"),
        ["BtnRestore"] = ("Geri yükle", "Restore", "恢复", "Восстановить"),
        ["BtnApply"] = ("Uygula", "Apply", "应用", "Применить"),
        ["BtnApplyLaunch"] = ("Uygula ve başlat", "Apply & launch", "应用并启动", "Применить и запустить"),

        // Ortak değerler
        ["CommonKeepCurrent"] = ("Mevcudu koru", "Keep current", "保持当前", "Оставить текущее"),
        ["CommonOn"] = ("Açık", "On", "开启", "Вкл"),
        ["CommonOff"] = ("Kapalı", "Off", "关闭", "Выкл"),
        ["CommonEnabled"] = ("Etkin", "Enabled", "已启用", "Включено"),
        ["CommonDisabled"] = ("Devre dışı", "Disabled", "已禁用", "Отключено"),
        ["CommonDefault"] = ("Varsayılan", "Default", "默认", "По умолчанию"),
        ["CommonMinimal"] = ("Minimal", "Minimal", "最小", "Минимально"),
        ["CommonUnknown"] = ("Bilinmiyor", "Unknown", "未知", "Неизвестно"),
        ["CommonNotDetected"] = ("Tespit edilemedi", "Not detected", "未检测到", "Не обнаружено"),

        // Panel (Dashboard)
        ["DashTitle"] = ("Panel", "Dashboard", "概览", "Панель"),
        ["DashResolution"] = ("Çözünürlük", "Resolution", "分辨率", "Разрешение"),
        ["DashFov"] = ("FOV", "FOV", "视野 (FOV)", "Угол обзора"),
        ["DashFpsLimit"] = ("FPS limiti", "FPS limit", "帧率上限", "Лимит FPS"),
        ["DashApex"] = ("Apex", "Apex", "Apex", "Apex"),
        ["DashLauncher"] = ("Başlatıcı", "Launcher", "启动器", "Лаунчер"),
        ["DashProtection"] = ("Koruma", "Protection", "保护", "Защита"),
        ["DashVideoConfig"] = ("Video config", "Video config", "视频配置", "Видеоконфиг"),
        ["DashRunning"] = ("Çalışıyor", "Running", "运行中", "Запущен"),
        ["DashOff"] = ("Kapalı", "Off", "未运行", "Не запущен"),
        ["DashProtected"] = ("Korumalı", "Protected", "受保护", "Защищён"),
        ["DashWritable"] = ("Yazılabilir", "Writable", "可写入", "Доступен для записи"),

        // Ekran (Display)
        ["DisplayTitle"] = ("Ekran", "Display", "显示", "Экран"),
        ["DisplayResolution"] = ("Çözünürlük", "Resolution", "分辨率", "Разрешение"),
        ["DisplayCustomResolution"] = ("Özel çözünürlük", "Custom resolution", "自定义分辨率", "Своё разрешение"),
        ["DisplayOptions"] = ("Seçenekler", "Options", "选项", "Параметры"),
        ["DisplayFullscreen"] = ("Tam ekran", "Fullscreen", "全屏", "Полноэкранный режим"),
        ["DisplayProtectFile"] = ("videoconfig.txt dosyasını koru", "Protect videoconfig.txt", "保护 videoconfig.txt 文件", "Защитить файл videoconfig.txt"),

        // Grafik (Graphics)
        ["GraphicsTitle"] = ("Grafik", "Graphics", "画质", "Графика"),
        ["GraphicsTextures"] = ("Dokular", "Textures", "材质", "Текстуры"),
        ["GraphicsStreamBudget"] = ("Akış bütçesi", "Streaming budget", "流送预算", "Бюджет потоковой передачи"),
        ["GraphicsFiltering"] = ("Filtreleme", "Filtering", "过滤方式", "Фильтрация"),
        ["GraphicsResolution"] = ("Çözünürlük", "Resolution", "材质分辨率", "Разрешение текстур"),
        ["GraphicsDetail"] = ("Detay", "Detail", "细节", "Детализация"),
        ["GraphicsModelDetail"] = ("Model detayı", "Model detail", "模型细节", "Детализация моделей"),
        ["GraphicsParticles"] = ("Parçacıklar", "Particles", "粒子效果", "Частицы"),
        ["GraphicsRagdolls"] = ("Ragdoll'lar", "Ragdolls", "布娃娃效果", "Рэгдоллы"),
        ["GraphicsWorldDecals"] = ("Dünya çıkartmaları", "World decals", "世界贴花", "Мировые декали"),
        ["GraphicsRendering"] = ("Render", "Rendering", "渲染", "Рендеринг"),
        ["GraphicsAntiAliasing"] = ("Kenar yumuşatma", "Anti-aliasing", "抗锯齿", "Сглаживание"),
        ["GraphicsVSync"] = ("Dikey senkron (VSync)", "VSync", "垂直同步 (VSync)", "Вертикальная синхронизация"),
        ["GraphicsVolumetricLighting"] = ("Hacimsel aydınlatma", "Volumetric lighting", "体积光照", "Объёмное освещение"),
        ["GraphicsVolumetricFog"] = ("Hacimsel sis", "Volumetric fog", "体积雾效", "Объёмный туман"),
        ["GraphicsAdvancedRendering"] = ("Gelişmiş render", "Advanced rendering", "高级渲染", "Расширенный рендеринг"),
        ["GraphicsDynamicResolution"] = ("Dinamik çözünürlük", "Dynamic resolution", "动态分辨率", "Динамическое разрешение"),
        ["GraphicsDynamicStreaming"] = ("Dinamik akış", "Dynamic streaming", "动态流送", "Динамическая потоковая передача"),
        ["GraphicsAmbientOcclusion"] = ("Ortam gölgelemesi (AO)", "Ambient occlusion", "环境光遮蔽 (AO)", "Затенение окружения (AO)"),
        ["GraphicsMapDetail"] = ("Harita detayı", "Map detail", "地图细节", "Детализация карты"),

        ["OptNone"] = ("Yok", "None", "无", "Нет"),
        ["OptVeryLow"] = ("Çok düşük", "Very low", "极低", "Очень низкое"),
        ["OptLow"] = ("Düşük", "Low", "低", "Низкое"),
        ["OptMedium"] = ("Orta", "Medium", "中", "Среднее"),
        ["OptHigh"] = ("Yüksek", "High", "高", "Высокое"),
        ["OptVeryHigh"] = ("Çok yüksek", "Very high", "极高", "Очень высокое"),
        ["OptUltra"] = ("Ultra", "Ultra", "极致", "Ультра"),
        ["OptLowest"] = ("En düşük", "Lowest", "最低", "Минимальное"),
        ["OptMinimum"] = ("Minimum", "Minimum", "最小", "Минимум"),
        ["OptBilinear"] = ("Bilinear", "Bilinear", "双线性", "Билинейная"),
        ["OptTrilinear"] = ("Trilinear", "Trilinear", "三线性", "Трилинейная"),
        ["OptAniso2"] = ("Anizotropik 2×", "Anisotropic 2×", "各向异性 2×", "Анизотропная 2×"),
        ["OptAniso4"] = ("Anizotropik 4×", "Anisotropic 4×", "各向异性 4×", "Анизотропная 4×"),
        ["OptAniso8"] = ("Anizotropik 8×", "Anisotropic 8×", "各向异性 8×", "Анизотропная 8×"),
        ["OptAniso16"] = ("Anizotropik 16×", "Anisotropic 16×", "各向异性 16×", "Анизотропная 16×"),
        ["OptTSAA"] = ("TSAA", "TSAA", "TSAA", "TSAA"),
        ["OptOptimized"] = ("Optimize", "Optimized", "优化", "Оптимизировано"),
        ["OptNormal"] = ("Normal", "Normal", "普通", "Обычная"),

        // Gölgeler (Shadows)
        ["ShadowsTitle"] = ("Gölgeler", "Shadows", "阴影", "Тени"),
        ["ShadowsDisableAll"] = ("Tüm gölgeleri kapat", "Disable all shadows", "禁用所有阴影", "Отключить все тени"),
        ["ShadowsRendering"] = ("Gölge render", "Shadow rendering", "阴影渲染", "Рендеринг теней"),
        ["ShadowsShadows"] = ("Gölgeler", "Shadows", "阴影", "Тени"),
        ["ShadowsDynamicShadows"] = ("Dinamik gölgeler", "Dynamic shadows", "动态阴影", "Динамические тени"),
        ["ShadowsCascadeShadows"] = ("Kademeli (cascade) gölgeler", "Cascade shadows", "级联阴影", "Каскадные тени"),
        ["ShadowsNewShadowSystem"] = ("Yeni gölge sistemi", "New shadow system", "新阴影系统", "Новая система теней"),
        ["ShadowsCascadeQuality"] = ("Kademe kalitesi", "Cascade quality", "级联质量", "Качество каскадов"),
        ["ShadowsCoverage"] = ("Kapsama", "Coverage", "覆盖范围", "Покрытие"),
        ["ShadowsCascadeResolution"] = ("Kademe çözünürlüğü", "Cascade resolution", "级联分辨率", "Разрешение каскадов"),
        ["ShadowsShadowDepth"] = ("Gölge derinliği", "Shadow depth", "阴影深度", "Глубина теней"),
        ["ShadowsUpresFactor"] = ("Yükseltme (upres) çarpanı", "Upres factor", "分辨率提升系数", "Коэффициент повышения разрешения"),

        // Performans (Performance)
        ["PerfTitle"] = ("Performans", "Performance", "性能", "Производительность"),
        ["PerfCompetitive"] = ("Rekabetçi", "Competitive", "竞技", "Соревновательный"),
        ["PerfCompetitiveShort"] = ("Az karmaşa, hızlı render", "Low clutter, fast rendering", "画面简洁，渲染迅速", "Меньше деталей, быстрый рендеринг"),
        ["PerfCompetitiveDesc"] = ("Orta düzey doku filtrelemesini korurken düşük maliyetli görsel ayarları yükler.", "Loads low-cost visual settings while keeping moderate texture filtering.", "在保留中等材质过滤的同时加载低开销的画面设置。", "Загружает лёгкие визуальные настройки, сохраняя умеренную фильтрацию текстур."),
        ["PerfMaxFps"] = ("Maksimum FPS", "Maximum FPS", "最大帧率", "Максимум FPS"),
        ["PerfMaxFpsShort"] = ("En düşük görsel maliyet", "Lowest visual cost", "最低画面开销", "Минимальная визуальная нагрузка"),
        ["PerfMaxFpsDesc"] = ("Araçtaki en agresif düşük grafik önayarını yükler.", "Loads the most aggressive low graphics preset available in the tool.", "加载本工具中最激进的低画质预设。", "Загружает самый агрессивный пресет низкого качества графики."),
        ["PerfBalanced"] = ("Dengeli", "Balanced", "均衡", "Сбалансированный"),
        ["PerfBalancedShort"] = ("Performans ve netlik", "Performance and clarity", "性能与清晰度兼顾", "Производительность и чёткость"),
        ["PerfBalancedDesc"] = ("Orta seviye bir grafik yapılandırması yükler.", "Loads a middle-ground graphics configuration.", "加载中等水平的画质配置。", "Загружает усреднённую конфигурацию графики."),
        ["PerfQuality"] = ("Kalite", "Quality", "画质优先", "Качество"),
        ["PerfQualityShort"] = ("Daha yüksek görsel kalite", "Higher visual fidelity", "更高的画面精细度", "Более высокая детализация"),
        ["PerfQualityDesc"] = ("Daha yüksek görsel kaliteli ayarları yükler.", "Loads higher visual-quality settings.", "加载更高画质的设置。", "Загружает настройки с более высоким визуальным качеством."),
        ["PerfSelectedPreset"] = ("Seçili önayar", "Selected preset", "已选预设", "Выбранный пресет"),
        ["PerfCustom"] = ("Özel", "Custom", "自定义", "Пользовательский"),
        ["StatusPresetLoaded"] = ("{0} önayarı yüklendi.", "{0} preset loaded.", "已加载“{0}”预设。", "Пресет «{0}» загружен."),

        // Autoexec
        ["AutoTitle"] = ("Autoexec", "Autoexec", "Autoexec", "Autoexec"),
        ["AutoInstallation"] = ("Kurulum", "Installation", "安装信息", "Установка"),
        ["AutoLauncher"] = ("Başlatıcı", "Launcher", "启动器", "Лаунчер"),
        ["AutoAutoexec"] = ("Autoexec", "Autoexec", "Autoexec", "Autoexec"),
        ["AutoGameFolder"] = ("Oyun klasörü", "Game folder", "游戏文件夹", "Папка игры"),
        ["AutoConfigPath"] = ("Config yolu", "Config path", "配置文件路径", "Путь к конфигу"),
        ["AutoOpenCfgFolder"] = ("Cfg klasörünü aç", "Open cfg folder", "打开 cfg 文件夹", "Открыть папку cfg"),
        ["AutoNotFound"] = ("Bulunamadı", "Not found", "未找到", "Не найден"),
        ["AutoFound"] = ("Bulundu", "Found", "已找到", "Найден"),
        ["AutoNotCreated"] = ("Oluşturulmadı", "Not created", "尚未创建", "Не создан"),
        ["AutoUnavailable"] = ("Kullanılamıyor", "Unavailable", "不可用", "Недоступно"),
        ["AutoFov"] = ("Görüş açısı", "Field of view", "视野角度", "Угол обзора"),
        ["AutoFovLabel"] = ("FOV", "FOV", "视野 (FOV)", "FOV"),
        ["AutoFovAdvanced"] = ("120 · Gelişmiş", "120 · Advanced", "120 · 进阶", "120 · Расширенный"),
        ["AutoAbilityFovScaling"] = ("Yetenek FOV ölçeklemesi", "Ability FOV scaling", "技能视野缩放", "Масштабирование FOV для способностей"),
        ["AutoFrameRate"] = ("Kare hızı", "Frame rate", "帧率", "Частота кадров"),
        ["AutoGameLimit"] = ("Oyun içi limit", "Game limit", "游戏内帧率上限", "Лимит в игре"),
        ["AutoLobbyLimit"] = ("Lobi limiti", "Lobby limit", "大厅帧率上限", "Лимит в лобби"),
        ["AutoInput"] = ("Giriş", "Input", "输入", "Ввод"),
        ["AutoRawMouseInput"] = ("Ham fare girişi", "Raw mouse input", "原始鼠标输入", "Необработанный ввод мыши"),
        ["AutoMouseAcceleration"] = ("Fare hızlanması", "Mouse acceleration", "鼠标加速", "Ускорение мыши"),
        ["AutoEffects"] = ("Efektler", "Effects", "特效", "Эффекты"),
        ["AutoFilmGrain"] = ("Film grenliliği", "Film grain", "胶片颗粒", "Зернистость плёнки"),
        ["AutoAdsDof"] = ("Nişan alırken alan derinliği (ADS DoF)", "ADS depth of field", "开镜景深 (ADS DoF)", "Глубина резкости при прицеливании"),
        ["AutoSprintViewShake"] = ("Koşarken görüş sarsıntısı", "Sprint view shake", "疾跑视角晃动", "Тряска камеры при беге"),
        ["AutoModelDecals"] = ("Model çıkartmaları", "Model decals", "模型贴花", "Декали на моделях"),
        ["AutoCheapWater"] = ("Su render kalitesi", "Water rendering", "水面渲染质量", "Качество рендеринга воды"),
        ["AutoAutoexecFile"] = ("Autoexec dosyası", "Autoexec file", "Autoexec 文件", "Файл autoexec"),
        ["AutoReload"] = ("Yeniden yükle", "Reload", "重新加载", "Обновить"),
        ["AutoBackup"] = ("Yedekle", "Backup", "备份", "Резерв. копия"),
        ["AutoLaunchOptions"] = ("Başlatma seçenekleri", "Launch options", "启动选项", "Параметры запуска"),
        ["AutoCopy"] = ("Kopyala", "Copy", "复制", "Копировать"),
        ["AutoRawAutoexec"] = ("Ham autoexec", "Raw autoexec", "原始 Autoexec 内容", "Исходный autoexec"),
        ["AutoSaveRawFile"] = ("Ham dosyayı kaydet", "Save raw file", "保存原始文件", "Сохранить файл"),

        // Profiller (Profiles)
        ["ProfTitle"] = ("Profiller", "Profiles", "配置文件", "Профили"),
        ["ProfSaveProfile"] = ("Profil kaydet", "Save profile", "保存配置文件", "Сохранить профиль"),
        ["ProfSaveCurrent"] = ("Mevcut yapılandırmayı kaydet", "Save current setup", "保存当前配置", "Сохранить текущую настройку"),
        ["ProfSavedProfiles"] = ("Kayıtlı profiller", "Saved profiles", "已保存的配置文件", "Сохранённые профили"),
        ["ProfLoad"] = ("Yükle", "Load", "加载", "Загрузить"),
        ["ProfDelete"] = ("Sil", "Delete", "删除", "Удалить"),
        ["ProfRefresh"] = ("Yenile", "Refresh", "刷新", "Обновить"),
        ["ProfOpenFolder"] = ("Klasörü aç", "Open folder", "打开文件夹", "Открыть папку"),
        ["ProfNamePlaceholder"] = ("Apex Profilim", "My Apex Profile", "我的 Apex 配置", "Мой профиль Apex"),

        // Yedekler (Backups)
        ["BackupsTitle"] = ("Yedekler", "Backups", "备份", "Резервные копии"),
        ["BackupsVideoConfig"] = ("Video config", "Video config", "视频配置", "Видеоконфиг"),
        ["BackupsAutoexec"] = ("Autoexec", "Autoexec", "Autoexec", "Autoexec"),
        ["BackupsRestore"] = ("Geri yükle", "Restore", "恢复", "Восстановить"),
        ["BackupsDelete"] = ("Sil", "Delete", "删除", "Удалить"),
        ["BackupsOpenFolder"] = ("Klasörü aç", "Open folder", "打开文件夹", "Открыть папку"),
        ["BackupsRefresh"] = ("Yenile", "Refresh", "刷新", "Обновить"),

        // Gelişmiş (Advanced)
        ["AdvTitle"] = ("Gelişmiş", "Advanced", "高级", "Дополнительно"),
        ["AdvInspectorTab"] = ("İnceleyici", "Inspector", "检查器", "Инспектор"),
        ["AdvRawEditorTab"] = ("Ham düzenleyici", "Raw editor", "原始编辑器", "Редактор исходного файла"),
        ["AdvRefresh"] = ("Yenile", "Refresh", "刷新", "Обновить"),
        ["AdvSettingsCount"] = ("{0} ayar", "{0} settings", "{0} 项设置", "Настроек: {0}"),
        ["AdvReload"] = ("Yeniden yükle", "Reload", "重新加载", "Перезагрузить"),
        ["AdvSave"] = ("Kaydet", "Save", "保存", "Сохранить"),

        // Önizleme penceresi
        ["PreviewTitle"] = ("Bekleyen değişiklikler", "Pending changes", "待处理的更改", "Ожидающие изменения"),
        ["PreviewSummary"] = ("{0} değişiklik · {1} video · {2} autoexec", "{0} changes · {1} video · {2} autoexec", "共 {0} 项更改 · 视频 {1} · autoexec {2}", "{0} изменений · видео {1} · autoexec {2}"),
        ["PreviewCancel"] = ("İptal", "Cancel", "取消", "Отмена"),
        ["PreviewApply"] = ("Uygula", "Apply", "应用", "Применить"),
        ["PreviewApplyLaunch"] = ("Uygula ve başlat", "Apply & launch", "应用并启动", "Применить и запустить"),
        ["PreviewColSource"] = ("Kaynak", "Source", "来源", "Источник"),
        ["PreviewColSetting"] = ("Ayar", "Setting", "设置项", "Настройка"),
        ["PreviewColCurrent"] = ("Mevcut", "Current", "当前值", "Текущее"),
        ["PreviewColNew"] = ("Yeni", "New", "新值", "Новое"),

        // Diff — ayar dostça adları
        ["DiffResolutionWidth"] = ("Çözünürlük genişliği", "Resolution width", "分辨率宽度", "Ширина разрешения"),
        ["DiffResolutionHeight"] = ("Çözünürlük yüksekliği", "Resolution height", "分辨率高度", "Высота разрешения"),
        ["DiffFullscreen"] = ("Tam ekran", "Fullscreen", "全屏", "Полноэкранный режим"),
        ["DiffVSync"] = ("VSync", "VSync", "垂直同步", "Верт. синхронизация"),
        ["DiffAntiAliasing"] = ("Kenar yumuşatma", "Anti-aliasing", "抗锯齿", "Сглаживание"),
        ["DiffTextureFiltering"] = ("Doku filtreleme", "Texture filtering", "材质过滤", "Фильтрация текстур"),
        ["DiffTextureResolution"] = ("Doku çözünürlüğü", "Texture resolution", "材质分辨率", "Разрешение текстур"),
        ["DiffStreamingBudget"] = ("Akış bütçesi", "Streaming budget", "流送预算", "Бюджет потоковой передачи"),
        ["DiffModelDetail"] = ("Model detayı", "Model detail", "模型细节", "Детализация моделей"),
        ["DiffParticleQuality"] = ("Parçacık kalitesi", "Particle quality", "粒子质量", "Качество частиц"),
        ["DiffRagdolls"] = ("Ragdoll'lar", "Ragdolls", "布娃娃效果", "Рэгдоллы"),
        ["DiffWorldDecals"] = ("Dünya çıkartmaları", "World decals", "世界贴花", "Мировые декали"),
        ["DiffVolumetricLighting"] = ("Hacimsel aydınlatma", "Volumetric lighting", "体积光照", "Объёмное освещение"),
        ["DiffVolumetricFog"] = ("Hacimsel sis", "Volumetric fog", "体积雾效", "Объёмный туман"),
        ["DiffDynamicResolution"] = ("Dinamik çözünürlük", "Dynamic resolution", "动态分辨率", "Динамическое разрешение"),
        ["DiffDynamicStreaming"] = ("Dinamik akış", "Dynamic streaming", "动态流送", "Динамическая потоковая передача"),
        ["DiffAmbientOcclusion"] = ("Ortam gölgelemesi", "Ambient occlusion", "环境光遮蔽", "Затенение окружения"),
        ["DiffMapDetail"] = ("Harita detayı", "Map detail", "地图细节", "Детализация карты"),
        ["DiffShadows"] = ("Gölgeler", "Shadows", "阴影", "Тени"),
        ["DiffDynamicShadows"] = ("Dinamik gölgeler", "Dynamic shadows", "动态阴影", "Динамические тени"),
        ["DiffCascadeShadows"] = ("Kademeli gölgeler", "Cascade shadows", "级联阴影", "Каскадные тени"),
        ["DiffCsmCoverage"] = ("Kademe kapsamı", "CSM coverage", "级联覆盖范围", "Покрытие каскадов"),
        ["DiffCsmResolution"] = ("Kademe çözünürlüğü", "CSM resolution", "级联分辨率", "Разрешение каскадов"),
        ["DiffShadowSystem"] = ("Gölge sistemi", "Shadow system", "阴影系统", "Система теней"),
        ["DiffFov"] = ("FOV", "FOV", "视野 (FOV)", "Угол обзора"),
        ["DiffAbilityFovScaling"] = ("Yetenek FOV ölçeklemesi", "Ability FOV scaling", "技能视野缩放", "Масштабирование FOV для способностей"),
        ["DiffFpsLimit"] = ("FPS limiti", "FPS limit", "帧率上限", "Лимит FPS"),
        ["DiffLobbyFpsLimit"] = ("Lobi FPS limiti", "Lobby FPS limit", "大厅帧率上限", "Лимит FPS в лобби"),
        ["DiffRawMouseInput"] = ("Ham fare girişi", "Raw mouse input", "原始鼠标输入", "Необработанный ввод мыши"),
        ["DiffMouseAcceleration"] = ("Fare hızlanması", "Mouse acceleration", "鼠标加速", "Ускорение мыши"),
        ["DiffCustomAcceleration"] = ("Özel hızlanma", "Custom acceleration", "自定义加速", "Пользовательское ускорение"),
        ["DiffFilmGrain"] = ("Film grenliliği", "Film grain", "胶片颗粒", "Зернистость плёнки"),
        ["DiffAdsDof"] = ("ADS alan derinliği", "ADS depth of field", "开镜景深", "Глубина резкости при прицеливании"),
        ["DiffSprintViewShake"] = ("Koşu görüş sarsıntısı", "Sprint view shake", "疾跑视角晃动", "Тряска камеры при беге"),
        ["DiffModelDecals"] = ("Model çıkartmaları", "Model decals", "模型贴花", "Декали на моделях"),
        ["DiffWaterRendering"] = ("Su render kalitesi", "Water rendering", "水面渲染质量", "Качество рендеринга воды"),
        ["DiffDefault"] = ("(varsayılan)", "(default)", "（默认）", "(по умолчанию)"),
        ["DiffMissing"] = ("(eksik)", "(missing)", "（缺失）", "(отсутствует)"),

        // Ana pencere durum mesajları
        ["StatusDetectionError"] = ("Tespit hatası: {0}", "Detection error: {0}", "检测出错：{0}", "Ошибка обнаружения: {0}"),
        ["StatusConfigNotFound"] = ("Apex videoconfig dosyası bulunamadı.", "Apex videoconfig could not be found.", "未找到 Apex 的 videoconfig 文件。", "Не удалось найти файл videoconfig игры Apex."),
        ["StatusInstallDetected"] = ("{0} kurulumu tespit edildi.", "{0} installation detected.", "已检测到 {0} 安装。", "Обнаружена установка {0}."),
        ["StatusConfigLoaded"] = ("Apex config yüklendi.", "Apex config loaded.", "Apex 配置已加载。", "Конфигурация Apex загружена."),
        ["StatusCouldNotReadResolution"] = ("Apex çözünürlüğü okunamadı.", "Could not read Apex resolution.", "无法读取 Apex 的分辨率。", "Не удалось прочитать разрешение Apex."),
        ["StatusApexConfigNotLoaded"] = ("Apex config yüklenmedi.", "Apex config is not loaded.", "Apex 配置尚未加载。", "Конфигурация Apex не загружена."),
        ["StatusApexConfigNotFound"] = ("Apex config bulunamadı.", "Apex config not found.", "未找到 Apex 配置。", "Конфигурация Apex не найдена."),
        ["StatusCloseApexBeforeApply"] = ("Değişiklikleri uygulamadan önce Apex'i kapatın.", "Close Apex before applying changes.", "应用更改前请先关闭 Apex。", "Закройте Apex перед применением изменений."),
        ["StatusInvalidDisplayConfig"] = ("Geçersiz ekran yapılandırması.", "Invalid display configuration.", "显示配置无效。", "Недопустимая конфигурация экрана."),
        ["StatusNoChangesPending"] = ("Bekleyen değişiklik yok.", "No changes are pending.", "没有待处理的更改。", "Нет ожидающих изменений."),
        ["StatusApplyCancelled"] = ("Uygulama iptal edildi.", "Apply cancelled.", "已取消应用。", "Применение отменено."),
        ["StatusVideoAndAutoexecApplied"] = ("Video config ve autoexec uygulandı.", "Video config and autoexec applied.", "视频配置与 autoexec 均已应用。", "Видеоконфиг и autoexec применены."),
        ["StatusVideoApplied"] = ("Video config uygulandı.", "Video config applied.", "视频配置已应用。", "Видеоконфиг применён."),
        ["StatusAutoexecApplied"] = ("Autoexec uygulandı.", "Autoexec applied.", "Autoexec 已应用。", "Autoexec применён."),
        ["StatusLaunchError"] = ("Başlatma hatası: {0}", "Launch error: {0}", "启动出错：{0}", "Ошибка запуска: {0}"),
        ["StatusLaunching"] = ("Apex başlatılıyor...", "Launching Apex...", "正在启动 Apex……", "Запуск Apex…"),
        ["StatusInstallationUnavailable"] = ("Apex kurulumu kullanılamıyor.", "Apex installation is unavailable.", "Apex 安装不可用。", "Установка Apex недоступна."),
        ["StatusBackupCreated"] = ("Yedek oluşturuldu.", "Backup created.", "备份已创建。", "Резервная копия создана."),
        ["StatusBackupError"] = ("Yedekleme hatası: {0}", "Backup error: {0}", "备份出错：{0}", "Ошибка резервного копирования: {0}"),
        ["StatusNoBackupFound"] = ("Yedek bulunamadı.", "No backup found.", "未找到备份。", "Резервная копия не найдена."),
        ["StatusLatestRestored"] = ("En son video config geri yüklendi.", "Latest video config restored.", "已恢复最新的视频配置。", "Восстановлена последняя резервная копия видеоконфига."),
        ["StatusRestoreError"] = ("Geri yükleme hatası: {0}", "Restore error: {0}", "恢复出错：{0}", "Ошибка восстановления: {0}"),
        ["StatusCloseApexBeforeRestore"] = ("Geri yüklemeden önce Apex'i kapatın.", "Close Apex before restoring.", "恢复前请先关闭 Apex。", "Закройте Apex перед восстановлением."),
        ["StatusProfileSaved"] = ("\"{0}\" profili kaydedildi.", "Profile \"{0}\" saved.", "配置文件“{0}”已保存。", "Профиль «{0}» сохранён."),
        ["StatusProfileSaveError"] = ("Profil kaydetme hatası: {0}", "Profile save error: {0}", "保存配置文件出错：{0}", "Ошибка сохранения профиля: {0}"),
        ["StatusProfileLoaded"] = ("\"{0}\" profili yüklendi.", "Profile \"{0}\" loaded.", "配置文件“{0}”已加载。", "Профиль «{0}» загружен."),
        ["StatusProfileLoadError"] = ("Profil yükleme hatası: {0}", "Profile load error: {0}", "加载配置文件出错：{0}", "Ошибка загрузки профиля: {0}"),
        ["StatusCloseApexBeforeLoadProfile"] = ("Profil yüklemeden önce Apex'i kapatın.", "Close Apex before loading a profile.", "加载配置文件前请先关闭 Apex。", "Закройте Apex перед загрузкой профиля."),
        ["StatusFpsRange"] = ("FPS limiti 30 ile 1000 arasında olmalıdır.", "FPS limit must be between 30 and 1000.", "帧率上限必须介于 30 到 1000 之间。", "Лимит FPS должен быть от 30 до 1000."),
        ["StatusLobbyFpsRange"] = ("Lobi FPS limiti 20 ile 300 arasında olmalıdır.", "Lobby FPS limit must be between 20 and 300.", "大厅帧率上限必须介于 20 到 300 之间。", "Лимит FPS в лобби должен быть от 20 до 300."),
        ["StatusCouldNotOpenFolder"] = ("Klasör açılamadı: {0}", "Could not open folder: {0}", "无法打开文件夹：{0}", "Не удалось открыть папку: {0}"),

        // Autoexec görünümü durum mesajları
        ["AutoStatusNotDetected"] = ("Apex kurulumu tespit edilemedi.", "Apex installation was not detected.", "未检测到 Apex 安装。", "Установка Apex не обнаружена."),
        ["AutoStatusCloseApexBeforeEdit"] = ("Autoexec'i düzenlemeden önce Apex'i kapatın.", "Close Apex before editing autoexec.", "编辑 autoexec 前请先关闭 Apex。", "Закройте Apex перед редактированием autoexec."),
        ["AutoStatusSaved"] = ("Autoexec kaydedildi.", "Autoexec saved.", "Autoexec 已保存。", "Autoexec сохранён."),
        ["AutoStatusSaveError"] = ("Autoexec kaydetme hatası: {0}", "Autoexec save error: {0}", "保存 autoexec 出错：{0}", "Ошибка сохранения autoexec: {0}"),
        ["AutoStatusReloaded"] = ("Autoexec yeniden yüklendi.", "Autoexec reloaded.", "Autoexec 已重新加载。", "Autoexec перезагружен."),
        ["AutoStatusUnavailable"] = ("Autoexec kullanılamıyor.", "Autoexec is unavailable.", "Autoexec 不可用。", "Autoexec недоступен."),
        ["AutoStatusNoFileYet"] = ("Henüz bir autoexec dosyası yok.", "No autoexec file exists yet.", "尚不存在 autoexec 文件。", "Файл autoexec ещё не существует."),
        ["AutoStatusBackupCreated"] = ("Autoexec yedeği oluşturuldu.", "Autoexec backup created.", "Autoexec 备份已创建。", "Резервная копия autoexec создана."),
        ["AutoStatusBackupError"] = ("Autoexec yedekleme hatası: {0}", "Autoexec backup error: {0}", "备份 autoexec 出错：{0}", "Ошибка резервного копирования autoexec: {0}"),
        ["AutoStatusLaunchOptionsCopied"] = ("Başlatma seçenekleri kopyalandı.", "Launch options copied.", "启动选项已复制。", "Параметры запуска скопированы."),

        // Yedekler görünümü durum mesajları
        ["BackupsStatusCloseApex"] = ("Yedek geri yüklemeden önce Apex Legends'ı kapatın.", "Close Apex Legends before restoring a backup.", "恢复备份前请先关闭 Apex Legends。", "Закройте Apex Legends перед восстановлением резервной копии."),
        ["BackupsStatusVideoUnavailable"] = ("Video config yedekleme servisi kullanılamıyor.", "VideoConfig backup service unavailable.", "视频配置备份服务不可用。", "Служба резервного копирования видеоконфига недоступна."),
        ["BackupsStatusSelectVideo"] = ("Önce bir videoconfig yedeği seçin.", "Select a videoconfig backup first.", "请先选择一个视频配置备份。", "Сначала выберите резервную копию видеоконфига."),
        ["BackupsStatusVideoRestored"] = ("Seçili videoconfig yedeği geri yüklendi.", "Selected videoconfig backup restored.", "所选视频配置备份已恢复。", "Выбранная резервная копия видеоконфига восстановлена."),
        ["BackupsStatusVideoDeleted"] = ("Videoconfig yedeği silindi.", "Videoconfig backup deleted.", "视频配置备份已删除。", "Резервная копия видеоконфига удалена."),
        ["BackupsStatusAutoexecUnavailable"] = ("Autoexec yedekleme servisi kullanılamıyor.", "Autoexec backup service unavailable.", "Autoexec 备份服务不可用。", "Служба резервного копирования autoexec недоступна."),
        ["BackupsStatusSelectAutoexec"] = ("Önce bir autoexec yedeği seçin.", "Select an autoexec backup first.", "请先选择一个 autoexec 备份。", "Сначала выберите резервную копию autoexec."),
        ["BackupsStatusAutoexecRestored"] = ("Seçili autoexec yedeği geri yüklendi.", "Selected autoexec backup restored.", "所选 autoexec 备份已恢复。", "Выбранная резервная копия autoexec восстановлена."),
        ["BackupsStatusAutoexecDeleted"] = ("Autoexec yedeği silindi.", "Autoexec backup deleted.", "Autoexec 备份已删除。", "Резервная копия autoexec удалена."),
        ["BackupsStatusDeleteError"] = ("Silme hatası: {0}", "Delete error: {0}", "删除出错：{0}", "Ошибка удаления: {0}"),
        ["BackupsStatusListRefreshed"] = ("Yedek listesi yenilendi.", "Backup list refreshed.", "备份列表已刷新。", "Список резервных копий обновлён."),
        ["BackupsConfirmRestore"] = ("Bu yedeği geri yüklemek istiyor musunuz?", "Restore this backup?", "确定要恢复此备份吗？", "Восстановить эту резервную копию?"),
        ["BackupsConfirmDelete"] = ("Bu yedeği kalıcı olarak silmek istiyor musunuz?", "Delete this backup permanently?", "确定要永久删除此备份吗？", "Удалить эту резервную копию без возможности восстановления?"),
        ["BackupsRestoreTitle"] = ("Yedeği geri yükle", "Restore Backup", "恢复备份", "Восстановление резервной копии"),
        ["BackupsDeleteTitle"] = ("Yedeği sil", "Delete Backup", "删除备份", "Удаление резервной копии"),

        // Profiller görünümü durum mesajları
        ["ProfStatusEnterName"] = ("Önce bir profil adı girin.", "Enter a profile name first.", "请先输入配置文件名称。", "Сначала введите имя профиля."),
        ["ProfStatusSelect"] = ("Önce bir profil seçin.", "Select a profile first.", "请先选择一个配置文件。", "Сначала выберите профиль."),
        ["ProfConfirmLoad"] = ("\"{0}\" profilini yüklemek istiyor musunuz?\n\nMevcut yapılandırmanız önce yedeklenecek.", "Load profile \"{0}\"?\n\nYour current config will be backed up first.", "要加载配置文件“{0}”吗？\n\n您当前的配置将会先被备份。", "Загрузить профиль «{0}»?\n\nТекущая конфигурация будет сначала сохранена в резервную копию."),
        ["ProfConfirmDelete"] = ("\"{0}\" profilini silmek istiyor musunuz?", "Delete profile \"{0}\"?", "确定要删除配置文件“{0}”吗？", "Удалить профиль «{0}»?"),
        ["ProfLoadTitle"] = ("Profili yükle", "Load Profile", "加载配置文件", "Загрузка профиля"),
        ["ProfDeleteTitle"] = ("Profili sil", "Delete Profile", "删除配置文件", "Удаление профиля"),
        ["ProfStatusDeleted"] = ("Profil silindi.", "Profile deleted.", "配置文件已删除。", "Профиль удалён."),
        ["ProfStatusDeleteError"] = ("Profil silme hatası: {0}", "Profile delete error: {0}", "删除配置文件出错：{0}", "Ошибка удаления профиля: {0}"),
        ["ProfStatusRefreshed"] = ("Profiller yenilendi.", "Profiles refreshed.", "配置文件列表已刷新。", "Список профилей обновлён."),

        // Gelişmiş görünüm durum mesajları
        ["AdvStatusRefreshed"] = ("Gelişmiş ayar inceleyicisi yenilendi.", "Advanced config inspector refreshed.", "高级设置检查器已刷新。", "Инспектор дополнительных настроек обновлён."),
        ["AdvStatusReloadedFromDisk"] = ("videoconfig.txt diskten yeniden yüklendi.", "videoconfig.txt reloaded from disk.", "videoconfig.txt 已从磁盘重新加载。", "videoconfig.txt перезагружен с диска."),
        ["AdvStatusConfigNotLoaded"] = ("Video config yüklenmedi.", "Video config is not loaded.", "视频配置尚未加载。", "Видеоконфиг не загружен."),
        ["AdvStatusCloseApexBeforeSave"] = ("Ham config kaydetmeden önce Apex'i kapatın.", "Close Apex before saving raw config.", "保存原始配置前请先关闭 Apex。", "Закройте Apex перед сохранением исходного файла конфигурации."),
        ["AdvConfirmSaveRaw"] = ("Tüm videoconfig.txt dosyası kaydedilsin mi?\n\nÖnce bir yedek alınacak.", "Save the complete raw videoconfig.txt?\n\nA backup will be created first.", "要保存整个 videoconfig.txt 文件吗？\n\n系统会先创建一个备份。", "Сохранить весь файл videoconfig.txt целиком?\n\nСначала будет создана резервная копия."),
        ["AdvSaveRawTitle"] = ("Ham config kaydet", "Save Raw Config", "保存原始配置", "Сохранение исходного конфига"),
        ["AdvStatusSavedSuccessfully"] = ("Ham videoconfig.txt başarıyla kaydedildi.", "Raw videoconfig.txt saved successfully.", "原始 videoconfig.txt 已成功保存。", "Исходный файл videoconfig.txt успешно сохранён."),
        ["AdvStatusSaveError"] = ("Ham config kaydetme hatası: {0}", "Raw config save error: {0}", "保存原始配置出错：{0}", "Ошибка сохранения исходного конфига: {0}"),
    };
}
