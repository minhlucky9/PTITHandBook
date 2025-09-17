using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Functional/Shop Data")]
public class ShopWindowDataSO : FunctionalWindowDataSO
{
    public List<ShopCategory> shopCategories;

    public override void Init(GameObject target)
    {
        base.Init(target);
        ShopWindowManager.instance.OpenWindow(this);
    }
}

[Serializable]
public class ShopCategory
{
    public string categoryName;
    public string categoryDescription;
    public Sprite iconInShop;
    public List<ShopItem> shopItems;
}

[Serializable]
public struct ShopItem
{
    public ItemSO itemInfo;
    public int price;
   // public string value;
    public Sprite valueIcon;

    public bool TryBuyItem()
    {
        return PlayerInventory.instance.TryBuyItem(price, itemInfo);
    }
}