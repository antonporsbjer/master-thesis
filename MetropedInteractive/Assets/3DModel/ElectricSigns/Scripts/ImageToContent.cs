//    DotMatrix - ImageToContent


using UnityEngine;

namespace Leguar.DotMatrix {

	/// <summary>
	/// Static class that can be used to convert image (texture) to content (two dimensional int array) that can be used as input for DotMatrix display.
	/// </summary>
	public class ImageToContent {

		/// <summary>
		/// List of possible resizing modes that can be used when changing texture to content. Default is None.
		/// Note that to preserve original colors of input texture, this resizing is just pixel resize without any smoothing or interpolation.
		/// </summary>
		public enum ResizeModes {
			/// <summary> No resizing is done. Resulting content is exactly same size as input texture.</summary>
			NoResizing,
			/// <summary> Fit texture to display size. Resulting content is exactly same size as display where it will be added. Both shrinking and enlarging may be done and texture may lose its original aspect ratio.</summary>
			FitToWholeDisplay,
			/// <summary> Shrink if needed to fit inside display while keeping aspect ratio. Resulting content may be smaller than display but will always fit inside display.</summary>
			ShrinkKeepingAspectRatio
		}

		/// <summary>
		/// Change texture to int[,] type content.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="texture">
		/// Texture to convert. Make sure texture is set to be readable in Unity.
		/// </param>
		/// <param name="targetDisplay">
		/// Sprite based display component of the DotMatrix where this content will be added. Palette to use will be read from this display.
		/// </param>
		public static int[,] getContent(Texture2D texture, Display_Sprite targetDisplay) {
			return getContent(texture, targetDisplay, ResizeModes.NoResizing);
		}

		/// <summary>
		/// Change texture to int[,] type content, with possible resizing.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="texture">
		/// Texture to convert. Make sure texture is set to be readable in Unity.
		/// </param>
		/// <param name="targetDisplay">
		/// Sprite based display component of the DotMatrix where this content will be added. Palette to use will be read from this display, as well as display size if 'resizeMode' is other than ResizeModes.None.
		/// </param>
		/// <param name="resizeMode">
		/// Possible resize mode, one of the values from enum ResizeModes.
		/// </param>
		public static int[,] getContent(Texture2D texture, Display_Sprite targetDisplay, ResizeModes resizeMode) {

			int colorCount = targetDisplay.ColorCount;
			Color32[] palette = new Color32[colorCount];
			for (int n = 0; n<colorCount; n++) {
				palette[n]=targetDisplay.GetColor(n);
			}

			return getContent(texture, palette, targetDisplay.WidthInDots, targetDisplay.HeightInDots, resizeMode);

		}

		/// <summary>
		/// Change texture to int[,] type content.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="texture">
		/// Texture to convert. Make sure texture is set to be readable in Unity.
		/// </param>
		/// <param name="targetDisplay">
		/// Image based display component of the DotMatrix where this content will be added. Palette to use will be read from this display.
		/// </param>
		public static int[,] getContent(Texture2D texture, Display_UI targetDisplay) {
			return getContent(texture, targetDisplay, ResizeModes.NoResizing);
		}

		/// <summary>
		/// Change texture to int[,] type content, with possible resizing.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="texture">
		/// Texture to convert. Make sure texture is set to be readable in Unity.
		/// </param>
		/// <param name="targetDisplay">
		/// Image based display component of the DotMatrix where this content will be added. Palette to use will be read from this display, as well as display size if 'resizeMode' is other than ResizeModes.None.
		/// </param>
		/// <param name="resizeMode">
		/// Possible resize mode, one of the values from enum ResizeModes.
		/// </param>
		public static int[,] getContent(Texture2D texture, Display_UI targetDisplay, ResizeModes resizeMode) {

			int colorCount = targetDisplay.ColorCount;
			Color32[] palette = new Color32[colorCount];
			for (int n = 0; n<colorCount; n++) {
				palette[n]=targetDisplay.GetColor(n);
			}

			return getContent(texture, palette, targetDisplay.WidthInDots, targetDisplay.HeightInDots, resizeMode);

		}

