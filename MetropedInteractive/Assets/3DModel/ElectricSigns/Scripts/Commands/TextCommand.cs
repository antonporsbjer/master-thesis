//    DotMatrix - Command - Text


using UnityEngine;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Command to add text on display. This is most common command to be added to Controller.
	/// </summary>
	/// <remarks>
	/// Characters are defined in internal class Characters. In case changes or additions to available characters are needed,
	/// they have to be changed to Characters class source directly.
	/// </remarks>
	public class TextCommand : AbsCmdPosition {

		/// <summary>
		/// List of differents fonts that can be used to display text.
		/// </summary>
		public enum Fonts {
			/// <summary>Large 9*5 (height*width) font that also have lower case characters.</summary>
			Large,
			/// <summary>Normal 7*5 (height*width) font that have only upper case characters.</summary>
			Normal,
			/// <summary>Small 5*3 (height*width) font that have only upper case characters.</summary>
			Small
		}
		
		private Fonts font;
		private bool fontSet;

		/// <summary>
		/// Set or get font used for this text. If font is not set, Controller default is used.
		/// </summary>
		/// <value>
		/// Wanted font, one of the values from enum Fonts.
		/// </value>
		public Fonts Font {
			set {
				font=value;
				fontSet=true;
			}
			get {
				if (!fontSet) {
					Debug.LogError("Unable to get command 'Font' since value is not set. Command will use Controller default font when executed.");
				}
				return font;
			}
		}

		/// <summary>
		/// List of possible text alignments in case there's multiple lines of text. 
		/// </summary>
		public enum TextAlignments {
			/// <summary>Text is aligned to center (compared to other lines of text).</summary>
			Center,
			/// <summary>Text is aligned to left (all lines first character starts from same position).</summary>
			Left,
			/// <summary>Text is aligned to right (all lines last character ends to same position).</summary>
			Right
		}
		
		private TextAlignments textAlignment;
		private bool textAlignmentSet;
		
		/// <summary>
		/// Set or get text alignment. This have effect only if there is multiple lines of text.
		/// By default text alignment follows content horizontal position. For example, if content horizontal position is right,
		/// also multiple lines of text are aligned to right. In most cases this is wanted and there is no need to set TextAlignment.
		/// </summary>
		/// <value>
		/// Wanted text alignment, one of the values from enum TextAlignments.
		/// </value>
		public TextAlignments TextAlignment {
			set {
				textAlignment=value;
				textAlignmentSet=true;
			}
			get {
				if (!textAlignmentSet) {
					Debug.LogError("Unable to get command 'TextAlignment' since value is not set. Command will use content HorPosition when executed.");
				}
				return textAlignment;
			}
		}

		private int charSpacing;
		
		/// <summary>
		/// Set or get character spacing. Default is 1.
		/// </summary>
		/// <value>
		/// Character spacing in dots.
		/// </value>
		public int CharSpacing {
			set {
				charSpacing=value;
			}
			get {
				return charSpacing;
			}
		}
		
		private int lineSpacing;
		
		/// <summary>
		/// Set or get line spacing. Default is 1.
		/// </summary>
		/// <value>
		/// Line spacing in dots.
		/// </value>
		public int LineSpacing {
			set {
				lineSpacing=value;
			}
			get {
				return lineSpacing;
			}
		}
		
		private bool fixedWidth;
		
		/// <summary>
		/// Set or get text to be fixed width. Default is true (using fixed width).
		/// </summary>
		/// <value>
		/// True if text is fixed width, false otherwise.
		/// </value>
		public bool FixedWidth {
			set {
				fixedWidth=value;
			}
			get {
				return fixedWidth;
			}
		}
		
		private bool bold;
		
		/// <summary>
		/// Set or get text to be bold. Default is false (not bold).
		/// </summary>
		/// <value>
		/// True if text is bold, false otherwise.
		/// </value>
		public bool Bold {
			set {
				bold=value;
			}
			get {
				return bold;
			}
		}
		
		private int textColor;
		
		/// <summary>
		/// Set or get text color. This is index of color, and colors themselves are defined in DotMatrix Display (typically in Unity inspector window).
		/// Default text color is 1. That is default "dot on-state" in display. In case of two-color display that is also only dot on-state.
		/// 
		/// To get inverse text, simply set TextColor to 0 and BackColor to 1.
		/// </summary>
		/// <value>
		/// Text color index.
		/// </value>
		public int TextColor {
			set {
				textColor=value;
			}
			get {
				return textColor;
			}
		}

		private int backColor;
		
		/// <summary>
		/// Set or get text background color. This is index of color, and colors themselves are defined in DotMatrix Display (typically in Unity inspector window).
		/// Default text background color is 0. That is "dot off-state" in display.
		/// </summary>
		/// <value>
		/// Text background color index.
		/// </value>
		public int BackColor {
			set {
				backColor=value;
			}
			get {
				return backColor;
			}
		}

		private string[] textLines;
		
		/// <summary>
		/// Create new text command. By default character spacing is one dot, and used character is 'fixed width' without bolding.
		/// Linebreak characters ('\n' or '\r') will cut the text to multiple lines.
		/// </summary>
		/// <param name="text">
		/// Single or multiple lines of text to be shown on display.
		/// </param>
		public TextCommand(string text) : this(TextToContent.splitStringToStringArray(text)) {
		}
		
		/// <summary>
		/// Create new text command with multiple lines of text. By default character and line spacing are one dot, and used character
		/// is 'fixed width' without bolding.
		/// </summary>
		/// <param name="textLines">
		/// Multiple lines of text to be shown on display.
		/// </param>
		public TextCommand(string[] textLines) {
			this.textLines=textLines;
			fontSet=false;
			textAlignmentSet=false;
			charSpacing=1;
			lineSpacing=1;
			fixedWidth=true;
			bold=false;
			textColor=1;
			backColor=0;
		}
		
		internal override RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller) {

			// Choose font and text alignment

			Fonts fontToUse=(fontSet?font:controller.DefaultTextFont);

			TextAlignments textAlignmentToUse;
			if (textAlignmentSet) {
				textAlignmentToUse=textAlignment;
			} else {
				if (this.HorPosition==HorPositions.Right) {
					textAlignmentToUse=TextAlignments.Right;
				} else if (this.HorPosition==HorPositions.Center) {
					textAlignmentToUse=TextAlignments.Center;
				} else {
					textAlignmentToUse=TextAlignments.Left;
				}
			}

			// Get content
			int[,] content=TextToContent.getContent(textLines,fontToUse,fixedWidth,bold,textColor,backColor,charSpacing,lineSpacing,textAlignmentToUse);

			// Create raw command with this content
			return base.getRawCommand(dotMatrix,displayModel,controller,content);
			
		}

	}

}
