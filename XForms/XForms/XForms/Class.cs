using SQLite;
using System.Xml.Linq;

namespace XForms.XForms
{
    public class Forms
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string uuid { get; set; }
        public string title { get; set; }
        public string version { get; set; }
        public string itemtype { get; set; }
        public int registerid { get; set; }
        public string instancexml { get; set; }        
    }
    public class Instance
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int formid { get; set; }
        public string xml { get => instance.ToString(); set => instance = XForm.LoadInstance(value); }        
        [Ignore]
        public XDocument instance { get; set; }
    }

    public class Bindings
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Indexed]
        public int formid { get; set; }
        public string name { get; set; }
        public string nodeset { get; set; }
        public int? type { get; set; }
        [Column("readonly")]
        public string isreadonly { get; set; }
        public string relevant { get; set; }
        public string required { get; set; }
        public string requiredMsg { get; set; }
        public string constraint { get; set; }
        public string constraintMsg { get; set; }
        public string calculate { get; set; }
    }
    public class Controls
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Indexed]
        public int formid { get; set; }
        [Column("ref")]
        public string reference { get; set; }
        public string bind { get; set; }
        public int type { get; set; } //Name of the node
        public string label {get;set;}
        public string xpathlabel { get; set; }
        public string hint { get; set; }
        public string xpathhint { get; set; }
        public string appearance { get; set; }
        public string selection { get; set; }
        public string mediatype { get; set; }        
        public int? groupid { get; set; }
    }
    public class ControlOptions
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Indexed]
        public int controlid { get; set; }
        [Indexed]
        public int formid { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        [Ignore]
        public bool selected { get; set; } = false;
    }
    public class Groups
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int formid { get; set; }
        public int? parentid { get; set; }
        [Column("ref")]
        public string reference { get; set; }
        public string bind { get; set; }
        public string label { get; set; }
        public string appearance { get; set; }
    }
    
}
