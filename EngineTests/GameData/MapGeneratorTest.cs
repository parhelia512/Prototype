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
    /// Generates a 16x16 texture atlas for isometric railroad tiles based on an 8-directional
    /// connectivity bitmask. This produces all 256 possible tile variations.
    /// </summary>
    public class RailroadTextureGenerator
    {
        // --- Configuration Constants ---
        private const int TileWidth = 128;
        private const int TileHeight = 64;
        private const int GridSize = 16;
        private const float CurveRandomness = 4f;

        // --- Railroad-specific Configuration ---
        private const float RailThickness = 2f;
        private const float TieThickness = 2f;
        private const float TrackWidth = 6f; // Distance between the two rails

        // --- Color Palette ---
        private static readonly Color BackgroundGreen = Color.Transparent;
        private static readonly Color DiamondMagenta = Color.Transparent;
        private static readonly Color TrackColor = Color.DarkSlateGray; // Color for rails and ties

        /// <summary>
        /// Generates and saves the complete railroad texture atlas.
        /// </summary>
        public void GenerateAndSave()
        {
            using var finalImage = new Image<Rgba32>(TileWidth * GridSize, TileHeight * GridSize);
            Console.WriteLine("Generating 256 railroad tiles based on 8-directional logic...");

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

            string fileName = "Civ3Railroads_8-Directional.png";
            finalImage.Save(fileName);
            Console.WriteLine($"Successfully generated and saved the texture atlas to: {System.IO.Path.GetFullPath(fileName)}");
        }

        /// <summary>
        /// Creates a single 128x64 tile image based on its 8-directional connectivity bitmask.
        /// </summary>
        /// <param name="railroadMask">The tile index (0-255), which serves as the connectivity bitmask.</param>
        private Image<Rgba32> CreateSingleTile(int railroadMask)
        {
            var tileImage = new Image<Rgba32>(TileWidth, TileHeight);

            // The curves should be deterministic for each specific mask.
            // Seeding Random with the mask itself ensures this.
            var random = new Random(railroadMask);

            DrawBackground(tileImage);
            DrawRailroads(tileImage, railroadMask, random);

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
        /// Draws the curved railroad segments based on the 8-directional connectivity bitmask.
        /// </summary>
        private void DrawRailroads(Image<Rgba32> image, int railroadMask, Random random)
        {
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
                // Check each bit in the mask and draw a railroad if it's set.
                if ((railroadMask & 1)   != 0) DrawRailroadPath(ctx, center, ne, random); // Bit 1: North-East
                if ((railroadMask & 2)   != 0) DrawRailroadPath(ctx, center, e,  random); // Bit 2: East
                if ((railroadMask & 4)   != 0) DrawRailroadPath(ctx, center, se, random); // Bit 3: South-East
                if ((railroadMask & 8)   != 0) DrawRailroadPath(ctx, center, s,  random); // Bit 4: South
                if ((railroadMask & 16)  != 0) DrawRailroadPath(ctx, center, sw, random); // Bit 5: South-West
                if ((railroadMask & 32)  != 0) DrawRailroadPath(ctx, center, w,  random); // Bit 6: West
                if ((railroadMask & 64)  != 0) DrawRailroadPath(ctx, center, nw, random); // Bit 7: North-West
                if ((railroadMask & 128) != 0) DrawRailroadPath(ctx, center, n,  random); // Bit 8: North
            });
        }

        /// <summary>
        /// Helper function to calculate a point on a quadratic Bezier curve.
        /// </summary>
        private Vector2 GetPointOnQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            return (uu * p0) + (2 * u * t * p1) + (tt * p2);
        }

        /// <summary>
        /// Draws a single railroad track segment, including two rails and connecting ties.
        /// </summary>
        private void DrawRailroadPath(IImageProcessingContext context, Vector2 start, Vector2 end, Random random)
        {
            var railPen = Pens.Solid(TrackColor, RailThickness);
            var tiePen = Pens.Solid(TrackColor, TieThickness);

            // Calculate direction and perpendicular normal for offsetting the rails
            var delta = end - start;
            var normal = Vector2.Normalize(new Vector2(-delta.Y, delta.X));
            var offset = normal * (TrackWidth / 2f);

            // Define start and end points for the two parallel rails
            var start1 = start + offset;
            var end1 = end + offset;
            var start2 = start - offset;
            var end2 = end - offset;

            // Use the same random factor for both curves to maintain parallelism
            var randomOffsetAmount = (float)((random.NextDouble() * 2) - 1) * CurveRandomness;
            var controlPointOffset = normal * randomOffsetAmount;

            // Calculate control points for each Bezier curve
            var controlPoint1 = ((start1 + end1) / 2f) + controlPointOffset;
            var controlPoint2 = ((start2 + end2) / 2f) + controlPointOffset;

            // --- Draw the two rails ---
            context.Draw(railPen, new PathBuilder().MoveTo(start1).QuadraticBezierTo(controlPoint1, end1).Build());
            context.Draw(railPen, new PathBuilder().MoveTo(start2).QuadraticBezierTo(controlPoint2, end2).Build());

            // --- Draw the ties ---
            int tieCount = (int)(delta.Length() / 12); // Adjust number of ties based on track length
            for (int i = 1; i <= tieCount; i++)
            {
                float t = (float)i / (tieCount + 1);
                var pointOnRail1 = GetPointOnQuadraticBezier(start1, controlPoint1, end1, t);
                var pointOnRail2 = GetPointOnQuadraticBezier(start2, controlPoint2, end2, t);
                context.DrawLine(tiePen, pointOnRail1, pointOnRail2);
            }
        }
    }

	[Fact]
	public void GameMapGeneration() {
		var generator = new RailroadTextureGenerator();
		generator.GenerateAndSave();
	}
}
