using Silk.NET.Maths;
using Rectangle = System.Drawing.Rectangle;

namespace Renderer.Display;

public class Texture
{
	public int       Width        { get; private set; }
	public int       Height       { get; private set; }
	public Rectangle ScreenRegion { get; private set; }
	/// <summary>
	/// RGB, without opacity. 3 bytes each
	/// </summary>
	private int[] Pixels { get; set; } // todo: enforce byte length and such with type system?

	public bool FontLoaded { get; private set; } = false;

	public Texture(int width, int height, Rectangle screenRegion)
	{
		Width = width;
		Height = height;
		ScreenRegion = screenRegion;

		Pixels = new int[width * height];
	}

	public Texture(string filePath) // TODO: accept some sort of stream?
	{
		throw new NotImplementedException("Did not spend time on this yet");
	}

	/// <summary>
	/// Ensures the texture is set for the given <paramref name="width"/> and <paramref name="height"/> and then
	/// returns a reference to the pixels.
	/// </summary>
	/// <param name="width">The width hat the texture should have.</param>
	/// <param name="height">The height that the texture should have.</param>
	/// <returns>A reference to the texture's contents: the pixels.</returns>
	public Span<int> GetWritablePixels(int width, int height)
	{
		if (width != Width || height != Height)
			Pixels = new int[width * height];

		Width = width;
		Height = height;
		return Pixels;
	}

	/// <summary>
	/// Forces use of the appropriate <see cref="GetWritablePixels"/> method in order to change the contents.
	/// </summary>
	/// <returns>Read-only reference to the pixels.</returns>
	public ReadOnlySpan<int> ReadPixels() => Pixels;

	public void SetScreenRegion(Rectangle screenRegion) => ScreenRegion = screenRegion;

	public void Clear(int c)
	{
		for (int i = 0; i < Height * Width; i++)
		{
			Pixels[i] = c;
		}
	}
	
	public void CopyTo(Texture target, int x = 0, int y = 0)
	{
		int src = 0;
		int dst = 0;
		int srcwidth = Width;
		int srcheight = Height;
		int dstwidth = target.Width;
		int dstheight = target.Height;
		if( srcwidth + x > dstwidth ) srcwidth = dstwidth - x;
		if( srcheight + y > dstheight ) srcheight = dstheight - y;
		if( x < 0 )
		{
			src -= x;
			srcwidth += x;
			x = 0;
		}
		if( y < 0 )
		{
			src -= y * Width;
			srcheight += y;
			y = 0;
		}
		if( srcwidth > 0 && srcheight > 0 )
		{
			dst += x + dstwidth * y;
			for( int v = 0; v < srcheight; v++ )
			{
				for( int u = 0; u < srcwidth; u++ ) target.Pixels[dst + u] = Pixels[src + u];
				dst += dstwidth;
				src += Width;
			}
		}
	}

	#region DebugTools

	public void Box(int x1, int y1, int x2, int y2, int c)
	{
		int dest = y1 * Width;
		for( int y = y1; y <= y2; y++, dest += Width )
		{
			Pixels[dest + x1] = c;
			Pixels[dest + x2] = c;
		}
		int dest1 = y1 * Width;
		int dest2 = y2 * Width;
		for( int x = x1; x <= x2; x++ )
		{
			Pixels[dest1 + x] = c;
			Pixels[dest2 + x] = c;
		}
	}
	
	public void DrawBar(int x1, int y1, int x2, int y2, int c)
	{
		int dest = y1 * Width;
		for( int y = y1; y <= y2; y++, dest += Width ) for( int x = x1; x <= x2; x++ )
		{
			Pixels[dest + x] = c;
		}
	}

	/// <summary>
	/// Helper method for <see cref="DrawLine"/>
	/// </summary>
	private int OutOfBoundsInt( int x, int y )
	{
		int xmin = 0, ymin = 0, xmax = Width - 1, ymax = Height - 1;
		return (x < xmin ? 1 : x > xmax ? 2 : 0) + (y < ymin ? 4 : y > ymax ? 8 : 0);
	}
		
