using C7GameData;
using C7Engine;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;


namespace C7GameDataTests;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class FogOfWarImageGenerator {
    // Configuration for the generated fog tiles
    private const int TileWidth = 128;
    private const int TileHeight = 64;
    private const int GridSize = 9; // 9x9 grid based on the game's logic

    // Define the colors for each tile state as requested
    private static readonly Rgba32 UnknownColor = Color.Black; // Fully opaque black
    private static readonly Rgba32 KnownColor = new Rgba32(0, 0, 0, 96); // Semi-transparent black (gray appearance)
    private static readonly Rgba32 ActiveColor = Color.Transparent; // Fully transparent

    // Array of states for easy lookup, matching the game's logic (0: Unknown, 1: Known, 2: Active)
    private static readonly Rgba32[] StateColors = { UnknownColor, KnownColor, ActiveColor };

    /// <summary>
    /// Generates the complete 9x9 fog of war sprite sheet.
    /// </summary>
    /// <param name="outputPath">The file path to save the generated PNG image.</param>
    public void GenerateFogSpriteSheet(string outputPath) {
        int sheetWidth = TileWidth * GridSize;
        int sheetHeight = TileHeight * GridSize;

        // Create the final sprite sheet image
        using var spriteSheet = new Image<Rgba32>(sheetWidth, sheetHeight);

        // Iterate through every possible tile combination in the 9x9 grid
        for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {
                // Decode the row and column back into the states of the four neighbor tiles
                // This reverses the logic from the game code to determine which tile to draw.
                int northStateIndex = col % 3;
                int westStateIndex = col / 3;
                int eastStateIndex = row % 3;
                int southStateIndex = row / 3;

                // Get the corner colors based on the decoded states
                Rgba32 colorNorth = StateColors[northStateIndex];
                Rgba32 colorWest = StateColors[westStateIndex];
                Rgba32 colorEast = StateColors[eastStateIndex];
                Rgba32 colorSouth = StateColors[southStateIndex];

                // Generate a single 128x64 fog tile with the specified corner colors
                using Image<Rgba32> singleTile = GenerateDiamondTile(colorNorth, colorSouth, colorEast, colorWest);

                // Draw the generated tile onto the main sprite sheet at the correct position
                spriteSheet.Mutate(ctx => {
                    ctx.DrawImage(singleTile, new Point(col * TileWidth, row * TileHeight), 1f);
                });
            }
        }

        // Hacky nearest neighbor with an outline to get the tricky edges fixed.
        //
        // Image<Rgba32> transparencyReference = (Image<Rgba32>)Image.Load("C:\\Users\\Tom\\Prototype2\\C7\\Art\\Terrain\\comparison outline.png");
        //
        // for (int x = 0; x < sheetWidth; x++) {
	       //  for (int y = 0; y < sheetHeight; y++) {
		      //   if (transparencyReference[x, y].A != 0 && spriteSheet[x, y].A == 0) {
			     //    Rgba32 left = spriteSheet[x - 1, y];
			     //    Rgba32 right = spriteSheet[x + 1, y];
			     //    Rgba32 bottom = spriteSheet[x, Math.Max(y - 1, 0)];
			     //    Rgba32 top =  spriteSheet[x, Math.Min(y + 1, sheetHeight - 1)];
        //
			     //    if (left.A > 0) {
				    //     spriteSheet[x, y] = left;
			     //    } else if (right.A > 0) {
				    //     spriteSheet[x, y] = right;
			     //    } else if (bottom.A > 0) {
				    //     spriteSheet[x, y] = bottom;
			     //    } else {
				    //     spriteSheet[x, y] = top;
			     //    }
		      //   }
	       //  }
        // }

