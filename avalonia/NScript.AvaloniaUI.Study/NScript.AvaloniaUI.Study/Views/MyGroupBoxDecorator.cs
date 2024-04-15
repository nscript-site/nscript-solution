using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Collections.Generic;

namespace NScript.AvaloniaUI.Study.Views;

public class MyGroupBoxDecorator : Control
{
    public static readonly StyledProperty<string> GroupNameProperty =
   AvaloniaProperty.Register<MyGroupBox, string>(nameof(GroupName));

    public string GroupName
    {
        get { return GetValue(GroupNameProperty); }
        set { SetValue(GroupNameProperty, value); }
    }

    public IBrush? BorderBrush { get; set; } = Brushes.Black;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = this.Bounds;

        const double boxTop = 10;
        const double groupNameX0 = 20;
        const double groupNameMargin = 5;

        // 太矮，不绘制了
        if (bounds.Height <= boxTop) return;
        if (BorderBrush == null) return;

        var drawRect = new Rect(bounds.X,bounds.Y + boxTop, bounds.Width,bounds.Height - boxTop);

        var pen = new Pen(BorderBrush, 1);

        // 创建一个 TextLayout 实例，测量其尺寸
        var textLayout = new TextLayout(
            GroupName,
            Typeface.Default,
            12, BorderBrush
        );

        // 绘制文字
        textLayout.Draw(context, new Point(groupNameX0, boxTop - textLayout.Height * 0.5));

        // 绘制边框
        var path = new PolylineGeometry();
        path.Points = new List<Point> {
            new Point(groupNameX0 - groupNameMargin, drawRect.Top),
            drawRect.TopLeft,
            drawRect.BottomLeft,
            drawRect.BottomRight,
            drawRect.TopRight,
            new Point(groupNameX0 + textLayout.Width + groupNameMargin, drawRect.Top)
        };
        context.DrawGeometry(BorderBrush, pen, path);
    }
}
