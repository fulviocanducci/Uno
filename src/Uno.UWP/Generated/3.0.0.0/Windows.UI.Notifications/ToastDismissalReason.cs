#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.UI.Notifications
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public   enum ToastDismissalReason 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		UserCanceled,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		ApplicationHidden,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		TimedOut,
		#endif
	}
	#endif
}
