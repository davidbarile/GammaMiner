using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

// Writes per-sprite UV coordinates in secondary UV channels:
// UV1: Sprite local UVs
// UV2: Sprite min UVs
// UV3: Sprite max UVs
// Credits for approach to BBO_Lagoon: https://discussions.unity.com/t/785133/30
public sealed class SpriteLocalUVPostProcessor : AssetPostprocessor
{
	private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
	{
		foreach (var sprite in sprites)
		{
			// Cache sprite UV array fetch native call
			var spriteUVs = sprite.uv;

			// Find min/max sprite UVs
			float minSpriteU = 1;
			float maxSpriteU = 0;
			float minSpriteV = 1;
			float maxSpriteV = 0;

			foreach (var spriteCornerUV in spriteUVs)
			{
				minSpriteU = Mathf.Min(spriteCornerUV.x, minSpriteU);
				maxSpriteU = Mathf.Max(spriteCornerUV.x, maxSpriteU);
				minSpriteV = Mathf.Min(spriteCornerUV.y, minSpriteV);
				maxSpriteV = Mathf.Max(spriteCornerUV.y, maxSpriteV);
			}

			var spriteLocalUVs = new NativeArray<Vector2>(spriteUVs.Length, Allocator.Temp);
			var spriteMinUVs = new NativeArray<Vector2>(spriteUVs.Length, Allocator.Temp);
			var spriteMaxUVs = new NativeArray<Vector2>(spriteUVs.Length, Allocator.Temp);

			for (var spriteCornerIndex = 0; spriteCornerIndex < spriteLocalUVs.Length; spriteCornerIndex++)
			{
				// Local UVs
				var spriteCornerUV = spriteUVs[spriteCornerIndex];
				var spriteCornerLocalU = Mathf.InverseLerp(minSpriteU, maxSpriteU, spriteCornerUV.x);
				var spriteCornerLocalV = Mathf.InverseLerp(minSpriteV, maxSpriteV, spriteCornerUV.y);
				spriteLocalUVs[spriteCornerIndex] = new Vector2(spriteCornerLocalU, spriteCornerLocalV);

				// Min UVs
				spriteMinUVs[spriteCornerIndex] = new Vector2(minSpriteU, minSpriteV);

				// Max UVs
				spriteMaxUVs[spriteCornerIndex] = new Vector2(maxSpriteU, maxSpriteV);
			}

			// UV1: Sprite local UVs
			sprite.SetVertexAttribute(VertexAttribute.TexCoord1, spriteLocalUVs);

			// UV2: Sprite min UVs
			sprite.SetVertexAttribute(VertexAttribute.TexCoord2, spriteMinUVs);

			// UV3: Sprite max UVs
			sprite.SetVertexAttribute(VertexAttribute.TexCoord3, spriteMaxUVs);
		}
	}
}