using UnityEngine;
using UnityEngine.UI;
using VersionHistory;

public class VersionHistoryController : MonoBehaviour
{
    public VersionHistoryScriptableObject VersionHistoryScriptableObject;
    public Text Text;
    public Text PreviousVersionButtonText;
    public Text NextVersionButtonText;
    public RectTransform ContentPanel;
    public ScrollRect ScrollRect;

    private int _index;

    public void OnEnable()
    {
        RefreshText();
    }

    public void SetText(string text)
    {
        Text.text = text;
        ContentPanel.anchoredPosition = Vector2.zero;
    }

    public void OnPreviousVersionButtonClick()
    {
        ++_index;
        if (_index >= VersionHistoryScriptableObject.Versions.Count)
            _index = VersionHistoryScriptableObject.Versions.Count-1;
        RefreshText();
    }

    public void OnNextVersionButtonClick()
    {
        --_index;
        if (_index < 0)
            _index = 0;
        RefreshText();
    }

    void RefreshText()
    {
        SetText(VersionHistoryScriptableObject.Versions[_index].Text.text);
    }

}
