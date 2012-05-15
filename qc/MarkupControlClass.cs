﻿using System;
using System.Linq;
using System.Text;

namespace qc
{
    /// <summary>
    /// The parsed representation of a Quick control class declaration.
    /// </summary>
    public class MarkupControlClass : MarkupNode
    {
        private const string DEFAULT_CLASS_NAME = "Control";

        public string Comments { get; set; }
        public string Generic { get; set; }
        public string Name { get; set; }
        public string Script { get; set; }
        public string Style { get; set; }
        public string Tag { get; set; }
        public MarkupNode Content { get; set; }
        public MarkupControlInstance Prototype { get; set; }

        public MarkupControlClass()
        {
        }

        public MarkupControlClass(MarkupControlInstance control)
            : this(control, null, null)
        {
        }

        /// <summary>
        /// Elevate the specific control instance to a control class declaration.
        /// </summary>
        /// <remarks>
        /// The simplest way to parse the class definition is to parse it just
        /// like an instance of a control, then pull out specific properties
        /// for the name, prototype, script, and style.
        /// 
        /// The script and style properties are those which were separated out
        /// by the preprocessor before the control instance was parsed. These
        /// must now be folded into the control class declaration.
        /// </remarks>
        public MarkupControlClass(MarkupControlInstance c, string script, string style)
        {
            // Copy over the script and style separated out by the preprocessor.
            Script = script;
            Style = style;

            // Read the explicitly defined class properties.
            ExtractExplicitClassProperties(c);
            VerifyProperties();
        }

        public string Css()
        {
            return CssProcessor.CssForClass(this);
        }

        /// <summary>
        /// Return the JavaScript for this control class.
        /// </summary>
        public override string JavaScript(int indentLevel)
        {
            string comment = String.IsNullOrEmpty(Comments)
                ? ""
                : Tabs(indentLevel) + "/*" + Comments + "*/\n";
            string baseClassProperties = EmitBaseClassProperties(indentLevel + 1);

            return Template.Format(
                "{Comment}" +
                "var {ClassName} = {BaseClassName}.sub({\n" +
                "{CssClassName}" +
                "{Generic}" +
                "{Tag}" +
                "{BaseClassProperties}" +
                "});\n" + 
                "{Script}\n", // Extra break at end helps delineate between successive controls in combined output.
                new
                {
                    Comment = comment,
                    ClassName = Name,
                    BaseClassName = BaseClassName,
                    CssClassName = EmitClassProperty( "className", Name, indentLevel + 1,
                        String.IsNullOrEmpty(Generic) && String.IsNullOrEmpty(Tag) && String.IsNullOrEmpty(baseClassProperties)
                    ),
                    Generic = EmitClassProperty("genericDefault", Generic, indentLevel + 1,
                        String.IsNullOrEmpty(Tag) && String.IsNullOrEmpty(baseClassProperties)
                    ),
                    Tag = EmitClassProperty("tag", Tag, indentLevel + 1,
                        String.IsNullOrEmpty(baseClassProperties)
                    ),
                    BaseClassProperties = baseClassProperties,
                    Script = EmitScript()
                });
        }

        public string BaseClassName
        {
            get
            {
                return (Prototype != null) ? Prototype.ClassName : DEFAULT_CLASS_NAME;
            }
        }

        private string EmitClassProperty(string propertyName, string propertyValue, int indentLevel, bool isLast)
        {
            if (propertyValue == null)
            {
                return String.Empty;
            }

            return Template.Format(
                "{Tabs}{PropertyName}: \"{PropertyValue}\"{Comma}\n",
                new
                {
                    Tabs = Tabs(indentLevel),
                    PropertyName = propertyName,
                    PropertyValue = propertyValue,
                    Comma = isLast ? "" : ","
                });
        }

