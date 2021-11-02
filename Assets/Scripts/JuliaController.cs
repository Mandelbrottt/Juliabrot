using TMPro;
using UnityEngine;

public class JuliaController : FractalController {
	DVector2 m_anchorPoint;

	public TMP_Text juliaText;

	public DVector2 AnchorPoint {
		get => m_anchorPoint;
		set {
			m_positionBoundsArray[2].x = value.x;
			m_positionBoundsArray[2].y = value.y;
			m_anchorPoint              = value;
		}
	}

	MandelbrotController m_mandelbrotController = null;

	protected override void Start() {
		base.Start();
		m_mandelbrotController = FindObjectOfType<MandelbrotController>();

		m_positionBoundsArray  = new DVector2[3];
		m_positionBoundsBuffer = new ComputeBuffer(6, sizeof(double));
	}

	protected override void Update() {
		base.Update();

		juliaText.text = $"{m_anchorPoint.x} + {m_anchorPoint.y}i";
	}

	void OnMouseDown() {
		//var ray = m_camera.ScreenPointToRay(Input.mousePosition);
		//Physics.Raycast(ray, out var info);
	}

	protected override void OnMouseOver() {
		base.OnMouseOver();
	}
}
