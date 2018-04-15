using System.Collections.Generic;
using System.Linq;
using SQLite;
using System;

namespace XForms.XForms
{
    public class DLL
    {
#if __ANDROID__
        private static string _dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "GoMobile.db");
#else
        private static string _dbPath = "GoMobile.db";
#endif
        public static void CreateDatabase()
        {
            var db = new SQLiteConnection(_dbPath);
            //drop each table based on <droptables> in dotget            
            if (!TableExists(db, "Forms")) db.CreateTable<Forms>();
            if (!TableExists(db, "Instance")) db.CreateTable<Instance>();
            if (!TableExists(db, "Bindings")) db.CreateTable<Bindings>();
            if (!TableExists(db, "Controls")) db.CreateTable<Controls>();
            if (!TableExists(db, "ControlOptions")) db.CreateTable<ControlOptions>();
            if (!TableExists(db, "Groups")) db.CreateTable<Groups>();
        }
        public static void DisposeTables()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                if (TableExists(db, "Forms")) db.DropTable<Forms>();
                if (TableExists(db, "Instance")) db.DropTable<Instance>();
                if (TableExists(db, "Bindings")) db.DropTable<Bindings>();
                if (TableExists(db, "Controls")) db.DropTable<Controls>();
                if (TableExists(db, "ControlOptions")) db.DropTable<ControlOptions>();
                if (TableExists(db, "Groups")) db.DropTable<Groups>();
            }
        }
        private static bool TableExists(SQLiteConnection sqlitedb, string tableName)
        {
            int tableCount;
            tableCount = sqlitedb.ExecuteScalar<int>("select count(*) table_count from sqlite_master where type='table' and name='" + tableName + "'");
            return tableCount > 0;
        }
        public static int InsertRow<T>(T row)
        {
            int id;
            using (var db = new SQLiteConnection(_dbPath))
            {
                id = db.Insert(row);
            }
            return id;
        }
        public static void InsertRows<T>(IEnumerable<T> row)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.InsertAll(row);
            }
        }

        public static Forms GetForm(int formid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Forms>().Where(x => x.id == formid).FirstOrDefault();
            }
        }
        public static Instance GetInstance()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Instance>().FirstOrDefault();
            }
        }
        public static Controls GetControl(int controlid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Controls>().Where(x => x.id == controlid).FirstOrDefault();
            }
        }
        public static List<Controls> GetControls(int formid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Controls>().Where(x => x.formid == formid).ToList();
            }
        }       

        public static List<int> GetControlsInGroup(int formid, int? ngroupid)
        {
            if (!ngroupid.HasValue)
                return GetControlIDs(formid);

            List<Controls> controls = new List<Controls>();
            int groupid = ngroupid.Value;

            foreach(int i in GetControlIDs(formid))
            {
                Controls control = GetControl(i);
                int? group = control.groupid;
                bool found = (group == groupid);
                while (!found && group.HasValue)
                {
                    group = GetParentOfGroup(group.Value);
                    if (group.HasValue && group.Value == groupid) found = true;
                }
                if (found)
                    controls.Add(control);
            }
            return controls.Select(x => x.id).ToList();
            
        }
        
        public static List<int> GetControlIDs(int formid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Controls>().Where(x => x.formid == formid).ToList().Select(x => x.id).ToList();
            }
        }
        public static Groups GetGroup(int groupid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Groups>().Where(x => x.id == groupid).FirstOrDefault();
            }
        }
        public static string GetGroupAppearance(int groupid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Groups>().Where(x => x.id == groupid).FirstOrDefault().appearance;
            }
        }
        public static int? GetParentOfGroup(int groupid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Groups>().Where(x => x.id == groupid).FirstOrDefault().parentid;
            }
        }
        public static List<Bindings> GetBindingsWithCalculations()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Bindings>().Where(x => x.calculate != null).ToList();
            }
        }
        public static Bindings GetBindingByReference(string reference)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Bindings>().Where(x => x.nodeset == reference).FirstOrDefault();
            }
        }
        public static Bindings GetBindingByName(string name)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Bindings>().Where(x => x.name == name).FirstOrDefault();
            }
        }

        public static Bindings GetBindingById(int id)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<Bindings>().Where(x => x.id == id).FirstOrDefault();
            }
        }

        public static List<ControlOptions> GetControlOptions(int controlid)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                return db.Table<ControlOptions>().Where(x => x.controlid == controlid).ToList();
            }
        }

        static object lockObj = new object();

        public static void UpdateInstance(Instance instance)
        {
            lock (lockObj)
            {
                using (var db = new SQLiteConnection(_dbPath))
                {
                    db.Update(instance);
                }
            }
        }
    }
}
