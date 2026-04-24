using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace VAWCV5Tubod
{
    internal static class PrintWatermarkHelper
    {
        private const string DefaultWatermarkText = "VAWC TUBOD";

        public static void Draw(Graphics graphics, RectangleF bounds, string? text = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (bounds.Width <= 0f || bounds.Height <= 0f)
            {
                return;
            }

            string requestedText = text == null ? string.Empty : text.Trim();
            string watermarkText = requestedText.Length == 0
                ? DefaultWatermarkText
                : requestedText;

            GraphicsState state = graphics.Save();

            try
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                float fontSize = Math.Max(36f, Math.Min(bounds.Width, bounds.Height) / 8f);
                using Font font = new("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Point);
                SizeF textSize = graphics.MeasureString(watermarkText, font);

                graphics.TranslateTransform(
                    bounds.Left + (bounds.Width / 2f),
                    bounds.Top + (bounds.Height / 2f));
                graphics.RotateTransform(-35f);

                using Brush brush = new SolidBrush(Color.FromArgb(35, 40, 40, 40));
                graphics.DrawString(
                    watermarkText,
                    font,
                    brush,
                    -(textSize.Width / 2f),
                    -(textSize.Height / 2f));
            }
            finally
            {
                graphics.Restore(state);
            }
        }
    }
}