        // Save the final composite image to the specified path
        spriteSheet.SaveAsPng(outputPath);
        Console.WriteLine($"Successfully generated fog of war sprite sheet at: {Path.GetFullPath(outputPath)}");
    }

    /// <summary>
    /// Generates a single 128x64 diamond-shaped tile by blending four corner colors.
    /// </summary>
    private Image<Rgba32> GenerateDiamondTile(Rgba32 colorN, Rgba32 colorS, Rgba32 colorE, Rgba32 colorW) {
        var tile = new Image<Rgba32>(TileWidth, TileHeight);

        float halfWidth = (float)TileWidth / 2;
        float halfHeight = (float)TileHeight / 2;

        tile.ProcessPixelRows(accessor => {
            for (int y = 0; y < accessor.Height; y++) {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++) {
                    // Check if the pixel is inside the diamond shape
                    float normX = (x - halfWidth) / halfWidth;
                    float normY = (y - halfHeight) / halfHeight;

                    if (Math.Abs(normX) + Math.Abs(normY) <= 1.0f) {
                        // For pixels inside the diamond, calculate the blended color.
                        // We use a technique similar to bilinear interpolation on a transformed space.
                        // This ensures smooth gradients from all four corners towards the center.
                        pixelRow[x] = BilinearInterpolateDiagonal(colorN, colorS, colorE, colorW, normX, normY);

                        if (pixelRow[x].A != 0) {
	                        double alpha = pixelRow[x].A / 256.0;
	                        if (alpha > 128) {
		                        alpha = Math.Pow(alpha, .1);
	                        }
	                        pixelRow[x].A = (byte)(Math.Pow(alpha, 1) * 256);
                        }
                    } else {
                        // Pixels outside the diamond are fully transparent
                        pixelRow[x] = Color.Transparent;
                    }
                }
            }
        });

        return tile;
    }

    /// <summary>
    /// Blends four colors using bilinear interpolation on a coordinate system rotated by 45 degrees,
    /// which aligns the interpolation axes with the diagonals of the diamond.
    /// </summary>
    private Rgba32 BilinearInterpolateDiagonal(Rgba32 colorN, Rgba32 colorS, Rgba32 colorE, Rgba32 colorW, float nx, float ny) {
	    // --- Coordinate Transformation ---
	    // Transform the diamond coordinates (nx, ny) into a square coordinate system (d1, d2).
	    // This rotation makes standard bilinear interpolation possible.
	    // d1 axis runs from SW to NE. d2 axis runs from NW to SE.
	    float d1 = nx - ny;
	    float d2 = nx + ny;

	    // In this new (d1, d2) space, the diamond's cardinal points (N,S,E,W)
	    // become the corners of a square:
	    // West (-1,0) -> (-1,-1) -> Uses colorW
	    // South (0,-1) -> (1,-1) -> Uses colorS
	    // North (0,1) -> (-1,1) -> Uses colorN
	    // East (1,0) -> (1,1) -> Uses colorE

	    // Normalize the new coordinates from [-1, 1] to [0, 1] for interpolation.
	    float u = (d1 + 1) / 2f;
	    float v = (d2 + 1) / 2f;

	    // --- Bilinear Interpolation ---
	    // 1. Interpolate along the bottom edge of the new square (between West and South).
	    Rgba32 bottom_inter = Lerp(colorW, colorN, u);

	    // 2. Interpolate along the top edge of the new square (between North and East).
	    Rgba32 top_inter = Lerp(colorS, colorE, u);

	    // 3. Interpolate vertically between the bottom and top results.
	    return Lerp(bottom_inter, top_inter, v);
    }

    /// <summary>
    /// Linearly interpolates between two Rgba32 color values.
    /// </summary>
    private Rgba32 Lerp(Rgba32 start, Rgba32 end, float amount) {
        amount = Math.Clamp(amount, 0.0f, 1.0f);
        byte r = (byte)(start.R + (end.R - start.R) * amount);
        byte g = (byte)(start.G + (end.G - start.G) * amount);
        byte b = (byte)(start.B + (end.B - start.B) * amount);
        byte a = (byte)(start.A + (end.A - start.A) * amount);
        return new Rgba32(r, g, b, a);
    }

	[Fact]
	public void GameMapGeneration() {
		var generator = new FogOfWarImageGenerator();
		generator.GenerateFogSpriteSheet("FogOfWar_SpriteSheet.png");
	}
}
