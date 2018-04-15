using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XForms.XForms
{
    public class Parser
    {
        public static void Parse(string xform, string formname)
        {
            int _FormID;
            //Create the new form 

            //Replace the replaced default namespace.
            xform = xform.Replace("xmlns=\"http://www.w3.org/2002/xforms\"", "");
            xform = xform.Replace("jr:", "");

            //Load the XForm in to a Document
            XDocument xDoc;
            XmlNamespaceManager namespaceManager;
            using (StringReader stReader = new StringReader(xform))
            {
                using (XmlReader xReader = XmlReader.Create(stReader))
                {
                    xDoc = XDocument.Load(xReader);
                    namespaceManager = new XmlNamespaceManager(xReader.NameTable);
                }
            }
            //Add the namespaces that are used by Kobo (minus the default);
            namespaceManager.AddNamespace("xf", "http://www.w3.org/2002/xforms");
            namespaceManager.AddNamespace("ev", "http://www.w3.org/2001/xml-events");
            namespaceManager.AddNamespace("h", "http://www.w3.org/1999/xhtml");
            namespaceManager.AddNamespace("jr", "http://openrosa.org/javarosa");
            namespaceManager.AddNamespace("orx", "http://openrosa.org/xforms");
            namespaceManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            DLL.CreateDatabase();
            //Load the instance node.
            //var instance = nav.SelectSingleNode("//h:html/h:head/model/instance/data", namespaceManager);            
            XElement xInstance = xDoc.XPathSelectElement("//h:html/h:head/model/instance/data", namespaceManager);
            Forms frm = new Forms()
            {
                instancexml = xInstance.ToString(),
                itemtype = "",
                registerid = 99,
                title = "TestForm",
                uuid = "",
                version = "" //attribute on <data>
            };
            DLL.InsertRow(frm);
            _FormID = frm.id;          
            //Load the bindings.
            List<Bindings> bindings = new List<Bindings>();
            foreach (XElement binding in xDoc.XPathSelectElements("//h:html/h:head/model/bind", namespaceManager).ToList())
            {
                var v = binding.Attribute("type")?.Value;
                bindings.Add(new Bindings()
                {
                    formid = _FormID,
                    name = binding.Attribute("id")?.Value,
                    type = GetBindingType(binding.Attribute("type")?.Value), //binding.Attribute("type")?.Value == null ? (int?)null : binding.Attribute("type")?.Value == "select" || binding.Attribute("type")?.Value == "select1" ? (int)BindType.xString : binding.Attribute("type").Value.Equals("integer", StringComparison.OrdinalIgnoreCase) ? (int)BindType.xInt : (int)Enum.Parse(typeof(BindType), $"x{binding.Attribute("type")?.Value}", true),
                    calculate = binding.Attribute("calculate")?.Value,
                    constraint = binding.Attribute("constraint")?.Value,
                    constraintMsg = binding.Attribute("constraintMsg")?.Value,
                    required = binding.Attribute("required")?.Value,
                    requiredMsg = binding.Attribute("requiredMsg")?.Value,
                    isreadonly = binding.Attribute("readonly")?.Value,
                    nodeset = binding.Attribute("nodeset")?.Value,
                    relevant = binding.Attribute("relevant")?.Value,
                });
            }
            DLL.InsertRows(bindings);

            //Parse the controls
            XElement body = xDoc.XPathSelectElement("//h:html/h:body", namespaceManager);
            ParseBody(body, null, _FormID);
        }
        private static void ParseBody(XElement body, int? parentid, int formID)
        {
            var elements = body.Elements();
            foreach (XElement ctrl in body.Elements())
            {
                if (ctrl.Name.LocalName == "label" || ctrl.Name.LocalName == "hint") //Ignore the labels for the groups
                    continue;
                //Control is a group.
                if (ctrl.Name.LocalName == "group")
                {
                    string _appearance = null;
                    if (ctrl.Attribute("appearance")?.Value != null)
                        _appearance = ctrl.Attribute("appearance").Value;
                    else if (ctrl.Attribute("id")?.Value != null)
                    {
                        if (ctrl.Attribute("id").Value.StartsWith("PAGE_"))
                            _appearance = "field-list";
                    }
                    else if (ctrl.Attribute("ref")?.Value != null)
                    {
                        string reference = ctrl.Attribute("ref").Value.Split('/')?.Last();
                        if (!string.IsNullOrWhiteSpace(reference) && reference.StartsWith("PAGE_"))
                        {
                            _appearance = "field-list";
                        }
                    }
                    //Create the group class item.
                    Groups group = new Groups()
                    {
                        appearance = _appearance,
                        formid = formID,
                        label = ctrl.Descendants("label")?.First()?.Value,
                        parentid = parentid.HasValue ? parentid : null,
                        reference = ctrl.Attribute("ref")?.Value,
                        bind = ctrl.Attribute("bind")?.Value //Not in kobo
                    };
                    DLL.InsertRow(group);
                    ParseBody(ctrl, group.id, formID);
                }
                else
                {
                    Controls control = new Controls()
                    {
                        formid = formID,
                        appearance = ctrl.Attribute("appearance")?.Value,
                        bind = ctrl.Attribute("bind")?.Value, //not in kobo
                        groupid = parentid = parentid.HasValue ? parentid : null,
                        label = ctrl.Element("label")?.Value,
                        hint = ctrl.Element("hint")?.Value,
                        mediatype = ctrl.Attribute("mediatype")?.Value,
                        reference = ctrl.Attribute("ref")?.Value,
                        selection = ctrl.Attribute("selection")?.Value,
                        type = (int)Enum.Parse(typeof(ControlType), ctrl.Name.LocalName, true),
                        xpathhint = GetXPathValue(ctrl.Element("hint")),
                        xpathlabel = GetXPathValue(ctrl.Element("label"))
                    };
                    DLL.InsertRow(control);
                    int controlid = control.id;
                    if (ctrl.Name.LocalName.StartsWith("select"))
                    {
                        List<ControlOptions> options = new List<ControlOptions>();
                        foreach (XElement items in ctrl.Descendants("item"))
                        {
                            options.Add(new ControlOptions()
                            {
                                controlid = controlid,
                                formid = formID,
                                label = items.Element("label").Value,
                                value = items.Element("value").Value
                            });
                        }
                        DLL.InsertRows(options);
                    }
                }
            }            
        }

        private static int? GetBindingType(string type)
        {
            //binding.Attribute("type")?.Value == null ? (int?)null : binding.Attribute("type")?.Value == "select" || binding.Attribute("type")?.Value == "select1" ? (int)BindType.xString : binding.Attribute("type").Value.Equals("integer", StringComparison.OrdinalIgnoreCase) ? (int)BindType.xInt : (int)Enum.Parse(typeof(BindType), $"x{binding.Attribute("type")?.Value}", true),
            if (string.IsNullOrWhiteSpace(type))
                return null;
            switch (type)
            {
                case "select":
                case "select1":
                    return (int)BindType.xString;

                case "integer":
                    return (int)BindType.xInt;

                case "boolean":
                    return (int)BindType.xString;

                //TODO: Write ability to handle these
                case "geotrace":
                case "geoshape":
                    return (int)BindType.xGeoPoint;
                case "barcode":
                    return (int)BindType.xString;


                default:
                    return (int)Enum.Parse(typeof(BindType), $"x{type}", true);
            }
        }

        private static string GetXPathValue(XElement element)
        {
            if (element == null)
                return null;
            string xpathlabel = null;
            if (element.HasElements)
            {                
                xpathlabel = "concat(''";
                string val = element.ToString().Replace($"<{element.Name.LocalName}>", "").Replace($"</{element.Name.LocalName}>", "");
                List<XElement> outputs = element.Descendants().ToList();
                List<int> openOutputs = val.AllIndexesOf("<output").ToList();
                List<int> closeOutputs = val.AllIndexesOf("/>").ToList();
                int position = 0;
                for (int i = 0; i < openOutputs.Count; i++)
                {
                    xpathlabel += $",'{val.Substring(position, openOutputs[i] - position)}'";
                    xpathlabel += $", {outputs[i].Attribute("value").Value}";
                    position = closeOutputs[i] + 2;
                }
                if (position < val.Length)
                    xpathlabel += $",'{val.Substring(position, val.Length - position)}'";
                xpathlabel += ")";
            }
            return xpathlabel;
        }
    }
    public enum BindType
    {
        xString,
        xInt,
        xDecimal,
        xDate,
        xTime,
        xDateTime,
        xGeoPoint,
        xBinary

    }
    public enum ControlType
    {
        input,
        select,
        select1,
        upload,
        output
    }
}
