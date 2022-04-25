using UnityEngine;

namespace PBToggleApplier
{
    public static class PathUtil
    {
        /**
         * gameObject パスを取得するターゲット
         * parentObject 取得するヒエラルキーの基点
         */
        public static string GetHierarchyPath(GameObject gameObject, GameObject parentObject)
        {
            string path = "";
            Transform transform = gameObject.transform;
            while (
                transform.parent != null && transform.gameObject != parentObject)
            {
                path = transform.gameObject.name + "/" + path;
                transform = transform.parent;
            }
            return path;
        }

        public static string DropLastSlash(string path)
        {
            if (path.EndsWith("/")) { return path.Substring(0, path.Length - 1); }
            return path;
        }
    }
}
