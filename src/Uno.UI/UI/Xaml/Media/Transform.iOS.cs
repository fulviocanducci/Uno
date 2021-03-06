﻿using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using CoreGraphics;
using System.Drawing;
using Uno.UI;

namespace Windows.UI.Xaml.Media
{
	/// <summary>
	/// Transform: iOS part
	/// </summary>
	public abstract partial class Transform
	{
		private bool _needsUpdate;

		public Transform()
		{
		}

		protected void SetNeedsUpdate()
		{
			if (_needsUpdate)
			{
				return;
			}
			_needsUpdate = true;
			Dispatcher.RunAsync(
				Core.CoreDispatcherPriority.Normal,
				() =>
				{
					Update();
					_needsUpdate = false;
				}
			);
		}

		/// <summary>
		/// Returns the native CGAffineTransform associated with this Transform.
		/// </summary>
		/// <param name="size">The size of the view.</param>
		/// <param name="withCenter">Whether Center (CenterX/CenterY) information should be part of the transform. Providing false is useful if a transform is applied to an element Center information is already part of the </param>
		/// <returns></returns>
		internal virtual CGAffineTransform ToNativeTransform(CGSize size, bool withCenter = true)
		{
			throw new NotImplementedException(nameof(ToNativeTransform) + " not implemented for " + this.GetType().ToString());
		}

		internal static double ToRadians(double angle) => MathEx.ToRadians(angle);

		partial void OnDetachedFromViewPartial(UIView view)
		{
			view.Transform = CGAffineTransform.MakeIdentity();

			if (view is FrameworkElement fe)
			{
				fe.SizeChanged -= Fe_SizeChanged;
			}
		}

		partial void OnAttachedToViewPartial(UIView view)
		{
			if (view is FrameworkElement fe)
			{
				fe.SizeChanged += Fe_SizeChanged;
			}
		}

		private void Fe_SizeChanged(object sender, SizeChangedEventArgs args)
		{
			Update();
		}

		partial void UpdatePartial()
		{
			if (View != null)
			{
				var size = GetViewSize(View);
				View.Layer.AnchorPoint = GetAnchorPoint(size);
				// Center (CenterX/CenterY) is already part of AnchorPoint, we don't want to apply it twice.
				View.Transform = ToNativeTransform(size, withCenter: false); 
			}
		}

		/// <summary>
		/// Get size of the view before any transform is applied.
		/// </summary>
		protected static CGSize GetViewSize(UIView view)
		{
			CGSize? appliedFrame = (view as IFrameworkElement)?.AppliedFrame.Size;
			return appliedFrame ?? view.Frame.Size;
		}

		/// <summary>
		/// Gets the AnchorPoint to be applied to the UIView's Layer, 
		/// considering both UIElement's RenderTransformOrigin and Transform's Center (CenterX/CenterY).
		/// </summary>
		private Foundation.Point GetAnchorPoint(Foundation.Size size)
		{
			if (size.Width == 0 || size.Height == 0)
			{
				return Origin;
			}

			var center = GetCenter();

			return new Foundation.Point(
				Origin.X + (center.X / size.Width),
				Origin.Y + (center.Y / size.Height)
			);
		}

		private Foundation.Point GetCenter()
		{
			switch (this)
			{
				case RotateTransform rotateTransform:
					return new Foundation.Point(rotateTransform.CenterX, rotateTransform.CenterY);
				case ScaleTransform scaleTransform:
					return new Foundation.Point(scaleTransform.CenterX, scaleTransform.CenterY);
				case SkewTransform skewTransform:
					return new Foundation.Point(skewTransform.CenterX, skewTransform.CenterY);
				case CompositeTransform compositeTransform:
					return new Foundation.Point(compositeTransform.CenterX, compositeTransform.CenterY);
				default:
					return new Foundation.Point(0, 0);
			}
		}
	}
}

