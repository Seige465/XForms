using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using SQLite;
using System.Threading.Tasks;
using XForms.XForms;

namespace XForms.Droid
{
    public static class FormDatabase
    {
        static string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "data.db");

        #region Initial Creation

        public static bool CreateDatabase()
        {
            try
            {
                var db = new SQLiteConnection(path);

                db.CreateTable<Forms>();
                db.CreateTable<Instance>();
                db.CreateTable<Bindings>();
                db.CreateTable<Controls>();
                db.CreateTable<ControlOptions>();
                db.CreateTable<Groups>();




                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"no. {ex.ToString()}");
                return false;
            }

        }
        #endregion

        #region Insert and Select
        public static int InsertData(object obj)
        {
            int id = 0;
            using (var db = new SQLiteConnection(path))
            {
                try
                {
                    db.RunInTransaction(() =>
                    {
                        id = db.Insert(obj);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    id = -1;
                }
            }
            return id;
        }

        /// <summary>
        /// Get the controls from a form definition
        /// </summary>
        /// <param name="id">Gets the controls from a form with provided id.</param>
        /// <returns>A list of controls.</returns>
        public static List<Controls> SelectControls(int id)
        {
            using (var db = new SQLiteConnection(path))
            {
                try
                {
                    return db.Table<Controls>().Where(x => x.formid == id).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }

        #endregion


        #region Selects

        #endregion
    }
}