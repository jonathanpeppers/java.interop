using System;
using System.Collections.Generic;
using Android.Runtime;
using Java.Interop;

namespace Xamarin.Test {

	// Metadata.xml XPath interface reference: path="/api/package[@name='xamarin.test']/interface[@name='ExtendedInterface']"
	[Register ("xamarin/test/ExtendedInterface", "", "Xamarin.Test.IExtendedInterfaceInvoker")]
	public partial interface IExtendedInterface : IJavaObject, IJavaPeerable {
		// Metadata.xml XPath method reference: path="/api/package[@name='xamarin.test']/interface[@name='ExtendedInterface']/method[@name='extendedMethod' and count(parameter)=0]"
		[Register ("extendedMethod", "()V", "GetExtendedMethodHandler:Xamarin.Test.IExtendedInterfaceInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
		void ExtendedMethod ();

		// Metadata.xml XPath method reference: path="/api/package[@name='xamarin.test']/interface[@name='ExtendedInterface']/method[@name='baseMethod' and count(parameter)=0]"
		[Register ("baseMethod", "()V", "GetBaseMethodHandler:Xamarin.Test.IExtendedInterfaceInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
		void BaseMethod ();

	}

	[global::Android.Runtime.Register ("xamarin/test/ExtendedInterface", DoNotGenerateAcw=true)]
	internal partial class IExtendedInterfaceInvoker : global::Java.Lang.Object, IExtendedInterface {
		static IntPtr java_class_ref {
			get { return _members_xamarin_test_ExtendedInterface.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers {
			get { return _members_xamarin_test_ExtendedInterface; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override IntPtr ThresholdClass {
			get { return _members_xamarin_test_ExtendedInterface.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable (global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable (global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType {
			get { return _members_xamarin_test_ExtendedInterface.ManagedPeerType; }
		}

		static readonly JniPeerMembers _members_xamarin_test_BaseInterface = new XAPeerMembers ("xamarin/test/BaseInterface", typeof (IExtendedInterfaceInvoker));

		static readonly JniPeerMembers _members_xamarin_test_ExtendedInterface = new XAPeerMembers ("xamarin/test/ExtendedInterface", typeof (IExtendedInterfaceInvoker));

		public IExtendedInterfaceInvoker (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
		{
		}

		static Delegate cb_extendedMethod_ExtendedMethod_V;
#pragma warning disable 0169
		static Delegate GetExtendedMethodHandler ()
		{
			return cb_extendedMethod_ExtendedMethod_V ??= new _JniMarshal_PP_V (n_ExtendedMethod);
		}

		[global::System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
		static void n_ExtendedMethod (IntPtr jnienv, IntPtr native__this)
		{
			if (!global::Java.Interop.JniEnvironment.BeginMarshalMethod (jnienv, out var __envp, out var __r))
				return;

			try {
				var __this = global::Java.Lang.Object.GetObject<global::Xamarin.Test.IExtendedInterface> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
				__this.ExtendedMethod ();
			} catch (global::System.Exception __e) {
				__r.OnUserUnhandledException (ref __envp, __e);
			} finally {
				global::Java.Interop.JniEnvironment.EndMarshalMethod (ref __envp);
			}
		}
#pragma warning restore 0169

		public unsafe void ExtendedMethod ()
		{
			const string __id = "extendedMethod.()V";
			try {
				_members_xamarin_test_ExtendedInterface.InstanceMethods.InvokeAbstractVoidMethod (__id, this, null);
			} finally {
			}
		}

		static Delegate cb_baseMethod_BaseMethod_V;
#pragma warning disable 0169
		static Delegate GetBaseMethodHandler ()
		{
			return cb_baseMethod_BaseMethod_V ??= new _JniMarshal_PP_V (n_BaseMethod);
		}

		[global::System.Diagnostics.DebuggerDisableUserUnhandledExceptions]
		static void n_BaseMethod (IntPtr jnienv, IntPtr native__this)
		{
			if (!global::Java.Interop.JniEnvironment.BeginMarshalMethod (jnienv, out var __envp, out var __r))
				return;

			try {
				var __this = global::Java.Lang.Object.GetObject<global::Xamarin.Test.IExtendedInterface> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
				__this.BaseMethod ();
			} catch (global::System.Exception __e) {
				__r.OnUserUnhandledException (ref __envp, __e);
			} finally {
				global::Java.Interop.JniEnvironment.EndMarshalMethod (ref __envp);
			}
		}
#pragma warning restore 0169

		public unsafe void BaseMethod ()
		{
			const string __id = "baseMethod.()V";
			try {
				_members_xamarin_test_ExtendedInterface.InstanceMethods.InvokeAbstractVoidMethod (__id, this, null);
			} finally {
			}
		}

	}
}
