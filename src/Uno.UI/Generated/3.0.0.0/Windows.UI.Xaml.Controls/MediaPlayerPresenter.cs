#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.UI.Xaml.Controls
{
	#if false || false || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public  partial class MediaPlayerPresenter : global::Windows.UI.Xaml.FrameworkElement
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public  global::Windows.UI.Xaml.Media.Stretch Stretch
		{
			get
			{
				return (global::Windows.UI.Xaml.Media.Stretch)this.GetValue(StretchProperty);
			}
			set
			{
				this.SetValue(StretchProperty, value);
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public  global::Windows.Media.Playback.MediaPlayer MediaPlayer
		{
			get
			{
				return (global::Windows.Media.Playback.MediaPlayer)this.GetValue(MediaPlayerProperty);
			}
			set
			{
				this.SetValue(MediaPlayerProperty, value);
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public  bool IsFullWindow
		{
			get
			{
				return (bool)this.GetValue(IsFullWindowProperty);
			}
			set
			{
				this.SetValue(IsFullWindowProperty, value);
			}
		}
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public static global::Windows.UI.Xaml.DependencyProperty IsFullWindowProperty { get; } = 
		Windows.UI.Xaml.DependencyProperty.Register(
			"IsFullWindow", typeof(bool), 
			typeof(global::Windows.UI.Xaml.Controls.MediaPlayerPresenter), 
			new FrameworkPropertyMetadata(default(bool)));
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public static global::Windows.UI.Xaml.DependencyProperty MediaPlayerProperty { get; } = 
		Windows.UI.Xaml.DependencyProperty.Register(
			"MediaPlayer", typeof(global::Windows.Media.Playback.MediaPlayer), 
			typeof(global::Windows.UI.Xaml.Controls.MediaPlayerPresenter), 
			new FrameworkPropertyMetadata(default(global::Windows.Media.Playback.MediaPlayer)));
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public static global::Windows.UI.Xaml.DependencyProperty StretchProperty { get; } = 
		Windows.UI.Xaml.DependencyProperty.Register(
			"Stretch", typeof(global::Windows.UI.Xaml.Media.Stretch), 
			typeof(global::Windows.UI.Xaml.Controls.MediaPlayerPresenter), 
			new FrameworkPropertyMetadata(default(global::Windows.UI.Xaml.Media.Stretch)));
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		[global::Uno.NotImplemented]
		public MediaPlayerPresenter() : base()
		{
			global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.UI.Xaml.Controls.MediaPlayerPresenter", "MediaPlayerPresenter.MediaPlayerPresenter()");
		}
		#endif
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.MediaPlayerPresenter()
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.MediaPlayer.get
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.MediaPlayer.set
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.Stretch.get
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.Stretch.set
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.IsFullWindow.get
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.IsFullWindow.set
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.MediaPlayerProperty.get
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.StretchProperty.get
		// Forced skipping of method Windows.UI.Xaml.Controls.MediaPlayerPresenter.IsFullWindowProperty.get
	}
}
