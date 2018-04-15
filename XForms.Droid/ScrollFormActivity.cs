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
using System.ComponentModel;
using XForms.XForms;
using System.Xml.Linq;
using Newtonsoft.Json;
using Android.Graphics;
using Android.Views.Animations;
using System.Threading;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using static Android.Views.View;

namespace XForms.Droid
{
    [Activity(Label = "ScrollFormActivity", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class ScrollFormActivity : BaseActivity, IXFormBuild
    {
        public List<Controls> FormControls;
        public List<Groups> FormGroups = new List<Groups>();

        public List<View> ControlViews = new List<View>();
        public ScrollView MainScrollView;
        LinearLayout MainFormLayout;

        public event EventHandler FormChanged;

        LayoutInflater inflater;

        ProgressDialog progress;

        public Animation ShowItem;
        public Animation HideItem;

        public bool PageLoaded = false;

        //if true, make every control readonly
        public bool isAssetForm = false;

        Forms form;

        public List<LinearLayout> OrderLayouts;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //this disables orientation changes while form loads, effectively ignoring the sensors
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Locked;

            SetContentView(Resource.Layout.Form2);
            inflater = LayoutInflater.From(this);

            MainFormLayout = FindViewById<LinearLayout>(Resource.Id.mainFormView);
            MainScrollView = FindViewById<ScrollView>(Resource.Id.mainScrollView);

            progress = new ProgressDialog(this);
            progress.SetCanceledOnTouchOutside(false);
            progress.SetCancelable(false);
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetProgressNumberFormat(null);
            progress.SetProgressPercentFormat(null);
            progress.Indeterminate = true;


            //grab the form name from previous
            string item = Intent.GetStringExtra("form_name");

            //if we came from a page button, these values will have something in them
            int groupId = Intent.GetIntExtra("groupId", -1);
            bool newForm = Intent.GetBooleanExtra("newForm", true);



            //get the form from the WS
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += delegate
            {

                if (newForm)
                {
                    RunOnUiThread(() =>
                    {
                        progress.SetMessage("Disposing tables...");
                        progress.Show();
                    });

                    if (XForm.instance == null)
                        DLL.DisposeTables();



                    RunOnUiThread(() => { progress.SetMessage("Downloading form..."); });

                    WebServerHelper.WebServiceResponse response =
                        JsonConvert.DeserializeObject<WebServerHelper.WebServiceResponse>(WebServerHelper.WebCall("https://macarthur.goget.co.nz:9725/apps/?getform=1",item));

                    string xform = response.data;

                    //puts it into the sqlite db
                    try
                    {
                        RunOnUiThread(() => { progress.SetMessage("Parsing form..."); });
                        Parser.Parse(xform, "Test Form");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    form = DLL.GetForm(1);

                    isAssetForm = item.ToLower().Contains("asset") ? true : false;
                    if(XForm.instance == null)
                    {
                        XForm.instance = new Instance()
                        {
                            formid = form.id,
                            instance = XForm.LoadInstance(form.instancexml),
                        };
                        DLL.InsertRow(XForm.instance);
                    }

                }


                try
                {
                    RunOnUiThread(() => { progress.SetMessage("Loading controls..."); });
                    //if we have a group id passed, this is a sub page
                    OrderLayouts = new List<LinearLayout>();

                    WidgetHelper.activity = this;
                    if (newForm)
                        XForm.LoadForm(null, this);
                    else
                    {
                        RunOnUiThread(() => {
                            progress.SetMessage($"Loading {item}...");
                            progress.Show();
                        });
                        XForm.LoadForm(groupId, this);
                        RunOnUiThread(() => { progress.Dismiss(); });
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    progress.Dismiss();

                    AlertDialog alert = new AlertDialog.Builder(this).Create();
                    alert.SetTitle("Error");
                    alert.SetMessage($"An error occured while loading the form. If the issue persists, check your form is valid XML. \n\nError:\n {ex.ToString()}");
                    alert.SetButton("OK", delegate { Finish(); });

                }

            };


            bw.RunWorkerCompleted += delegate { RunOnUiThread(() => {
                PageLoaded = true;
                //this reenables the app listening to orientation sensors
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Sensor;

                foreach(LinearLayout ol in OrderLayouts)
                {
                    TextView label = ol.FindViewById<TextView>(Resource.Id.textLabel);
                    EditText edit = ol.FindViewById<EditText>(Resource.Id.textEdit);

                    ol.Measure(0, 0);
                    int spec = MeasureSpec.MakeMeasureSpec(ol.MeasuredWidth, MeasureSpecMode.AtMost);

                    label.Measure(spec, spec);
                    edit.Measure(spec, spec);

                    Console.WriteLine(label.Text);

                   // if (label.MeasuredWidth > edit.MeasuredWidth)
                      //  ol.Orientation = Orientation.Vertical;

                }

                FireFormChangeEvent(null);
                progress.Dismiss();


            }); };

            bw.RunWorkerAsync();

            MainFormLayout.ChildViewAdded += delegate
            {
                RunOnUiThread(() => {
                    progress.SecondaryProgress += 1;
                    progress.Progress++;                    
                });
            };

        }

        protected override void OnResume()
        {
            base.OnResume();
            //when we return to the page
            //if (PageLoaded)
            FireFormChangeEvent(null);

            MainFormLayout.SetOnTouchListener(this);
            Iterate(MainFormLayout);
            touchEventsSetup = true;


        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean("inProgress", true);
        }

        public void FireFormChangeEvent(object sender)
        {
            if (PageLoaded)
            {
                View v = (View)sender;

                XForm.RunCalculates();
                try
                {
                    //bool result = await DLL.UpdateInstanceAsync(XForm.instance);
                    DLL.UpdateInstance(XForm.instance);
                }
                catch (SQLite.SQLiteException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                OnFormChanged(v);
            }
        }


        private void OnFormChanged(View v)
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                Console.WriteLine("Thread working in formchanged event!");
                try
                {
                    FormChanged?.Invoke(null, new XFormChangedEventArgs()
                    {
                    });

                    //handle group show/hide
                    foreach(Groups g in FormGroups)
                    {
                        View groupView = MainFormLayout.FindViewWithTag(g.id);
                        if(groupView != null)
                        {
                            Bindings binding = DLL.GetBindingByReference(g.reference);
                            if(binding != null && binding.relevant != null)
                            {
                                var eval = XForm.Evaluate(binding.relevant);
                                if (eval != null)
                                    RunOnUiThread(() => { groupView.Visibility = (bool)eval ? ViewStates.Visible : ViewStates.Gone; });
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });




        }

        #region XForms Interface 

        public void SetTitle(string title)
        {
            RunOnUiThread(() => { DesignHelper.SetBar(this, $"{title}", $"Form - {title}", 0); });
        }

        public void AddGroup(int groupid)
        {
            try
            {
                Groups group = DLL.GetGroup(groupid);

                //use MainFormLayout, add these groups to it

                View groupView = inflater.Inflate(Resource.Layout.Group, null);
                LinearLayout groupParent = groupView.FindViewById<LinearLayout>(Resource.Id.groupParent);
                LinearLayout groupLayout = groupView.FindViewById<LinearLayout>(Resource.Id.mainGroupLayout);
                TextView groupTitle = groupView.FindViewById<TextView>(Resource.Id.groupTitle);

                //the root layout of the group, with the group to encompass the controls and its title
                groupParent.Tag = group.id;

            

                groupTitle.Text = group.label;

                //add this to the main view
                RunOnUiThread(() => { MainFormLayout.AddView(groupView); });
                FormGroups.Add(group);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add group {groupid}");
                Console.WriteLine(ex.ToString());
            }
        }

        public void AddControl(Controls control, int? sectionID)
        {

            Bindings binding = XForm.GetBindingForControl(control);
            try
            {


                View controlView = new View(this);
                switch ((ControlType)control.type)
                {
                    case ControlType.input:
                        //the user puts something in 
                        controlView = WidgetHelper.CreateInput(control, binding, inflater, this);

                        break;
                    case ControlType.output:
                        Console.WriteLine("not yet");
                        controlView = new LinearLayout(this);
                        controlView.SetBackgroundColor(Color.Red);
                        controlView.SetMinimumHeight(25);


                        break;

                    case ControlType.select:
                        controlView = WidgetHelper.CreateSelect(control, binding, inflater, this);

                        break;
                    case ControlType.select1:
                        controlView = WidgetHelper.CreateSelectOne(control, binding, inflater, this);

                        break;
                    case ControlType.upload:
                        controlView = WidgetHelper.CreateUpload(control, binding, inflater, this);
                        break;

                }

                MainFormLayout.LayoutTransition.EnableTransitionType(Android.Animation.LayoutTransitionType.ChangeAppearing | Android.Animation.LayoutTransitionType.ChangeDisappearing);

                //root
                if (sectionID == null || sectionID == -1)
                    RunOnUiThread(() => { MainFormLayout.AddView(controlView); });
                
                //add to the group based on the ui
                else
                {
                    RunOnUiThread(() => {
                        LinearLayout groupParent = (LinearLayout)MainFormLayout.FindViewWithTag(sectionID.Value);
                        LinearLayout groupLayout = groupParent.FindViewById<LinearLayout>(Resource.Id.mainGroupLayout);

                        groupLayout.LayoutTransition.EnableTransitionType(Android.Animation.LayoutTransitionType.ChangeAppearing | Android.Animation.LayoutTransitionType.ChangeDisappearing);


                        Console.WriteLine($"Group tag: {groupParent.Tag}");

                        groupLayout.AddView(controlView);
                    });
                }
                //Console.WriteLine(binding.nodeset);
                ControlViews.Add(controlView);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void AddPageStub(int groupid, int? sectionID)
        {
            LinearLayout groupParent;
            LinearLayout groupLayout;

            //string parentName;
            //parentName = sectionID.Value;

            RunOnUiThread(() => {

                Groups group = DLL.GetGroup(groupid);
                //this could either be the MainFormLayout or a Group
                //groupParent = (LinearLayout)MainFormLayout.FindViewWithTag(parentName);

                View pageItem = inflater.Inflate(Resource.Layout.pageButton ,null);

                Button pageButton = pageItem.FindViewById<Button>(Resource.Id.pageButton);
                pageButton.Text = group.label;
                pageButton.Tag = groupid;

                pageButton.Click += PageButton_Click;

                try
                {
                    //on the root layout
                    if (sectionID == null || sectionID == -1)
                    {
                        groupParent = (LinearLayout)MainFormLayout.FindViewWithTag("mainView");
                        groupParent.AddView(pageItem);
                        //RunOnUiThread(() => { groupParent.AddView(pageItem); });

                    }
                    //inside a group
                    else
                    {
                        groupParent = (LinearLayout)MainFormLayout.FindViewWithTag(sectionID.Value);
                        groupLayout = groupParent.FindViewById<LinearLayout>(Resource.Id.mainGroupLayout);
                        groupLayout.AddView(pageItem);
                        //RunOnUiThread(() => { groupLayout.AddView(pageItem); });


                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to add page {groupid} on {sectionID}");
                    Console.WriteLine(ex.ToString());
                }
            });

        }

        //New page of a form
        private void PageButton_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;

            Intent pageScrollFormActivity = new Intent(this, typeof(ScrollFormActivity));
            pageScrollFormActivity.PutExtra("groupId", (int)b.Tag);
            pageScrollFormActivity.PutExtra("newForm", false);
            pageScrollFormActivity.PutExtra("form_name", b.Text);

            StartActivity(pageScrollFormActivity);

        }

        #endregion
    }
}