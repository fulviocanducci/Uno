#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.ApplicationModel
{
	#if false || false || false || false
	[global::Uno.NotImplemented]
	#endif
	public  partial class SuspendingOperation : global::Windows.ApplicationModel.ISuspendingOperation
	{
		#if false || false || false || false
		[global::Uno.NotImplemented]
		public  global::System.DateTimeOffset Deadline
		{
			get
			{
				throw new global::System.NotImplementedException("The member DateTimeOffset SuspendingOperation.Deadline is not implemented in Uno.");
			}
		}
		#endif
		#if false || false || false || false
		[global::Uno.NotImplemented]
		public  global::Windows.ApplicationModel.SuspendingDeferral GetDeferral()
		{
			throw new global::System.NotImplementedException("The member SuspendingDeferral SuspendingOperation.GetDeferral() is not implemented in Uno.");
		}
		#endif
		// Forced skipping of method Windows.ApplicationModel.SuspendingOperation.Deadline.get
		// Processing: Windows.ApplicationModel.ISuspendingOperation
	}
}
