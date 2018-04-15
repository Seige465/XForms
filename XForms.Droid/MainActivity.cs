using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System;
using XForms.XForms;
using Android.Content;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using Android.Views;

namespace XForms.Droid
{
    [Activity(Label = "Form List",MainLauncher = true)]
    public class MainActivity : BaseActivity
    {
        ListView lv;
        List<string> forms;

        ProgressDialog progress;

        FormListAdapter fla;
        BackgroundWorker bw;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            progress = new ProgressDialog(this);
            progress.SetCanceledOnTouchOutside(false);
            progress.SetCancelable(false);
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetProgressNumberFormat(null);
            progress.SetProgressPercentFormat(null);
            progress.Indeterminate = true;

            progress.SetMessage("Loading forms...");


            downloadForms();


        }

        protected override void OnResume()
        {
            base.OnResume();
            //TEST PURPOSES FOR NOW
            XForm.instance = null;
            DLL.DisposeTables();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }


        private void downloadForms()
        {
            bw = new BackgroundWorker();

            progress.Show();
            bw.DoWork += delegate
            {
                try
                {
                    WebServerHelper.WebServiceResponse resp = JsonConvert.DeserializeObject<WebServerHelper.WebServiceResponse>(WebServerHelper.WebCall("https://macarthur.goget.co.nz:9725/apps/?formlist=1"));
                    forms = resp.data.Split('|').ToList();
                    forms = forms.Select(s => Path.GetFileName(s)).ToList();

                    RunOnUiThread(() =>
                    {
                        lv = FindViewById<ListView>(Resource.Id.mainMenu);
                        fla = new FormListAdapter(forms, this);
                        lv.Adapter = fla;
                    
                        lv.ItemClick += Lv_ItemClick;

                    });

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            };

            bw.RunWorkerCompleted += delegate
            {
                progress.Dismiss();
            };

            bw.RunWorkerAsync();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.menu_refresh:
                    if(lv != null)
                        lv.ItemClick -= Lv_ItemClick;
                    downloadForms();
                    //fla.NotifyDataSetChanged();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);


            }
        }


        private void Lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent i = new Intent(this, typeof(ScrollFormActivity));

            string form = forms[e.Position];
            string[] splitForm = form.Split('\\');
            i.PutExtra("form_name", splitForm[splitForm.Length - 1]);
            StartActivity(i);
        }
    }
}

