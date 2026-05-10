using System.Drawing;

namespace Mawtrix.Functions;

public static class ImageManipulator
{
    public static Bitmap Resize(Image image, int width, int height, bool keepAspectRatio, bool enlargeSmallerImages)
    {
        if (!enlargeSmallerImages && image.Width <= width && image.Height <= height)
        {
            return new Bitmap(image);
        }
        if (!keepAspectRatio)
        {
            return new Bitmap(image, width, height);
        }
        else
        {
            double aspectRatio = image.Width / (double)image.Height;
            double newAspectRatio = width / height;

            if (aspectRatio >= newAspectRatio) //fit horizontally
            {
                double scale = image.Width / (double)width;
                int newHeight = (int)(image.Height / scale);
                return new Bitmap(image, width, newHeight);
            }
            else //fit vertically
            {
                double scale = image.Height / (double)height;
                int newWidth = (int)(image.Width / scale);
                return new Bitmap(image, newWidth, height);
            }
        }
    }
}