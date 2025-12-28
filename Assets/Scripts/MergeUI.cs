using UnityEngine;
using UnityEngine.EventSystems;

public class MergeUI : MonoBehaviour, IDropHandler
{
    public GameObject resultPrefab;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        if (dropped == gameObject) return;

        MergeUI other = dropped.GetComponent<MergeUI>();
        if (other == null) return;

        SoundManager.Instance.PlayMerge();

        RectTransform myRect = GetComponent<RectTransform>();
        GameObject result = Instantiate(resultPrefab, myRect.parent);

        result.GetComponent<RectTransform>().anchoredPosition =
            myRect.anchoredPosition;

        Destroy(dropped);
        Destroy(gameObject);
    }
}
