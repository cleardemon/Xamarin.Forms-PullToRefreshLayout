using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Refractored.XamForms.PullToRefresh.PullToRefreshLayout), typeof(Refractored.XamForms.PullToRefresh.UWP.PullToRefreshLayoutRenderer))]

namespace Refractored.XamForms.PullToRefresh.UWP
{
	static class WindowsExtensions
	{
		public static Windows.UI.Color ToWindowsColor(this Xamarin.Forms.Color c)
		{
			return Windows.UI.Color.FromArgb((byte)(c.A * byte.MaxValue), (byte)(c.R * byte.MaxValue), (byte)(c.G * byte.MaxValue), (byte)(c.B * byte.MaxValue));
		}
	}

	public class PullToRefreshLayoutRenderer : VisualElementRenderer<PullToRefreshLayout, PullToRefreshControlUWP>
    {
		public static void Init()
		{
		}

		VisualElement _currentView;

		protected override void OnElementChanged(ElementChangedEventArgs<PullToRefreshLayout> e)
		{
			base.OnElementChanged(e);

			if(e.NewElement != null)
			{
				SetNativeControl(new PullToRefreshControlUWP());
				Control.RefreshRequested += Control_RefreshRequested;
				UpdateColours();
				LoadContent();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if(e.PropertyName == "Content")
				LoadContent();
			if(e.PropertyName == PullToRefreshLayout.IsRefreshingProperty.PropertyName)
			{
				if(Element.IsRefreshing == false)
					Control.CancelRefresh();
			}
		}

		private void Control_RefreshRequested(object sender, EventArgs e)
		{
			Element.IsRefreshing = true;
		}

		void UpdateColours()
		{
			Control.SetColours(Element.RefreshBackgroundColor.ToWindowsColor(), Element.RefreshColor.ToWindowsColor());
		}


		void LoadContent()
		{
			_currentView = Element.Content;
			IVisualElementRenderer visualElementRenderer = null;
			if(_currentView != null)
				visualElementRenderer = VisualElementExtensions.GetOrCreateRenderer(_currentView);
			Control.SetContent(visualElementRenderer != null ? visualElementRenderer.ContainerElement : null);
		}

	}
}
