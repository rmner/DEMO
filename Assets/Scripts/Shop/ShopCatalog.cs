using UnityEngine;
using System.Collections.Generic;

// 商城目录：所有商品都放在这个资源里。
// 运行时从 Resources 加载；你在编辑器里编辑它（增删商品、改名字/描述/价格/图标）即刻生效。
// 添加新商品 = 在 Inspector 里给 items 列表点“+”号，不需要写代码。
[CreateAssetMenu(menuName = "跃境/商城目录", fileName = "ShopCatalog")]
public class ShopCatalog : ScriptableObject
{
    public List<ShopItem> items = new List<ShopItem>();
}
