using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ElectronBotExpressionController;

internal static class ImageFrameConverter
{
	public static byte[] ToBotBgrBuffer(string path)
	{
		using Image<Rgba32> image = Image.Load<Rgba32>(path);
		image.Mutate(delegate(IImageProcessingContext x)
		{
			x.Resize(240, 240);
		});
		byte[] array = new byte[172800];
		for (int num = 0; num < image.Height; num++)
		{
			for (int num2 = 0; num2 < image.Width; num2++)
			{
				Rgba32 rgba = image[num2, num];
				int num3 = (num * image.Width + num2) * 3;
				array[num3] = rgba.B;
				array[num3 + 1] = rgba.G;
				array[num3 + 2] = rgba.R;
			}
		}
		return array;
	}
}
