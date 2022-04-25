using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using VRC.SDK3.Dynamics.PhysBone.Components;
using System;

namespace PBToggleApplier
{
    public static class Applier
    {
        public static void OverwriteAnimationClip<T>(AnimationClip clip, bool isGenerateBackup, GameObject _avatar) where T: Component
        {
            if (isGenerateBackup) _CreateBackupFile(clip);

            (float latestKeyframeTime, List<(GameObject GameObject, float Value)> keys) = AnimationClipUtil.ListGameObjectFromAnimation(clip, _avatar);

            List<GameObject> enableObjectList = keys.Where(o => o.Value == 1).Select(o => o.GameObject).ToList();
            List<GameObject> disableObjectList = keys.Where(o => o.Value == 0).Select(o => o.GameObject).ToList();
            (List<T> enableComponentList, List<T> disableComponentList) = _ListTargetComponent<T>(enableObjectList, disableObjectList);

            var frameTimes = new List<float>{0};
            if (latestKeyframeTime != 0) frameTimes.Add(latestKeyframeTime); // SDK2のAnimation Overrideを流用している場合など、フレームが2つあるケース
            _SetToggleKeyFrames<T>(enableComponentList, disableComponentList, clip, frameTimes, _avatar);
        }

        private static (
            List<T> enableComponentList,
            List<T> disableComponentList
        ) _ListTargetComponent<T>(List<GameObject> enableList, List<GameObject> disableList)
        {
            List<SkinnedMeshRenderer> enableSMR = Util.ListSkinnedMeshRenderersInChildren(enableList);
            List<SkinnedMeshRenderer> disableSMR = Util.ListSkinnedMeshRenderersInChildren(disableList);

            // SkinnedMeshRenderer 階層に置かれる DynamicBone/PhysBone に対応
            var bs1 = Util.ListBoneFromSkinnedMeshRenderers(enableSMR).Union(enableList).ToList();
            var bs2 = Util.ListBoneFromSkinnedMeshRenderers(disableSMR).Union(disableList).ToList();
            // 共通の bone を参照する SkinnedMeshRenderer （素体など）の DynamicBone/PhysBone には干渉しない
            var enableUniqueList = bs1.Except(bs2).ToList();
            var disableUniqueList = bs2.Except(bs1).ToList();
            List<T> enableComponentList =  Util.ListComponentsFromGameObjectList<T>(enableUniqueList);
            List<T> disableComponentList = Util.ListComponentsFromGameObjectList<T>(disableUniqueList);

            return (enableComponentList, disableComponentList);
        }

        private static void _SetToggleKeyFrames<T>(List<T> enableCompoentList, List<T> disableComponentList, AnimationClip clip, List<float> frameTimes, GameObject _avatar) where T: Component
        {
            var editor = new AnimationClipEditor(_avatar, "", clip); // exportしないためsave pathは使用しない
            foreach (T comp in enableCompoentList)
            {
                editor.SetCurveEnableComponent<T>(comp, frameTimes);
            };
            foreach (T comp in disableComponentList)
            {
                editor.SetCurveDisableComponent<T>(comp, frameTimes);
            };
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
        }

        private static void _CreateBackupFile(AnimationClip clip)
        {
            var assetPath = AssetDatabase.GetAssetPath(clip);
            var dirPath = Path.GetDirectoryName(assetPath);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var bkupPath = Path.Combine(dirPath, fileName) + "_bkup.anim";
            AssetDatabase.CopyAsset(assetPath, bkupPath);
        }
    }
}