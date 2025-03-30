//    DotMatrix - Characters


namespace Leguar.DotMatrix.Internal {

	/// <remarks>
	/// This class doesn't contain all the same characters as 7 and 9 dot high fonts.
	/// 
	/// Missing symbols: # € $ @ ~
	/// Missing extra characters: Ã Æ Õ Ø Œ Ñ Š Ž
	/// </remarks>
	internal class CharacterDef_5 : CharacterDef {
		
		private const int FONT_HEIGHT=5;
		private const int FONT_WIDTH=3; // Fixed width (also maximum width)

		private const string UPPERCASE_CHARS="ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private static readonly string[,] UPPERCASE_CHARS_DOTS={
			{" # ","## "," ##","## ","###","###"," ##","# #","#","  #","# #","#  ","# #","# #"," # ","## "," # ","## "," ##","###","# #","# #","# #","# #","# #","###"},
			{"# #","# #","#  ","# #","#  ","#  ","#  ","# #","#","  #","# #","#  ","###","###","# #","# #","# #","# #","#  "," # ","# #","# #","# #","# #","# #","  #"},
			{"###","## ","#  ","# #","## ","## ","# #","###","#","  #","## ","#  ","# #","###","# #","## ","# #","## "," # "," # ","# #","# #","# #"," # "," # "," # "},
			{"# #","# #","#  ","# #","#  ","#  ","# #","# #","#","# #","# #","#  ","# #","###","# #","#  ","###","# #","  #"," # ","# #","## ","###","# #"," # ","#  "},
			{"# #","## "," ##","## ","###","#  "," # ","# #","#"," # ","# #","###","# #","# #"," # ","#  "," ##","# #","## "," # "," # ","#  ","# #","# #"," # ","###"}};

		private const string NUMBERS="0123456789";
		private static readonly string[,] NUMBERS_DOTS={
			{" # "," #","## ","## ","# #","###"," # ","###"," # "," # "},
			{"# #","##","  #","  #","# #","#  ","#  ","  #","# #","# #"},
			{"# #"," #"," # "," # ","###","## ","## "," # "," # "," ##"},
			{"# #"," #","#  ","  #","  #","  #","# #","#  ","# #","  #"},
			{" # "," #","###","## ","  #","## "," # ","#  "," # "," # "}};

		private const string SYMBOLS=" .,!?:;-_'\"%&/\\()<>=+*|[]{}^°¡¿`\u00B4\u2019";
		private static readonly string[,] SYMBOLS_DOTS={
			{"   "," ","  ","#","## "," ","  ","   ","   ","#","# #","# #"," # ","  #","#  "," #","# ","  #","#  ","   ","   ","   ","#","##","##"," ##","## "," # "," # ","#"," # ","# "," #"," #"},
			{"   "," ","  ","#","  #","#"," #","   ","   ","#","# #","  #","# #","  #","#  ","# "," #"," # "," # ","###"," # ","# #","#","# "," #"," # "," # ","# #","# #"," ","   "," #","# ","# "},
			{"   "," ","  ","#"," # "," ","  ","###","   "," ","   "," # "," # "," # "," # ","# "," #","#  ","  #","   ","###"," # ","#","# "," #","#  ","  #","   "," # ","#"," # ","  ","  ","  "},
			{"   "," "," #"," ","   ","#"," #","   ","   "," ","   ","#  ","# #","#  ","  #","# "," #"," # "," # ","###"," # ","# #","#","# "," #"," # "," # ","   ","   ","#","#  ","  ","  ","  "},
			{"   ","#","# ","#"," # "," ","# ","   ","###"," ","   ","# #"," ##","#  ","  #"," #","# ","  #","#  ","   ","   ","   ","#","##","##"," ##","## ","   ","   ","#"," ##","  ","  ","  "}};

		private const string UPPERCASE_EXTRAS="\u00C1\u00C0\u00C2\u00C4\u00C5\u00C9\u00C8\u00CA\u00CB\u00CD\u00CC\u00CE\u00CF\u00D3\u00D2\u00D4\u00D6\u00DA\u00D9\u00DB\u00DC\u00DD\u0178\u1E9E\u00C7"; // ÁÀÂÄÅÉÈÊËÍÌÎÏÓÒÔÖÚÙÛÜÝŸẞÇ
		private static readonly string[,] UPPERCASE_EXTRAS_DOTS= {
			{"  #","#  "," # ","# #"," # ","  #","#  "," # ","# #"," #","# "," # ","# #"," # "," # "," # ","# #","  #","#  "," # ","# #","  #","# #","## "," ##"},
			{" # "," # ","# #","   ","   "," # "," # ","# #","   ","# "," #","# #","   ","#  ","  #","# #","   "," # "," # ","# #","   "," # ","   ","# #","#  "},
			{"###","###","###","###","###","###","###","###","###"," #","# "," # "," # ","###","###","###","###","# #","# #","# #","# #","# #","# #","## ","#  "},
			{"###","###","###","###","###","## ","## ","## ","## "," #","# "," # "," # ","# #","# #","# #","# #","# #","# #","# #","# #"," # "," # ","# #"," ##"},
			{"# #","# #","# #","# #","# #","###","###","###","###"," #","# "," # "," # ","###","###","###","###","###","###","###","###"," # "," # ","###"," # "}};

		internal override int getFontHeight() {
			return FONT_HEIGHT;
		}
		
		internal override int getFontWidth() {
			return FONT_WIDTH;
		}

		internal override object[] getSourceAndIndex(char chr) {
			// This font supports only upper case letters
			chr=base.toUpper(chr);
			// Character index
			int index=UPPERCASE_CHARS.IndexOf(chr);
			if (index>=0) {
				return (new object[]{UPPERCASE_CHARS_DOTS,index});
			}
			index=NUMBERS.IndexOf(chr);
			if (index>=0) {
				return (new object[]{NUMBERS_DOTS,index});
			}
			index=SYMBOLS.IndexOf(chr);
			if (index>=0) {
				return (new object[]{SYMBOLS_DOTS,index});
			}
			index=UPPERCASE_EXTRAS.IndexOf(chr);
			if (index>=0) {
				return (new object[]{UPPERCASE_EXTRAS_DOTS,index});
			}
			// Unknown character
			return null;
		}

	}

}