		private static int[,] getContent(Texture2D texture, Color32[] palette, int displayWidth, int displayHeight, ResizeModes resizeMode) {

			int textureWidth = texture.width;
			int textureHeight = texture.height;

			int contentWidth = textureWidth;
			int contentHeight = textureHeight;
			bool doResize = false;

			if (resizeMode==ResizeModes.FitToWholeDisplay) {
				if (textureWidth!=displayWidth || textureHeight!=displayHeight) {
					contentWidth=displayWidth;
					contentHeight=displayHeight;
					doResize=true;
				}
			} else if (resizeMode==ResizeModes.ShrinkKeepingAspectRatio) {
				if (textureWidth>displayWidth || textureHeight>displayHeight) {
					float xD = 1f*displayWidth/textureWidth;
					float yD = 1f*displayHeight/textureHeight;
					float minD = Mathf.Min(xD, yD);
					contentWidth=(int)(textureWidth*minD);
					contentHeight=(int)(textureHeight*minD);
					doResize=true;
				}
			}

			Color32[,] colors = getPixelColors(texture, textureWidth, textureHeight, contentWidth, contentHeight, doResize);

			int[,] content = new int[contentHeight, contentWidth];

			bool anyColorDiff = false;
			for (int y = 0; y<contentHeight; y++) {
				for (int x = 0; x<contentWidth; x++) {
					content[y, x] = getContentPixel(colors[y,x], palette, false);
					if (content[y, x] != content[0, 0]) {
						anyColorDiff = true;
					}
				}
			}

			if (!anyColorDiff) {
				int[,] fallbackContent = new int[contentHeight, contentWidth];
				for (int y = 0; y<contentHeight; y++) {
					for (int x = 0; x<contentWidth; x++) {
						fallbackContent[y, x] = getContentPixel(colors[y, x], palette, true);
						if (fallbackContent[y, x] != fallbackContent[0, 0]) {
							anyColorDiff = true;
						}
					}
				}
				if (anyColorDiff) {
					content = fallbackContent;
				}
			}

			return content;

		}

		private static int getContentPixel(Color32 forColor, Color32[] palette, bool rgbMaxBrightnessOnly) {
			int bestIndex = 0;
			float bestDiff = getDifference(forColor, palette[0], rgbMaxBrightnessOnly);
			for (int n = 1; n<palette.Length; n++) {
				float diff = getDifference(forColor, palette[n], rgbMaxBrightnessOnly);
				if (diff<bestDiff) {
					bestIndex=n;
					bestDiff=diff;
				}
			}
			return bestIndex;
		}

		private static float getDifference(Color32 c1, Color32 c2, bool rgbMaxBrightnessOnly) {
			if (rgbMaxBrightnessOnly) {
				float max1 = Mathf.Max(c1.r, Mathf.Max(c1.g, c1.b));
				float max2 = Mathf.Max(c2.r, Mathf.Max(c2.g, c2.b));
				return Mathf.Abs(max1-max2);
			}
			float ra = (c1.r+c2.r)*0.5f;
			float dr = c1.r-c2.r;
			float dg = c1.g-c2.g;
			float db = c1.b-c2.b;
//			float cs = dr*dr+dg*dg+db*db;
//			float cs;
//			if (ra<128f) {
//				cs = 2f*dr*dr+4f*dg*dg+3f*db*db;
//			} else {
//				cs = 3f*dr*dr+4f*dg*dg+2f*db*db;
//			}
			float cs = (2f+ra/256f)*dr*dr+4f*dg*dg+(2f+(255f-ra)/256f)*db*db;
			return cs;
		}

		private static Color32[,] getPixelColors(Texture2D texture, int textureWidth, int textureHeight, int contentWidth, int contentHeight, bool doResize) {

			Color32[] pixels = texture.GetPixels32();

			Color32[,] colors = new Color32[contentHeight, contentWidth];
			for (int y = 0; y<contentHeight; y++) {
				for (int x = 0; x<contentWidth; x++) {
					if (doResize) {
						int tx = (int)(1f*x/(contentWidth-1f)*(textureWidth-1f)+0.5f);
						int ty = (int)(1f*y/(contentHeight-1f)*(textureHeight-1f)+0.5f);
						colors[y, x] = pixels[(textureHeight-1-ty)*textureWidth+tx];
					} else {
						colors[y, x] = pixels[(contentHeight-1-y)*contentWidth+x];
					}
				}
			}

			return colors;

		}

	}

}
