using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace PicToText;

internal static class OcrService
{
    public static async Task<string> RecognizeAsync(Bitmap bitmap)
    {
        using var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
        var pixels = GetPixels(bitmap);

        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Ignore,
            (uint)bitmap.Width,
            (uint)bitmap.Height,
            96,
            96,
            pixels);
        await encoder.FlushAsync();
        stream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(stream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Gray8,
            BitmapAlphaMode.Ignore);

        var engine = OcrEngine.TryCreateFromUserProfileLanguages()
            ?? throw new InvalidOperationException("No Windows OCR language is installed. Add a language pack in Windows Settings.");
        var result = await engine.RecognizeAsync(softwareBitmap);
        return result.Text.Trim();
    }

    private static byte[] GetPixels(Bitmap source)
    {
        using var normalized = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(normalized))
            graphics.DrawImageUnscaled(source, 0, 0);

        var rectangle = new Rectangle(0, 0, normalized.Width, normalized.Height);
        var data = normalized.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var rowBytes = normalized.Width * 4;
            var pixels = new byte[rowBytes * normalized.Height];
            for (var y = 0; y < normalized.Height; y++)
            {
                var sourceRow = data.Stride >= 0 ? y : normalized.Height - 1 - y;
                Marshal.Copy(data.Scan0 + sourceRow * data.Stride, pixels, y * rowBytes, rowBytes);
            }
            return pixels;
        }
        finally
        {
            normalized.UnlockBits(data);
        }
    }
}
