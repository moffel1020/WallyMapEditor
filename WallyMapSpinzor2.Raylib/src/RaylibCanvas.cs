using Rl = Raylib_cs.Raylib;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class RaylibCanvas : ICanvas<Texture2DWrapper>
{
    public string BrawlPath { get; set; }
    public BucketPriorityQueue<Action> DrawingQueue { get; set; } = new(Enum.GetValues<DrawPriorityEnum>().Length);
    public Dictionary<string, Texture2DWrapper> TextureCache { get; } = new();
    public float Scale { get; set; } = 1;

    public RaylibCanvas(string brawlPath)
    {
        BrawlPath = brawlPath;
    }

    public void ClearTextureCache()
    {
        TextureCache.Clear();
    }

    public void DrawCircle(double x, double y, double radius, Color color, Transform trans, DrawPriorityEnum priority)
    {
        (x, y) = trans * new Position(x, y);
        
        DrawingQueue.Push(() =>
        {
            Rl.DrawCircle((int)x, (int)y, (float)radius, Utils.ToRlColor(color));
        }, (int)priority);
    }

    public void DrawLine(double x1, double y1, double x2, double y2, Color color, Transform trans, DrawPriorityEnum priority)
    {
        (x1, y1) = trans * new Position(x1, y1);
        (x2, y2) = trans * new Position(x2, y2);

        DrawingQueue.Push(() =>
        {
            Rl.DrawLine((int)x1, (int)y1, (int)x2, (int)y2, Utils.ToRlColor(color));
        }, (int)priority);
    }

    public const double MULTI_COLOR_LINE_OFFSET = Editor.LINE_WIDTH;
    public void DrawLineMultiColor(double x1, double y1, double x2, double y2, Color[] colors, Transform trans, DrawPriorityEnum priority)
    {
        (x1, y1) = trans * new Position(x1, y1);
        (x2, y2) = trans * new Position(x2, y2);
        if(x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        double center = (colors.Length - 1) / 2.0;
        (double offX, double offY) = (y1 - y2, x2 - x1);
        (offX, offY) = BrawlhallaMath.Normalize(offX, offY);
        for(int i = 0; i < colors.Length; ++i)
        {
            double mult = MULTI_COLOR_LINE_OFFSET * (i - center);
            DrawLine(x1 + offX * mult, y1 + offY * mult, x2 + offX * mult, y2 + offY * mult, colors[i], Transform.IDENTITY, priority);
        }
    }

    public void DrawRect(double x, double y, double w, double h, bool filled, Color color, Transform trans, DrawPriorityEnum priority)
    {
        Position p1 = trans * new Position(x, y);
        Position p2 = trans * new Position(x + w, y + h);
        w = p2.X - p1.X; 
        h = p2.Y - p1.Y;
        
        DrawingQueue.Push(() =>
        {
            if(filled) Rl.DrawRectangle((int)p1.X, (int)p1.Y, (int)w, (int)h, Utils.ToRlColor(color));
            else
            {
                //lines in DrawRectangleLines scale with Camera2D so the width needs to be calculated manually to ensure it is atleast 1 pixel wide
                float lineWidth = Editor.LINE_WIDTH * Scale;
                if (lineWidth < 1) lineWidth = 1;
                Rl.DrawRectangleLinesEx(new((int)p1.X, (int)p1.Y, (int)w, (int)h), lineWidth / Scale, Utils.ToRlColor(color));
            } 
        }, (int)priority);
    }

    public void DrawString(double x, double y, string text, double fontSize, Color color, Transform trans, DrawPriorityEnum priority)
    {

    }

    public void DrawTexture(double x, double y, Texture2DWrapper texture, Transform trans, DrawPriorityEnum priority)
    {
        if (texture.Texture is null) return;

        (double w, double h) = trans * new Position(x + texture.W, y + texture.H);
        (x, y) = trans * new Position(x, y);
        w -= x;
        h -= y;

        DrawingQueue.Push(() =>
        {
            Rl.DrawTexturePro(
                (Texture2D)texture.Texture,
                new(0, 0, Math.Sign(w) * texture.W, Math.Sign(h) * texture.H),
                new((float)(w < 0 ? x + w : x), (float)(h < 0 ? y + h : y), (float)Math.Abs(w), (float)Math.Abs(h)),
                new(0, 0),
                0,
                Raylib_cs.Color.White
            );
        }, (int)priority);
    }

    public void DrawTextureRect(double x, double y, double w, double h, Texture2DWrapper texture, Transform trans, DrawPriorityEnum priority)
    {
        if (texture.Texture is null) return;

        Position p1 = trans * new Position(x, y);
        Position p2 = trans * new Position(x + w, y + h);
        w = p2.X - p1.X; 
        h = p2.Y - p1.Y;

        DrawingQueue.Push(() =>
        {
            Rl.DrawTexturePro(
                (Texture2D)texture.Texture, 
                new(0, 0, texture.W, texture.H),
                new((float)p1.X, (float)p1.Y, (float)w, (float)h),
                new(0, 0),
                0,
                Raylib_cs.Color.White
            );

        }, (int)priority);
    }

    public Texture2DWrapper LoadTextureFromPath(string path)
    {
        string finalPath = Path.Join(BrawlPath, "mapArt", path);
        TextureCache.TryGetValue(finalPath, out Texture2DWrapper? texture);
        if (texture is not null) return texture;

        texture = new(Utils.LoadRlTexture(finalPath));
        TextureCache.Add(finalPath, texture);
        return texture;
    }

    public Texture2DWrapper LoadTextureFromSWF(string filePath, string name)
    {
        return new(null);
    }

    public void FinalizeDraw()
    {
        while (DrawingQueue.Count > 0)
        {
            Action drawAction = DrawingQueue.PopMin();
            drawAction();
        }
    }
}