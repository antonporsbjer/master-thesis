//    DotMatrix - DisplayModel


using UnityEngine;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Class that contains current data (state of all dots) of the display. Any changes made here will be updated
	/// on display view in next update loop. This data tells what should be visible on actual display on screen.
	/// Actual display however may have broken or delayed pixels, or visible display is not yet updated to match this data.
	/// 
	/// This class doesn't have public constructor. Instance of this class is automatically created when DotMatrix initializes.
	/// After that reference to this class can be get from DotMatrix using GetDisplayModel() method.
	/// 
	/// In most cases there is no need to access this class directly but use Controller instead.
	/// </summary>
	public class DisplayModel {
		
		private int width;
		private int height;
		
		/// <summary>
		/// Gets the width of display.
		/// </summary>
		/// <value>
		/// Width of display in dots.
		/// </value>
		public int Width {
			get {
				return width;
			}
		}
		
		/// <summary>
		/// Gets the height of display.
		/// </summary>
		/// <value>
		/// Height of display in dots.
		/// </value>
		public int Height {
			get {
				return height;
			}
		}
		
		private int[,] content;
		private bool changes;
		
		private int offsetX;
		private int offsetY;
		
		internal DisplayModel(int width, int height) {
			this.width=width;
			this.height=height;
			content=new int[height,width];
			offsetX=0;
			offsetY=0;
			changes=false;
		}
		
		/// <summary>
		/// Set state of single dot to on or off.
		/// </summary>
		/// <param name="x">
		/// X coordinate of dot to set, 0 is left.
		/// </param>
		/// <param name="y">
		/// Y coordinate of dot to set, 0 is top.
		/// </param>
		/// <param name="on">
		/// True to set on, false to set off.
		/// </param>
		public void SetDot(int x, int y, bool on) {
			SetDot(x,y,(on?1:0));
		}

		/// <summary>
		/// Set state of single dot to any state. In two color display there is only two states, 0 (off) and 1 (on).
		/// Additional states are usually additional colors.
		/// </summary>
		/// <param name="x">
		/// X coordinate of dot to set, 0 is left.
		/// </param>
		/// <param name="y">
		/// Y coordinate of dot to set, 0 is top.
		/// </param>
		/// <param name="state">
		/// New state for dot.
		/// </param>
		public void SetDot(int x, int y, int state) {
			content[(offsetY+y)%height,(offsetX+x)%width]=state;
			changes=true;
		}

		/// <summary>
		/// Set new content to whole display.
		/// </summary>
		/// <remarks>
		/// This method is here mostly for backward compatibility. Rather use method SetFullContent(int[,])
		/// </remarks>
		/// <param name="newContent">
		/// New content, height and width must match display size.
		/// </param>
		public void SetContent(bool[,] newContent) {
			if (newContent.GetLength(0)!=height) {
				Debug.LogError("DisplayModel.SetContent(...): New content height ("+newContent.GetLength(0)+") must match display height ("+height+")");
				return;
			}
			if (newContent.GetLength(1)!=width) {
				Debug.LogError("DisplayModel.SetContent(...): New content width ("+newContent.GetLength(1)+") must match display width ("+width+")");
				return;
			}
			for (int y=0; y<height; y++) {
				for (int x=0; x<width; x++) {
					content[y,x]=(newContent[y,x]?1:0);
				}
			}
			offsetX=0;
			offsetY=0;
			changes=true;
		}

		/// <summary>
		/// Set new content to whole display.
		/// </summary>
		/// <param name="newContent">
		/// New content, height and width must match display size.
		/// </param>
		public void SetFullContent(int[,] newContent) {
			if (newContent.GetLength(0)!=height) {
				Debug.LogError("DisplayModel.SetFullContent(...): New content height ("+newContent.GetLength(0)+") must match display height ("+height+")");
				return;
			}
			if (newContent.GetLength(1)!=width) {
				Debug.LogError("DisplayModel.SetFullContent(...): New content width ("+newContent.GetLength(1)+") must match display width ("+width+")");
				return;
			}
//			content=newContent;
			for (int y = 0; y<height; y++) {
				for (int x = 0; x<width; x++) {
					content[y, x]=newContent[y,x];
				}
			}
			offsetX=0;
			offsetY=0;
			changes=true;
		}

		/// <summary>
		/// Set new content to part of the display. Any dots that would go outside of the display will be ignored. So for example parameter
		/// positions (x,y) can be also negative and parameter content can be bigger than display. Any dots in display which this new content
		/// doesn't cover, will remain unchanged.
		/// </summary>
		/// <param name="partialNewContent">
		/// New content to add to certain point of display.
		/// </param>
		/// <param name="posX">
		/// X position of display where leftmost dot of new content is added.
		/// </param>
		/// <param name="posY">
		/// Y position of display where uppermost dot of new content is added.
		/// </param>
		public void SetPartialContent(int[,] partialNewContent, int posX, int posY) {
			int contentHeight=partialNewContent.GetLength(0);
			int contentWidth=partialNewContent.GetLength(1);
			for (int cY=0; cY<contentHeight; cY++) {
				for (int cX=0; cX<contentWidth; cX++) {
					int x=posX+cX;
					int y=posY+cY;
					if (x>=0 && x<width && y>=0 && y<height) {
						content[(y+offsetY)%height,(x+offsetX)%width]=partialNewContent[cY,cX];
					}
				}
			}
			changes=true;
		}

		/// <summary>
		/// Gets the full content of the display (state of every dot) at this very moment.
		/// Returned array is copy of the internal content of display so making changes to returned array doesn't change anything in this display.
		/// </summary>
		/// <param name="target">
		/// Optional target array. If null, new array is created. If not null, content is copied to this array and reference to this same array is also returned.
		/// </param>
		/// <returns>
		/// State of all the dots as table of integers. Upper left dot is at index [0,0].
		/// In actual values 0 is off state, 1 is default on state, 2 and forward are additional states dots can have.
		/// </returns>
		public int[,] GetFullContent(int[,] target) {
			if (target==null) {
				target=new int[height, width];
			}
			for (int y = 0; y<height; y++) {
				for (int x = 0; x<width; x++) {
					target[y,x]=content[(y+offsetY)%height,(x+offsetX)%width];
				}
			}
			return target;
		}

		/// <summary>
		/// Clears the display by setting all dots 'off' (all dots to state 0).
		/// </summary>
		public void Clear() {
			SetAll(0);
		}
		
		/// <summary>
		/// Fills the display by setting all dots 'on' (all dots to state 1).
		/// </summary>
		public void Fill() {
			SetAll(1);
		}

		/// <summary>
		/// Set all the dots in display to certain state.
		/// </summary>
		/// <param name="state">
		/// New state (color/rotation) for all the dots.
		/// </param>
		public void SetAll(int state) {
			for (int y=0; y<height; y++) {
				for (int x=0; x<width; x++) {
					content[y,x]=state;
				}
			}
			offsetX=0; // This is not necessary...
			offsetY=0; // ...but maybe more clear to reset these
			changes=true;
		}

		/// <summary>
		/// Clears the single column of display.
		/// </summary>
		/// <param name="x">
		/// X coordinate of column to clear, 0 is left.
		/// </param>
		public void ClearColumn(int x) {
			SetColumn(x,0);
		}
		
		/// <summary>
		/// Fills the single column of display.
		/// </summary>
		/// <param name="x">
		/// X coordinate of column to fill, 0 is left.
		/// </param>
		public void FillColumn(int x) {
			SetColumn(x,1);
		}
		
		/// <summary>
		/// Set all the dots in single column to certain state.
		/// </summary>
		/// <param name="x">
		/// X coordinate of column to set, 0 is left.
		/// </param>
		/// <param name="state">
		/// New state (color/rotation) for all the dots in this column.
		/// </param>
		public void SetColumn(int x, int state) {
			for (int y=0; y<height; y++) {
				content[y,(offsetX+x)%width]=state;
			}
			changes=true;
		}

		/// <summary>
		/// Clears the single row of display.
		/// </summary>
		/// <param name="y">
		/// Y coordinate of row to clear, 0 is top.
		/// </param>
		public void ClearRow(int y) {
			SetRow(y,0);
		}
		
		/// <summary>
		/// Fills the single row of display.
		/// </summary>
		/// <param name="y">
		/// Y coordinate of row to fill, 0 is top.
		/// </param>
		public void FillRow(int y) {
			SetRow(y,1);
		}
		
		/// <summary>
		/// Set all the dots in single row to certain state.
		/// </summary>
		/// <param name="y">
		/// Y coordinate of row to set, 0 is top.
		/// </param>
		/// <param name="state">
		/// New state (color/rotation) for all the dots in this row.
		/// </param>
		public void SetRow(int y, int state) {
			for (int x=0; x<width; x++) {
				content[(offsetY+y)%height,x]=state;
			}
			changes=true;
		}

		/// <summary>
		/// Check if single dot is on or off.
		/// </summary>
		/// <param name="x">
		/// X coordinate of dot to check, 0 is left.
		/// </param>
		/// <param name="y">
		/// Y coordinate of dot to check, 0 is top.
		/// </param>
		/// <returns>
		/// False if dot state (color/rotation) is 0, true in all other cases.
		/// </returns>
		public bool IsDotOn(int x, int y) {
			return (GetDotState(x,y)>0);
		}

		/// <summary>
		/// Get state (color/rotation) of single dot.
		/// </summary>
		/// <param name="x">
		/// X coordinate of dot to check, 0 is left.
		/// </param>
		/// <param name="y">
		/// Y coordinate of dot to check, 0 is top.
		/// </param>
		/// <returns>
		/// State of the dot. 0 is off state, 1 is default on state, 2 and forward are additional states dot can have.
		/// </returns>
		public int GetDotState(int x, int y) {
			return content[(y+offsetY)%height,(x+offsetX)%width];
		}

		/// <summary>
		/// Check if display is clear (all dots are off).
		/// </summary>
		/// <returns>
		/// True if display is clear, false otherwise.
		/// </returns>
		public bool IsClear() {
			return IsAllDotsInState(0);
		}
		
		/// <summary>
		/// Check if display is full (all dots are on). In display that have more than 1 on-states, it doesn't matter on what on-state dots are as long as they are not off.
		/// </summary>
		/// <returns>
		/// False if there is any dot that is off, true otherwise.
		/// </returns>
		public bool IsFull() {
			for (int y=0; y<height; y++) {
				for (int x=0; x<width; x++) {
					if (content[y,x]>0) {
						return false;
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// Check whatever all the dots in display are in certain state.
		/// </summary>
		/// <param name="state">
		/// State where all the dots have to be for this method to return true.
		/// </param>
		/// <returns>
		/// True if all the dots are in parameter state, false otherwise.
		/// </returns>
		public bool IsAllDotsInState(int state) {
			for (int y=0; y<height; y++) {
				for (int x=0; x<width; x++) {
					if (content[y,x]!=state) {
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Cycle everything on display to one dot left. Dots in far left are copied to column far right.
		/// </summary>
		public void CycleLeft() {
			offsetX=(offsetX+1)%width;
			changes=true;
		}
		
		/// <summary>
		/// Move everything on display to one dot left. Dots in far left will disappear and empty column appears to right.
		/// </summary>
		public void PushLeft() {
			CycleLeft();
			SetColumn(width-1,0);
			changes=true;
		}
		
		/// <summary>
		/// Cycle everything on display to one dot right. Dots in far right are copied to column far left.
		/// </summary>
		public void CycleRight() {
			offsetX--;
			if (offsetX<0) {
				offsetX+=width;
			}
			changes=true;
		}
		
		/// <summary>
		/// Move everything on display to one dot right. Dots in far right will disappear and empty column appears to left.
		/// </summary>
		public void PushRight() {
			CycleRight();
			SetColumn(0,0);
			changes=true;
		}
		
		/// <summary>
		/// Cycle everything on display to one dot up. Dots in top row are copied to bottom row.
		/// </summary>
		public void CycleUp() {
			offsetY=(offsetY+1)%height;
			changes=true;
		}
		
		/// <summary>
		/// Move everything on display to one dot up. Dots in top row will disappear and empty row appears to bottom.
		/// </summary>
		public void PushUp() {
			CycleUp();
			SetRow(height-1,0);
			changes=true;
		}
		
		/// <summary>
		/// Cycle everything on display to one dot down. Dots in bottom row are copied to top row.
		/// </summary>
		public void CycleDown() {
			offsetY--;
			if (offsetY<0) {
				offsetY+=height;
			}
			changes=true;
		}
		
		/// <summary>
		/// Move everything on display to one dot down. Dots in bottom row will disappear and empty row appears to top.
		/// </summary>
		public void PushDown() {
			CycleDown();
			SetRow(0,0);
			changes=true;
		}
		
		internal void pushRightAndSetLeftColumn(int[,] newContent, int sourcePosX, int targetPosY) {
			PushRight();
			int newContentHeight=newContent.GetLength(0);
			for (int ncY=0; ncY<newContentHeight; ncY++) {
				int y=targetPosY+ncY;
				if (y>=0 && y<height) {
					content[(offsetY+y)%height,offsetX]=newContent[ncY,sourcePosX];
				}
			}
			changes=true;
		}
		
		internal void pushLeftAndSetRightColumn(int[,] newContent, int sourcePosX, int targetPosY) {
			PushLeft();
			int newContentHeight=newContent.GetLength(0);
			for (int ncY=0; ncY<newContentHeight; ncY++) {
				int y=targetPosY+ncY;
				if (y>=0 && y<height) {
					content[(offsetY+y)%height,(offsetX+width-1)%width]=newContent[ncY,sourcePosX];
				}
			}
			changes=true;
		}
		
		internal void pushDownAndSetTopRow(int[,] newContent, int sourcePosY, int targetPosX) {
			PushDown();
			int newContentWidth=newContent.GetLength(1);
			for (int ncX=0; ncX<newContentWidth; ncX++) {
				int x=targetPosX+ncX;
				if (x>=0 && x<width) {
					content[offsetY,(offsetX+x)%width]=newContent[sourcePosY,ncX];
				}
			}
			changes=true;
		}
		
		internal void pushUpAndSetBottomRow(int[,] newContent, int sourcePosY, int targetPosX) {
			PushUp();
			int newContentWidth=newContent.GetLength(1);
			for (int ncX=0; ncX<newContentWidth; ncX++) {
				int x=targetPosX+ncX;
				if (x>=0 && x<width) {
					content[(offsetY+height-1)%height,(offsetX+x)%width]=newContent[sourcePosY,ncX];
				}
			}
			changes=true;
		}
		
		internal bool checkChangesAndReset() {
			bool ret=changes;
			changes=false;
			return ret;
		}
		
	}

}
