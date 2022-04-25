using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using System.Collections.Generic;
using System.IO;
using System;

namespace PBToggleApplier
{
    public static class AnimationClipUtil
    {
        /**
         * AnimationClip を受け取り、操作している GameObject のリストと最終フレームの時刻を返す
         */
        public static (float latestKeyframeTime, List<(GameObject GameObject, float Value)> keys) ListGameObjectFromAnimation(AnimationClip clip, GameObject rootObject)
        {
            List<(GameObject GameObject, float Value)> keys = new List<(GameObject GameObject, float Value)>();
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
            // EditorCurveBinding[] referenceBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            float latestKeyframeTime = 0;
            foreach (var binding in curveBindings)
            {
                if (binding.type == typeof(GameObject))
                {
                    var target = rootObject.transform.Find(binding.path);
                    if (target != null)
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                        if (curve.keys.Length == 0) continue;

                        Keyframe lastKeyFrame = curve.keys[curve.length - 1];
                        keys.Add((target.gameObject, lastKeyFrame.value));
                        latestKeyframeTime = Math.Max(latestKeyframeTime, lastKeyFrame.time);
                        Debug.Log(lastKeyFrame.value + ", " + binding.path);
                    } else {
                        Debug.Log(binding.path + " is no there.");
                    }
                }
            }
            return (latestKeyframeTime, keys);
        }
    }
}
