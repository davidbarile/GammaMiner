using UnityEngine;

[RequireComponent(typeof(Rock))]
public class RockColorAnim : MonoBehaviour
{
    private Rock rock;

    [Header("Main Color")]
    [SerializeField] private Color[] rockAnimColors;
    [SerializeField] private AnimationCurve animCurve;
    [Range(0,1)][SerializeField] private float changePerFrame;

    [Header("Outline Color")]
    [SerializeField] private bool shouldAnimateOutline;
    [SerializeField] private Color[] rockOutlineColors;
    [SerializeField] private AnimationCurve outlineAnimCurve;
    [Range(0, 1)][SerializeField] private float outlineChangePerFrame;

    private float timer;
    private float outlineTimer;

    private void OnValidate()
    {
        this.rock = this.GetComponent<Rock>();
    }

    private void Start()
    {
        this.rock = this.GetComponent<Rock>();
        
        if (this.rock && this.rock.RockData != null)
            ApplyRockData(this.rock.RockData);
    }

    public void ApplyRockData(RockData inRockData)
    {
        this.rockAnimColors = inRockData.RockAnimColors;
        this.animCurve = inRockData.AnimCurve;
        this.changePerFrame = inRockData.ChangePerFrame;

        this.shouldAnimateOutline = inRockData.ShouldAnimateOutline;
        this.rockOutlineColors = inRockData.RockOutlineColors;
        this.outlineAnimCurve = inRockData.OutlineAnimCurve;
        this.outlineChangePerFrame = inRockData.OutlineChangePerFrame;
    }

    private void Update()
    {
        if (TileEditorTool.IsEditing) return;

        this.timer += this.changePerFrame * Time.deltaTime * 60;

        if (this.timer > 1)
            this.timer = 0;

        var lerpVal = this.animCurve.Evaluate(this.timer);
        var lerpColor = Color.Lerp(this.rockAnimColors[0], this.rockAnimColors[1], lerpVal);

        ColorOutline();

        if(this.rock.Fill)
            this.rock.Fill.color = lerpColor;
    }

    private void ColorOutline()
    {
        if (!this.shouldAnimateOutline) return;

        this.outlineTimer += this.outlineChangePerFrame * Time.deltaTime * 60;

        if (this.outlineTimer > 1)
            this.outlineTimer = 0;

        var outlineLerpVal = this.outlineAnimCurve.Evaluate(this.outlineTimer);
        var outlineLerpColor = Color.Lerp(this.rockOutlineColors[0], this.rockOutlineColors[1], outlineLerpVal);

        if (this.rock.Outline)
            this.rock.Outline.color = outlineLerpColor;
    }
}