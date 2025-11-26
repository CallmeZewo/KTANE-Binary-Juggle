using System;
using System.Collections.Generic;
using static Enums;

public class Ball
{
    public BallColor color;
    public Symbol symbol;
    public Rotation rotation;
    public BallDirection direction;
    public int position;
    public bool isOne;
    public bool needsClick;

    public readonly bool[,,,] Table = new bool[4, 4, 2, 2]
    {
        {
            {
                { false, true },
                { false, false }
            },
            {
                { false, false },
                { true, false }
            },
            {
                { true, false },
                { false, true }
            },
            {
                { true, true },
                { false, true }
            }
        },
        {
            {
                { true, false },
                { true, true }
            },
            {
                { true, true },
                { false, true }
            },
            {
                { true, false },
                { false, false }
            },
            {
                { false, false },
                { true, false }
            }
        },
        {
            {
                { true, true },
                { false, true }
            },
            {
                { true, false },
                { true, true }
            },
            {
                { false, true },
                { true, false }
            },
            {
                { true, false },
                { false, true }
            }
        },
        {
            {
                { false, false },
                { true, false }
            },
            {
                { false, true },
                { false, false }
            },
            {
                { true, false },
                { true, true }
            },
            {
                { false, true },
                { true, false }
            }
        }
    };


    public Ball(BallColor col, Symbol sym, Rotation rot, BallDirection dir, int pos)
    {
        color = col;
        symbol = sym;
        rotation = rot;
        direction = dir;
        position = pos;
        isOne = Table[(int)col, (int)sym, (int)rot, (int)dir];
    }
}