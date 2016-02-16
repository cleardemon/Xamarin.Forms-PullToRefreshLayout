using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Refractored.XamForms.PullToRefresh.UWP
{
	public sealed partial class PullToRefreshControlUWP : ContentControl
	{
		public PullToRefreshControlUWP()
		{
			InitializeComponent();

			Loaded += delegate {
				// hide refresh indicator
				scrollInner.ChangeView(null, 100, null);
			};

			panelInner.SizeChanged += (sender, e) => scrollInner.ChangeView(null, 100.0, null, true);
			Window.Current.SizeChanged += (sender, e) => panelInner.InvalidateMeasure();
		}
		
		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			ScrollViewer sv = sender as ScrollViewer;
			if(sv.VerticalOffset == 0)
			{
				scrollInner.DirectManipulationCompleted += ScrollDirectManipulationCompleted;
				VisualStateManager.GoToState(this, "Refreshing", false);
			}
		}

		private void ScrollDirectManipulationCompleted(object sender, object e)
		{
			scrollInner.DirectManipulationCompleted -= ScrollDirectManipulationCompleted;
			// start refreshing
			if(RefreshRequested != null)
				RefreshRequested(this, EventArgs.Empty);
		}

		internal void SetColours(Color background, Color foreground)
		{
			borderInner.Background = new SolidColorBrush(background);
			// TODO: can't change the foreground colour since it is a graphic at the moment
		}

		internal void CancelRefresh()
		{
			scrollInner.ChangeView(null, 0, null, true);
			VisualStateManager.GoToState(this, "PullToRefresh", false);
		}

		internal void SetContent(object content)
		{
			contentPresenter.Content = new Border { Background = new SolidColorBrush(Colors.Orange) };
		}

		internal event EventHandler RefreshRequested;
		
	}

	public class PullToRefreshControlOuterPanel : Panel
	{
		public Size AvailableSize
		{
			get;
			private set;
		}

		public Size FinalSize
		{
			get;
			private set;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			AvailableSize = availableSize;
			// Children[0] is the outer ScrollViewer
			Children[0].Measure(availableSize);
			return Children[0].DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			FinalSize = finalSize;
			// Children[0] is the outer ScrollViewer
			Children[0].Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
			return finalSize;
		}
	}

	public class PullToRefreshControlInnerPanel : Panel, IScrollSnapPointsInfo
	{
		EventRegistrationTokenTable<EventHandler<object>> _verticaltable = new EventRegistrationTokenTable<EventHandler<object>>();
		EventRegistrationTokenTable<EventHandler<object>> _horizontaltable = new EventRegistrationTokenTable<EventHandler<object>>();

		protected override Size MeasureOverride(Size availableSize)
		{
			// need to get away from infinity
			var parent = Parent as FrameworkElement;
			while(!(parent is PullToRefreshControlOuterPanel))
			{
				parent = parent.Parent as FrameworkElement;
			}

			var outerPanel = parent as PullToRefreshControlOuterPanel;

			// Children[0] is the Border that comprises the refresh UI
			Children[0].Measure(outerPanel.AvailableSize);
			// Children[1] is the ListView
			Children[1].Measure(new Size(outerPanel.AvailableSize.Width, outerPanel.AvailableSize.Height));
			return new Size(Children[1].DesiredSize.Width, Children[0].DesiredSize.Height + outerPanel.AvailableSize.Height);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			// need to get away from infinity
			var parent = Parent as FrameworkElement;
			while(!(parent is PullToRefreshControlOuterPanel))
				parent = parent.Parent as FrameworkElement;

			var outerPanel = parent as PullToRefreshControlOuterPanel;

			// Children[0] is the Border that comprises the refresh UI
			Children[0].Arrange(new Rect(0, 0, Children[0].DesiredSize.Width, Children[0].DesiredSize.Height));
			// Children[1] is the ListView
			Children[1].Arrange(new Rect(0, Children[0].DesiredSize.Height, outerPanel.FinalSize.Width, outerPanel.FinalSize.Height));
			return finalSize;
		}

		bool IScrollSnapPointsInfo.AreHorizontalSnapPointsRegular
		{
			get
			{
				return false;
			}
		}

		bool IScrollSnapPointsInfo.AreVerticalSnapPointsRegular
		{
			get
			{
				return false;
			}
		}

		IReadOnlyList<float> IScrollSnapPointsInfo.GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
		{
			if(orientation == Orientation.Vertical)
			{
				var l = new List<float>();
				l.Add((float)Children[0].DesiredSize.Height);
				return l;
			}
			else
			{
				return new List<float>();
			}
		}

		float IScrollSnapPointsInfo.GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
		{
			throw new NotImplementedException();
		}

		event EventHandler<object> IScrollSnapPointsInfo.HorizontalSnapPointsChanged
		{
			add
			{
				return EventRegistrationTokenTable<EventHandler<object>>
						.GetOrCreateEventRegistrationTokenTable(ref _horizontaltable)
						.AddEventHandler(value);

			}
			remove
			{
				EventRegistrationTokenTable<EventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _horizontaltable)
					.RemoveEventHandler(value);
			}
		}

		event EventHandler<object> IScrollSnapPointsInfo.VerticalSnapPointsChanged
		{
			add
			{
				return EventRegistrationTokenTable<EventHandler<object>>
						.GetOrCreateEventRegistrationTokenTable(ref _verticaltable)
						.AddEventHandler(value);

			}
			remove
			{
				EventRegistrationTokenTable<EventHandler<object>>
					.GetOrCreateEventRegistrationTokenTable(ref _verticaltable)
					.RemoveEventHandler(value);
			}
		}

	}
}
