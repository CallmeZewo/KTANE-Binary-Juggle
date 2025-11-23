using static Enums;

public class Ball
{
    public Color color;
    public Symbol symbol;
    public Rotation rotation;
    public Direction direction;

    public Ball(Color col, Symbol sym, Rotation rot, Direction dir)
    {
        color = col;
        symbol = sym;
        rotation = rot;
        direction = dir;
    }
}
