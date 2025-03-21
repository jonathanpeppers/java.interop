using System;
using System.Collections.Generic;
using Android.Runtime;
using Java.Interop;

namespace Com.Google.Android.Exoplayer.Drm {

	// Metadata.xml XPath interface reference: path="/api/package[@name='com.google.android.exoplayer.drm']/interface[@name='ExoMediaDrm.OnEventListener']"
	[Register ("com/google/android/exoplayer/drm/ExoMediaDrm$OnEventListener", "", "Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListenerInvoker")]
	[global::Java.Interop.JavaTypeParameters (new string [] {"T extends com.google.android.exoplayer.drm.ExoMediaCrypto"})]
	public partial interface IExoMediaDrmOnEventListener : IJavaObject, IJavaPeerable {
		// Metadata.xml XPath method reference: path="/api/package[@name='com.google.android.exoplayer.drm']/interface[@name='ExoMediaDrm.OnEventListener']/method[@name='onEvent' and count(parameter)=5 and parameter[1][@type='com.google.android.exoplayer.drm.ExoMediaDrm&lt;T&gt;'] and parameter[2][@type='byte[]'] and parameter[3][@type='int'] and parameter[4][@type='int'] and parameter[5][@type='byte[]']]"
		[Register ("onEvent", "(Lcom/google/android/exoplayer/drm/ExoMediaDrm;[BII[B)V", "GetOnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayBHandler:Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
		void OnEvent (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm p0, byte[] p1, int p2, int p3, byte[] p4);

	}

	[global::Android.Runtime.Register ("com/google/android/exoplayer/drm/ExoMediaDrm$OnEventListener", DoNotGenerateAcw=true)]
	internal partial class IExoMediaDrmOnEventListenerInvoker : global::Java.Lang.Object, IExoMediaDrmOnEventListener {
		static IntPtr java_class_ref {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override IntPtr ThresholdClass {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener.ManagedPeerType; }
		}

		static readonly JniPeerMembers _members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener = new XAPeerMembers ("com/google/android/exoplayer/drm/ExoMediaDrm$OnEventListener", typeof (IExoMediaDrmOnEventListenerInvoker));

		public IExoMediaDrmOnEventListenerInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
		{
		}

		static Delegate cb_onEvent_OnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayB_V;
#pragma warning disable 0169
		static Delegate GetOnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayBHandler ()
		{
			return cb_onEvent_OnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayB_V ??= new _JniMarshal_PPLLIIL_V (n_OnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayB);
		}

		[global::System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
		static void n_OnEvent_Lcom_google_android_exoplayer_drm_ExoMediaDrm_arrayBIIarrayB (IntPtr jnienv, IntPtr native__this, IntPtr native_p0, IntPtr native_p1, int p2, int p3, IntPtr native_p4)
		{
			if (!global::Java.Interop.JniEnvironment.BeginMarshalMethod (jnienv, out var __envp, out var __r))
				return;

			try {
				var __this = global::Java.Lang.Object.GetObject<global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListener> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
				var p0 = (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm)global::Java.Lang.Object.GetObject<global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm> (native_p0, JniHandleOwnership.DoNotTransfer);
				var p1 = (byte[]) JNIEnv.GetArray (native_p1, JniHandleOwnership.DoNotTransfer, typeof (byte));
				var p4 = (byte[]) JNIEnv.GetArray (native_p4, JniHandleOwnership.DoNotTransfer, typeof (byte));
				__this.OnEvent (p0, p1, p2, p3, p4);
				if (p1 != null)
					JNIEnv.CopyArray (p1, native_p1);
				if (p4 != null)
					JNIEnv.CopyArray (p4, native_p4);
			} catch (global::System.Exception __e) {
				__r.OnUserUnhandledException (ref __envp, __e);
			} finally {
				global::Java.Interop.JniEnvironment.EndMarshalMethod (ref __envp);
			}
		}
#pragma warning restore 0169

		public unsafe void OnEvent (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm p0, byte[] p1, int p2, int p3, byte[] p4)
		{
			const string __id = "onEvent.(Lcom/google/android/exoplayer/drm/ExoMediaDrm;[BII[B)V";
			IntPtr native_p1 = JNIEnv.NewArray (p1);
			IntPtr native_p4 = JNIEnv.NewArray (p4);
			try {
				JniArgumentValue* __args = stackalloc JniArgumentValue [5];
				__args [0] = new JniArgumentValue ((p0 == null) ? IntPtr.Zero : ((global::Java.Lang.Object) p0).Handle);
				__args [1] = new JniArgumentValue (native_p1);
				__args [2] = new JniArgumentValue (p2);
				__args [3] = new JniArgumentValue (p3);
				__args [4] = new JniArgumentValue (native_p4);
				_members_com_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener.InstanceMethods.InvokeAbstractVoidMethod (__id, this, __args);
			} finally {
				if (p1 != null) {
					JNIEnv.CopyArray (native_p1, p1);
					JNIEnv.DeleteLocalRef (native_p1);
				}
				if (p4 != null) {
					JNIEnv.CopyArray (native_p4, p4);
					JNIEnv.DeleteLocalRef (native_p4);
				}
				global::System.GC.KeepAlive (p0);
				global::System.GC.KeepAlive (p1);
				global::System.GC.KeepAlive (p4);
			}
		}

	}

	// event args for com.google.android.exoplayer.drm.ExoMediaDrm.OnEventListener.onEvent
	public partial class ExoMediaDrmOnEventEventArgs : global::System.EventArgs {
		public ExoMediaDrmOnEventEventArgs (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm p0, byte[] p1, int p2, int p3, byte[] p4)
		{
			this.p0 = p0;
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
			this.p4 = p4;
		}

		global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm p0;

		public global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm P0 {
			get { return p0; }
		}

		byte[] p1;

		public byte[] P1 {
			get { return p1; }
		}

		int p2;

		public int P2 {
			get { return p2; }
		}

		int p3;

		public int P3 {
			get { return p3; }
		}

		byte[] p4;

		public byte[] P4 {
			get { return p4; }
		}

	}

	[global::Android.Runtime.Register ("mono/com/google/android/exoplayer/drm/ExoMediaDrm_OnEventListenerImplementor")]
	internal sealed partial class IExoMediaDrmOnEventListenerImplementor : global::Java.Lang.Object, IExoMediaDrmOnEventListener {

		object sender;

		public unsafe IExoMediaDrmOnEventListenerImplementor (object sender) : base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
		{
			const string __id = "()V";
			if (((global::Java.Lang.Object) this).Handle != IntPtr.Zero)
				return;
			var h = JniPeerMembers.InstanceMethods.StartCreateInstance (__id, ((object) this).GetType (), null);
			SetHandle (h.Handle, JniHandleOwnership.TransferLocalRef);
			JniPeerMembers.InstanceMethods.FinishCreateInstance (__id, this, null);
			this.sender = sender;
		}

		#pragma warning disable 0649
		public EventHandler<ExoMediaDrmOnEventEventArgs> Handler;
		#pragma warning restore 0649

		public void OnEvent (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm p0, byte[] p1, int p2, int p3, byte[] p4)
		{
			var __h = Handler;
			if (__h != null)
				__h (sender, new ExoMediaDrmOnEventEventArgs (p0, p1, p2, p3, p4));
		}

		internal static bool __IsEmpty (IExoMediaDrmOnEventListenerImplementor value)
		{
			return value.Handler == null;
		}

	}

	// Metadata.xml XPath interface reference: path="/api/package[@name='com.google.android.exoplayer.drm']/interface[@name='ExoMediaDrm']"
	[Register ("com/google/android/exoplayer/drm/ExoMediaDrm", "", "Com.Google.Android.Exoplayer.Drm.IExoMediaDrmInvoker")]
	[global::Java.Interop.JavaTypeParameters (new string [] {"T extends com.google.android.exoplayer.drm.ExoMediaCrypto"})]
	public partial interface IExoMediaDrm : IJavaObject, IJavaPeerable {
		// Metadata.xml XPath method reference: path="/api/package[@name='com.google.android.exoplayer.drm']/interface[@name='ExoMediaDrm']/method[@name='setOnEventListener' and count(parameter)=1 and parameter[1][@type='com.google.android.exoplayer.drm.ExoMediaDrm.OnEventListener&lt;T&gt;']]"
		[Register ("setOnEventListener", "(Lcom/google/android/exoplayer/drm/ExoMediaDrm$OnEventListener;)V", "GetSetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener_Handler:Com.Google.Android.Exoplayer.Drm.IExoMediaDrmInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
		void SetOnEventListener (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListener p0);

	}

	[global::Android.Runtime.Register ("com/google/android/exoplayer/drm/ExoMediaDrm", DoNotGenerateAcw=true)]
	internal partial class IExoMediaDrmInvoker : global::Java.Lang.Object, IExoMediaDrm {
		static IntPtr java_class_ref {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override IntPtr ThresholdClass {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType {
			get { return _members_com_google_android_exoplayer_drm_ExoMediaDrm.ManagedPeerType; }
		}

		static readonly JniPeerMembers _members_com_google_android_exoplayer_drm_ExoMediaDrm = new XAPeerMembers ("com/google/android/exoplayer/drm/ExoMediaDrm", typeof (IExoMediaDrmInvoker));

		public IExoMediaDrmInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
		{
		}

		static Delegate cb_setOnEventListener_SetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener__V;
#pragma warning disable 0169
		static Delegate GetSetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener_Handler ()
		{
			return cb_setOnEventListener_SetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener__V ??= new _JniMarshal_PPL_V (n_SetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener_);
		}

		[global::System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
		static void n_SetOnEventListener_Lcom_google_android_exoplayer_drm_ExoMediaDrm_OnEventListener_ (IntPtr jnienv, IntPtr native__this, IntPtr native_p0)
		{
			if (!global::Java.Interop.JniEnvironment.BeginMarshalMethod (jnienv, out var __envp, out var __r))
				return;

			try {
				var __this = global::Java.Lang.Object.GetObject<global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrm> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
				var p0 = (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListener)global::Java.Lang.Object.GetObject<global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListener> (native_p0, JniHandleOwnership.DoNotTransfer);
				__this.SetOnEventListener (p0);
			} catch (global::System.Exception __e) {
				__r.OnUserUnhandledException (ref __envp, __e);
			} finally {
				global::Java.Interop.JniEnvironment.EndMarshalMethod (ref __envp);
			}
		}
#pragma warning restore 0169

		public unsafe void SetOnEventListener (global::Com.Google.Android.Exoplayer.Drm.IExoMediaDrmOnEventListener p0)
		{
			const string __id = "setOnEventListener.(Lcom/google/android/exoplayer/drm/ExoMediaDrm$OnEventListener;)V";
			try {
				JniArgumentValue* __args = stackalloc JniArgumentValue [1];
				__args [0] = new JniArgumentValue ((p0 == null) ? IntPtr.Zero : ((global::Java.Lang.Object) p0).Handle);
				_members_com_google_android_exoplayer_drm_ExoMediaDrm.InstanceMethods.InvokeAbstractVoidMethod (__id, this, __args);
			} finally {
				global::System.GC.KeepAlive (p0);
			}
		}

	}
}
