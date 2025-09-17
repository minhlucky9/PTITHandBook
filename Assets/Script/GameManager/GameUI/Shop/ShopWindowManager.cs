using Interaction;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindowManager : MonoBehaviour
{
    public static ShopWindowManager instance;

    public GameObject categoryPrefab;
    public GameObject productPrefab;
    public Transform itemContainer;
    public Button closeBtn;
    public Scrollbar containerScroll;
    public Button prevBtn;
    public Button nextBtn;

    [Header("Category Selection Buttons")]
    public Button categoryButton1;
    public Button categoryButton2;
    public Button categoryButton3;

    [Header("Category Name Display")]
    public TextMeshProUGUI categoryNameText;
    public TextMeshProUGUI categoryDescriptionText;

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    UIAnimationController openAnimation;
    ShopWindowDataSO data;
    int currentStep = 0;
    int stemNums = 5;

    private void Awake()
    {
        instance = this;
        openAnimation = GetComponent<UIAnimationController>();
    }

    private void Start()
    {
        PlayerInventory.instance.OnGoldChanged += UpdateGoldText;
        goldText.text = PlayerInventory.instance.gold.ToString();
        prevBtn.onClick.AddListener(delegate { PreviousTab(); });
        nextBtn.onClick.AddListener(delegate { NextTab(); });
        closeBtn.onClick.AddListener(delegate { CloseWindow(); });

        categoryButton1.onClick.AddListener(() => GoToCategory(0));
        categoryButton2.onClick.AddListener(() => GoToCategory(1));
        categoryButton3.onClick.AddListener(() => GoToCategory(2));
    }

   

    public void UpdateGoldText(int changeAmount, int goldAmount)
    {
        goldText.text = goldAmount.ToString();
    }

    public void OpenWindow(ShopWindowDataSO data)
    {
        
        this.data = data;
        InitShop();
        StartCoroutine(ActivateWindow());
    }

    public void InitShop()
    {
        //delete old objects
        for(int i = 0; i < itemContainer.childCount; i ++)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }

        //create new objects
        for (int i = 0; i < data.shopCategories.Count; i ++)
        {
            GameObject category = Instantiate(categoryPrefab, itemContainer);
            List<ShopItem> items = data.shopCategories[i].shopItems;
            
            for(int j = 0; j < items.Count; j ++)
            {
                ShopProductUI productUI = Instantiate(productPrefab, category.transform).GetComponent<ShopProductUI>();
                productUI.Init(items[j]);
            }
        }

        stemNums = data.shopCategories.Count;
        CartManager.instance.ClearCart();
        GoToCategory(0);
        openAnimation.UpdateObjectChange();
    }

    IEnumerator ActivateWindow()
    {
        yield return ConservationManager.instance.DeactivateConservationDialog();
        yield return new WaitForSeconds(0.7f);
        openAnimation.Activate();

    }

    public void CloseWindow()
    {
        StartCoroutine(DeactivateWindow());
    }

    IEnumerator DeactivateWindow()
    {
        openAnimation.UpdateObjectChange();
        openAnimation.Deactivate();
        yield return new WaitForSeconds(0.7f);
        PlayerManager.instance.ActivateController();
        PlayerManager.instance.isInteract = false;
    }

    public void NextTab()
    {
        currentStep++;
        currentStep = currentStep % stemNums;
        containerScroll.value = currentStep * (1f / (stemNums - 1));
    }

    public void PreviousTab()
    {
        currentStep--;
        if(currentStep < 0)
        {
            currentStep = stemNums - 1;
        }
        containerScroll.value = currentStep * (1f / (stemNums - 1));
    }


    public void GoToCategory(int index)
    {
        if (data == null || index < 0 || index >= stemNums)
            return;
        currentStep = index;
        containerScroll.value = currentStep * (1f / (stemNums - 1));
        UpdateCategoryName();
    }

    /// <summary>
    /// Cập nhật tên category hiển thị
    /// </summary>
    private void UpdateCategoryName()
    {
        if (categoryNameText == null || data == null)
            return;

        if (currentStep < data.shopCategories.Count)
        {
            categoryNameText.text = data.shopCategories[currentStep].categoryName;
            categoryDescriptionText.text = data.shopCategories[currentStep].categoryDescription;
        }
            
    }
}

