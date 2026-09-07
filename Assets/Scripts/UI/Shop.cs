using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// 商城界面：按分类(皮肤/道具/能力)列出商品，金币够了就能买，买过的皮肤/能力能装备。
// 商品内容全部来自 ShopCatalog.asset（可在 Tuanjie 界面里编辑/新增，无需改代码）。
public class Shop : MonoBehaviour
{
    ShopCatalog catalog;
    GUIStyle _name, _desc, _price, _type, _tag;
    bool built;

    void Ensure()
    {
        if (built) return;
        built = true;
        _name = new GUIStyle(GUI.skin.label);
        _name.fontSize = 15; _name.fontStyle = FontStyle.Bold; _name.normal.textColor = Color.white; _name.alignment = TextAnchor.MiddleLeft;

        _desc = new GUIStyle(GUI.skin.label);
        _desc.fontSize = 12; _desc.normal.textColor = new Color(0.62f, 0.66f, 0.74f, 1f); _desc.alignment = TextAnchor.MiddleLeft;

        _price = new GUIStyle(GUI.skin.label);
        _price.fontSize = 14; _price.fontStyle = FontStyle.Bold; _price.normal.textColor = new Color(1f, 0.82f, 0.35f, 1f); _price.alignment = TextAnchor.MiddleCenter;

        _type = new GUIStyle(GUI.skin.label);
        _type.fontSize = 13; _type.fontStyle = FontStyle.Bold; _type.normal.textColor = new Color(0.45f, 0.9f, 0.8f, 1f); _type.alignment = TextAnchor.MiddleLeft;

        _tag = new GUIStyle(GUI.skin.label);
        _tag.fontSize = 12; _tag.normal.textColor = new Color(0.5f, 0.9f, 0.7f, 1f); _tag.alignment = TextAnchor.MiddleCenter;
    }

    void OnGUI()
    {
        Ensure();
        if (catalog == null) catalog = Resources.Load<ShopCatalog>("Data/ShopCatalog");

        if (catalog == null)
        {
            UiHelper.Dim(150);
            UiHelper.Centered(() =>
            {
                UiHelper.Title("商 城");
                UiHelper.Sub("缺少商城目录资源（ShopCatalog.asset）");
                UiHelper.Divider();
                if (UiHelper.Button("返回主菜单")) SceneManager.LoadScene("MainMenu");
            }, width: 360f, maxHeight: Mathf.Min(Screen.height * 0.7f, 560f));
            return;
        }

        var sd = SaveManager.Load();
        UiHelper.Dim(150);

        UiHelper.Centered(() =>
        {
            UiHelper.Title("商 城");
            UiHelper.Space(2);
            UiHelper.Divider();
            UiHelper.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            UiHelper.Money("金币   " + sd.coins);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            UiHelper.Space(6);
            RenderCatalog(sd);

            UiHelper.Space(10);
            UiHelper.Divider();
            UiHelper.Space(10);
            if (UiHelper.Button("返回主菜单")) SceneManager.LoadScene("MainMenu");
        }, width: 470f, maxHeight: Mathf.Min(Screen.height * 0.86f, 800f));
    }

    void RenderCatalog(SaveData sd)
    {
        if (catalog.items == null || catalog.items.Count == 0)
        {
            UiHelper.Hint("商城目录还没有商品，请去 ShopCatalog 里添加。");
            return;
        }

        for (int i = 0; i < (int)ShopItemType.Ability + 1; i++)
        {
            var type = (ShopItemType)i;
            if (!catalog.items.Exists(x => x.type == type)) continue;

            GUILayout.Label(TypeName(type), _type);
            GUILayout.Space(4);
            foreach (var it in catalog.items)
                if (it.type == type) RenderItem(sd, it);
            GUILayout.Space(12);
        }
    }

    void RenderItem(SaveData sd, ShopItem it)
    {
        bool owned = sd.ownedItems.Contains(it.id);

        GUILayout.BeginHorizontal();
        {
            // 图标（可选）
            if (it.icon != null)
            {
                GUILayout.Box(it.icon.texture, GUILayout.Width(44), GUILayout.Height(44));
                GUILayout.Space(8);
            }

            // 名称 + 描述
            GUILayout.BeginVertical(GUILayout.Width(260));
            GUILayout.Label(it.displayName, _name);
            GUILayout.Label(it.description, _desc);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 价格
            GUILayout.Label(it.price + "", _price, GUILayout.Width(48));

            // 状态按钮
            if (owned)
            {
                if (it.type == ShopItemType.Skin)
                {
                    bool isEquipped = sd.equippedSkin == it.id;
                    if (isEquipped)
                        GUILayout.Label("已装备", _tag, GUILayout.Width(64));
                    else if (UiHelper.SmallButton("装备", 64)) EquipSkin(sd, it);
                }
                else // Ability：买了即解锁，被动生效，无需装备
                {
                    GUILayout.Label("已解锁", _tag, GUILayout.Width(64));
                }
            }
            else
            {
                GUI.enabled = sd.coins >= it.price;
                if (UiHelper.SmallButton("购买", 64)) Buy(it);
                GUI.enabled = true;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    void Buy(ShopItem it)
    {
        var sd = SaveManager.Load();
        if (sd.ownedItems.Contains(it.id) || sd.coins < it.price) return;
        sd.coins -= it.price;
        sd.ownedItems.Add(it.id);
        SaveManager.Save(sd);
    }

    void EquipSkin(SaveData sd, ShopItem it)
    {
        if (it.type == ShopItemType.Skin) sd.equippedSkin = it.id;
        SaveManager.Save(sd);
    }

    string TypeName(ShopItemType t)
    {
        switch (t)
        {
            case ShopItemType.Skin: return "· 皮肤 ·";
            case ShopItemType.Ability: return "· 能力 ·";
        }
        return "";
    }
}
