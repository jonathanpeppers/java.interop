using System;
using System.Collections.Generic;
using System.Linq;
using Java.Interop.Tools.Generator;
using Xamarin.Android.Binder;

namespace MonoDroid.Generation
{
	public class InterfaceGen : GenBase, IRequireGenericMarshal
	{
		public InterfaceGen (GenBaseSupport support) : base (support)
		{
			DefaultValue = "IntPtr.Zero";
			NativeType = "IntPtr";
		}

		public override void AddNestedType (GenBase gen)
		{
			base.AddNestedType (gen);

			var nest_name = gen.JavaName.Substring (JavaName.Length + 1);

			if (nest_name.IndexOf ('.') < 0) {
				// We don't need to mangle the name if we support nested interface types
				// ex: my.namespace.IParent.IChild
				if (!gen.Unnest) {
					gen.FullName = FullName + "." + gen.Name;
					return;
				}

				if (gen is InterfaceGen) {
					// ex: my.namespace.IParentChild
					gen.FullName = FullName + gen.Name.Substring (1);
					gen.Name = Name + gen.Name.Substring (1);
				} else {
					gen.FullName = FullName.Substring (0, FullName.Length - Name.Length) + Name.Substring (1) + gen.Name;
					gen.Name = Name.Substring (1) + gen.Name;
				}
			}
		}

		public string ArgsType { get; set; }

		public override string FromNative (CodeGenerationOptions opt, string varname, bool owned)
		{
			if (opt.CodeGenerationTarget == CodeGenerationTarget.JavaInterop1) {
				return "global::Java.Interop.JniEnvironment.Runtime.ValueManager.GetValue<" +
					opt.GetOutputName (FullName) +
					$"> (ref {varname}, JniObjectReferenceOptions.{(owned ? "CopyAndDispose" : "Copy")})";
			}
			return string.Format ("global::Java.Lang.Object.GetObject<{0}> ({1}, {2})", opt.GetOutputName (FullName), varname, owned ? "JniHandleOwnership.TransferLocalRef" : "JniHandleOwnership.DoNotTransfer");
			/*
			if (String.IsNullOrEmpty (Marshaler))
				return String.Format ("global::Java.Lang.Object.GetObject<{0}> ({1}, {2})", opt.GetOutputName (FullName), varname, owned ? "JniHandleOwnership.TransferLocalRef" : "JniHandleOwnership.DoNotTransfer");
			else
				return String.Format ("new {1} ({0})", varname, Marshaler);
			*/
		}

		public override void FixupAccessModifiers (CodeGenerationOptions opt)
		{
			if (!IsAnnotation) {
				foreach (var implementedInterface in ImplementedInterfaces) {
					if (string.IsNullOrEmpty (implementedInterface)) {
						System.Diagnostics.Debug.Assert (false, "BUGBUG - We should never have an empty or null string added on the implemented interface list.");
						continue;
					}

					var baseType = opt.SymbolTable.Lookup (implementedInterface);
					if (baseType is InterfaceGen interfaceGen && interfaceGen.RawVisibility != "public") {
						// Copy over "private" methods
						foreach (var method in interfaceGen.Methods.ToList ())
							if (!Methods.Any (m => m.Matches (method)))
								Methods.Add (method.Clone (this));
					} else {
						break;
					}
				}
			}


			base.FixupAccessModifiers (opt);
		}

		public override void Generate (CodeGenerationOptions opt, GenerationInfo gen_info)
		{
			using (var sw = gen_info.OpenStream (opt.GetFileName (FullName))) {
				sw.WriteLine ("using System;");
				sw.WriteLine ("using System.Collections.Generic;");
				if (opt.CodeGenerationTarget != CodeGenerationTarget.JavaInterop1) {
					sw.WriteLine ("using Android.Runtime;");
				}
				sw.WriteLine ("using Java.Interop;");
				sw.WriteLine ();
				var hasNamespace = !string.IsNullOrWhiteSpace (Namespace);
				if (hasNamespace) {
					sw.WriteLine ("namespace {0} {{", Namespace);
					sw.WriteLine ();
				}

				var generator = opt.CreateCodeGenerator (sw);
				generator.WriteType (this, hasNamespace ? "\t" : string.Empty, gen_info);

				if (hasNamespace) {
					sw.WriteLine ("}");
				}
			}

			GenerateAnnotationAttribute (opt, gen_info);
		}

		internal string GetArgsName (Method m)
		{

			string nameBase;
			int start;
			int trim = 0;

			if (Methods.Count > 1) {
				if (!string.IsNullOrEmpty (m.ArgsType))
					return m.ArgsType;
				if (m.IsSimpleEventHandler)
					return "EventArgs";
				nameBase = m.AdjustedName;
				start = nameBase.StartsWith ("On", StringComparison.Ordinal) ? 2 : 0;
			} else {
				if (!string.IsNullOrEmpty (ArgsType))
					return ArgsType;
				if (m.IsSimpleEventHandler)
					return "EventArgs";
				nameBase = Name;
				start = Name.StartsWith ("IOn", StringComparison.Ordinal) ? 3 : 1;
				trim = 8; // "Listener"
			}
			return nameBase.Substring (start, nameBase.Length - start - trim) + "EventArgs";
		}

