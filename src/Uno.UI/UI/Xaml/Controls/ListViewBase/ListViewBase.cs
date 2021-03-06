﻿#if !NET46 && !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
#if XAMARIN_ANDROID
using _View = Android.Views.View;
#elif XAMARIN_IOS
using _View = UIKit.UIView;
#else
using View = Windows.UI.Xaml.FrameworkElement;
#endif
using Uno;
using Uno.Extensions;
using Windows.UI.Xaml.Data;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Uno.Extensions.Specialized;
using System.Collections;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using Uno.Logging;
using Uno.Disposables;
using Uno.Client;
using System.Threading.Tasks;
using System.Threading;
using Windows.Foundation;
using Uno.UI;

namespace Windows.UI.Xaml.Controls
{
	public partial class ListViewBase : Selector
	{
		internal NativeListViewBase NativePanel { get { return InternalItemsPanelRoot as NativeListViewBase; } }
		internal ScrollViewer ScrollViewer { get; private set; }

		/// <summary>
		/// When this flag is set, the ListViewBase will process every notification from <see cref="INotifyCollectionChanged"/> as if it 
		/// were a 'Reset', triggering a complete refresh of the list. By default this is false.
		/// </summary>
		public bool RefreshOnCollectionChanged { get; set; } = false;

		internal override bool IsSingleSelection => SelectionMode == ListViewSelectionMode.Single;
		private bool IsSelectionMultiple => SelectionMode == ListViewSelectionMode.Multiple || SelectionMode == ListViewSelectionMode.Extended;
		private bool _modifyingSelectionInternally;
		private readonly List<object> _oldSelectedItems = new List<object>();
		/// <summary>
		/// Whether an incremental data loading request is currently under way.
		/// </summary>
		private bool _isIncrementalLoadingInFlight;
		/// <summary>
		/// The number of currently visible items, ie a 'page' from the point of view of incremental data loading.
		/// </summary>
		private int PageSize
		{
			get
			{
				if (NativePanel == null)
				{
					// Not supported
					return 0;
				}
				var lastVisibleIndex = NativePanel.NativeLayout.LastVisibleIndex;
				var firstVisibleIndex = NativePanel.NativeLayout.FirstVisibleIndex;
				if (lastVisibleIndex == -1)
				{
					return 0;
				}
				return lastVisibleIndex + 1 - firstVisibleIndex;
			}
		}

		protected ListViewBase()
		{
			Initialize();

			var selectedItems = new ObservableCollection<object>();
			selectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
			SelectedItems = selectedItems;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (!HasItems)
			{
				// If there are no items in the source, try to load at least one.
				TryLoadFirstItem();
			}
			return base.ArrangeOverride(finalSize);
		}

		private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_modifyingSelectionInternally)
			{
				//Internal operations that modify the list are responsible for invoking SelectionChanged, etc if necessary
				_oldSelectedItems.Clear();
				_oldSelectedItems.AddRange(SelectedItems);
				return;
			}

			var items = GetItems();
			if (items == null && _oldSelectedItems.Any())
			{
				// The list is being reset therefore we just need to reset the selection
				ResetSelection();
				return;
			}

