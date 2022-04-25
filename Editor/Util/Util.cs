using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PBToggleApplier
{
    public static class Util
    {
        public static List<SkinnedMeshRenderer> ListSkinnedMeshRenderersInChildren(List<GameObject> gameObjects)
        {
            List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
            foreach (var gameObject in gameObjects)
            {
                var array = gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                list.AddRange(array);
            }
            return list;
        }

        public static List<GameObject> ListBoneFromSkinnedMeshRenderers(List<SkinnedMeshRenderer> list)
        {
            List<GameObject> boneList = new List<GameObject>();
            foreach (var skinnedMeshRenderer in list)
            {
                if (skinnedMeshRenderer.bones == null || skinnedMeshRenderer.bones.Length == 0) continue;
                boneList.AddRange(skinnedMeshRenderer.bones.Select(e => e.gameObject));
            }
            return boneList.Distinct().ToList();
        }

        public static List<T> ListComponentsFromGameObjectList<T>(List<GameObject> list)
        {
            List<T> componentList = new List<T>();
            foreach (GameObject obj in list)
            {
                T[] components = obj.GetComponentsInChildren<T>(true);
                componentList.AddRange(components);
            }
            return componentList.Distinct().ToList();
        }
    }
}
