#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Gaming.Input.Custom
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public  partial interface IHidGameControllerInputSink : global::Windows.Gaming.Input.Custom.IGameControllerInputSink
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		void OnInputReportReceived( ulong timestamp,  byte reportId,  byte[] reportBuffer);
		#endif
	}
}
