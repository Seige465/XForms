using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace XForms.XForms
{
    public static class XForm
    {
        public static Instance instance;

        //public static object Evaluate(string instance, string evaluation)
        //{
        //    XDocument doc;
        //    doc = LoadInstance(instance);
        //    if (evaluation.Contains("selected("))
        //        evaluation = evaluation.Replace("selected(", "contains(");
        //    var eval = doc.XPathEvaluate(evaluation);
        //    return eval;
        //}
        public static object Evaluate(string evaluation)
        {
            object Eval = null;
            try
            {
                if (evaluation.Contains("selected("))
                    evaluation = evaluation.Replace("selected(", "contains(");
                Eval = instance.instance.XPathEvaluate(evaluation);
            }            
            catch (XmlException xex)
            {
                Console.WriteLine(xex.ToString());
            }
            catch { }
            return Eval;
        }
        public static object Evaluate(string evaluation, string nodeset)
        {
            object Eval = null;
            try
            {              
                XPathNavigator navigator = instance.instance.CreateNavigator();
                XPathExpression xPathExpression = XPathExpression.Compile(evaluation);
                XPathNodeIterator node = navigator.Select(nodeset);
                node.MoveNext();
                Eval = navigator.Evaluate(xPathExpression, node);
            }
            catch (XmlException xex)
            {
                Console.WriteLine(xex.ToString());
            }
            catch { }
            return Eval;
        }
        public static void RunCalculates()
        {
            bool valuesChanged = false;
            List<Bindings> bindings = DLL.GetBindingsWithCalculations();
            foreach (Bindings binding in bindings)
            {
                object currentvalue = GetValue( binding.nodeset);
                object newvalue = Evaluate(binding.calculate);
                if (newvalue == null)
                    continue;
                GetElement(binding.nodeset).Value = newvalue.ToString();
                if (newvalue.ToString() != currentvalue.ToString())
                {
                    valuesChanged = true;
                    break;
                }
            }
            if (valuesChanged)
                RunCalculates();
        }

        





        public static XDocument LoadInstance(string instance)
        {
            XDocument doc;
            using (StringReader stReader = new StringReader(instance))
            {
                using (XmlReader xReader = XmlReader.Create(stReader))
                {
                    doc = XDocument.Load(xReader);                    
                }
            }
            return doc;
        }
        public static XElement GetElement(string path)
        {
            return instance.instance.XPathSelectElement(path);
        }
        public static string GetValue(string path)
        {
            string val = instance.instance.XPathSelectElement(path)?.Value;
            return string.IsNullOrWhiteSpace(val) ? string.Empty : val;
        }
        public static void SetValue(string path,string value)
        {
            instance.instance.XPathSelectElement(path).Value = value;
        }

        public static Bindings GetBindingForControl(Controls control)
        {
            object binding = DLL.GetBindingByReference(control.reference);
            if (binding == null)
                binding = DLL.GetBindingByName(control.bind);
            return (Bindings)binding;
        }
        public static Bindings GetBindingForGroup(Groups group)
        {
            object binding = DLL.GetBindingByReference(group.reference);
            if (binding == null)
                binding = DLL.GetBindingByName(group.bind);
            return (Bindings)binding;
        }

        public static void LoadForm(int? groupid, IXFormBuild parent)
        {
            //Load the form.     
            Console.WriteLine($"Loading form: {instance.formid}, Group: {groupid}");

            //Set the title
            if (groupid.HasValue)
                parent.SetTitle(DLL.GetGroup(groupid.Value).label);             
            else
                parent.SetTitle(DLL.GetForm(instance.formid).title);

            Groups section = null;
            int? sectiongroup = null;
            int? subpage = null;

            //int currentgroupid = -1;            
            List<int> controlIDs = DLL.GetControlsInGroup(instance.formid, groupid);
            foreach (int controlID in controlIDs)
            {
                Controls control = DLL.GetControl(controlID);
                Console.WriteLine($"Placing control: {control.label}");

                int? group = control.groupid;
                SortedDictionary<int, bool> path = new SortedDictionary<int, bool>();
                while ((group.HasValue ? group.Value : -1) != (groupid.HasValue ? groupid.Value : -1))
                {
                    path.Add(group.Value, DLL.GetGroupAppearance(group.Value) == "field-list" ? true : false);
                    group = DLL.GetParentOfGroup(group.Value);
                }                

                //DEBUG CODE
                List<string> strpath = new List<string>();
                foreach (var kvp in path)
                {
                    strpath.Add((kvp.Value ? "page" : "section") + kvp.Key);
                }
                string consolepath = $"PATH: {string.Join("/", strpath)}";
                Console.WriteLine(consolepath);


                //Get first object.
                int displaygroup = -1;
                if(path.Count > 0)
                    displaygroup = path.First().Value ? -1 : path.First().Key;
                Console.WriteLine($"Display Group: {displaygroup}");

                //Start a new section, if required.
                if (displaygroup != (sectiongroup.HasValue ? sectiongroup : -1))
                {
                    if (displaygroup != -1)
                    {
                        parent.AddGroup(displaygroup);
                        sectiongroup = displaygroup;
                    }
                    else
                        sectiongroup = null;
                }
                if (path.Count == 0)
                    group = null;
                else 
                    group = path.Last().Key;

                // If control is directly under current section then add row for it
                if ((group.HasValue ? group.Value : -1) == displaygroup)
                {
                    parent.AddControl(control, (sectiongroup.HasValue ? sectiongroup.Value : -1));
                }
                // Otherwise control must be under subpage
                else
                {
                    // Otherwise control is under a subpage; find the topmost (ie first) subpage group
                    foreach (var node in path)
                    {
                        if (node.Value)
                        {
                            group = node.Key;
                            break;
                        }
                    }
                    //while (group.HasValue && DLL.GetGroupAppearance(group.Value) == "field-list")
                    //    group = DLL.GetParentOfGroup(group.Value);

                    // If different (or new) subpage, then add summary row for the page
                    if ((group.HasValue ? group.Value : -1) != (subpage.HasValue ? subpage.Value : -1))
                    {
                        parent.AddPageStub((group.HasValue ? group.Value : -1), (sectiongroup.HasValue ? sectiongroup.Value : -1));
                        subpage = group;
                    }
                    else
                    {
                        //Need to do nothing because control is buried under a page.
                    }
                }
            }
        }

    }
    public class XFormChangedEventArgs : EventArgs
    {
#if __ANDROID__
        //if I am to show/hide what I want, i need to pass a view through
        //public Android.Views.View _view { get; set; }
#endif
    }
}
