using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Parabola.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parabola.GameObjects;

internal class Parabola : IMonoGame
{
    const string TextureName = "blue-circle-fadeout";

    private readonly GraphicsDevice _graphicsDevice;
    private Texture2D _texture;
    const float sizeGrowth = 0.00125f;


    /// <summary>
    /// Constructor
    /// </summary>
    public Parabola(GraphicsDevice graphicDevice, Vector2 from, Vector2 to)
    {
        _graphicsDevice = graphicDevice;       

        FromVector = from;
        ToVector = to;

        // Might need to switch the coordinates depending on x vs y
        if (from.X > to.X)
        {
            var a = to;
            to = from;
            from = a;
        }

        if (FromVector.Y < ToVector.Y)
            Points = DefineUpwardsFacingParabola(from, to).ToList();
        else
            Points = DefineDownwardsFacingParabola(from, to).ToList();
    }

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
        System.Diagnostics.Debug.WriteLine(v);

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
                //spriteBatch.Draw(pixel, new Vector2(x, y), Color.Red);
                yield return new Vector2((float)Math.Round(x, 0), (float)Math.Round(y, 0));
            }
        }
    }
    // yield return new Vector2(x, y);

    public void LoadContent(ContentManager contentManager)
    {
        // Load the Camera Point
        _texture = contentManager.Load<Texture2D>(TextureName);
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
        foreach (var v in Points)
        {
            float width = (_texture.Width * scale) / 2;
            Vector2 origin = new Vector2(-width, -width);
            spriteBatch.Draw(_texture, v, null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
            scale += sizeGrowth;
            if (scale > 1f) scale = 1f;
            count++;
            
        }
    }

    #region Properties

    public Vector2 FromVector { get; init; }
    public Vector2 ToVector { get; init; }    
    public List<Vector2> Points { get; init; }

    #endregion

}


