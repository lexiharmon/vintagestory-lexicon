using SkiaSharp;

// Draws the Lexicon logo: an open handbook with a magnifying glass highlighting one exact word.
// Usage: dotnet run -- <output.png> <size>   (e.g. modicon.png 256)

string output = args.Length > 0 ? args[0] : "logo.png";
int size = args.Length > 1 ? int.Parse(args[1]) : 1024;

using var bitmap = new SKBitmap(size, size);
using var canvas = new SKCanvas(bitmap);
canvas.Clear(SKColors.Transparent);
canvas.Scale(size / 1024f);

SKColor C(string hex, byte alpha = 255) => SKColor.Parse(hex).WithAlpha(alpha);

SKPaint Fill(SKColor color) => new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
SKPaint Stroke(SKColor color, float width, SKStrokeCap cap = SKStrokeCap.Butt) =>
    new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = cap, StrokeJoin = SKStrokeJoin.Round };

// --- Badge background: dark leather with a stitched-looking inner border
var badge = new SKRoundRect(new SKRect(0, 0, 1024, 1024), 210);
using (var paint = Fill(C("#4a2e1f")))
{
    paint.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, 1024), new[] { C("#6a4630"), C("#33201a") }, SKShaderTileMode.Clamp);
    canvas.DrawRoundRect(badge, paint);
}
using (var paint = Stroke(C("#c99a5b", 120), 10))
{
    paint.PathEffect = SKPathEffect.CreateDash(new[] { 26f, 18f }, 0);
    canvas.DrawRoundRect(new SKRoundRect(new SKRect(52, 52, 972, 972), 165), paint);
}

// --- Book cover (slightly larger than the pages, so a leather edge shows)
SKPath Page(float dir, float inset = 0)
{
    // dir -1 = left page, +1 = right page; spine at x = 512
    float x(float dx) => 512 + dir * dx;
    var path = new SKPath();
    path.MoveTo(512, 318 - inset);
    path.CubicTo(x(70), 282 - inset, x(210), 270 - inset, x(342 + inset), 312 - inset);
    path.LineTo(x(342 + inset), 722 + inset);
    path.CubicTo(x(210), 680 + inset, x(70), 692 + inset, 512, 738 + inset);
    path.Close();
    return path;
}

foreach (float dir in new[] { -1f, 1f })
{
    using var cover = Page(dir, 26);
    using var paint = Fill(C("#8a5632"));
    canvas.DrawPath(cover, paint);
    using var edge = Stroke(C("#2a170f"), 8);
    canvas.DrawPath(cover, edge);
}

// --- Pages
foreach (float dir in new[] { -1f, 1f })
{
    using var page = Page(dir);
    using var paint = Fill(C("#f4e7c6"));
    paint.Shader = SKShader.CreateLinearGradient(new SKPoint(512, 0), new SKPoint(512 + dir * 342, 0), new[] { C("#d9c394"), C("#f7ecd0"), C("#f4e7c6") }, new[] { 0f, 0.18f, 1f }, SKShaderTileMode.Clamp);
    canvas.DrawPath(page, paint);
    using var outline = Stroke(C("#b99d68"), 5);
    canvas.DrawPath(page, outline);
}

// Text lines on the pages
using (var line = Stroke(C("#a38a5c", 190), 14, SKStrokeCap.Round))
{
    float[] ys = { 400, 456, 512, 568, 624 };
    foreach (float y in ys)
    {
        float lift = (y - 400) * 0.0f;
        canvas.DrawLine(190, y + lift, 440 - (y == 624 ? 90 : 0), y + lift - 12, line);
        if (y < 560) canvas.DrawLine(584, y - 12, 834, y + lift, line);
    }
}

// Spine groove
using (var spine = Stroke(C("#7a5a30", 160), 7))
{
    canvas.DrawLine(512, 318, 512, 738, spine);
}

// --- Magnifying glass (bottom right, lens over the right page)
const float lx = 650, ly = 540, lr = 200;

// Handle
using (var handleShadow = Stroke(C("#000000", 70), 84, SKStrokeCap.Round))
    canvas.DrawLine(lx + 142 + 8, ly + 142 + 12, 872 + 8, 762 + 12, handleShadow);
using (var handle = Stroke(C("#6b3f22"), 80, SKStrokeCap.Round))
    canvas.DrawLine(lx + 142, ly + 142, 872, 762, handle);
using (var grip = Stroke(C("#8f5a33"), 26, SKStrokeCap.Round))
    canvas.DrawLine(lx + 150, ly + 134, 862, 752, grip);

// Lens shadow + glass
using (var shadow = Fill(C("#000000", 60)))
    canvas.DrawCircle(lx + 10, ly + 14, lr + 22, shadow);
using (var glass = Fill(C("#fff8e2", 235)))
    canvas.DrawCircle(lx, ly, lr, glass);

// What the lens shows: a magnified line of text with one highlighted (exact) word
canvas.Save();
using (var clip = new SKPath())
{
    clip.AddCircle(lx, ly, lr);
    canvas.ClipPath(clip, SKClipOperation.Intersect, true);
}
using (var dim = Stroke(C("#a38a5c", 200), 22, SKStrokeCap.Round))
{
    canvas.DrawLine(lx - 170, ly - 110, lx + 60, ly - 110, dim);
    canvas.DrawLine(lx - 170, ly + 112, lx + 110, ly + 112, dim);
}
var wordBox = new SKRoundRect(new SKRect(lx - 150, ly - 52, lx + 150, ly + 52), 26);
using (var highlight = Fill(C("#f2b73c")))
    canvas.DrawRoundRect(wordBox, highlight);
using (var highlightEdge = Stroke(C("#c98a14"), 6))
    canvas.DrawRoundRect(wordBox, highlightEdge);
using (var word = Stroke(C("#3a2013"), 30, SKStrokeCap.Round))
    canvas.DrawLine(lx - 108, ly, lx + 108, ly, word);
canvas.Restore();

// Glass sheen
using (var sheen = Stroke(C("#ffffff", 150), 18, SKStrokeCap.Round))
{
    using var arc = new SKPath();
    arc.AddArc(new SKRect(lx - lr + 34, ly - lr + 34, lx + lr - 34, ly + lr - 34), 200, 55);
    canvas.DrawPath(arc, sheen);
}

// Lens rim
using (var rimDark = Stroke(C("#7a4d12"), 62))
    canvas.DrawCircle(lx, ly, lr + 4, rimDark);
using (var rim = Stroke(C("#e0a63a"), 50))
{
    rim.Shader = SKShader.CreateLinearGradient(new SKPoint(lx - lr, ly - lr), new SKPoint(lx + lr, ly + lr), new[] { C("#f6cf72"), C("#c9871f") }, SKShaderTileMode.Clamp);
    canvas.DrawCircle(lx, ly, lr + 4, rim);
}

using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes(output, data.ToArray());
Console.WriteLine($"Wrote {output} ({size}x{size})");