		internal string GetEventDelegateName (Method m)
		{
			int start = Name.StartsWith ("IOn", StringComparison.Ordinal) ? 3 : 1;
			if (m.RetVal.IsVoid) {
				if (m.IsSimpleEventHandler)
					return "EventHandler";
				else {
					return "EventHandler<" + GetArgsName (m) + ">";
				}
			} else if (m.IsEventHandlerWithHandledProperty) {
				return "EventHandler<" + GetArgsName (m) + ">";
			} else {
				string methodSpec = Methods.Count > 1 ? m.AdjustedName : string.Empty;
				return Name.Substring (start, Name.Length - start - 8) + methodSpec + "Handler";
			}
		}

		// These are fields that we currently support generating on the interface with DIM
		public IEnumerable<Field> GetGeneratableFields (CodeGenerationOptions options)
		{
			var fields = new List<Field> ();

			// Constant fields
			if (options.SupportInterfaceConstants)
				fields.AddRange (Fields.Where (f => !f.NeedsProperty && !(f.DeprecatedComment?.Contains ("constant will be removed") == true)));

			// Invoked fields exposed as properties
			if (options.SupportDefaultInterfaceMethods)
				fields.AddRange (Fields.Where (f => f.NeedsProperty && !(f.DeprecatedComment?.Contains ("constant will be removed") == true)));

			return fields;
		}

		public bool HasDefaultMethods => GetAllMethods ().Any (m => m.IsInterfaceDefaultMethod);

		public bool HasFieldsAsProperties => Fields.Any (f => f.NeedsProperty);

		public bool HasStaticMethods => GetAllMethods ().Any (m => m.IsStatic);

		public bool IsConstSugar (CodeGenerationOptions options)
		{
			if (!options.RemoveConstSugar)
				return false;

			if (Methods.Count > 0 || Properties.Count > 0)
				return false;

			foreach (InterfaceGen impl in GetAllDerivedInterfaces ())
				if (!impl.IsConstSugar (options))
					return false;

			// Need to keep Java.IO.ISerializable as a "marker interface"; want to
			// hide android.provider.ContactsContract.DataColumnsWithJoins
			if (Fields.Count == 0 && Interfaces.Count == 0)
				return false;

			return true;
		}

		// If there is a property it cannot generate valid implementor, so reject this at least so far.
		public bool IsListener => Name.EndsWith ("Listener", StringComparison.Ordinal) && Properties.Count == 0 && Interfaces.Count == 0;

		public bool HasManagedName { get; set; }

		public bool MayHaveManagedGenericArguments { get; set; }

		internal bool NeedsSender =>
			Methods.Any (m => (m.RetVal.IsVoid && !m.Parameters.HasSender) || (m.IsEventHandlerWithHandledProperty && !m.Parameters.HasSender));

		// If true, we will no longer generate the "interface alternative" legacy classes
		// used to hold interface constants/static methods before we had C#8
		public bool NoAlternatives { get; set; }

		protected override bool OnValidate (CodeGenerationOptions opt, GenericParameterDefinitionList type_params, CodeGeneratorContext context)
		{
			if (validated)
				return IsValid;

			validated = true;

			// Due to demand to validate in prior to validate ClassGen's BaseType, it is *not* done at
			// GenBase.
			if (TypeParameters != null && !TypeParameters.Validate (opt, type_params, context))
				return false;

			if (!base.OnValidate (opt, type_params, context) || iface_validation_failed || MethodValidationFailed) {
				if (iface_validation_failed)
					Report.LogCodedWarning (0, Report.WarningInvalidDueToInterfaces, this, FullName);
				else if (MethodValidationFailed)
					Report.LogCodedWarning (0, Report.WarningInvalidDueToMethods, this, FullName);
				foreach (GenBase nest in NestedTypes)
					nest.Invalidate ();
				IsValid = false;
				return false;
			}

			return true;
		}

		public override void ResetValidation ()
		{
			validated = false;
			base.ResetValidation ();
		}

		public override string ToNative (CodeGenerationOptions opt, string varname, Dictionary<string, string> mappings = null)
		{
			if (opt.CodeGenerationTarget == CodeGenerationTarget.JavaInterop1) {
				return $"({varname}?.PeerReference ?? default)";
			}
			return string.Format ("JNIEnv.ToLocalJniHandle ({0})", varname);
			/*
			if (String.IsNullOrEmpty (Marshaler))
				return String.Format ("JNIEnv.ToLocalJniHandle ({0})", varname);
			else
				return GetObjectHandleProperty (varname);
			*/
		}

		#region IRequireGenericMarshal implementation.
		// SymbolTable.Lookup() for IList/IDictioanry/etc. results in this InterfaceGen,
		// so we also have to override this property here.
		public string GetGenericJavaObjectTypeOverride ()
		{
			int idx = FullName.IndexOf ('<');
			return TypeNameUtilities.GetGenericJavaObjectTypeOverride (
				idx < 0 ? FullName : FullName.Substring (0, idx),
				idx < 0 ? null : FullName.Substring (idx + 1).TrimEnd ('>'));
		}

		public string ToInteroperableJavaObject (string var_name)
		{
			return GetGenericJavaObjectTypeOverride () != null ? TypeNameUtilities.GetNativeName (var_name) : var_name;
		}
		#endregion
	}
}
