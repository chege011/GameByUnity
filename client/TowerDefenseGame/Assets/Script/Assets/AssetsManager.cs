using System.Collections;
using System.Collections.Generic;
using GameBox.Framework;
using GameBox.Service.AssetManager;
using UnityEngine;

namespace Assets{
    public static class AssetsManager
    {
        private static IAssetManager assetManager;

        public static void Init()
        {
            new ServiceTask<IAssetManager>()
                .Start()
                .Continue(task =>
                {
                    assetManager = task.Result as IAssetManager;
                    return null;
                });
        }

        public static GameObject LoadAsset(string path, bool state)
        {
            var asset = assetManager.Load(path, AssetType.Prefab);
            if (asset != null)
            {
                var obj = GameObject.Instantiate(asset.Cast<GameObject>());
                obj.SetActive(state);
                return obj;
            }

            throw new MissingComponentException(path + "not find");
        }

        public static Sprite LoadImage(string path)
        {
            return Resources.Load(path, typeof(Sprite)) as Sprite;
        }

        public static bool IsAssetManagerOk()
        {
            return assetManager != null;
        }
    }
}

