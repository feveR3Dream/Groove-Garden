using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestImageInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
    }

    [SerializeField] private Image m_CellImage;


    // Hero_0. Hero_1... Hero_300
    // Hero_ + hero.id

    // 

    private void Start()
    {
        m_CellImage.sprite = Resources.Load<Sprite>("TestImage");

        int heroId = Resources.Load<HeroData>("Hero_" + 0).heroId;
    }
}

public class HeroData : ScriptableObject
{
    public int heroId;
    public string heroName;
}