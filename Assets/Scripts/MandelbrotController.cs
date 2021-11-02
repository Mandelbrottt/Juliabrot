using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MandelbrotController : FractalController {
	JuliaController m_juliaController = null;

	protected override void Start() {
		base.Start();
		m_juliaController = FindObjectOfType<JuliaController>();

		m_positionBoundsArray  = new DVector2[2];
		m_positionBoundsBuffer = new ComputeBuffer(4, sizeof(double));
	}

	protected override void OnMouseDrag() {
		base.OnMouseDrag();

		if (!Input.GetMouseButton((int) MouseButton.RightMouse)) {
			var pos = GetPositionInBoundsFromMouse();
			m_juliaController.AnchorPoint = pos;
		}
	}

	protected override void OnMouseOver() {
		base.OnMouseOver();

	}
}
