using ElmSharp;

namespace Microsoft.Maui.Controls.Platform
{
	public class PinchGestureHandler : GestureHandler
	{
		Graphics.Point _currentScalePoint;
		int _previousPinchRadius;
		double _originalPinchScale;
		IViewHandler _handler;

		public PinchGestureHandler(IGestureRecognizer recognizer, IViewHandler handler) : base(recognizer)
		{
			_handler = handler;
		}

		public override GestureLayer.GestureType Type
		{
			get
			{
				return GestureLayer.GestureType.Zoom;
			}
		}

		protected override void OnStarted(View sender, object data)
		{
			var geometry = (_handler.PlatformView as EvasObject).Geometry;
			var zoomData = (GestureLayer.ZoomData)data;
			_currentScalePoint = new Graphics.Point((zoomData.X - geometry.X) / (double)geometry.Width, (zoomData.Y - geometry.Y) / (double)geometry.Height);
			_originalPinchScale = sender.Scale;
			_previousPinchRadius = zoomData.Radius;
			(Recognizer as IPinchGestureController)?.SendPinchStarted(sender, _currentScalePoint);
		}

		protected override void OnMoved(View sender, object data)
		{
			var zoomData = (GestureLayer.ZoomData)data;
			if (_previousPinchRadius <= 0)
				_previousPinchRadius = 1;
			// functionality limitation: _currentScalePoint is not updated
			(Recognizer as IPinchGestureController)?.SendPinch(sender,
				1 + _originalPinchScale * (zoomData.Radius - _previousPinchRadius) / _previousPinchRadius,
				_currentScalePoint
			);
			_previousPinchRadius = zoomData.Radius;
		}

		protected override void OnCompleted(View sender, object data)
		{
			(Recognizer as IPinchGestureController)?.SendPinchEnded(sender);
		}

		protected override void OnCanceled(View sender, object data)
		{
			(Recognizer as IPinchGestureController)?.SendPinchCanceled(sender);
		}
	}
}
