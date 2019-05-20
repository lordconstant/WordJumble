using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Effects/SolidOutline", 14)]
	public class SolidOutline : BaseMeshEffect
	{
		[SerializeField]
		public Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

		[SerializeField]
		public float EffectDistance = 5.0f;

		[SerializeField]
		private bool m_UseGraphicAlpha = true;

		private const float kMaxEffectDistance = 600f;

		protected SolidOutline()
		{}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			effectDistance = EffectDistance;
			base.OnValidate();
		}

		#endif

		public Color effectColor
		{
			get { return m_EffectColor; }
			set
			{
				m_EffectColor = value;
				if (graphic != null)
					graphic.SetVerticesDirty();
			}
		}

		public float effectDistance
		{
			get { return EffectDistance; }
			set
			{
				if (value > kMaxEffectDistance)
					value = kMaxEffectDistance;
				if (value < -kMaxEffectDistance)
					value = -kMaxEffectDistance;

				if (EffectDistance == value)
					return;

				EffectDistance = value;

				if (graphic != null)
					graphic.SetVerticesDirty();
			}
		}

		public bool useGraphicAlpha
		{
			get { return m_UseGraphicAlpha; }
			set
			{
				m_UseGraphicAlpha = value;
				if (graphic != null)
					graphic.SetVerticesDirty();
			}
		}

		protected void ApplyOutline(List<UIVertex> verts, Color32 color, int start, int end, float distance)
		{
			UIVertex vt;

			var neededCapacity = verts.Count + end - start;
			if (verts.Capacity < neededCapacity)
				verts.Capacity = neededCapacity;

			float yScale = 1.0f;
			float xScale = 1.0f;

			Rect rect = gameObject.GetComponent<RectTransform>().rect;

			if(rect.width > rect.height)
			{
				xScale = 1.0f;
				yScale = rect.width / rect.height;
			}
			else
			{
				yScale = 1.0f;
				xScale = rect.height / rect.width;
			}

			for (int i = start; i < end; ++i)
			{
				vt = verts[i];
				verts.Add(vt);

				Vector3 v = vt.position;
				Vector3 vertNormal = v; //= new Vector3(vt.position.x > 0.0f ? 1.0f : (vt.position.x == 0.0f ? 0.0f : -1.0f), vt.position.y > 0.0f ? 1.0f : (vt.position.y == 0.0f ? 0.0f : -1.0f), 0.0f);  //new Vector3(vt.uv0.x, vt.uv0.y, 0.0f);

//				for(int j = 0; j < indices.Length; j+=3)
//				{
//					if(indices[j] != i && indices[j+1] != i && indices[j+2] != i)
//						continue;
//
//					for(int k = 0; k < 3; k++)
//					{
//						if(indices[j+k] == i)
//							continue;
//						
//						UIVertex neighVert = verts[indices[j+k]];
//
//						vertNormal += (v - neighVert.position).normalized;
//					}
//				}

				vertNormal.Normalize();
				vertNormal.y *= yScale;
				vertNormal.x *= xScale;

				v += vertNormal * distance;

				vt.position = v;
				var newColor = color;
				if (m_UseGraphicAlpha)
					newColor.a = (byte)((newColor.a * verts[i].color.a) / 255.0f);
				vt.color = newColor;
				verts[i] = vt;
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive())
				return;
			
			var output = ListPool<UIVertex>.Get();
	
			vh.GetUIVertexStream(output);

			ApplyOutline(output, effectColor, 0, output.Count, effectDistance);
			vh.Clear();

			vh.AddUIVertexTriangleStream(output);
			ListPool<UIVertex>.Release(output);
		}
	}
}