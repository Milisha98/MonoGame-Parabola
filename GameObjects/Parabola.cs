using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using Parabola.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parabola.GameObjects;

internal class Parabola : IMonoGame
{
    const string ParabolaTextureName = "blue-circle-fadeout";
    const string ArrowTextureName = "blue-arrow";
    const float sizeGrowth = 0.00125f;

    private readonly GraphicsDevice _graphicsDevice;
    private Texture2D _parabolaTexture;
    private Texture2D _arrowTexture;
    private float _arrowRadians = 0;


    /// <summary>
    /// Constructor
    /// </summary>
    public Parabola(GraphicsDevice graphicDevice, Vector2 from, Vector2 to)
    {
        _graphicsDevice = graphicDevice;       

        FromVector = from;
        ToVector = to;

        // Might need to switch the coordinates depending on x vs y
        bool flip = false;
        if (from.X > to.X)
        {
            (from, to) = (to, from);    // Flip
            flip = true;
        }

        var points = (FromVector.Y < ToVector.Y)
            ? DefineUpwardsFacingParabola(from, to)
            : DefineDownwardsFacingParabola(from, to);

        if (flip) points = points.Reverse();
        Points = points.ToList();

        if (Points.Count > 1) 
        {
            var first = Points[^1];
            var last = Points[^2];
            _arrowRadians = last.AngleTo(first);
        }
    }


    public void LoadContent(ContentManager contentManager)
    {
        // Load the Textures
        _parabolaTexture = contentManager.Load<Texture2D>(ParabolaTextureName);
        _arrowTexture = contentManager.Load<Texture2D>(ArrowTextureName);
    }

    public void Update(GameTime gameTime)
    {
        // Do Nothing
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Rectangle viewPort)
    {
        // Determine the scale
        int count = 0;
        float scale = 0.33f;
        float width;
        Vector2 origin;
        foreach (var v in Points)
        {
            width = (_parabolaTexture.Width * scale) / 2;
            origin = new Vector2(-width, -width);
            spriteBatch.Draw(_parabolaTexture, v - origin, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
            scale += sizeGrowth;
            if (scale > 1f) scale = 1f;
            count++;
        }

        // Draw the arrow head
        width = ((_arrowTexture.Width * scale) / 2);
        origin = new Vector2(width, width);
        _arrowRadians = -MathHelper.PiOver2 - MathHelper.PiOver4;       // Angle To isn't working
        spriteBatch.Draw(_arrowTexture, ToVector + origin, null, Color.White, _arrowRadians, origin, scale, SpriteEffects.None, 0);
    }

    #region Parabola Methods

    /// <summary>
    /// If end.Y > start.Y
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private IEnumerable<Vector2> DefineUpwardsFacingParabola(Vector2 start, Vector2 end)
    {
        (float startX, float startY) = (start.X, start.Y);
        (float endX, float endY) = (end.X, end.Y);
        float height = endY - (startX - endX) * (startX - endX) / (4 * (startY - endY));

        // Calculate the vertex of the parabola
        float vertexX = (startX + endX) / 2f;
        float vertexY = (startY + endY) / 2f + ((vertexX - startX) * (vertexX - endX) / (4f * height - 2f * startY - 2f * endY));

        // Adjust the end point so it lies on the parabola
        endY = 2f * vertexY - startY;

        // Calculate the coefficients of the parabola's equation
        float a = (vertexY - startY) / ((vertexX - startX) * (vertexX - startX));
        float b = -2f * a * startX;
        float c = startY - a * startX * startX - b * startX;

        // Draw the parabola
        Vector2 v = Vector2.Zero;
        for (float x = startX; x <= endX; x++)
        {
            float y = a * x * x + b * x + c;
            if (y >= startY && y <= endY)
            {
                v = new Vector2((float)Math.Round(x, 0), (float)Math.Round(y, 0));
                yield return v;
            }
        }
        //System.Diagnostics.Debug.WriteLine(v);

    }

    private IEnumerable<Vector2> DefineDownwardsFacingParabola(Vector2 start, Vector2 end)
    {
        (float startX, float startY) = (start.X, start.Y);
        (float endX, float endY) = (end.X, end.Y);

        float a = (startY - endY) / ((startX - endX) * (startX - endX));
        float b = -2 * a * endX;
        float c = endY - (a * endX * endX + b * endX);

        float xMin = Math.Min(startX, endX);
        float xMax = Math.Max(startX, endX);

        for (float x = xMin; x <= xMax; x++)
        {
            float y = a * x * x + b * x + c;

            if (y >= Math.Min(startY, endY) && y <= Math.Max(startY, endY))
            {
                yield return new Vector2((float)Math.Round(x, 0), (float)Math.Round(y, 0));
            }
        }
    }
    // yield return new Vector2((float)Math.Round(x, 0), (float)Math.Round(y, 0));

    #endregion

    #region Properties

    public Vector2 FromVector { get; init; }
    public Vector2 ToVector { get; init; }    
    public List<Vector2> Points { get; init; }
    
    #endregion

}

internal static class Helper
{
    internal static float AngleTo(this Vector2 a, Vector2 b)
    {
        float dotProduct = Vector2.Dot(a, b);
        float magnitude1 = a.Length();
        float magnitude2 = b.Length();

        float angleInRadians = (float)Math.Acos(dotProduct / (magnitude1 * magnitude2));
        return angleInRadians;
    }
}


