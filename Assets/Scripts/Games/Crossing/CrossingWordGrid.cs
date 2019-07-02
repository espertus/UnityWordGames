public class CrossingWordGrid : CrossingGrid
{
    private static int ROWS = 7;
    private static int COLUMNS = 8;
    private static float Y_OFFSET = 2.2f;

    public CrossingWordGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.WORD_GRID) {}
}