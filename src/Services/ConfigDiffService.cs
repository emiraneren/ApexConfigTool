using System.Collections.Generic;
using System.Linq;
using ApexConfigTool.Models;

namespace ApexConfigTool.Services;

/// <summary>
/// Eski ve yeni videoconfig.txt içeriği arasındaki farkları listeler.
/// </summary>
public class ConfigDiffService
{
    public List<ConfigChange> Compare(VideoConfigService service, string oldConfig, string newConfig)
    {
        var oldSettings = service.GetAllSettings(oldConfig)
            .GroupBy(x => x.Setting)
            .ToDictionary(x => x.Key, x => x.Last().Value);

        var newSettings = service.GetAllSettings(newConfig)
            .GroupBy(x => x.Setting)
            .ToDictionary(x => x.Key, x => x.Last().Value);

        var keys = new HashSet<string>(oldSettings.Keys);
        keys.UnionWith(newSettings.Keys);

        var changes = new List<ConfigChange>();
        var loc = LocalizationService.Instance;

        foreach (string key in keys)
        {
            string oldValue = oldSettings.TryGetValue(key, out string? oldResult) ? oldResult : loc.T("DiffMissing");
            string newValue = newSettings.TryGetValue(key, out string? newResult) ? newResult : loc.T("DiffMissing");

            if (oldValue == newValue)
                continue;

            changes.Add(new ConfigChange
            {
                Source = "Video",
                Setting = GetFriendlyName(key),
                OldValue = oldValue,
                NewValue = newValue
            });
        }

        return changes.OrderBy(x => x.Setting).ToList();
    }

    private string GetFriendlyName(string key)
    {
        var loc = LocalizationService.Instance;

        return key switch
        {
            "setting.defaultres" => loc.T("DiffResolutionWidth"),
            "setting.defaultresheight" => loc.T("DiffResolutionHeight"),
            "setting.fullscreen" => loc.T("DiffFullscreen"),
            "setting.mat_vsync_mode" => loc.T("DiffVSync"),
            "setting.mat_antialias_mode" => loc.T("DiffAntiAliasing"),
            "setting.mat_forceaniso" => loc.T("DiffTextureFiltering"),
            "setting.mat_picmip" => loc.T("DiffTextureResolution"),
            "setting.stream_memory" => loc.T("DiffStreamingBudget"),
            "setting.r_lod_switch_scale" => loc.T("DiffModelDetail"),
            "setting.particle_cpu_level" => loc.T("DiffParticleQuality"),
            "setting.cl_ragdoll_maxcount" => loc.T("DiffRagdolls"),
            "setting.r_decals" => loc.T("DiffWorldDecals"),
            "setting.volumetric_lighting" => loc.T("DiffVolumetricLighting"),
            "setting.volumetric_fog" => loc.T("DiffVolumetricFog"),
            "setting.dvs_enable" => loc.T("DiffDynamicResolution"),
            "setting.dynamic_streaming_budget" => loc.T("DiffDynamicStreaming"),
            "setting.ssao_quality" => loc.T("DiffAmbientOcclusion"),
            "setting.ssao_enabled" => loc.T("DiffAmbientOcclusion"),
            "setting.map_detail_level" => loc.T("DiffMapDetail"),
            "setting.shadow_enable" => loc.T("DiffShadows"),
            "setting.shadow_maxdynamic" => loc.T("DiffDynamicShadows"),
            "setting.csm_enabled" => loc.T("DiffCascadeShadows"),
            "setting.csm_coverage" => loc.T("DiffCsmCoverage"),
            "setting.csm_cascade_res" => loc.T("DiffCsmResolution"),
            "setting.new_shadow_settings" => loc.T("DiffShadowSystem"),
            _ => key
        };
    }
}
