#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Storage.Pickers.Provider
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public   enum AddFileResult 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		Added,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		AlreadyAdded,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		NotAllowed,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		Unavailable,
		#endif
	}
	#endif
}
