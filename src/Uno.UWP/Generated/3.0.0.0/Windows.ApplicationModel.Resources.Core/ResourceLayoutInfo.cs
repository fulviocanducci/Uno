#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.ApplicationModel.Resources.Core
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public  partial struct ResourceLayoutInfo 
	{
		// Forced skipping of method Windows.ApplicationModel.Resources.Core.ResourceLayoutInfo.ResourceLayoutInfo()
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		public  uint MajorVersion;
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		public  uint MinorVersion;
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		public  uint ResourceSubtreeCount;
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		public  uint NamedResourceCount;
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		public  int Checksum;
		#endif
	}
}
