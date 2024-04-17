using HarmonyLib;
using UnityEngine;

namespace SmolMod.Patches;

// adapted from https://github.com/Among-Us-Modding/Laboratory/blob/main/Laboratory/Utilities/CameraZoomController.cs
[HarmonyPatch(typeof(HudManager),nameof(HudManager.Start))]
public static class HudStartPatch
{
    private static ShadowCollab _shadowCollab;
    private static Camera _cam;
    private static float _defaultOrthographicSize;

    public static void Postfix(HudManager __instance)
    {
        _shadowCollab = Object.FindObjectOfType<ShadowCollab>();
        _shadowCollab.StopAllCoroutines();

        HudManager.Instance.FullScreen.transform.localScale *= 50;
        
        var mainCam = Camera.main!;

        GameObject newCamObj = new("ZoomCamera");
        var newCamTransform = newCamObj.transform;
        newCamTransform.parent = mainCam.transform;
        newCamTransform.localPosition = new Vector3(0, 0, 0);
        newCamTransform.localScale = new Vector3(1, 1, 1);
        
        _cam = newCamObj.AddComponent<Camera>();
        _cam.CopyFrom(mainCam);
        _cam.depth += 1;
        mainCam.ResetReplacementShader();

        _defaultOrthographicSize = _cam.orthographicSize;
        
        UpdateSize();
    }

    public static void UpdateSize()
    {
        if (!_shadowCollab || !_cam)
        {
            return;
        }
        
        _shadowCollab.ShadowCamera.aspect = _cam.aspect;
        var val = _shadowCollab.ShadowCamera.orthographicSize = _cam.orthographicSize = _defaultOrthographicSize / SmolModPlugin.ScaleMod;
        _shadowCollab.ShadowQuad.transform.localScale = new Vector3(val * _cam.aspect, val) * 2f;
    }
}