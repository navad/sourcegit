using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace SourceGit.Views
{
    public class ImageContainer : Control
    {
        public override void Render(DrawingContext context)
        {
            if (_bgBrush == null)
            {
                var maskBrush = new SolidColorBrush(ActualThemeVariant == ThemeVariant.Dark ? 0xFF404040 : 0xFFBBBBBB);
                var bg = new DrawingGroup()
                {
                    Children =
                    {
                        new GeometryDrawing() { Brush = maskBrush, Geometry = new RectangleGeometry(new Rect(0, 0, 12, 12)) },
                        new GeometryDrawing() { Brush = maskBrush, Geometry = new RectangleGeometry(new Rect(12, 12, 12, 12)) },
                    }
                };

                _bgBrush = new DrawingBrush(bg)
                {
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    DestinationRect = new RelativeRect(new Size(24, 24), RelativeUnit.Absolute),
                    Stretch = Stretch.None,
                    TileMode = TileMode.Tile,
                };
            }

            context.FillRectangle(_bgBrush, new Rect(Bounds.Size));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property.Name == "ActualThemeVariant")
            {
                _bgBrush = null;
                InvalidateVisual();
            }
        }

        private DrawingBrush _bgBrush = null;
    }

    public class ImagesSwipeControl : ImageContainer
    {
        public static readonly StyledProperty<double> AlphaProperty =
            AvaloniaProperty.Register<ImagesSwipeControl, double>(nameof(Alpha), 0.5);

        public double Alpha
        {
            get => GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public static readonly StyledProperty<Bitmap> OldImageProperty =
            AvaloniaProperty.Register<ImagesSwipeControl, Bitmap>(nameof(OldImage), null);

        public Bitmap OldImage
        {
            get => GetValue(OldImageProperty);
            set => SetValue(OldImageProperty, value);
        }

        public static readonly StyledProperty<Bitmap> NewImageProperty =
            AvaloniaProperty.Register<ImagesSwipeControl, Bitmap>(nameof(NewImage), null);

        public Bitmap NewImage
        {
            get => GetValue(NewImageProperty);
            set => SetValue(NewImageProperty, value);
        }

        static ImagesSwipeControl()
        {
            AffectsMeasure<ImagesSwipeControl>(OldImageProperty, NewImageProperty);
            AffectsRender<ImagesSwipeControl>(AlphaProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var alpha = Alpha;
            var w = Bounds.Width;
            var h = Bounds.Height;
            var x = w * alpha;
            var left = OldImage;
            if (left != null && alpha > 0)
            {
                var src = new Rect(0, 0, left.Size.Width * alpha, left.Size.Height);
                var dst = new Rect(0, 0, x, h);
                context.DrawImage(left, src, dst);
            }

            var right = NewImage;
            if (right != null && alpha < 1)
            {
                var src = new Rect(right.Size.Width * alpha, 0, right.Size.Width * (1 - alpha), right.Size.Height);
                var dst = new Rect(x, 0, w - x, h);
                context.DrawImage(right, src, dst);
            }

            context.DrawLine(new Pen(Brushes.DarkGreen, 2), new Point(x, 0), new Point(x, Bounds.Height));
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var p = e.GetPosition(this);
            var hitbox = new Rect(Math.Max(Bounds.Width * Alpha - 2, 0), 0, 4, Bounds.Height);
            var pointer = e.GetCurrentPoint(this);
            if (pointer.Properties.IsLeftButtonPressed && hitbox.Contains(p))
            {
                _pressedOnSlider = true;
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
                e.Pointer.Capture(this);
                e.Handled = true;
            }                
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _pressedOnSlider = false;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            var w = Bounds.Width;
            var p = e.GetPosition(this);

            if (_pressedOnSlider)
            {
                SetCurrentValue(AlphaProperty, Math.Clamp(p.X, 0, w) / w);
            }
            else
            {
                var hitbox = new Rect(Math.Max(w * Alpha - 2, 0), 0, 4, Bounds.Height);
                if (hitbox.Contains(p))
                {
                    if (!_lastInSlider)
                    {
                        _lastInSlider = true;
                        Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    }
                }
                else
                {
                    if (_lastInSlider)
                    {
                        _lastInSlider = false;
                        Cursor = null;
                    }
                }                    
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var left = OldImage;
            var right = NewImage;

            if (left == null)
                return right == null ? availableSize : GetDesiredSize(right.Size, availableSize);

            if (right == null)
                return GetDesiredSize(left.Size, availableSize);

            var ls = GetDesiredSize(left.Size, availableSize);
            var rs = GetDesiredSize(right.Size, availableSize);
            return ls.Width > rs.Width ? ls : rs;
        }

        private Size GetDesiredSize(Size img, Size available)
        {
            var w = available.Width;
            var h = available.Height;

            var sw = available.Width / img.Width;
            var sh = available.Height / img.Height;
            var scale = Math.Min(sw, sh);

            return new Size(scale * img.Width, scale * img.Height);
        }

        private bool _pressedOnSlider = false;
        private bool _lastInSlider = false;
    }

    public class ImageBlendControl : ImageContainer
    {
        public static readonly StyledProperty<double> AlphaProperty =
            AvaloniaProperty.Register<ImageBlendControl, double>(nameof(Alpha), 1.0);

        public double Alpha
        {
            get => GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public static readonly StyledProperty<Bitmap> OldImageProperty =
            AvaloniaProperty.Register<ImageBlendControl, Bitmap>(nameof(OldImage), null);

        public Bitmap OldImage
        {
            get => GetValue(OldImageProperty);
            set => SetValue(OldImageProperty, value);
        }

        public static readonly StyledProperty<Bitmap> NewImageProperty =
            AvaloniaProperty.Register<ImageBlendControl, Bitmap>(nameof(NewImage), null);

        public Bitmap NewImage
        {
            get => GetValue(NewImageProperty);
            set => SetValue(NewImageProperty, value);
        }

        static ImageBlendControl()
        {
            AffectsMeasure<ImageBlendControl>(OldImageProperty, NewImageProperty);
            AffectsRender<ImageBlendControl>(AlphaProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            var alpha = Alpha;
            var left = OldImage;
            var right = NewImage;
            var drawLeft = left != null && alpha < 1.0;
            var drawRight = right != null && alpha > 0;
            var psize = left == null ? right.PixelSize : left.PixelSize;
            var dpi = left == null ? right.Dpi : left.Dpi;

            using (var rt = new RenderTargetBitmap(psize, dpi))
            {
                var rtRect = new Rect(rt.Size);
                using (var dc = rt.CreateDrawingContext())
                {
                    if (drawLeft)
                    {
                        if (drawRight)
                        {
                            using (dc.PushRenderOptions(RO_SRC))
                            using (dc.PushOpacity(1 - alpha))
                                dc.DrawImage(left, rtRect);

                            using (dc.PushRenderOptions(RO_DST))
                            using (dc.PushOpacity(alpha))
                                dc.DrawImage(right, rtRect);
                        }
                        else
                        {
                            using (dc.PushRenderOptions(RO_SRC))
                            using (dc.PushOpacity(1 - alpha))
                                dc.DrawImage(left, rtRect);
                        }
                    }
                    else if (drawRight)
                    {
                        using (dc.PushRenderOptions(RO_SRC))
                        using (dc.PushOpacity(alpha))
                            dc.DrawImage(right, rtRect);
                    }
                }

                context.DrawImage(rt, rtRect, rect);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var left = OldImage;
            var right = NewImage;

            if (left == null)
                return right == null ? availableSize : GetDesiredSize(right.Size, availableSize);

            if (right == null)
                return GetDesiredSize(left.Size, availableSize);

            var ls = GetDesiredSize(left.Size, availableSize);
            var rs = GetDesiredSize(right.Size, availableSize);
            return ls.Width > rs.Width ? ls : rs;
        }

        private Size GetDesiredSize(Size img, Size available)
        {
            var w = available.Width;
            var h = available.Height;

            var sw = available.Width / img.Width;
            var sh = available.Height / img.Height;
            var scale = Math.Min(sw, sh);

            return new Size(scale * img.Width, scale * img.Height);
        }

        private static readonly RenderOptions RO_SRC = new RenderOptions() { BitmapBlendingMode = BitmapBlendingMode.Source, BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };
        private static readonly RenderOptions RO_DST = new RenderOptions() { BitmapBlendingMode = BitmapBlendingMode.Plus, BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };
    }

    public partial class ImageDiffView : UserControl
    {
        public ImageDiffView()
        {
            InitializeComponent();
        }
    }
}
