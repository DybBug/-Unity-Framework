public class PathNode
{
    public CustomTile.TileType TileType { get; private set; }
    public int Column { get; private set; }
    public int Row { get; private set; }
    public int G { get; set; }
    public int H { get; set; }
    public int F => G + H;
    public PathNode PrevNode { get; set; }

    private CustomTile m_Tile;

    public PathNode(CustomTile.TileType tileType, int column, int row)
    {
        TileType = tileType;
        Column = column;
        Row = row;
        G = 0;
        H = 0;
    }
}
