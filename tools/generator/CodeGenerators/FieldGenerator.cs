using System;
using System.IO;
using System.Linq;

namespace MonoDroid.Generation
{
	abstract class FieldGenerator
	{
		internal abstract void WriteIdField (Field field, TextWriter writer, string indent, CodeGenerationOptions opt);
		internal abstract void WriteGetBody (Field field, TextWriter writer, string indent, CodeGenerationOptions opt, GenBase type);
		internal abstract void WriteSetBody (Field field, TextWriter writer, string indent, CodeGenerationOptions opt, GenBase type);

		internal virtual void Write (Field field, TextWriter writer, string indent, CodeGenerationOptions opt, GenBase type)
		{
			if (field.IsEnumified)
				writer.WriteLine ("[global::Android.Runtime.GeneratedEnum]");
			if (field.NeedsProperty) {
				string fieldType = field.Symbol.IsArray ? "IList<" + field.Symbol.ElementType + ">" : opt.GetOutputName (field.Symbol.FullName);
				WriteIdField (field, writer, indent, opt);
				writer.WriteLine ();
				writer.WriteLine ("{0}// Metadata.xml XPath field reference: path=\"{1}/field[@name='{2}']\"", indent, type.MetadataXPathReference, field.JavaName);
				writer.WriteLine ("{0}[Register (\"{1}\"{2})]", indent, field.JavaName, field.AdditionalAttributeString ());
				writer.WriteLine ("{0}{1} {2}{3} {4} {{", indent, field.Visibility, field.IsStatic ? "static " : String.Empty, fieldType, field.Name);
				writer.WriteLine ("{0}\tget {{", indent);
				WriteGetBody (field, writer, indent + "\t\t", opt, type);
				writer.WriteLine ("{0}\t}}", indent);

				if (!field.IsConst) {
					writer.WriteLine ("{0}\tset {{", indent);
					WriteSetBody (field, writer, indent + "\t\t", opt, type);
					writer.WriteLine ("{0}\t}}", indent);
				}
				writer.WriteLine ("{0}}}", indent);
			} else {
				writer.WriteLine ("{0}// Metadata.xml XPath field reference: path=\"{1}/field[@name='{2}']\"", indent, type.MetadataXPathReference, field.JavaName);
				writer.WriteLine ("{0}[Register (\"{1}\"{2})]", indent, field.JavaName, field.AdditionalAttributeString ());
				if (field.IsDeprecated)
					writer.WriteLine ("{0}[Obsolete (\"{1}\")]", indent, field.DeprecatedComment);
				if (field.Annotation != null)
					writer.WriteLine ("{0}{1}", indent, field.Annotation);

				// the Value complication is due to constant enum from negative integer value (C# compiler requires explicit parenthesis).
				writer.WriteLine ("{0}{1} const {2} {3} = ({2}) {4};", indent, field.Visibility, opt.GetOutputName (field.Symbol.FullName), field.Name, field.Value.Contains ('-') && field.Symbol.FullName.Contains ('.') ? '(' + field.Value + ')' : field.Value);
			}
		}
	}
}
