using Android.App;
using Android.OS;
using Android.Widget;
using System.ComponentModel;
using XForms.XForms;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Xml.Linq;
using Android.Views;

namespace XForms.Droid
{
    [Activity(Label = "Form", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class FormActivity : BaseActivity
    {
        ListView listView;
        List<Controls> FormControls;
        ProgressDialog progress;

        public FormActivity()
        {
        }

        public XDocument instance;
        public event EventHandler FormChanged;
        private void OnFormChanged(View v)
        {
            FormChanged?.Invoke(null, new XFormChangedEventArgs() {
            });
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            progress = new ProgressDialog(this);
            progress.SetMessage("Loading...");
            progress.SetCanceledOnTouchOutside(false);
            progress.SetCancelable(false);
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetProgressNumberFormat(null);
            progress.SetProgressPercentFormat(null);
            progress.Indeterminate = true;

            SetContentView(Resource.Layout.Form);
            listView = FindViewById<ListView>(Resource.Id.controlList);



            

            //get the form from the WS
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += delegate
            {
                DLL.DisposeTables();

                RunOnUiThread(() =>
                {
                    progress.Show();
                });

                WebServerHelper.WebServiceResponse response =  
                    JsonConvert.DeserializeObject<WebServerHelper.WebServiceResponse>(WebServerHelper.WebCall("https://macarthur.goget.co.nz:9725/apps/?getform=1"));

                string xform = response.data;

                //puts it into the sqlite db
                try
                {
                    Parser.Parse(xform, "Test Form");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Forms form = DLL.GetForm(1);
                //this is where we put our filled out values back into
                instance = XForm.LoadInstance(form.instancexml);

                //the controls we put onto the UI
                FormControls = DLL.GetControls(1);

                foreach (Controls c in FormControls)
                {
                    Console.WriteLine(c.type);
                }

                RunOnUiThread(() => {
                    Title = "Test Form";
                    listView.Adapter = new FormAdapter(FormControls, this);
                });
                

            };


            //setup ui now we are done
            bw.RunWorkerCompleted += delegate {
                RunOnUiThread(() => {

                    //trigger any show/hide stuff :D
                    FireFormChangeEvent(null);
                    progress.Dismiss();
                });
            };


            bw.RunWorkerAsync();


        }

        public void FireFormChangeEvent(object sender)
        {
            View v = (View)sender;
            OnFormChanged(v);
        }

    }
}