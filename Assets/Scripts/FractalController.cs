using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public struct DVector2 {
	public DVector2(double a_x, double a_y) {
		x = a_x;
		y = a_y;
	}

	public double x;
	public double y;
}

public class FractalController : MonoBehaviour {
	public GameObject rin;

	[SerializeField]
	protected Slider m_iterSlider;

	[SerializeField]
	protected TMP_Text m_iterValueText;

	[SerializeField]
	protected TMP_Text m_orderText;

	protected Material m_material = null;

	protected Camera m_camera = null;

	protected double   m_zoomFactor = 1;
	protected DVector2 m_center     = new DVector2(0.0, 0.0);

	[SerializeField]
	protected DVector2 m_defaultBounds = new DVector2(4.0, 4.0);

	protected DVector2 m_position = new DVector2();
	protected DVector2 m_bounds;

	protected int m_order = 2;

	protected static readonly int boundsIdX    = Shader.PropertyToID("_PositionBounds");
	protected static readonly int iterationsId = Shader.PropertyToID("_NumIterations");
	protected static readonly int orderId = Shader.PropertyToID("_Order");

	protected DVector2[]    m_positionBoundsArray;
	protected ComputeBuffer m_positionBoundsBuffer;

	Vector2 m_lastMousePos;

	protected virtual void Start() {
		m_camera = Camera.main;

		m_material = GetComponent<Renderer>().material;

		m_bounds = m_defaultBounds;

		m_lastMousePos = Input.mousePosition;

		m_iterSlider.value = 200;
		
		Shader.SetGlobalInt(orderId, 2);
	}

	protected virtual void Update() {
		m_position.x = m_center.x - m_bounds.x * 0.5;
		m_position.y = m_center.y - m_bounds.y * 0.5;

		m_positionBoundsArray[0].x = m_position.x;
		m_positionBoundsArray[0].y = m_position.y;
		m_positionBoundsArray[1].x = m_bounds.x;
		m_positionBoundsArray[1].y = m_bounds.y;

		m_positionBoundsBuffer.SetData(m_positionBoundsArray);

		m_material.SetBuffer(boundsIdX, m_positionBoundsBuffer);

		m_lastMousePos = Input.mousePosition;

		if (m_zoomFactor > 48 && !rin.activeInHierarchy) {
			rin.SetActive(true);
			rin.GetComponentInChildren<Renderer>().material = m_material;
		}
		
		m_material.SetInt(iterationsId, (int) m_iterSlider.value);
		m_iterValueText.text = $"{m_iterSlider.value}";

		if (m_orderText != null) {
			m_orderText.text = $"Order: {m_order - 1}";
		}
	}

	protected virtual void OnDestroy() {
		m_positionBoundsBuffer.Release();
	}

	protected virtual void OnMouseDrag() {}

	void OnMouseDown() {}

	protected virtual void OnMouseOver() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			m_center.x   = 0;
			m_center.y   = 0;
			m_bounds     = m_defaultBounds;
			m_zoomFactor = 1;
		}

		if (Input.GetMouseButton((int) MouseButton.RightMouse)) {
			double scalex = m_bounds.x / (Screen.width * 0.5);
			double scaley = m_bounds.y / Screen.height;

			m_center.x -= (Input.mousePosition.x - m_lastMousePos.x) * scalex;
			m_center.y -= (Input.mousePosition.y - m_lastMousePos.y) * scaley;
		}

		var scroll = Input.GetAxisRaw("Mouse ScrollWheel");

		if (Mathf.Abs(scroll) > 0) {

			double scrolladj = scroll;
			m_zoomFactor += scrolladj;

			if (m_zoomFactor <= 1) {
				m_bounds     = m_defaultBounds;
				m_zoomFactor = 1;
			} else {
				var factorOutput = 1 / Math.Pow(2, m_zoomFactor - 1);
				//factorOutput = 1.0 / m_zoomFactor;

				//m_bounds.x *= scrolladj;
				//m_bounds.y *= scrolladj;

				m_bounds.x = m_defaultBounds.x * factorOutput;
				m_bounds.y = m_defaultBounds.y * factorOutput;
			}
		}

		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
			m_order++;
			Shader.SetGlobalInt(orderId, m_order);
		}
		else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
			m_order--;
			m_order = Math.Max(1, m_order);
			Shader.SetGlobalInt(orderId, m_order);
		}
	}

	protected Vector2 GetTextureCoordsFromMouse() {
		var ray = m_camera.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out var info);
		return info.textureCoord;
	}

	protected DVector2 GetPositionInBoundsFromMouse() {
		var uvs = GetTextureCoordsFromMouse();

		var vec = new DVector2 {
			x = m_position.x + m_bounds.x * uvs.x,
			y = m_position.y + m_bounds.y * uvs.y
		};
		return vec;
	}
}
