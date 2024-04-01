namespace WallyMapSpinzor2.Raylib;

public class KillBoundsChange(LevelType lt, int top, int bottom, int left, int right): ICommand
{
    public int Top { get; set; } = top;
    public int Bottom { get; set; } = bottom;
    public int Left { get; set; } = left;
    public int Right { get; set; } = right;
    private readonly LevelType _levelType = lt;

    public void Execute()
    {
        _levelType.TopKill += Top;
        _levelType.BottomKill += Bottom;
        _levelType.LeftKill += Left;
        _levelType.RightKill += Right;
    }

    public void Undo()
    {
        _levelType.TopKill -= Top;
        _levelType.BottomKill -= Bottom;
        _levelType.LeftKill -= Left;
        _levelType.RightKill -= Right;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is KillBoundsChange other && _levelType == other._levelType)
        {
            Top += other.Top;
            Bottom += other.Bottom;
            Left += other.Left;
            Right += other.Right;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}