			object[] validAdditions;
			object[] validRemovals;
			if (e.Action != NotifyCollectionChangedAction.Reset)
			{
				validAdditions = (e.NewItems ?? new object[0]).Where(item => items.Contains(item)).ToObjectArray();
				validRemovals = (e.OldItems ?? new object[0]).Where(item => items.Contains(item)).ToObjectArray();
			}
			else
			{
				validAdditions = new object[0];
				validRemovals = _oldSelectedItems.Where(item => items.Contains(item)).ToObjectArray();
			}
			try
			{
				_modifyingSelectionInternally = true;
				SelectedItem = SelectedItems.Where(item => items.Contains(item)).FirstOrDefault();
			}
			finally
			{
				_modifyingSelectionInternally = false;
			}
			if (validAdditions.Any() || validRemovals.Any())
			{
				InvokeSelectionChanged(validRemovals, validAdditions);
			}
			_oldSelectedItems.Clear();
			_oldSelectedItems.AddRange(SelectedItems);
		}

		private void ResetSelection()
		{
			try
			{
				_modifyingSelectionInternally = true;

				_oldSelectedItems.Clear();
				_oldSelectedItems.AddRange(SelectedItems);
				SelectedItems.Clear();
			}
			finally
			{
				_modifyingSelectionInternally = false;
			}
		}

		internal override void OnSelectedItemChanged(object oldSelectedItem, object selectedItem)
		{
			if (_modifyingSelectionInternally)
			{
				return;
			}
			if (IsSelectionMultiple)
			{
				var items = GetItems();
				if (selectedItem == null || items.Contains(selectedItem))
				{
					object[] removedItems = null;
					object[] addedItems = null;
					try
					{
						_modifyingSelectionInternally = true;
						removedItems = SelectedItems.Except(selectedItem).ToObjectArray();
						var isRealSelection = selectedItem != null || items.Contains(null);
						addedItems = SelectedItems.Contains(selectedItem) || !isRealSelection ? new object[0] : new[] { selectedItem };
						SelectedItems.Clear();
						if (isRealSelection)
						{
							SelectedItems.Add(selectedItem);
						}
					}
					finally
					{
						_modifyingSelectionInternally = false;
					}
					//Invoke event after resetting flag, in case callbacks in user code modify the collection
					if (addedItems.Length > 0 || removedItems.Length > 0)
					{
						InvokeSelectionChanged(removedItems, addedItems);
					}
				}
				else
				{
					SelectedItem = oldSelectedItem;
				}
			}
			else
			{
				base.OnSelectedItemChanged(oldSelectedItem, selectedItem);
			}
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// In Single mode, we respond to SelectedIndex changing, which is more reliable if there are duplicate items
			if (IsSelectionMultiple)
			{
				foreach (var item in e.AddedItems)
				{
					SetSelectedState(IndexFromItem(item), true);
				}
				foreach (var item in e.RemovedItems)
				{
					SetSelectedState(IndexFromItem(item), false);
				}
			}
		}

		private void OnSelectedIndexChanged(int oldSelectedIndex, int newSelectedIndex)
		{
			SetSelectedState(oldSelectedIndex, false);
			SetSelectedState(newSelectedIndex, true);
		}

		protected override void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnItemsSourceChanged(e);

			Refresh();
		}

		protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
		{
			base.OnItemContainerStyleChanged(oldItemContainerStyle, newItemContainerStyle);
			Refresh();
		}

		protected override void OnItemContainerStyleSelectorChanged(StyleSelector oldItemContainerStyleSelector, StyleSelector newItemContainerStyleSelector)
		{
			base.OnItemContainerStyleSelectorChanged(oldItemContainerStyleSelector, newItemContainerStyleSelector);
			Refresh();
		}

		protected override void OnItemTemplateSelectorChanged(DataTemplateSelector oldItemTemplateSelector, DataTemplateSelector newItemTemplateSelector)
		{
			base.OnItemTemplateSelectorChanged(oldItemTemplateSelector, newItemTemplateSelector);
			Refresh();
		}

		protected override void OnItemTemplateChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
		{
			base.OnItemTemplateChanged(oldItemTemplate, newItemTemplate);
			Refresh();
		}

		public event ItemClickEventHandler ItemClick;

		private void Initialize()
		{
			this.RegisterDisposablePropertyChangedCallback(SelectedIndexProperty, (s, e) => (s as ListViewBase).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue));
			SelectionChanged += OnSelectionChanged;
		}

		private ICommand _itemClickCommand;
		//TODO: Remove this as it doesn't exist on Windows
		public ICommand ItemClickCommand
		{
			get { return _itemClickCommand; }
			set
			{
				_itemClickCommand = value;
				OnItemClickCommandChangedPartial(value);
			}
		}

		partial void OnItemClickCommandChangedPartial(ICommand value);

		//Note: by a hackishly convenient coincidence, the binding u:ListViewBaseCommand.Command="{Binding [DoSomething]}" will bind to this property on Uno,
		// because the xaml source generation fails to find ListViewBaseCommand and ignores that part entirely. So...when this property is removed, port ListViewBaseCommand
		// to Xamarin, and all will be well.
		/// <summary>
		/// Deprecated, use ItemClickCommand instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ICommand Command
		{
			get { return ItemClickCommand; }
			set
			{
				ItemClickCommand = value;
			}
		}

		public IList<object> SelectedItems { get; }

		internal bool ShouldShowHeader => Header != null || HeaderTemplate != null;
		internal bool ShouldShowFooter => Footer != null || FooterTemplate != null;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ScrollViewer = this.GetTemplateChild("ScrollViewer") as ScrollViewer;

			// NativePanel may not exist if we're using a non-virtualizing ItemsPanel.
			if (NativePanel != null)
			{
				NativePanel.XamlParent = this;
				// Propagate the DataContext manually, since ItemsPanelRoot isn't really part of the visual tree
				ItemsPanelRoot.SetValue(DataContextProperty, DataContext, DependencyPropertyValuePrecedences.Inheritance);
				OnApplyTemplatePartial();

				if (ScrollViewer?.Style.Precedence == DependencyPropertyValuePrecedences.ImplicitStyle)
				{
					if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
					{
						this.Log().Warn($"Performance hit: {this} is using a ScrollViewer in its template with a default style, which will break virtualization. A Style containing {nameof(ListViewBaseScrollContentPresenter)} should be used.");
					}
					ScrollViewer.Style = Uno.UI.GlobalStaticResources.ListViewBaseScrollViewerStyle;
				}

				if (ScrollViewer != null)
				{
					NativePanel.HorizontalScrollBarVisibility = ScrollViewer.HorizontalScrollBarVisibility;
					NativePanel.VerticalScrollBarVisibility = ScrollViewer.VerticalScrollBarVisibility;
				}
			}
			else
			{
				if (ScrollViewer?.Style == Uno.UI.GlobalStaticResources.ListViewBaseScrollViewerStyle)
				{
					// We're not using NativeListViewBase so we need a 'real' ScrollViewer
					ScrollViewer.Style = Uno.UI.GlobalStaticResources.DefaultScrollViewerStyle;
				}
			}
		}
		partial void OnApplyTemplatePartial();

		partial void OnSelectionModeChangedPartial(ListViewSelectionMode oldSelectionMode, ListViewSelectionMode newSelectionMode)
		{
			SelectedIndex = -1;
			foreach (var item in SelectedItems)
			{
				SetSelectedState(IndexFromItem(item), false);
			}
			SelectedItems.Clear();
		}

		protected override _View ResolveInternalItemsPanel(_View itemsPanel)
		{
			// If the items panel is a virtualizing panel, we substitute it with NativeListViewBase
			var virtualizingPanel = itemsPanel as IVirtualizingPanel;
			if (virtualizingPanel != null)
			{
				var internalPanel = new NativeListViewBase();
				var layouter = virtualizingPanel.GetLayouter();
				PrepareNativeLayout(layouter);
				internalPanel.NativeLayout = layouter;
				internalPanel.BindToEquivalentProperty(virtualizingPanel, "Background");
				return internalPanel;
			}
			else
			{
				// Otherwise act as a normal ItemsControl
				return base.ResolveInternalItemsPanel(itemsPanel);
			}
		}

		partial void PrepareNativeLayout(VirtualizingPanelLayout layout);



		internal override void OnItemClicked(int clickedIndex)
		{
			var item = ItemFromIndex(clickedIndex);
			if (IsItemClickEnabled)
			{
				ItemClickCommand.ExecuteIfPossible(item);
				ItemClick?.Invoke(this, new ItemClickEventArgs { ClickedItem = item });
			}

			//Handle selection
			switch (SelectionMode)
			{
				case ListViewSelectionMode.None:
					break;
				case ListViewSelectionMode.Single:
					if (ItemsSource is ICollectionView collectionView)
					{
						//NOTE: Windows seems to call MoveCurrentTo(item); we set position instead to have expected behavior when you have duplicate items in the list.
						collectionView.MoveCurrentToPosition(clickedIndex);

						// The CollectionView may have intercepted the change
						clickedIndex = collectionView.CurrentPosition;
					}
					SelectedIndex = clickedIndex;
					break;
				case ListViewSelectionMode.Multiple:
				case ListViewSelectionMode.Extended:
					HandleMultipleSelection(clickedIndex, item);
					break;
			}
		}

		private void HandleMultipleSelection(int clickedIndex, object item)
		{
			if (!SelectedItems.Contains(item))
			{
				SelectedItems.Add(item);
				SetSelectedState(clickedIndex, true);
			}
			else
			{
				SelectedItems.Remove(item);
				SetSelectedState(clickedIndex, false);
			}
		}

		private void SetSelectedState(int clickedIndex, bool selected)
		{
			var selectorItem = ContainerFromIndex(clickedIndex) as SelectorItem;
			if (selectorItem != null)
			{
				selectorItem.IsSelected = selected;
			}
		}

		internal protected override void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnDataContextChanged(e);

			// Propagate the DataContext manually, since ItemsPanelRoot isn't really part of the visual tree
			ItemsPanelRoot?.SetValue(DataContextProperty, DataContext, DependencyPropertyValuePrecedences.Inheritance);

			OnDataContextChangedPartial();
		}

		partial void OnDataContextChangedPartial();

		internal object ResolveFooterContext()
		{
			return ResolveHeaderOrFooterContext(FooterProperty, FooterTemplateProperty);
		}

		internal override void OnItemsSourceSingleCollectionChanged(object sender, NotifyCollectionChangedEventArgs args, int section)
		{
			if (RefreshOnCollectionChanged)
			{
				completeRefresh();
				return;
			}

			// https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (AreEmptyGroupsHidden && (sender as IEnumerable).Count() == args.NewItems.Count)
					{
						//If HidesIfEmpty is true and a group becomes non-empty it is 'new' from the view of UICollectionView and we need to reset state
						// TODO: This could call AddGroup now that it's implemented
						completeRefresh();
						return;
					}
					if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
					{
						this.Log().Debug($"Inserting {args.NewItems.Count} items starting at {args.NewStartingIndex}");
					}
					AddItems(args.NewStartingIndex, args.NewItems.Count, section);
					break;
				case NotifyCollectionChangedAction.Remove:
					if (AreEmptyGroupsHidden && (sender as IEnumerable).None())
					{
						//If HidesIfEmpty is true and a group becomes empty it is 'vanished' from the view of UICollectionView and we need to reset state
						// TODO: This could call RemoveGroup now that it's implemented
						completeRefresh();
						return;
					}
					if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
					{
						this.Log().Debug($"Deleting {args.OldItems.Count} items starting at {args.OldStartingIndex}");
					}
					RemoveItems(args.OldStartingIndex, args.OldItems.Count, section);
					break;
				case NotifyCollectionChangedAction.Replace:
					ReplaceItems(args.NewStartingIndex, args.NewItems.Count, section);
					break;
				case NotifyCollectionChangedAction.Move:
					// TODO PBI #19974: Fully implement NotifyCollectionChangedActions and map them to the appropriate calls
					// on UICollectionView, MoveItems
					Refresh();
					break;
				case NotifyCollectionChangedAction.Reset:
					Refresh();
					break;
			}

			//Call base after so that list state is 'fresh' when we update SelectedItem
			base.OnItemsSourceSingleCollectionChanged(sender, args, section);

			void completeRefresh()
			{
				Refresh();
				ObserveCollectionChanged();
				base.OnItemsSourceSingleCollectionChanged(sender, args, section);
			}
		}

		internal override void OnItemsSourceGroupsChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (RefreshOnCollectionChanged)
			{
				Refresh();
				ObserveCollectionChanged();
				base.OnItemsSourceGroupsChanged(sender, args);
				return;
			}

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					//Add group header(s), group items
					for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
					{
						// On Android we add all items before any group headers; otherwise, since group headers are 'after' all items in the indexing 
						// used by the RecyclerView adapter, the additions will not be registered correctly.
						AddGroupItems(i);
					}
					for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
					{
						// Notify add of groups that are visible to the native list
						if (!AreEmptyGroupsHidden || GetGroupCount(i) > 0)
						{
							var adjustedIndex = AdjustGroupIndexForHidesIfEmpty(i);
							AddGroup(adjustedIndex);
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					// For Android, remove old group items and add new group items
					for (int i = args.OldItems.Count - 1; i >= 0; i--)
					{
						RemoveGroupItems(args.OldStartingIndex + i, GetCachedGroupCount(args.OldStartingIndex + i));
					}
					for (int i = args.OldItems.Count - 1; i >= 0; i--)
					{
						// Notify remove of groups that are visible to the native list
						if (!AreEmptyGroupsHidden || GetCachedGroupCount(args.OldStartingIndex + i) > 0)
						{
							var adjustedIndex = AdjustGroupIndexForHidesIfEmpty(args.OldStartingIndex + i);
							RemoveGroup(adjustedIndex);
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					// For Android, remove old group items and add new group items
					for (int i = args.OldItems.Count - 1; i >= 0; i--)
					{
						RemoveGroupItems(args.OldStartingIndex + i, GetCachedGroupCount(args.OldStartingIndex + i));
					}
					for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
					{
						AddGroupItems(i);
					}

					for (int i = args.NewStartingIndex; i < args.NewStartingIndex + args.NewItems.Count; i++)
					{
						var adjustedIndex = AdjustGroupIndexForHidesIfEmpty(i);
						ReplaceGroup(adjustedIndex);
					}
					break;
				default:
					//TODO: handle Move
					Refresh();
					break;
			}

			ObserveCollectionChanged();

			base.OnItemsSourceGroupsChanged(sender, args);
		}

		/// <summary>
		/// Update onscreen group header DataContext when ICollectionViewGroup.Group property changes.
		/// </summary>
		internal override void OnGroupPropertyChanged(ICollectionViewGroup group, int groupIndex)
		{
			base.OnGroupPropertyChanged(group, groupIndex);

			var groupContainer = ContainerFromGroupIndex(groupIndex);

			if (groupContainer != null)
			{
				groupContainer.DataContext = group.Group;
			}
		}

		/// <summary>
		/// Insert items in a newly inserted group. This is only needed on Android, which doesn't natively support the concept of a group.
		/// </summary>
		partial void AddGroupItems(int groupIndex);

		/// <summary>
		/// Remove items in a disappearing group. This is only needed on Android, which doesn't natively support the concept of a group.
		/// </summary>
		partial void RemoveGroupItems(int groupIndex, int groupCount);

		/// <summary>
		/// Carry out collection Replace action. We just rebind the existing item, rather than calling the native replace method, to minimize 'flicker' when the item is nearly the same. 
		/// </summary>
		private void ReplaceItems(int firstItem, int count, int section)
		{
			for (int i = 0; i < count; i++)
			{
				var unoIndexPath = IndexPath.FromRowSection(firstItem + i, section);
				var flatIndex = GetIndexFromIndexPath(unoIndexPath);
				var container = ContainerFromIndex(flatIndex);
				if (container != null)
				{
					var item = GetDisplayItemFromIndexPath(unoIndexPath);
					PrepareContainerForIndex(container, flatIndex);
				}
				else
				{
					// On Android, we call the native replace-equivalent to make sure that views awaiting recycling are correctly marked as needing rebinding.
					NativeReplaceItems(i, 1, section);
				}
			}
		}

		partial void NativeReplaceItems(int firstItem, int count, int section);

		internal object ResolveHeaderContext()
		{
			return ResolveHeaderOrFooterContext(HeaderProperty, HeaderTemplateProperty);
		}

		/// <summary>
		/// Resolve the context to use for header/footer. This should be the Header/Footer properties if they are set;
		/// if not, but the HeaderTemplate/FooterTemplate is non-null, then the ListViewBase's DataContext should be used.
		/// </summary>
		private object ResolveHeaderOrFooterContext(DependencyProperty contextProperty, DependencyProperty templateProperty)
		{
			if (this.IsDependencyPropertySet(contextProperty))
			{
				return GetValue(contextProperty);
			}
			else if (GetValue(templateProperty) != null)
			{
				return DataContext;
			}
			else
			{
				return null;
			}
		}

		internal override bool IsSelected(int index)
		{
			switch (SelectionMode)
			{
				case ListViewSelectionMode.None:
					return false;
				case ListViewSelectionMode.Single:
					return index == SelectedIndex;
				case ListViewSelectionMode.Multiple:
				case ListViewSelectionMode.Extended:
					return SelectedItems.Any(item => IndexFromItem(item).Equals(index));
			}

			return false;
		}

		/// <summary>
		/// Try to fetch more items, if the ItemsSource supports <see cref="ISupportIncrementalLoading"/>.
		/// </summary>
		/// <param name="currentLastItem">The last item index currently visible. (In practice this may also be less than the last visible index, without ill effects.)</param>
		internal void TryLoadMoreItems(int currentLastItem)
		{
			if (CanLoadItems)
			{
				// IncrementalLoadingThreshold = 0 means 'load when we have less than a page of items left to show'
				var desiredItemBuffer = (IncrementalLoadingThreshold + 1) * PageSize;
				var remainingItems = NumberOfItems - 1 - currentLastItem;
				if (remainingItems <= desiredItemBuffer)
				{
					var pageSize = Math.Max(1, PageSize); //TODO: PageSize should probably report how many items *could* fit on the page (based on item extent /viewport height), not how many actually do
					TryLoadMoreItemsInner((int)(DataFetchSize * pageSize));
				}
			}
		}

		private void TryLoadFirstItem()
		{
			if (CanLoadItems)
			{
				TryLoadMoreItemsInner(1);
			}
		}

		/// <summary>
		/// True if the source is an <see cref="ISupportIncrementalLoading"/> with more items, incremental loading is enabled, and no request is pending.
		/// </summary>
		private bool CanLoadItems => !_isIncrementalLoadingInFlight
			&& IncrementalLoadingTrigger == IncrementalLoadingTrigger.Edge
			&& SourceHasMoreItems;

		private bool SourceHasMoreItems => (GetItems() is ISupportIncrementalLoading incrementalSource && incrementalSource.HasMoreItems) ||
			(GetItems() is ICollectionView collectionView && collectionView.HasMoreItems);


		private void TryLoadMoreItemsInner(int count)
		{
			_isIncrementalLoadingInFlight = true;
			LoadMoreItemsAsync(GetItems(), (uint)count);
		}

		private async Task LoadMoreItemsAsync(object incrementalSource, uint count)
		{
			LoadMoreItemsResult result = default(LoadMoreItemsResult);
			try
			{
				if (incrementalSource is ISupportIncrementalLoading supportIncrementalLoading)
				{
					result = await supportIncrementalLoading.LoadMoreItemsAsync(count);
				}
				else if (incrementalSource is ICollectionView collectionView)
				{
					result = await collectionView.LoadMoreItemsAsync(count);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			catch (Exception e)
			{
				if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
				{
					this.Log().Warn($"{nameof(LoadMoreItemsAsync)} failed.", e);
				}
			}
			finally
			{
				_isIncrementalLoadingInFlight = false;
			}

			// If we got some items, try to get some more if we haven't filled the desired buffer. This is mainly needed to handle an 
			// unfilled viewport because PageSize doesn't return the 'potential' number of visible items.
			if (result.Count > 0)
			{
				TryLoadMoreItems(NativePanel.NativeLayout.LastVisibleIndex);
			}
		}
	}
}

#endif