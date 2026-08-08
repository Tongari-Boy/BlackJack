using Item;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    /// <summary>
    /// <para>UI関連のユーティリティー</para>
    /// </summary>
    public class UIUtil
    {
        /// <summary>
        /// <para>GameObjectのImageコンポーネントにItemDataを表すImageを設定する（GameManager.GetItemImageHolderより取得可能なImage）</para>
        /// <para>設定できた場合はそのImageを含むItemImageHolder、できなかった場合はItemImageHolder.EMPTYを返す</para>
        /// </summary>
        public static ItemImageHolder ApplyItemDataToImage(GameObject gameObject, ItemData itemData)
        {
            return UIUtil.InvokeIfPresent<Image, ItemImageHolder>(gameObject, image =>
            {
                if (itemData != null)
                {
                    ItemImageHolder itemImageHolder = GameManager.INSTANCE.GetItemImageHolder(itemData.Name, itemData.Rarity);

                    image.sprite = itemImageHolder.ItemImage != null ? itemImageHolder.ItemImage.sprite : null;
                    image.color = itemImageHolder.ItemImage != null && itemData.Count > 0 ? itemImageHolder.ItemImage.color : new(1.0F, 1.0F, 1.0F, 0.0F);

                    return itemImageHolder;
                }

                return ItemImageHolder.EMPTY;
            }) ?? ItemImageHolder.EMPTY;
        }

        /// <summary>
        /// <para>GameObjectのSpriteRendererコンポーネントにItemDataを表すImageを設定する（GameManager.GetItemImageHolderより取得可能なImage）</para>
        /// <para>設定できた場合はそのImageを含むItemImageHolder、できなかった場合はItemImageHolder.EMPTYを返す</para>
        /// </summary>
        public static ItemImageHolder ApplyItemDataToSpriteRenderer(GameObject gameObject, ItemData itemData)
        {
            return UIUtil.InvokeIfPresent<SpriteRenderer, ItemImageHolder>(gameObject, spriteRenderer =>
            {
                if (itemData != null)
                {
                    ItemImageHolder itemImageHolder = GameManager.INSTANCE.GetItemImageHolder(itemData.Name, itemData.Rarity);

                    spriteRenderer.sprite = itemImageHolder.ItemImage != null ? itemImageHolder.ItemImage.sprite : null;
                    spriteRenderer.color = itemImageHolder.ItemImage != null && itemData.Count > 0 ? itemImageHolder.ItemImage.color : new(1.0F, 1.0F, 1.0F, 0.0F);

                    return itemImageHolder;
                }

                return ItemImageHolder.EMPTY;
            }) ?? ItemImageHolder.EMPTY;
        }

        /// <summary>
        /// <para>GameObjectに指定されたコンポーネントが存在する場合はActionを実行する</para>
        /// <para>実行できた場合はtrue、できなかった場合はfalseを返す</para>
        /// </summary>
        public static bool InvokeIfPresent<T>(GameObject gameObject, Action<T> action) where T : Component
        {
            if (gameObject != null && action != null)
            {
                T component = gameObject.GetComponent<T>();

                if (component != null)
                {
                    action.Invoke(component);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <para>GameObjectに指定されたコンポーネントが存在する場合はFuncを実行する</para>
        /// <para>実行できた場合はFuncの返り値、できなかった場合はdefault(R)を返す</para>
        /// </summary>
        public static R InvokeIfPresent<T, R>(GameObject gameObject, Func<T, R> func) where T : Component
        {
            if (gameObject != null && func != null)
            {
                T component = gameObject.GetComponent<T>();

                if (component != null)
                {
                    return func.Invoke(component);
                }
            }

            return default;
        }

        /// <summary>
        /// <para>子GameObjectから、指定されたnameの親GameObjectを返す</para>
        /// <para>存在しない場合はnullを返す</para>
        /// </summary>
        public static GameObject GetParent(GameObject gameObject, string name, int minParentLevel = 0, int maxParentLevel = int.MaxValue)
        {
            if (gameObject == null || name == null)
                return null;

            Transform parentTransform = gameObject.transform.parent;
            int parentLevel = 1;

            while (parentTransform != null)
            {
                if (parentTransform.gameObject.name == name && minParentLevel <= parentLevel && parentLevel <= maxParentLevel)
                    return parentTransform.gameObject;

                parentTransform = parentTransform.parent;
                ++parentLevel;
            }

            UnityEngine.Debug.LogWarning($"GameObject（Name: {gameObject.name}）に親GameObject（Name: {name}）は存在しません…");

            return null;
        }

        /// <summary>
        /// <para>親GameObjectから、指定されたpathに存在する子GameObjectを返す</para>
        /// <para>存在しない場合はnullを返す</para>
        /// </summary>
        public static GameObject GetChild(GameObject parentObject, string path, char split = '/')
        {
            if (parentObject == null)
                return null;

            string[] names = path.Split(split);
            int targetIndex = names.Length >= 1 && names[0] == "." ? 1 : 0;

            Transform targetParent = parentObject.transform;
            Transform targetChild;
            int childCount;

            Scan:
            childCount = targetParent.transform.childCount;

            if (childCount > 0)
            {
                for (int i = 0; i < childCount; ++i)
                {
                    targetChild = targetParent.transform.GetChild(i);

                    if (targetChild != null && targetChild.gameObject != null && targetChild.gameObject.name == names[targetIndex])
                    {
                        ++targetIndex;

                        if (targetIndex < names.Length)
                        {
                            // さらに下の階層を探す
                            targetParent = targetChild;
                            targetChild = null;

                            goto Scan;
                        }
                        else
                        {
                            // 子GameObjectを返す
                            return targetChild.gameObject;
                        }
                    }
                }
            }

            UnityEngine.Debug.LogWarning($"GameObject（Name: {parentObject.name}）に子GameObject（Path: {path}）は存在しません…");

            return null;
        }

        public static void DestoryAll(params GameObject[] gameObjects)
        {
            GameObject gameObject;
            int length = gameObjects.Length;

            for (int i = 0; i < length; ++i)
            {
                gameObject = gameObjects[i];

                if (gameObject != null)
                {
                    UnityEngine.Object.Destroy(gameObject);

                    gameObjects[i] = null;
                }
            }
        }

        public static void DestoryAll(List<GameObject> gameObjects)
        {
            if (gameObjects == null)
                return;

            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }

            gameObjects.Clear();
        }
    }
}