//    DotMatrix - TextToContent


using UnityEngine;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {

	/// <summary>
	/// Static class that can be used to convert text (string) to content (two dimensional int array) that can be used as input for DotMatrix display..
	/// 
	/// Note that this class is normally used just internally by TextCommand and in most cases it is better to just use TextCommand directly.
	/// If this class is used to convert text to content, resulting content must be added to display using DisplayModel or ContentCommand.
	/// This however gives much more freedom where to put text on display, or gives possibility to make changes to resulting content before using it.
	/// </summary>
	public class TextToContent {

		/// <summary>
		/// Change line of text (string) to int[,] content.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="textLine">
		/// Text to convert. If text contains linefeed characters, it will split text to multiple rows and default line spacing and text alignment will be used.
		/// </param>
		/// <param name="font">
		/// Font to be used to generate content, one of the values from enum TextCommand.Fonts
		/// </param>
		/// <param name="fixedWidth">
		/// If true, all the characters takes same amount of space. If false, different characters may take different amount of horizontal space.
		/// </param>
		/// <param name="bold">
		/// If true, resulting text characters are bold. If false, then using normal characters.
		/// </param>
		/// <param name="charSpacing">
		/// Amount of dots between each character.
		/// </param>
		public static int[,] getContent(string textLine,
		                                TextCommand.Fonts font,
		                                bool fixedWidth,
		                                bool bold,
		                                int charSpacing) {
			return getContent(splitStringToStringArray(textLine),font,fixedWidth,bold,1,0,charSpacing,1,TextCommand.TextAlignments.Left);
		}

		/// <summary>
		/// Change multiple lines of text (string) to int content.
		/// </summary>
		/// <returns>
		/// Two dimensional int array defining states of dots on each row and column.
		/// </returns>
		/// <param name="textLines">
		/// One of more lines of text.
		/// </param>
		/// <param name="font">
		/// Font to be used to generate content, one of the values from enum TextCommand.Fonts
		/// </param>
		/// <param name="fixedWidth">
		/// If true, all the characters takes same amount of space. If false, different characters may take different amount of horizontal space.
		/// </param>
		/// <param name="bold">
		/// If true, resulting text characters are bold. If false, then using normal characters.
		/// </param>
		/// <param name="textColor">
		/// Color used for any dots that should be on.
		/// </param>
		/// <param name="backColor">
		/// Color used for any dots that should be off.
		/// </param>
		/// <param name="charSpacing">
		/// Amount of dots between each character.
		/// </param>
		/// <param name="lineSpacing">
		/// Amount of dots between each line if there is multiple lines of text.
		/// </param>
		/// <param name="multiLineTextAlignment">
		/// Text alignment in case there is multiple lines of text, one of the values from enum TextCommand.TextAlignments
		/// </param>
		public static int[,] getContent(string[] textLines,
		                                TextCommand.Fonts font,
		                                bool fixedWidth,
		                                bool bold,
		                                int textColor,
		                                int backColor,
		                                int charSpacing,
		                                int lineSpacing,
		                                TextCommand.TextAlignments multiLineTextAlignment) {

			CharacterDef characterDef;
			if (font==TextCommand.Fonts.Large) {
				characterDef=new CharacterDef_9();
			} else if (font==TextCommand.Fonts.Small) {
				characterDef=new CharacterDef_5();
			} else {
				characterDef=new CharacterDef_7();
			}

			int rowCount=textLines.Length;
			int rowHeight=characterDef.getFontHeight();
			int totalHeight=rowHeight*rowCount+lineSpacing*(rowCount-1);
			int[] rowWidths=new int[rowCount];
			int totalWidth=0;
			for (int n=0; n<rowCount; n++) {
				int charCount=textLines[n].Length;
				rowWidths[n]=Mathf.Max(charSpacing*(charCount-1),0);
				for (int m=0; m<textLines[n].Length; m++) {
					rowWidths[n]+=characterDef.getCharWidth(textLines[n][m],fixedWidth,bold);
					if (rowWidths[n]>totalWidth) {
						totalWidth=rowWidths[n];
					}
				}
			}
			
			int[,] content=new int[totalHeight,totalWidth];
			for (int y=0; y<totalHeight; y++) {
				for (int x=0; x<totalWidth; x++) {
					content[y,x]=backColor;
				}
			}

			for (int row=0; row<rowCount; row++) {
				int startX=0;
				if (multiLineTextAlignment==TextCommand.TextAlignments.Right) {
					startX=totalWidth-rowWidths[row];
				} else if (multiLineTextAlignment==TextCommand.TextAlignments.Center) {
					startX=(totalWidth-rowWidths[row])/2;
				}
				int x=0;
				for (int col=0; col<textLines[row].Length; col++) {
					addCharToContent(textLines[row][col],characterDef,fixedWidth,bold,content,startX+x,(rowHeight+lineSpacing)*row,textColor,backColor);
					x+=characterDef.getCharWidth(textLines[row][col],fixedWidth,bold)+charSpacing;
				}
			}
			
			return content;

		}

		internal static string[] splitStringToStringArray(string text) {
			string[] lines=text.Split(new char[]{'\n','\r'},System.StringSplitOptions.None);
			return lines;
		}

		private static void addCharToContent(char chr, CharacterDef characterDef, bool fixedWidth, bool bold, int[,] content, int left, int top, int textColor, int backColor) {

			// Actual width (and height) to be used
			int charWidth=characterDef.getCharWidth(chr,fixedWidth,bold); // Taking count 'fixedWidth' and 'bold'
			int charHeight=characterDef.getFontHeight();

			// Possible padding for non-fixed width text
			int charRealWidth;
			int paddingLeft;
			if (fixedWidth) {
				charRealWidth=characterDef.getCharWidth(chr,false,bold); // If not using fixed width
				paddingLeft=(charWidth-charRealWidth)/2; // Padding around to make character always using same space
			} else {
				charRealWidth=charWidth;
				paddingLeft=0;
			}

			// Get character source and index
			object[] sourceAndIndex=characterDef.getSourceAndIndex(chr);

			// Unknown character?
			if (sourceAndIndex==null) {
				// Fill area with solid square
				for (int y=0; y<charHeight; y++) {
					for (int x=0; x<charWidth; x++) {
						content[top+y,left+x]=textColor;
					}
				}
				return;
			}

			// Copy character dots (plain without bold)
			string[,] source=(string[,])(sourceAndIndex[0]);
			int index=(int)(sourceAndIndex[1]);
			for (int y=0; y<charHeight; y++) {
				for (int x=0; x<charWidth-(bold?1:0); x++) {
					content[top+y,left+x] = ((x>=paddingLeft && x<paddingLeft+charRealWidth && x-paddingLeft<source[y,index].Length && source[y,index][x-paddingLeft]=='#') ? textColor : backColor);
				}
			}

			// Turn bold if needed
			if (bold) {
				bool[] boldCols=new bool[charWidth]; // 'charWidth' is minimum 2 because bolding is already taken count to width
				boldCols[0]=false;
				boldCols[charWidth-1]=true;
				if (charWidth>2) {
					boldCols[charWidth-2]=true;
					for (int x=1; x<charWidth-2; x++) {
						boldCols[x]=true;
						bool oneOcc=false;
						for (int y=0; y<charHeight; y++) {
							if (content[top+y,left+x-1]==textColor && content[top+y,left+x]==backColor && content[top+y,left+x+1]==textColor) {
								if (oneOcc) {
									boldCols[x]=false;
									break;
								}
								oneOcc=true;
							}
						}
					}
				}
				for (int x=charWidth-1; x>=0; x--) {
					if (boldCols[x]) {
						for (int y=0; y<charHeight; y++) {
							if (content[top+y,left+x-1]==textColor) {
								content[top+y,left+x]=textColor;
							}
						}
					}
				}
			}

		}
		
	}

}
