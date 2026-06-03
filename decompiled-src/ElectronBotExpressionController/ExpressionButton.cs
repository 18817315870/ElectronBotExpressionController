using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ElectronBotExpressionController;

internal sealed class ExpressionButton : Button
{
	public ExpressionButton(ExpressionItem item)
	{
		base.Width = 105;
		base.Height = 124;
		base.Margin = new Padding(5);
		Text = item.Title;
		TextAlign = ContentAlignment.BottomCenter;
		base.ImageAlign = ContentAlignment.TopCenter;
		base.TextImageRelation = TextImageRelation.ImageAboveText;
		base.FlatStyle = FlatStyle.Flat;
		base.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
		base.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
		Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular);
		BackColor = Color.White;
		base.Image = Thumbnail(item.ImagePath);
	}

	private static Bitmap Thumbnail(string path)
	{
		using Image image = System.Drawing.Image.FromFile(path);
		Bitmap bitmap = new Bitmap(94, 78);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.Black);
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.DrawImage(image, new Rectangle(0, 0, 94, 78));
		return bitmap;
	}
}
