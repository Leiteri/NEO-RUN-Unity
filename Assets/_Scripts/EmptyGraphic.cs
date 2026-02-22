using UnityEngine.UI;

public class EmptyGraphic : Graphic
{
    public override void SetAllDirty() { }
    public override void Rebuild(CanvasUpdate update) { }
    protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
}