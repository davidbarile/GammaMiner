using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiSafeAreaSizer : MonoBehaviour
{
    private RectTransform _rectTrans;

    private float _origRectLeft = 0;
    private float _origRectTop = 0;
    private float _origRectRight = 0;
    private float _origRectBottom = 0;

    [SerializeField] private bool _showDebugText;
    [SerializeField] private bool _doValidate;

    [Header("Sides To Ignore")]
    [SerializeField] private bool _ignoreTop;
    [SerializeField] private bool _ignoreBottom;
    [SerializeField] private bool _ignoreLeft;
    [SerializeField] private bool _ignoreRight;

    private void Start()
    {
        _rectTrans = GetComponent<RectTransform>();

        _origRectLeft = _rectTrans.offsetMin.x;
        _origRectTop = _rectTrans.offsetMin.y;
        _origRectRight = _rectTrans.offsetMax.x;
        _origRectBottom = _rectTrans.offsetMax.y;

        Refresh();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(!Application.isPlaying || !_doValidate) return;

        _doValidate = false;

        Refresh();
    }
#endif

    private void Refresh()
    {
        //Screen.safeArea is measured from the bottom left corner of the screen
        var newLeft = _ignoreLeft ? _origRectLeft : _origRectLeft + Screen.safeArea.x;
        var newBottom = _ignoreBottom ? _origRectBottom : _origRectBottom + Screen.safeArea.y;

        var rightOffset = Screen.width - Screen.safeArea.width - Screen.safeArea.x ;
        var newRight = _ignoreRight ? _origRectRight : _origRectRight - rightOffset;

        var topOffset = Screen.height - Screen.safeArea.height - Screen.safeArea.y;
        var newTop = _ignoreTop ? _origRectTop : _origRectTop - topOffset;

#if UNITY_EDITOR
        if(_rectTrans == null)
            _rectTrans = GetComponent<RectTransform>();
#endif

        _rectTrans.offsetMin = new Vector2(newLeft, newBottom);
        _rectTrans.offsetMax = new Vector2(newRight, newTop);

        if(_showDebugText && transform.parent != null && transform.parent.parent != null)
            Debug.Log($"DTB: <color=white>Refresh() {transform.parent.parent.name}   Screen = {Screen.width} x {Screen.height}    SafeArea x = {Screen.safeArea.x}   y = {Screen.safeArea.y}   width = {Screen.safeArea.width}   height = {Screen.safeArea.height}    xMin = {Screen.safeArea.xMin}    xMax = {Screen.safeArea.xMax}   newLeft = {newLeft}  newTop = {newTop}</color>", gameObject);
    }
}