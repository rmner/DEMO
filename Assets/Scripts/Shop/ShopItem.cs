using UnityEngine;

// 商城商品类型：现在是 皮肤 + 能力（去掉了武器/道具栏）
public enum ShopItemType
{
    Skin,     // 皮肤
    Ability,  // 能力
}

// 单个商品。你在 Tuanjie 里双击 ShopCatalog.asset，就能像填表格一样改这些字段，
// 并且点“+”号加新商品，完全不用写代码。
[System.Serializable]
public class ShopItem
{
    public string id;                 // 编号（皮肤如 SK001；能力如 AB_DASH / AB_DBJUMP / AB_WALL）
    public ShopItemType type;
    public string displayName = "未命名";
    [TextArea] public string description = "待定";
    public int price = 100;           // 价格（金币）
    public Sprite icon;               // 可选：图标
    public Color tint = Color.white;  // 皮肤：玩家颜色（能力可忽略）
}
