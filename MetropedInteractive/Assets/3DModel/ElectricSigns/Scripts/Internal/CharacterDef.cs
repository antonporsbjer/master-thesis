//    DotMatrix - Characters


namespace Leguar.DotMatrix.Internal {
	
	internal abstract class CharacterDef {
		
		internal abstract int getFontHeight();
		internal abstract int getFontWidth();
		internal abstract object[] getSourceAndIndex(char chr);

		internal int getCharWidth(char chr, bool fixedWidth, bool bold) {
			int fontWidth=this.getFontWidth()+(bold?1:0);
			if (fixedWidth) {
				return fontWidth;
			}
			object[] sourceAndIndex=this.getSourceAndIndex(chr);
			if (sourceAndIndex==null) {
				return fontWidth;
			}
			string[,] source=(string[,])(sourceAndIndex[0]);
			int index=(int)(sourceAndIndex[1]);
			return (source[0,index].Length+(bold?1:0));
		}

		internal char toUpper(char possiblyLowerChar) {
			// Use manual uppercasing for special characters to avoid any possible problems with system localization
			int index=CharacterDef_9.LOWERCASE_EXTRAS.IndexOf(possiblyLowerChar);
			if (index>=0) {
				return CharacterDef_9.UPPERCASE_EXTRAS[index];
			}
			// Default system character to upper
			return char.ToUpper(possiblyLowerChar);
		}

	}

}
