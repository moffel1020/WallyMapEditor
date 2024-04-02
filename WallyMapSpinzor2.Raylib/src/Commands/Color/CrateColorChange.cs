namespace WallyMapSpinzor2.Raylib;

public class CrateColorChange(LevelType lt, CrateColor newColor, bool inner): ICommand
{
    public CrateColor NewColor { get; set; } = newColor;
    private readonly LevelType _levelType = lt;
    private readonly CrateColor _oldColor = inner ? lt.CrateColorB!.Value : lt.CrateColorA!.Value;

    public bool Inner { get; set; } = inner;
    public bool Outer { get => !Inner; set => Inner = !value; }

    public void Execute()
    {
        if (Inner) _levelType.CrateColorB = NewColor;
        else _levelType.CrateColorA = NewColor;
    }

    public void Undo()
    {
        if (Inner) _levelType.CrateColorB = _oldColor;
        else _levelType.CrateColorA = _oldColor;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is CrateColorChange other && Inner == other.Inner)
        {
            NewColor = other.NewColor;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}