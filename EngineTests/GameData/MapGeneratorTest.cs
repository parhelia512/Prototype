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
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Numerics;


namespace C7GameDataTests;

public class GameMapTest {

    /// <summary>
    /// Generates a 16x16 texture atlas for isometric road tiles based on an 8-directional
    /// connectivity bitmask. This produces all 256 possible tile variations.
    /// </summary>
    public class RoadTextureGenerator
    {
        // --- Configuration Constants ---
        private const int TileWidth = 128;
        private const int TileHeight = 64;
        private const int GridSize = 16;
        private const float RoadThickness = 4f;
        private const float CurveRandomness = 16f; // Slightly reduced for cleaner diagonals

        // --- Color Palette ---
        private static readonly Color BackgroundGreen = Color.Transparent;
        private static readonly Color DiamondMagenta = Color.Transparent;
        private static readonly Color RoadBrown = Color.SaddleBrown;

        /// <summary>
        /// Generates and saves the complete road texture atlas.
        /// </summary>
        public void GenerateAndSave()
        {
            using var finalImage = new Image<Rgba32>(TileWidth * GridSize, TileHeight * GridSize);
            Console.WriteLine("Generating 256 road tiles based on 8-directional logic...");

            // Loop through all 256 possible tile indices (bitmasks).
            for (int index = 0; index < 256; index++)
            {
                // Each index is a unique connectivity bitmask.
                using (var tileImage = CreateSingleTile(index))
                {
                    // The original Civ3 logic for placing the tile in the atlas.
                    int row = index >> 4;
                    int column = index & 0x0F;
                    var location = new Point(column * TileWidth, row * TileHeight);
                    finalImage.Mutate(ctx => ctx.DrawImage(tileImage, location, 1f));
                }
            }

            string fileName = "Civ3Roads_8-Directional.png";
            finalImage.Save(fileName);
            Console.WriteLine($"Successfully generated and saved the texture atlas to: {System.IO.Path.GetFullPath(fileName)}");
        }

        /// <summary>
        /// Creates a single 128x64 tile image based on its 8-directional connectivity bitmask.
        /// </summary>
        /// <param name="roadMask">The tile index (0-255), which serves as the connectivity bitmask.</param>
        private Image<Rgba32> CreateSingleTile(int roadMask)
        {
            var tileImage = new Image<Rgba32>(TileWidth, TileHeight);

            // The curves should be deterministic for each specific mask.
            // Seeding Random with the mask itself ensures this.
            var random = new Random(roadMask);

            DrawBackground(tileImage);
            DrawRoads(tileImage, roadMask, random);

            return tileImage;
        }

        private void DrawBackground(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.Fill(BackgroundGreen);
                var diamond = new Polygon(new LinearLineSegment(new PointF[]
                {
                    new(TileWidth / 2f, 0),
                    new(TileWidth, TileHeight / 2f),
                    new(TileWidth / 2f, TileHeight),
                    new(0, TileHeight / 2f)
                }));
                ctx.Fill(DiamondMagenta, diamond);
            });
        }

        /// <summary>
        /// Draws the curved road segments based on the 8-directional connectivity bitmask.
        /// </summary>
        private void DrawRoads(Image<Rgba32> image, int roadMask, Random random)
        {
            var pen = Pens.Solid(RoadBrown, RoadThickness);

            // Define all 8 connection points plus the center.
            // Cardinal points are on the edge midpoints.
            // Diagonal points are at the corners.
            var center = new Vector2(TileWidth / 2f, TileHeight / 2f);
            var n  = new Vector2(TileWidth / 2f, 0);
            var ne = new Vector2(TileWidth * 3.0f/4.0f, TileHeight * 1.0f/4.0f);
            var e  = new Vector2(TileWidth, TileHeight / 2f);
            var se = new Vector2(TileWidth * 3.0f/4.0f, TileHeight * 3.0f/4.0f);
            var s  = new Vector2(TileWidth / 2f, TileHeight);
            var sw = new Vector2(TileWidth * 1.0f/4.0f, TileHeight * 3.0f/4.0f);
            var w  = new Vector2(0, TileHeight / 2f);
            var nw = new Vector2(TileWidth * 1.0f/4.0f, TileHeight * 1.0f/4.0f);

            image.Mutate(ctx =>
            {
                // Check each bit in the mask and draw a road if it's set.
                if ((roadMask & 1)   != 0) DrawCurvedPath(ctx, pen, center, ne, random); // Bit 1: North-East
                if ((roadMask & 2)   != 0) DrawCurvedPath(ctx, pen, center, e,  random); // Bit 2: East
                if ((roadMask & 4)   != 0) DrawCurvedPath(ctx, pen, center, se, random); // Bit 3: South-East
                if ((roadMask & 8)  != 0) DrawCurvedPath(ctx, pen, center, s,  random); // Bit 4: South
                if ((roadMask & 16)  != 0) DrawCurvedPath(ctx, pen, center, sw, random); // Bit 5: South-West
                if ((roadMask & 32)  != 0) DrawCurvedPath(ctx, pen, center, w,  random); // Bit 6: West
                if ((roadMask & 64) != 0) DrawCurvedPath(ctx, pen, center, nw, random); // Bit 7: North-West
                if ((roadMask & 128) != 0) DrawCurvedPath(ctx, pen, center, n, random); // Bit 8: N

                // For intersections of 3 or more roads, draw a central node to make the join look cleaner.
                if (BitOperations.PopCount((uint)roadMask) > 2)
                {
                    ctx.Fill(RoadBrown, new EllipsePolygon(center, RoadThickness * 1.5f));
                }
            });
        }

        /// <summary>
        /// Draws a single curved road segment using a quadratic Bezier curve.
        /// </summary>
        private void DrawCurvedPath(IImageProcessingContext context, Pen pen, Vector2 start, Vector2 end, Random random)
        {
            var midPoint = (start + end) / 2.0f;
            var delta = end - start;
            var normal = Vector2.Normalize(new Vector2(-delta.Y, delta.X));
            var randomOffset = (float)((random.NextDouble() * 2) - 1) * CurveRandomness;
            var controlPoint = midPoint + normal * randomOffset;

            var pathBuilder = new PathBuilder();
            pathBuilder.MoveTo(start);
            pathBuilder.QuadraticBezierTo(controlPoint, end);
            var path = pathBuilder.Build();

            context.Draw(pen, path);
        }
    }

	[Fact]
	public void GameMapGeneration() {
		var generator = new RoadTextureGenerator();
		generator.GenerateAndSave();
	}
}