	// Just don't question this
	public void DrawLine(int x1, int y1, int x2, int y2, int c)
	{
		int xmin = 0, ymin = 0, xmax = Width - 1, ymax = Height - 1;
		int c0 = OutOfBoundsInt( x1, y1 ), c1 = OutOfBoundsInt( x2, y2 );
		bool accept = false;
		while( true )
		{
			if( c0 == 0 && c1 == 0 ) { accept = true; break; }

			if( (c0 & c1) > 0 ) break;
			
			int x = 0, y = 0;
			int co = c0 > 0 ? c0 : c1;
			if( (co & 8) > 0 ) { x = x1 + (x2 - x1) * (ymax - y1) / (y2 - y1); y = ymax; }
			else if( (co & 4) > 0 ) { x = x1 + (x2 - x1) * (ymin - y1) / (y2 - y1); y = ymin; }
			else if( (co & 2) > 0 ) { y = y1 + (y2 - y1) * (xmax - x1) / (x2 - x1); x = xmax; }
			else if( (co & 1) > 0 ) { y = y1 + (y2 - y1) * (xmin - x1) / (x2 - x1); x = xmin; }
			if( co == c0 ) { x1 = x; y1 = y; c0 = OutOfBoundsInt( x1, y1 ); }
			else { x2 = x; y2 = y; c1 = OutOfBoundsInt( x2,           y2 ); }
		}
		if( !accept ) return;
		if( Math.Abs( x2 - x1 ) >= Math.Abs( y2 - y1 ) )
		{
			if( x2 < x1 ) { int h = x1; x1 = x2; x2 = h; h = y2; y2 = y1; y1 = h; }
			int l = x2 - x1;
			if( l == 0 ) return;
			int dy = (y2 - y1) * 8192 / l;
			y1 *= 8192;
			for( int i = 0; i < l; i++ )
			{
				Pixels[x1++ + y1 / 8192 * Width] = c;
				y1 += dy;
			}
		}
		else
		{
			if( y2 < y1 ) { int h = x1; x1 = x2; x2 = h; h = y2; y2 = y1; y1 = h; }
			int l = y2 - y1;
			if( l == 0 ) return;
			int dx = (x2 - x1) * 8192 / l;
			x1 *= 8192;
			for( int i = 0; i < l; i++ )
			{
				Pixels[x1 / 8192 + y1++ * Width] = c;
				x1 += dx;
			}
		}

		return;
	}
	
	public void DrawPixel(int x, int y, int c)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			throw new IndexOutOfRangeException();
		}

		Pixels[x + y * Width] = c;
	}
	
	
	public void DrawString(string t, int x, int y, int c)
	{
		throw new NotImplementedException("Requires loading textures from files");
		// if(!FontLoaded) // Load font
		// {
		// 	font = new Display( "../../assets/font.png" );
		// 	string ch = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+={}[];:<>,.?/\\ ";
		// 	fontRedir = new int[256];
		// 	for( int i = 0; i < 256; i++ ) fontRedir[i] = 0;
		// 	for( int i = 0; i < ch.Length; i++ )
		// 	{
		// 		int l = (int)ch[i];
		// 		fontRedir[l & 255] = i;
		// 	}
		// 	fontReady = true;
		// }
		// for( int i = 0; i < t.Length; i++ )
		// {
		// 	int f = fontRedir[(int)t[i] & 255];
		// 	int dest = x + i * 12 + y * Width;
		// 	int src = f * 12;
		// 	for( int v = 0; v < font.Height; v++, src += font.Width, dest += Width ) for( int u = 0; u < 12; u++ )
		// 		{
		// 			if( (font.Pixels[src + u] & 0xffffff) != 0 ) Pixels[dest + u] = c;
		// 		}
		// }
	}

	#endregion
}