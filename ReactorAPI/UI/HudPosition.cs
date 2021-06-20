using ReactorAPI.Extensions;
using System;
using UnityEngine;

namespace ReactorAPI.UI
{
    public class HudPosition
    {
        public static int PixelWidth { get; private set; }
        public static int PixelHeight { get; private set; }
        public static float Width { get; private set; }
        public static float Height { get; private set; }
        public static Vector2 TopLeft { get; private set; }
        public static Vector2 TopRight { get; private set; }
        public static Vector2 BottomLeft { get; private set; }
        public static Vector2 BottomRight { get; private set; }
        public static Vector2 Top { get; private set; }
        public static Vector2 Bottom { get; private set; }
        public static Vector2 Left { get; private set; }
        public static Vector2 Right { get; private set; }

        internal static void Load()
        {
            Events.HudCreated += HudUpdated;
            Events.HudUpdated += HudUpdated;
        }

        private static void HudUpdated(object sender, EventArgs e)
        {
            if (PixelWidth == Camera.main.pixelWidth && PixelHeight == Camera.main.pixelHeight) return;

            int oldPixelWidth = PixelWidth, oldPixelHeight = PixelHeight;
            float oldWidth = Width, oldHeight = Height;

            PixelWidth = Camera.main.pixelWidth;
            PixelHeight = Camera.main.pixelHeight;

            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) - Camera.main.transform.localPosition;

            Width = -bottomLeft.x * 2;
            Height = -bottomLeft.y * 2;

            BottomLeft = bottomLeft;
            BottomRight = GetAlignedOffset(BottomLeft, HudAlignment.BottomRight);
            TopLeft = GetAlignedOffset(BottomLeft, HudAlignment.TopLeft);
            TopRight = GetAlignedOffset(BottomLeft, HudAlignment.TopRight);
            Top = new Vector2(0, -BottomLeft.y);
            Bottom = new Vector2(0, BottomLeft.y);
            Left = new Vector2(BottomLeft.x, 0);
            Right = new Vector2(-BottomLeft.x, 0);

            if (oldPixelWidth != 0 && oldPixelHeight != 0) Events.RaiseResolutionChanged(oldPixelWidth, oldPixelHeight, oldWidth, oldHeight);
        }

        public static Vector2 GetAlignmentVector(HudAlignment alignment)
        {
            return alignment switch
            {
                HudAlignment.BottomLeft => BottomLeft,
                HudAlignment.BottomRight => BottomRight,
                HudAlignment.TopLeft => TopLeft,
                HudAlignment.TopRight => TopRight,
                HudAlignment.Bottom => Bottom,
                HudAlignment.Top => Top,
                HudAlignment.Left => Left,
                HudAlignment.Right => Right,
                _ => BottomLeft,
            };
        }

        public static Vector2 GetAlignedOffset(Vector2 offset, HudAlignment alignment)
        {
            return alignment switch
            {
                HudAlignment.BottomLeft => offset,
                HudAlignment.BottomRight => new Vector2(-offset.x, offset.y),
                HudAlignment.TopLeft => new Vector2(offset.x, -offset.y),
                HudAlignment.TopRight => new Vector2(-offset.x, -offset.y),
                HudAlignment.Bottom => offset,
                HudAlignment.Top => new Vector2(offset.x, -offset.y),
                HudAlignment.Left => offset,
                HudAlignment.Right => new Vector2(-offset.x, offset.y),
                _ => offset,
            };
        }

        public Vector2 Offset { get; set; }
        public HudAlignment Alignment { get; set; }

        public HudPosition(Vector2 offset, HudAlignment alignment = HudAlignment.BottomLeft)
        {
            Offset = offset;

            Alignment = alignment;
        }

        public HudPosition(float xOffset = 0F, float yOffset = 0F, HudAlignment alignment = HudAlignment.BottomLeft) : this(new Vector2(xOffset, yOffset), alignment)
        {
        }

        public HudPosition(HudAlignment alignment) : this(Vector2.zero, alignment)
        {
        }

        private Vector2 GetAlignmentVector()
        {
            return GetAlignmentVector(Alignment);
        }

        private Vector2 GetAlignedOffset()
        {
            return GetAlignedOffset(Offset, Alignment);
        }

        public Vector2 GetVector()
        {
            return GetAlignmentVector() + GetAlignedOffset();
        }

        public Vector3 GetVector3(float z = 0F)
        {
            return GetVector().ToVector3(z);
        }

        public static implicit operator HudPosition(Vector2 offset)
        {
            return new HudPosition(offset);
        }

        public static implicit operator Vector2(HudPosition uiPos)
        {
            return uiPos.GetVector();
        }

        public static implicit operator Vector3(HudPosition uiPos)
        {
            return uiPos.GetVector();
        }
    }
}