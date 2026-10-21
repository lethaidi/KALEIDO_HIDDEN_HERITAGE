
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Button_highlight_click : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject[] buttons;
    public GameObject[] highlight_buttons;
    public TextMeshProUGUI[] name_buttons;
    public int button_index = -1;

    [Header("Other Mode")]
    public bool enable_bySelf = false;

    // Lưu listener do chính script này đăng ký
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction>
        registeredListeners = new();

    void Start()
    {
        InitButtons();
    }

    public void InitButtons()
    {
        // Gỡ các listener cũ của riêng script này.
        ClearRegisteredListeners();

        // Giữ lại Button được chọn nếu vẫn tồn tại.
        GameObject selectedButton = null;

        if (buttons != null && button_index >= 0 && button_index < buttons.Length)
        {
            selectedButton = buttons[button_index];
        }

        int count = transform.childCount;

        buttons = new GameObject[count];
        highlight_buttons = new GameObject[count];
        name_buttons = new TextMeshProUGUI[count];

        button_index = -1;

        for (int i = 0; i < count; i++)
        {
            GameObject buttonObj =
                transform.GetChild(i).gameObject;

            buttons[i] = buttonObj;

            Transform highlight = buttonObj.transform.Find("highlight");

            if (highlight != null)
            {
                highlight_buttons[i] = highlight.gameObject;
                highlight.gameObject.SetActive(false);
            }

            Transform nameObj = buttonObj.transform.Find("name");

            if (nameObj != null)
            {
                name_buttons[i] = nameObj.GetComponent<TextMeshProUGUI>();

                if (name_buttons[i] != null)
                    name_buttons[i].text = i.ToString();
            }

            // Đăng ký Button
            Button btn = buttonObj.GetComponent<Button>();

            if (btn != null)
            {
                int index = i;

                UnityEngine.Events.UnityAction listener =
                    () => HighlightButton(index);

                btn.onClick.AddListener(listener);

                registeredListeners[btn] = listener;
            }

            // Khôi phục lựa chọn nếu Button còn tồn tại
            if (buttonObj == selectedButton)
            {
                button_index = i;
            }
        }

        // Khôi phục highlight
        if (button_index >= 0 && highlight_buttons[button_index] != null)
        {
            highlight_buttons[button_index].SetActive(true);
        }
    }

    public void HighlightButton(int index)
    {
        if (highlight_buttons == null || index < 0 || index >= highlight_buttons.Length)
            return;

        if (index == button_index && enable_bySelf)
        {
            UnenableAll();
            return;
        }

        UnenableAll();

        if (highlight_buttons[index] != null)
        {
            highlight_buttons[index].SetActive(true);
            button_index = index;
        }
    }

    public void UnenableAll()
    {
        if (highlight_buttons != null)
        {
            foreach (var h in highlight_buttons)
            {
                if (h != null)
                    h.SetActive(false);
            }
        }

        button_index = -1;
    }

    private void ClearRegisteredListeners()
    {
        foreach (var entry in registeredListeners)
        {
            if (entry.Key != null)
                entry.Key.onClick.RemoveListener(entry.Value);
        }

        registeredListeners.Clear();
    }

    private void OnDestroy()
    {
        ClearRegisteredListeners();
    }

    public void RefreshButtons()
    {
        // Xóa các listener cũ do script quản lý
        ClearRegisteredListeners();

        // Xóa toàn bộ tham chiếu cũ
        buttons = System.Array.Empty<GameObject>();
        highlight_buttons = System.Array.Empty<GameObject>();
        name_buttons = System.Array.Empty<TextMeshProUGUI>();

        button_index = -1;

        // Khởi tạo lại dựa trên các Slot đang tồn tại
        InitButtons();
    }


    public void RemoveButtonAt(int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Length)
            return;

        // Gỡ listener cũ trước khi thay đổi index
        ClearRegisteredListeners();

        // Ghi nhớ button đang được chọn
        GameObject selectedButton = null;

        if (button_index >= 0 && button_index < buttons.Length)
            selectedButton = buttons[button_index];

        // Nếu đang chọn button bị xóa thì bỏ chọn
        if (selectedButton == buttons[index])
            selectedButton = null;

        // Xóa phần tử tương ứng trong cả 3 mảng
        var buttonList = new List<GameObject>(buttons);
        var highlightList = new List<GameObject>(highlight_buttons);
        var nameList = new List<TextMeshProUGUI>(name_buttons);

        buttonList.RemoveAt(index);
        highlightList.RemoveAt(index);
        nameList.RemoveAt(index);

        buttons = buttonList.ToArray();
        highlight_buttons = highlightList.ToArray();
        name_buttons = nameList.ToArray();

        // Reset index và highlight
        button_index = -1;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (highlight_buttons[i] != null)
                highlight_buttons[i].SetActive(false);

            if (name_buttons[i] != null)
                name_buttons[i].text = i.ToString();

            if (buttons[i] == selectedButton)
                button_index = i;

            Button btn = buttons[i] != null
                ? buttons[i].GetComponent<Button>()
                : null;

            if (btn != null)
            {
                int newIndex = i;

                UnityEngine.Events.UnityAction listener =
                    () => HighlightButton(newIndex);

                btn.onClick.AddListener(listener);
                registeredListeners[btn] = listener;
            }
        }

        // Khôi phục highlight đã chọn
        if (button_index >= 0 &&
            highlight_buttons[button_index] != null)
        {
            highlight_buttons[button_index].SetActive(true);
        }
    }
}
