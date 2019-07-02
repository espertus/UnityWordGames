public class CrossingPanelGrid : CrossingGrid
{
    private static int ROWS = 4;
    private static int COLUMNS = 12;
    private static float Y_OFFSET = -5.5f;

    public CrossingPanelGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.PANEL_GRID) { }
}