        /// <summary>
        /// Extract explicitly declared class properties.
        /// </summary>
        /// <remarks>
        /// The parser will read a .qui file as if the whole thing were an instance
        /// of a class called Control with properties like "name" and "prototype".
        /// We translate those key properties into the relevant members of the
        /// ControlClass type.
        /// 
        /// Note: At this point, the script and style tags are highly unlikely
        /// to appear, having been previously separated out by the preprocessor.
        /// However, for completeness, we check here for those properties. The
        /// only way they could get this far were if they were set as attributes
        /// of the top-level Control node: <Control script="alert('Hi');"/> which
        /// would be pretty odd.
        /// </remarks>
        private void ExtractExplicitClassProperties(MarkupControlInstance control)
        {
            foreach (string propertyName in control.Properties.Keys)
            {
                MarkupNode node = control.Properties[propertyName];
                string text = (node is MarkupHtmlElement) ? ((MarkupHtmlElement) node).Html : null;

                switch (propertyName)
                {
                    case "className":
                        VerifyPropertyIsNull(propertyName, this.Name);
                        this.Name = text;
                        break;

                    case "content":
                        VerifyPropertyIsNull(propertyName, this.Content);
                        this.Content = node;
                        break;

                    case "genericDefault":
                        VerifyPropertyIsNull(propertyName, this.Generic);
                        this.Generic = text;
                        break;

                    case "prototype":
                        VerifyPropertyIsNull(propertyName, this.Prototype);
                        this.Prototype = VerifyPrototype(node);
                        break;

                    case "script":
                        VerifyPropertyIsNull(propertyName, this.Script);
                        this.Script = text;
                        break;

                    case "style":
                        VerifyPropertyIsNull(propertyName, this.Style);
                        this.Style = text;
                        break;

                    case "tag":
                        VerifyPropertyIsNull(propertyName, this.Tag);
                        this.Tag = text;
                        break;

                    default:
                        throw new CompilerException(
                            String.Format("Unknown class definition element: \"{0}\".", propertyName));
                }
            }
        }

        /// <summary>
        /// Return the JavaScript for the control properties which will be set
        /// via calls to the base class.
        /// </summary>
        /// <remarks>
        /// This covers virtually all the properties (except className) which the
        /// developer will set on a control.
        /// </remarks>
        private string EmitBaseClassProperties(int indentLevel)
        {
            if (Content == null && Prototype == null)
            {
                return String.Empty;
            }

            string properties;
            if (Content != null)
            {
                properties = EmitProperty("content", Content.JavaScript(indentLevel + 1), true, indentLevel + 1);
            }
            else
            {
                properties = EmitProperties(Prototype.Properties, indentLevel + 1);
            }

            return Template.Format(
                "{Tabs}inherited: {\n" +
                "{Properties}" +
                "{Tabs}}\n",
                new
                {
                    Tabs = Tabs(indentLevel),
                    Properties = properties
                });
        }

        /// <summary>
        /// Return the control's script.
        /// </summary>
        /// <remarks>
        /// We also add back a final newline (which was stripped during reading)
        /// to make sure the generated code looks pretty.
        /// </remarks>
        private string EmitScript()
        {
            return String.IsNullOrEmpty(Script)
                ? String.Empty
                : Script.Trim() + "\n";
        }

        /// <summary>
        /// Throw an exception if the given property value is not null.
        /// </summary>
        private void VerifyPropertyIsNull(string propertyName, object propertyValue)
        {
            if (propertyValue != null)
            {
                throw new CompilerException(
                    String.Format("The \"{0}\" property can't be set more than once.", propertyName));
            }
        }

        // Perform final tests for a well-formed control.
        private void VerifyProperties()
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new CompilerException(
                    String.Format("No class \"name\" attribute was defined on the root <Control> element."));
            }

            if (Content != null && Prototype != null)
            {
                throw new CompilerException(
                    String.Format("A control can't define both content and a prototype."));
            }
        }

        /// <summary>
        /// Ensure the prototype is well-formed.
        /// </summary>
        private MarkupControlInstance VerifyPrototype(MarkupNode node)
        {
            if (node is MarkupControlInstance)
            {
                return (MarkupControlInstance) node;
            }
            if (node is MarkupElementCollection)
            {
                // There should be only one non-whitespace node in the collection.
                try
                {
                    MarkupElement element = ((MarkupElementCollection) node)
                        .Single(item => !item.IsWhiteSpace());
                    if (element is MarkupControlInstance)
                    {
                        return (MarkupControlInstance) element;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }

            throw new CompilerException(
                "A control's <prototype> must be a single instance of a QuickUI control class.");
        }
    }
}
