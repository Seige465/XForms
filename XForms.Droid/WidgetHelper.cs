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
using XForms.XForms;
using Android.Text;
using XForms.Droid.Fragments;
using Android.Util;
using System.Xml.Linq;
using Android.Graphics;
using System.ComponentModel;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using XForms.Droid.Helpers;
using Android.Graphics.Drawables;
using System.Threading;
//using Android.Media;
using Android.Support.Media;
using Newtonsoft.Json;

namespace XForms.Droid
{
    class WidgetHelper
    {
     
        static object obj = new object();

        public static ScrollFormActivity activity;
        public static View viewToPass;

        public static Paint mPaint;
        static int strokeWidth = 4;
        static bool highlight = false;

        static DisplayMetrics parentDm;


        #region Input
        public static View CreateInput(Controls control, Bindings binding, LayoutInflater inflater, ScrollFormActivity parent)
        {
            View v = inflater.Inflate(Resource.Layout.BindTextInput, null);

            //get the main layout
            LinearLayout main = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);

            //layout that can change to horizontal if needed be
            LinearLayout orderLayout = v.FindViewById<LinearLayout>(Resource.Id.orderLayout);

            TextView label = v.FindViewById<TextView>(Resource.Id.textLabel);
            EditText edit = v.FindViewById<EditText>(Resource.Id.textEdit);

            ImageButton hintButton = v.FindViewById<ImageButton>(Resource.Id.hintButton);

            if(binding.isreadonly != null)
            {
                edit.Focusable = !(bool)XForm.Evaluate(binding.isreadonly);
                if (!edit.Focusable) edit.Visibility = ViewStates.Gone;
            }

           
            label.Text = control.label;
            //edit.Hint = control.hint;

            if (!string.IsNullOrWhiteSpace(control.xpathhint))
                control.hint = XForm.Evaluate(control.xpathhint).ToString();

            //clear hint button if no hint
            if (string.IsNullOrWhiteSpace(control.hint) && string.IsNullOrWhiteSpace(control.xpathhint))
                hintButton.Visibility = ViewStates.Gone;

            else
                hintButton.Click += delegate
                {
                    Utilities.SetupHint(parent, control.hint);
                };

                //hintButton.SetupHint(parent, !string.IsNullOrWhiteSpace(control.xpathhint) ? XForm.Evaluate(control.xpathhint).ToString() : control.hint);

            /*
            else if (!string.IsNullOrWhiteSpace(control.hint) && string.IsNullOrWhiteSpace(control.xpathhint))
                hintButton.SetupHint(parent, control.hint);*/



            parent.FormChanged += delegate
            {
                UpdateControl(main, binding, control);
            };      

            edit.Text = XForm.GetValue(binding.nodeset);

            //if the value is some date object
            if (DateTime.TryParse(XForm.GetValue(binding.nodeset), out DateTime value))
            {
                switch (binding.type)
                {
                    case 3:
                        edit.Text = value.ToString("dd/MM/yyyy");
                        break;
                    case 4:
                        edit.Text = value.ToString("HH:mm z");
                        break;
                    case 5:
                        edit.Text = value.ToString("dd/MM/yyyy HH:mm:ss");
                        break;
                        
                }
            }

            if (parent.isAssetForm)
            {
                edit.Enabled = false;
                return v;
            }

            EventHandler<TextChangedEventArgs> ev = (s, e) =>
            {
                string val = edit.Text;
                string curval = XForm.GetValue(binding.nodeset);
                XForm.SetValue(binding.nodeset, val);
                if (binding.constraint != null)
                {
                    if (!CheckConstraint(main,binding))
                        XForm.SetValue(binding.nodeset, curval);
                }

                XForm.GetElement(binding.nodeset).Value = edit.Text;
                parent.FireFormChangeEvent(main);

            };

            EventHandler<View.FocusChangeEventArgs> focusEvent = (s, e) =>
            {
                Console.WriteLine("Focus Event!");
                if (!e.HasFocus)
                {
                    string val = edit.Text;
                    string curval = XForm.GetValue(binding.nodeset);
                    XForm.SetValue(binding.nodeset, val);
                    if (binding.constraint != null)
                    {
                        if (!CheckConstraint(main, binding))
                            XForm.SetValue(binding.nodeset, curval);
                    }

                    XForm.GetElement(binding.nodeset).Value = edit.Text;
                    parent.FireFormChangeEvent(main);
                }

            };

            //get the value from the instance
            //edit.TextChanged += ev;

            edit.FocusChange += focusEvent;

            switch (binding.type)
            {
                //string
                case 0:
                    if (control.appearance == "multiline")
                    {
                        edit.SetMaxLines(50);
                        orderLayout.Orientation = Android.Widget.Orientation.Vertical;
                    }
                    else if (control.appearance == "numbers")
                        edit.InputType = InputTypes.ClassNumber;

                    //edit.TextChanged += ev;

                    break;

                //int
                case 1:
                    //check keyboard type
                    edit.InputType = InputTypes.ClassNumber;
                    //edit.TextChanged += ev;

                    break;

                //decimal
                case 2:
                    edit.InputType = InputTypes.NumberFlagDecimal;

                    break;

                //date
                case 3:
                    edit.SetTextIsSelectable(false);
                    edit.InputType = InputTypes.Null;


                    switch (control.appearance)
                    {
                        case "year":

                            EventHandler yearHandler = (s, e) =>
                            {
                                parent.RunOnUiThread(() => {
                                    //picker like the old holo theme style
                                    NumberPicker yearPicker = new NumberPicker(parent)
                                    {
                                        MaxValue = 2100,
                                        MinValue = 1900,

                                        WrapSelectorWheel = false,
                                        Value = DateTime.Today.Year,
                                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                                    };
                                    AlertDialog dlg;
                                    dlg = new AlertDialog.Builder(parent).Create();
                                    dlg.SetView(yearPicker);
                                    //dlg.AddContentView(yearPicker, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                                    dlg.SetButton("Done", delegate
                                    {
                                        XForm.GetElement(binding.nodeset).Value = new DateTime(yearPicker.Value,DateTime.Now.Month,DateTime.Now.Day).ToString("yyyy-MM-dd");
                                        edit.Text = yearPicker.Value.ToString();
                                    });
                                    dlg.Window.SetSoftInputMode(SoftInput.StateHidden);
                                    dlg.Show();
                                });

                            };

                            label.Click += yearHandler;
                            main.Click += yearHandler;
                            edit.Click += yearHandler;


                            break;
                        case "month-year":

                            EventHandler monthYearHandler = (s, e) =>
                            {
                                parent.RunOnUiThread(() => {
                                    //picker like the old holo theme style
                                    NumberPicker yearPicker = new NumberPicker(parent)
                                    {
                                        MaxValue = 2100,
                                        MinValue = 1900,
                                        WrapSelectorWheel = false,
                                        Value = DateTime.Today.Year
                                    };

                                    //params for each picker
                                    LinearLayout.LayoutParams parameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent)
                                    {
                                        Weight = 1
                                    };

                                    NumberPicker monthPicker = new NumberPicker(parent)
                                    {
                                        MaxValue = 12,
                                        MinValue = 1,
                                        WrapSelectorWheel = false,
                                        Value = DateTime.Today.Month
                                    };

                                    LinearLayout ll = new LinearLayout(parent)
                                    {
                                        Orientation = Android.Widget.Orientation.Horizontal,
                                        WeightSum = 2
                                    };


                                    yearPicker.LayoutParameters = parameters;
                                    monthPicker.LayoutParameters = parameters;

                                    ll.AddView(monthPicker);
                                    ll.AddView(yearPicker);


                                    AlertDialog dlg;
                                    dlg = new AlertDialog.Builder(parent).Create();
                                    dlg.SetView(ll);
                                    //dlg.AddContentView(yearPicker, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                                    dlg.SetButton("Done", delegate
                                    {
                                        XForm.GetElement(binding.nodeset).Value = new DateTime(yearPicker.Value, monthPicker.Value, DateTime.Now.Day).ToString("yyyy-MM-dd");
                                        edit.Text = $"{monthPicker.Value.ToString()}/{yearPicker.Value.ToString()}";
                                    });
                                    dlg.Window.SetSoftInputMode(SoftInput.StateHidden);
                                    dlg.Show();
                                });

                            };

                            label.Click += monthYearHandler;
                            main.Click += monthYearHandler;
                            edit.Click += monthYearHandler;

                            break;

                        //everything else
                        default:

                            EventHandler dateHandler1 = (s, e) =>
                            {
                                FragmentTransaction ft = parent.FragmentManager.BeginTransaction();


                                //the delegate handles putting the value back in the editText
                                Fragment frag = DatePickerFragment.NewInstance(delegate (DateTime date)
                                {
                                    //put the value back into the instance
                                    XForm.GetElement(binding.nodeset).Value = date.ToString("yyyy-MM-dd");

                                    //based on appearance, show the appropriate value
                                    if (control.appearance == "month-year")
                                        edit.Text = date.ToString("MM/yyyy");
                                    else if (control.appearance == "year")
                                        edit.Text = date.ToString("yyyy");
                                    else
                                        edit.Text = date.ToString("dd/MM/yyyy");

                                    //UpdateViews();
                                    //parent.FireFormChangeEvent(main);

                                }, control.appearance, inflater);
                                ft.Add(frag, "dateFrag");
                                ft.Commit();
                            };

                            //make the items all clickable to bring up the date dialog
                            label.Click += dateHandler1;
                            main.Click += dateHandler1;
                            edit.Click += dateHandler1;

                            break;
                    }


                    break;

                //time
                case 4:
                    edit.SetTextIsSelectable(false);
                    edit.InputType = InputTypes.Null;


                    EventHandler dateHandler2 = (s, e) =>
                    {
                        FragmentTransaction ft = parent.FragmentManager.BeginTransaction();

                        Fragment frag = TimePickerFragment.NewInstance(delegate (DateTime time)
                        {
                            //put the value back into the instance
                            XForm.GetElement(binding.nodeset).Value = time.ToString("HH:mm:ssz");

                            edit.Text = time.ToShortTimeString();

                            //UpdateViews();
                            //parent.FireFormChangeEvent(main);

                        });
                        ft.Add(frag, "timeFrag");
                        ft.Commit();
                    };

                    label.Click += dateHandler2;
                    edit.Click += dateHandler2;
                    main.Click += dateHandler2;

                    break;

                //datetime 
                case 5:
                    edit.SetTextIsSelectable(false);
                    edit.InputType = InputTypes.Null;


                    EventHandler dateHandler3 = (s, e) =>
                    {
                        FragmentTransaction ft = parent.FragmentManager.BeginTransaction();

                        string selectedDate = "";
                        string selectedTime = "";
                        //the delegate handles putting the value back in the editText
                        Fragment frag;
                        frag = DatePickerFragment.NewInstance(delegate (DateTime date)
                        {
                            selectedDate = date.ToString("yyyy-MM-dd");


                            Fragment timeFrag = TimePickerFragment.NewInstance(delegate (DateTime time)
                            {
                                selectedTime = time.ToString("HH:mm:ss");

                                DateTime dt = DateTime.Parse($"{selectedDate} {selectedTime}");

                                XForm.GetElement(binding.nodeset).Value = dt.ToString("s") + "Z";

                                //putting the date and time into a readable thing
                                edit.Text = dt.ToString("dd/MM/yyyy HH:mm:ss tt");
                                //parent.FireFormChangeEvent(main);

                               // UpdateViews();

                            });
                            FragmentTransaction ft2 = parent.FragmentManager.BeginTransaction();
                            ft2.Add(timeFrag, "timeFrag");
                            ft2.Commit();


                        }, "", inflater);
                        ft.Add(frag, "dateFrag");
                        ft.Commit();
                    };

                    label.Click += dateHandler3;
                    main.Click += dateHandler3;
                    edit.Click += dateHandler3;

                    break;

                //geopoints
                case 6:
                    edit.SetTextIsSelectable(false);
                    edit.InputType = InputTypes.Null;
                    //edit.TextChanged -= ev;

                    EventHandler mapEvent = (s, e) => {
                        AlertDialog dlg;

                        //location is turned off
                        if (!parent.IsLocationEnabled())
                        {
                            dlg = new AlertDialog.Builder(parent).Create();
                            dlg.SetTitle("Warning");
                            dlg.SetMessage("Location Services are required to display user location.");
                            dlg.SetCancelable(false);
                            //take me to location settings
                            dlg.SetButton2("Settings", delegate
                            {
                                var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                                parent.StartActivity(intent);
                                dlg.Dismiss();
                            });
                            //just keep going anyway
                            dlg.SetButton("Continue", delegate 
                            {
                                dlg.Dismiss();
                                CreateMapDialog(inflater, parent, edit, binding);
                            });
                            dlg.SetCanceledOnTouchOutside(false);
                            dlg.Show();
                        }
                        //no connection but we have gps
                        else if(!Utilities.HasConnection() && parent.IsLocationEnabled())
                        {
                            CreateLocationDialog(inflater, parent, binding, edit);
                        }

                        else
                        {
                            CreateMapDialog(inflater, parent, edit, binding);
                        }
                    };
                    

                    label.Click += mapEvent;
                    main.Click += mapEvent;
                    edit.Click += mapEvent;

                    

                    break;

                //binary - TODO
                case 7:

                    break;
                default:
                    //edit.TextChanged += ev;

                    break;
            }

            parent.OrderLayouts.Add(orderLayout);

            return v;
        }
        #endregion

        #region Select1
        public static View CreateSelectOne(Controls control, Bindings binding, LayoutInflater inflater, ScrollFormActivity parent)
        {
            //get control options
            List<ControlOptions> cOptions = DLL.GetControlOptions(control.id);


            View v = inflater.Inflate(Resource.Layout.BindSelectOne, null);

            //get the main layout
            LinearLayout main = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            RelativeLayout orderLayout = v.FindViewById<RelativeLayout>(Resource.Id.orderLayout);
            orderLayout.LayoutTransition.EnableTransitionType(Android.Animation.LayoutTransitionType.Changing);


            TextView label = v.FindViewById<TextView>(Resource.Id.selectOneTitle);
            //compact group
            RadioGroup radioGroup = v.FindViewById<RadioGroup>(Resource.Id.selectOneGroup);
            ImageButton hintButton = v.FindViewById<ImageButton>(Resource.Id.hintButton);
            //minimal (segmented control)
            RadioGroup segmentedControl = v.FindViewById<RadioGroup>(Resource.Id.segmentedControl);

            label.Text = control.label;

            if (!string.IsNullOrWhiteSpace(control.xpathhint))
                control.hint = XForm.Evaluate(control.xpathhint).ToString();

            //clear hint button if no hint
            if (string.IsNullOrWhiteSpace(control.hint) && string.IsNullOrWhiteSpace(control.xpathhint))
                hintButton.Visibility = ViewStates.Gone;

            else
                hintButton.Click += delegate
                {
                    Utilities.SetupHint(parent, control.hint);
                };


            //The event handler for managing visibility
            parent.FormChanged += delegate
            {
                UpdateControl(main, binding, control);
            };

            //compact horizontal will be minimal for now
            if (control.appearance == "compact-horizontal")
                control.appearance = "minimal";


            List<string> items = cOptions.Select(x => x.label).ToList();

            items.Add(!parent.isAssetForm ? "Select an item..." : "No option selected.");

            switch (control.appearance)
            {
                //make popup dialog
                case "full":

                    radioGroup.Visibility = ViewStates.Gone;

                    string[] minLabels = cOptions.Select(x => x.label).ToArray();
                    //Spinner minSpinner = spinner;

                    Spinner fullSpinner = new Spinner(parent, SpinnerMode.Dialog);


                    fullSpinner.SetMinimumHeight(90);
                    main.AddView(fullSpinner);
                    SelectOneAdapter soa = new SelectOneAdapter(parent, items);
                    fullSpinner.Adapter = soa;
                    fullSpinner.Prompt = control.hint;
                    fullSpinner.SetSelection(items.Count - 1);



                    EventHandler<AdapterView.ItemSelectedEventArgs> ev = (s, e) =>
                    {
                        if (e.Position != items.Count - 1)
                        {
                            XForm.GetElement(binding.nodeset).Value = cOptions[e.Position].value;
                            parent.FireFormChangeEvent(main);
                        }
                    };


                    //set the item
                    for (int i = 0; i < cOptions.Count; i++)
                    {
                        if (XForm.GetElement(binding.nodeset).Value.Contains(cOptions[i].value))
                        {
                            fullSpinner.ItemSelected -= ev;
                            fullSpinner.SetSelection(i);
                            fullSpinner.ItemSelected += ev;
                        }
                    }

                    if (parent.isAssetForm)
                    {
                        fullSpinner.Enabled = false;
                        return v;
                    }

                    fullSpinner.ItemSelected -= ev;
                    fullSpinner.ItemSelected += ev;


                    break;

                //horizonal radio buttons
                case "compact":
                case "minimal":
                    bool compact = control.appearance.ToLower().Contains("compact");
                    //the expand/contract stuff
                    if (compact)
                    {
                        CreateRadioGroup(radioGroup, cOptions, parent, compact);
                        segmentedControl.Visibility = ViewStates.Gone;
                    }
                    //the segmented control
                    else
                    {
                        CreateRadioGroup(segmentedControl, cOptions, parent, compact);
                        radioGroup.Visibility = ViewStates.Gone;
                    }

                    
                    //event to show/hide the group
                    EventHandler compactClick = delegate
                    {
                        if (radioGroup.Visibility == ViewStates.Gone)
                        {
                            radioGroup.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            radioGroup.Visibility = ViewStates.Gone;
                        }

                    };



                    EventHandler<RadioGroup.CheckedChangeEventArgs> radioCheck = (s, e) =>
                    {
                        XForm.GetElement(binding.nodeset).Value = cOptions[e.CheckedId].value;

                        RadioGroup rg = (RadioGroup)s;
                        //set the text to a readable colour
                        /*
                        for (int i = 0; i < rg.ChildCount; i++)
                            if (rg.GetChildAt(i) is RadioButton rb)
                                rb.SetTextColor(rb.Checked ? Color.White : Color.Black);*/

                        if (compact)
                        {
                            radioGroup.Visibility = ViewStates.Gone;
                        }
                        parent.FireFormChangeEvent(main);

                        //UpdateViews();
                    };

                    //preselect if needed
                    string eval = XForm.GetElement(binding.nodeset).Value;
                    //iterate through the main LinearLayout's items, which in this case should be radiobutton
                    if(radioGroup.ChildCount > 0)
                    {
                        for (int i = 0; i < radioGroup.ChildCount; i++)
                        {
                            View cView = radioGroup.GetChildAt(i);
                            radioGroup.CheckedChange -= radioCheck;
                            if (cView is RadioButton rb)
                            {
                                Console.WriteLine(rb.Tag.ToString());
                                parent.RunOnUiThread(() => {
                                    if (eval == (rb.Tag.ToString()))
                                    {
                                        rb.Checked = true;
                                    }
                                });
                            }
                            radioGroup.CheckedChange += radioCheck;

                        }
                    }

                    //make our label clickable to expand and contract, hide it after values have been preset
                    if (compact)
                    {
                        radioGroup.Visibility = ViewStates.Gone;
                        label.Click += compactClick;
                    }

                    if (segmentedControl.ChildCount > 0)
                    {
                        for (int i = 0; i < segmentedControl.ChildCount; i++)
                        {
                            View cView = segmentedControl.GetChildAt(i);
                            segmentedControl.CheckedChange -= radioCheck;
                            if (cView is RadioButton rb)
                                parent.RunOnUiThread(() => { rb.Checked = (eval == rb.Tag.ToString()); });
                            segmentedControl.CheckedChange += radioCheck;

                        }
                    }

                    if (parent.isAssetForm)
                    {
                        radioGroup.Enabled = false;
                        return v;
                    }

                    radioGroup.CheckedChange -= radioCheck;
                    radioGroup.CheckedChange += radioCheck;
                    break;

                //dropdown
                default:
                    string[] compactLabels = cOptions.Select(x => x.label).ToArray();
                    //Spinner compactSpinner = spinner;
                    radioGroup.Visibility = ViewStates.Gone;
                    

                    parent.RunOnUiThread(() => {
                        Spinner minSpinner = new Spinner(parent, SpinnerMode.Dropdown);

                        SelectOneAdapter soa2 = new SelectOneAdapter(parent, items);
                        minSpinner.Adapter = soa2;
                        minSpinner.SetMinimumHeight(90);

                        main.AddView(minSpinner);
                        minSpinner.Prompt = control.hint;
                        minSpinner.SetSelection(items.Count - 1);



                        //compactSpinner.Touch += delegate {
                        //    soa.HideHintView();
                        //};


                        EventHandler<AdapterView.ItemSelectedEventArgs> ev2 = (s, e) =>
                        {
                            if (e.Position != items.Count - 1)
                            {
                                XForm.GetElement(binding.nodeset).Value = cOptions[e.Position].value;
                                parent.FireFormChangeEvent(main);
                            }


                            //UpdateViews();
                        };

                        //set the item
                        for (int i = 0; i < cOptions.Count; i++)
                        {
                            minSpinner.ItemSelected -= ev2;
                            if (XForm.GetElement(binding.nodeset).Value.Contains(cOptions[i].value))
                                minSpinner.SetSelection(i, false);
                            minSpinner.ItemSelected += ev2;
                        }

                        if (parent.isAssetForm)
                            minSpinner.Enabled = false;
                    });
                        
                    
                    break;
            }

            //fire event to update with information
            //_control = control;
            //_binding = binding;
            //CheckRelevence(v);
            //CheckRequired(v);
            //CheckCalculate(v, _control, _binding);
            //CheckConstraint(v);




            return v;
        }
        #endregion

        #region Select
        public static View CreateSelect(Controls control, Bindings binding, LayoutInflater inflater, ScrollFormActivity parent)
        {
            //get control options
            List<ControlOptions> cOptions = DLL.GetControlOptions(control.id);


            View v = inflater.Inflate(Resource.Layout.BindSelect, null);

            //get the main layout
            LinearLayout main = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            RelativeLayout orderLayout = v.FindViewById<RelativeLayout>(Resource.Id.orderLayout);
            LinearLayout multiSelect = v.FindViewById<LinearLayout>(Resource.Id.multiSelectSegmentedControl);
            ImageButton hintButton = v.FindViewById<ImageButton>(Resource.Id.hintButton);


            TextView label = v.FindViewById<TextView>(Resource.Id.selectTitle);
            label.Text = $"{control.label}";


            if (!string.IsNullOrWhiteSpace(control.xpathhint))
                control.hint = XForm.Evaluate(control.xpathhint).ToString();

            //clear hint button if no hint
            if (string.IsNullOrWhiteSpace(control.hint) && string.IsNullOrWhiteSpace(control.xpathhint))
                hintButton.Visibility = ViewStates.Gone;

            else
                hintButton.Click += delegate
                {
                    Utilities.SetupHint(parent, control.hint);
                };


            //evaluations from the control
            List<string> evalList = new List<string>();
            string eval = XForm.GetElement(binding.nodeset).Value;


            //The event handler for managing visibility
            parent.FormChanged += delegate
            {
                UpdateControl(main, binding, control);
            };



            CheckBox[] CheckGroup;

            switch (control.appearance)
            {
                //horizontal setup, like a segmented control
                case "minimal":
                    CheckGroup = CreateSelectGroup(cOptions, parent, multiSelect, false, parent);
                    break;
                //vertical select buttons
                case "compact":
                        CheckGroup = CreateSelectGroup(cOptions, parent, main, true, parent);
                    break;

                default:

                    //prepopulate label if there are any values in it
                    TextView valueText = new TextView(parent);
                    main.AddView(valueText);
                    valueText.SetPadding(15,5,5,5);
                    valueText.SetTextSize(ComplexUnitType.Dip, 18);
                    if (!string.IsNullOrEmpty(eval))
                    {
                        eval = eval.Replace("|", ", ");
                        valueText.Text = eval;
                    }

                    //label.TextSize = 28;

                    ListView lv = null;
                    View multiSelectView;
                    Dialog dlg = null;

                    parent.RunOnUiThread(() =>
                    {


                        dlg = new Dialog(parent);
                        dlg.SetTitle(control.hint);
                        multiSelectView = inflater.Inflate(Resource.Layout.BindSelect_Multi, null, false);

                        lv = multiSelectView.FindViewById<ListView>(Resource.Id.multiSelect);
                        lv.Adapter = new ArrayAdapter(parent, Android.Resource.Layout.SimpleListItemMultipleChoice, cOptions.Select(x => x.label).ToArray());

                        dlg.SetContentView(multiSelectView);

                        //preselect any items if required
                        for (int i = 0; i < cOptions.Count; i++)
                            lv.SetItemChecked(i, eval.Contains(cOptions[i].value));

                        dlg.DismissEvent += delegate
                        {
                            evalList = new List<string>();
                            SparseBooleanArray checkedItems = lv.CheckedItemPositions;
                            for (int i = 0; i < lv.Adapter.Count; i++)
                            {
                                //if this index
                                if (checkedItems.Get(i))
                                    evalList.Add(cOptions[i].value);
                            }

                            //throw it back into the instance
                            eval = string.Join("|", evalList.ToArray());
                            XForm.GetElement(binding.nodeset).Value = eval;

                            valueText.Text = string.Join(", ", evalList.ToArray());

                            parent.FireFormChangeEvent(main);
                            //UpdateViews();


                        };

                    });
                    if (parent.isAssetForm)
                    {
                        return v;
                    }

                    label.Click += delegate { dlg.Show(); };
                    valueText.Click += delegate { dlg.Show(); };

                    break;
            }


            //iterate through the main LinearLayout's items, which in this case should be checkboxes
            //start at 1, because index 0 is the label
            for (int i = 1; i < main.ChildCount; i++)
            {
                View cView = main.GetChildAt(i);
                if (cView is CheckBox cb)
                {
                    //checked change event inline
                    EventHandler<CompoundButton.CheckedChangeEventArgs> ev = (s, e) =>
                    {
                        cb = (CheckBox)s;

                        //if checked, grab the id and place it in the string
                        if (cb.Checked)
                            evalList.Add(cb.Tag.ToString());

                        //otherwise remove it from the eval provided
                        else
                            evalList.Remove(cb.Tag.ToString());

                        eval = string.Join("|", evalList.ToArray());

                        //throw it back into the instance
                        XForm.GetElement(binding.nodeset).Value = eval;

                        parent.FireFormChangeEvent(main);
                        //UpdateViews();
                    };

                    parent.RunOnUiThread(() =>
                    {
                        cb.CheckedChange -= ev;
                        cb.Checked = eval.Contains(cb.Tag.ToString());
                        cb.CheckedChange += ev;
                    });

                    if (parent.isAssetForm)
                    {
                        cb.Enabled = false;
                    }
                }
            }
            //_control = control;
            //_binding = binding;
            //CheckRelevence(v);
            //CheckRequired(v);
            //CheckCalculate(v, _control, _binding);
            //CheckConstraint(v);



            return v;
        }
        #endregion

        #region Upload
        public static View CreateUpload(Controls control, Bindings binding, LayoutInflater inflater, ScrollFormActivity parent)
        {
            //get control options
            List<ControlOptions> cOptions = DLL.GetControlOptions(control.id);

            View v = inflater.Inflate(Resource.Layout.BindImageInput, null);

            //get the main layout
            RelativeLayout main = v.FindViewById<RelativeLayout>(Resource.Id.mainLayout);
            ImageButton hintButton = v.FindViewById<ImageButton>(Resource.Id.hintButton);

            parent.FormChanged += delegate
            {
                UpdateControl(main, binding, control);
            };

            TextView label = v.FindViewById<TextView>(Resource.Id.textLabel);
            label.Text = $"{control.label}";

            if (!string.IsNullOrWhiteSpace(control.xpathhint))
                control.hint = XForm.Evaluate(control.xpathhint).ToString();

            //clear hint button if no hint
            if (string.IsNullOrWhiteSpace(control.hint) && string.IsNullOrWhiteSpace(control.xpathhint))
                hintButton.Visibility = ViewStates.Gone;

            else
                hintButton.Click += delegate
                {
                    Utilities.SetupHint(parent, control.hint);
                };


            ImageView iv = (ImageView)v.FindViewWithTag("image");
            string imagePath = XForm.GetValue(binding.nodeset);
            if (!string.IsNullOrEmpty(imagePath))
                iv.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(imagePath, 100,100, parent));

            if (parent.isAssetForm)
            {
                main.Enabled = false;
                return v;
            }

            mPaint = new Paint
            {
                AntiAlias = true,
                Dither = true,
                Color = Color.Green
            };
            mPaint.SetStyle(Paint.Style.Stroke);
            mPaint.StrokeJoin = Paint.Join.Round;
            mPaint.StrokeCap = Paint.Cap.Round;
            mPaint.StrokeWidth = 12;

            EventHandler drawHandler = (s, e) =>
            {


                AlertDialog alert = new AlertDialog.Builder(parent, Android.Resource.Style.ThemeMaterialLightNoActionBarFullscreen).Create();
                alert.Window.SetSoftInputMode(SoftInput.StateHidden);
                //Bitmap bitmap = BitmapFactory.DecodeFile(XForm.GetValue(binding.nodeset));


                View cView = inflater.Inflate(Resource.Layout.CanvasDraw, null);
                LinearLayout cLayout = cView.FindViewById<LinearLayout>(Resource.Id.mainCanvasLayout);
                LinearLayout buttons = cView.FindViewById<LinearLayout>(Resource.Id.colourButtonLayout);

                EditText exifEdit = cView.FindViewById<EditText>(Resource.Id.editExifText);
                Switch highlightSwitch = cView.FindViewById<Switch>(Resource.Id.switchHighlight);

                //add transparency in the drawing
                highlightSwitch.CheckedChange += (t, f) =>
                {
                    if (f.IsChecked)
                        mPaint.Color = Color.Argb(126, mPaint.Color.R, mPaint.Color.G, mPaint.Color.B);
                    else
                        mPaint.Color = Color.Argb(255, mPaint.Color.R, mPaint.Color.G, mPaint.Color.B);

                    highlight = f.IsChecked;
                };


                TextView widthText = cView.FindViewById<TextView>(Resource.Id.widthText);
                widthText.Text = $"Width: {strokeWidth.ToString()}";

                SeekBar widthSeek = cView.FindViewById<SeekBar>(Resource.Id.widthSeek);
                Bitmap bitmap = null;
                int offsetHeight = (buttons.Height + widthSeek.Height + 20);

                parentDm = parent.GetDisplayInfo();


                if (!string.IsNullOrEmpty(XForm.GetValue(binding.nodeset)))
                {
                    
                    bitmap = BitmapFactory.DecodeFile(XForm.GetValue(binding.nodeset));

                    //for those less than android 7, gotta use support library
                    ExifInterface exif = new ExifInterface(XForm.GetValue(binding.nodeset));
                    int orientation = exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 1);
                    //grab any note from earlier
                    string note = exif.GetAttribute(ExifInterface.TagImageDescription);
                    exifEdit.Text = note;
                    switch (orientation)
                    {
                        case 6:
                            bitmap = BitmapHelpers.RotateBitmap(bitmap, 90f);
                            break;
                        case 3:
                            bitmap = BitmapHelpers.RotateBitmap(bitmap, 180f);
                            break;
                        case 8:
                            bitmap = BitmapHelpers.RotateBitmap(bitmap, 270f);
                            break;
                    }


                    decimal scale;
                    if (bitmap.Width > bitmap.Height)
                        scale = Decimal.Divide(parentDm.WidthPixels, bitmap.Width);
                    else
                        scale = Decimal.Divide(parentDm.HeightPixels - 600, bitmap.Height);

                    int width = (int)(bitmap.Width * scale);
                    int height = (int)(bitmap.Height * scale);
                    bitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, false);
                    
                    //temp.Recycle();
                    GC.Collect();
                }
                //bitmap = Bitmap.CreateScaledBitmap(BitmapHelpers.LoadAndResizeBitmap(XForm.GetValue(binding.nodeset), parentDm.WidthPixels, parentDm.HeightPixels - offsetHeight, parent), parentDm.HeightPixels - offsetHeight, parentDm.WidthPixels,false);
                //bitmap = BitmapHelpers.LoadAndResizeBitmap(XForm.GetValue(binding.nodeset), parentDm.WidthPixels, parentDm.HeightPixels - offsetHeight, parent);
                //DrawingView dv = new DrawingView(parent, bitmap, strokeWidth);
                cLayout.SetFitsSystemWindows(true);
                //cLayout.AddView(dv);

                widthSeek.Progress = strokeWidth;
                widthSeek.ProgressChanged += delegate
                {
                    strokeWidth = widthSeek.Progress;
                //    dv.UpdateStrokeWidth(strokeWidth);
                    widthText.Text = $"Width: {strokeWidth.ToString()}";
                };




                //we don't need colours, text box or the highlight switch if signature
                if (control.appearance == "signature")
                {
                    mPaint.Color = Color.Black;
                    buttons.Visibility = ViewStates.Gone;
                    exifEdit.Visibility = ViewStates.Gone;
                    highlightSwitch.Visibility = ViewStates.Gone;
                }
                //setup events to change paint colour
                else
                    ApplyButtonEvents(buttons, mPaint);



                cView.SetFitsSystemWindows(true);
                alert.SetView(cView);

                //save the bitmap to a file
                alert.SetButton("Save", delegate
                {
                    string fullFilePath;
                    if (string.IsNullOrEmpty(XForm.GetValue(binding.nodeset)))
                        fullFilePath = System.IO.Path.Combine(parent.filePath, $"drawing_{Guid.NewGuid()}.jpg");
                    else
                        fullFilePath = XForm.GetValue(binding.nodeset);
                    try
                    {
                        using (var os = new System.IO.FileStream(fullFilePath, System.IO.FileMode.Create))
                        {
                     //       dv.bm.Compress(Bitmap.CompressFormat.Jpeg, 70, os);
                        }

                        XForm.SetValue(binding.nodeset, fullFilePath);

                        //save the note into the exif
                        ExifInterface exif = new ExifInterface(XForm.GetValue(binding.nodeset));
                        exif.SetAttribute(ExifInterface.TagImageDescription, exifEdit.Text);
                        exif.SaveAttributes();

                        //thumbnail
                        Bitmap imageBitmap = BitmapHelpers.LoadAndResizeBitmap(fullFilePath, iv.Width, iv.Height, parent);
                        iv.SetImageBitmap(imageBitmap);
                       // imageBitmap.Recycle();
                        GC.Collect();
                        parent.FireFormChangeEvent(main);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                });

                alert.SetButton2("Clear", delegate
                {
                    AlertDialog deleteAlert = new AlertDialog.Builder(parent).Create();
                    deleteAlert.SetTitle("Warning");
                    deleteAlert.SetMessage("Do you wish to clear the canvas?");
                    deleteAlert.SetButton("Yes", delegate {

                   //     dv.ClearCanvas();
                        //iv.Background = new ColorDrawable(Color.Transparent);
                        iv.SetImageResource(Resource.Drawable.ic_create_black_18dp);
                        

                        if (!string.IsNullOrEmpty(XForm.GetElement(binding.nodeset).Value))
                        {
                            System.IO.File.Delete(XForm.GetElement(binding.nodeset).Value);
                            XForm.SetValue(binding.nodeset, "");

                        }
                        parent.FireFormChangeEvent(main);


                    });
                    deleteAlert.SetButton2("No", delegate { alert.Show(); });
                    deleteAlert.Show();


                });

                alert.SetButton3("Cancel", delegate { alert.Dismiss(); });


                //cView.SetFitsSystemWindows(true);

                //alert.SetDialogButtons();

                //if appearance requires annotation of some form
                if (control.appearance.ToLower() == "annotate" || control.appearance.ToLower() == "textannotate")
                    if (string.IsNullOrEmpty(XForm.GetElement(binding.nodeset).Value))
                        parent.TakePhoto($"IMG_{XForm.instance.formid}_{control.appearance.ToLower()}_", binding, iv, parent, true);
                    else
                        alert.Show();
                else
                    alert.Show();



                //alert.Window.SetLayout(parentDm.WidthPixels, parentDm.HeightPixels);


            };

            //open up our canvas activity for drawing and annotating
            EventHandler CanvasActivityHandler = (s, e) => 
            {
                Intent i = new Intent(parent, typeof(DrawingActivity));
                i.PutExtra("title", control.label);
                i.PutExtra("binding", JsonConvert.SerializeObject(binding));
                i.PutExtra("control", JsonConvert.SerializeObject(control));
                viewToPass = v;
                parent.StartActivityForResult(i, parent.REQUEST_DRAWING);
            };

            switch (control.appearance)
            {
                case "signature":
                case "draw":
                case "annotate":
                case "textannotate":
                    iv.Click += CanvasActivityHandler;
                    if (string.IsNullOrWhiteSpace(imagePath))
                        iv.SetImageResource(Resource.Drawable.ic_create_black_18dp);

                    break;
                default:

                    EventHandler imageHandler = (s, e) => 
                    {
                        AlertDialog alert = new AlertDialog.Builder(parent).Create();

                        View alertView = inflater.Inflate(Resource.Layout.UploadDialog, null);
                        alert.SetView(alertView);


                        alert.SetButton("Take Photo", delegate
                        {
                            parent.TakePhoto($"IMG_{XForm.instance.formid}_{control.appearance.ToLower()}_", binding, v, parent, false);
                        });
                        ImageView iv2 = alertView.FindViewById<ImageView>(Resource.Id.selectedImage);

                        alert.SetButton2("Delete Photo", delegate
                        {
                            AlertDialog deleteAlert = new AlertDialog.Builder(parent).Create();
                            deleteAlert.SetTitle("Warning");
                            deleteAlert.SetMessage("Do you wish to delete this photo?");
                            deleteAlert.SetButton("Yes", delegate
                            {
                                try
                                {
                                    iv2.SetImageBitmap(null);
                                    System.IO.File.Delete(XForm.GetElement(binding.nodeset).Value);
                                    XForm.GetElement(binding.nodeset).Value = "";
                                    //iv.Background = new ColorDrawable(Color.Transparent);
                                    iv.SetImageResource(Resource.Drawable.ic_create_black_18dp);

                                    parent.FireFormChangeEvent(main);

                                    //UpdateViews();
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            });
                            deleteAlert.SetButton2("No", delegate {
                                alert.Show();
                            });
                            deleteAlert.Show();

                        });


                        //if there is an no image, pop up the dialog, then show image select
                        alert.Show();

                        parentDm = parent.GetDisplayInfo();
                        alert.Window.SetLayout((int)(parentDm.WidthPixels * 0.9), (int)(parentDm.HeightPixels * 0.9));

                        string path = XForm.GetElement(binding.nodeset).Value;
                        if (!string.IsNullOrEmpty(path))
                            iv2.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(path, 1000, 1000, parent));
                        
                    };


                    EventHandler defaultHandler = delegate
                    {
                        parent.TakePhoto($"IMG_{XForm.instance.formid}_", binding, v, parent, false);
                    };
                    iv.Click += defaultHandler;

                    break;


            }





            return v;
        }
        #endregion

        #region Making the radio and checkbox groups
        public static void CreateRadioGroup(RadioGroup group, List<ControlOptions> co, Context context, bool compact)
        {
            RadioButton[] rb = new RadioButton[co.Count];

            Drawable left = context.GetDrawable(Resource.Drawable.radioButtonLeftBackground);
            Drawable right = context.GetDrawable(Resource.Drawable.radioButtonRightBackground);
            Drawable middle = context.GetDrawable(Resource.Drawable.radioButtonBackground);

            for (int i = 0; i < co.Count; i++)
            {
                rb[i] = new RadioButton(context)
                {
                    Text = co[i].label,
                    Tag = co[i].value,
                    Id = i
                };


                //minimal here
                if (!compact)
                {
                    rb[i].SetPadding(40, 25, 40, 25);

                    
                    rb[i].SetButtonDrawable(new ColorDrawable(Color.Transparent));
                    rb[i].Background = (i == 0 ? left : i == co.Count - 1 ? right : middle);
                }

                group.AddView(rb[i]);


            }

        }
        
        public static CheckBox[] CreateSelectGroup(List<ControlOptions> co, Context context, View v, bool compact, ScrollFormActivity parent)
        {
            CheckBox[] cb = new CheckBox[co.Count];

            Drawable left = context.GetDrawable(Resource.Drawable.radioButtonLeftBackground);
            Drawable right = context.GetDrawable(Resource.Drawable.radioButtonRightBackground);
            Drawable middle = context.GetDrawable(Resource.Drawable.radioButtonBackground);


            int width = 0;
            LinearLayout ll = (LinearLayout)v;
            
            if (compact)
            {

                DisplayMetrics dm = context.Resources.DisplayMetrics;
                var widthDP = dm.WidthPixels / context.Resources.DisplayMetrics.Density;
                width = (int)widthDP / co.Count;
            }
            else
            {
              //  ll.Orientation = Orientation.Horizontal;
            }

            //group.Orientation = Orientation.Horizontal;
            /*
            if (!compact)
            {
                ll.LayoutParameters = new ViewGroup.LayoutParams(WindowManagerLayoutParams.WrapContent, WindowManagerLayoutParams.WrapContent);
                parent.RunOnUiThread(() => { ll.Orientation = Orientation.Horizontal; });
            }
            */


            for (int i = 0; i < co.Count; i++)
            {
                cb[i] = new CheckBox(parent)
                {
                    Text = co[i].label,
                    Tag = co[i].value,
                    Id = i
                };
                if (compact)
                    cb[i].LayoutParameters = new ViewGroup.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.MatchParent);
                else
                {

                    cb[i].SetPadding(40, 25, 40, 25);
                    cb[i].SetButtonDrawable(new ColorDrawable(Color.Transparent));
                    cb[i].Background = (i == 0 ? left : i == co.Count - 1 ? right : middle);

                }


                ll.AddView(cb[i]);


            }
            return cb;

        }

        #endregion

        #region map dialog and location dialog
        private static void CreateMapDialog(LayoutInflater inflater, ScrollFormActivity parent, EditText edit, Bindings binding)
        {
            AlertDialog dlg = new AlertDialog.Builder(parent, Android.Resource.Style.ThemeMaterialLightNoActionBarFullscreen).Create();
            View mapView = inflater.Inflate(Resource.Layout.BindMapInput, null);
            Toolbar tb = mapView.FindViewById<Toolbar>(Resource.Id.mapToolbar);

            DesignHelper.SetBackgroundBar(tb, parent);
            mapView.SetFitsSystemWindows(true);

            dlg.SetView(mapView);

            //eventhandler listening to when the parent's lat long object changes
            parent.LatLngChanged += delegate
            {
                tb.Title = $"{parent.Lat_Lng.Latitude:F4},{parent.Lat_Lng.Longitude:F4}";
            };


            MapView map = mapView.FindViewById<MapView>(Resource.Id.geopointMap);
            map.OnCreate(dlg.OnSaveInstanceState());
            map.OnResume();
            map.GetMapAsync(parent);

            dlg.SetButton("Save", delegate
            {
                if (parent.Lat_Lng != null)
                {
                    edit.Text = $"{parent.Lat_Lng.Latitude:F4}, {parent.Lat_Lng.Longitude:F4}";
                    XForm.GetElement(binding.nodeset).Value = $"{parent.Lat_Lng.Latitude:F4} {parent.Lat_Lng.Longitude:F4}";
                    Thread.Sleep(500);

                    //parent.FireFormChangeEvent(main);
                }
            });

            dlg.SetButton2("Use Location", delegate
            {
                parent.MarkWithGPS();
                edit.Text = $"{parent.Lat_Lng.Latitude:F4}, {parent.Lat_Lng.Longitude:F4}";
                XForm.GetElement(binding.nodeset).Value = $"{parent.Lat_Lng.Latitude:F4} {parent.Lat_Lng.Longitude:F4}";
                dlg.Window.Attributes.WindowAnimations = Android.Resource.Animation.FadeOut;
            });

            dlg.SetButton3("Cancel", delegate
            {
                dlg.Window.Attributes.WindowAnimations = Android.Resource.Animation.FadeOut;
                dlg.Dismiss();
            });

            dlg.Window.Attributes.WindowAnimations = Android.Resource.Animation.FadeIn;
            dlg.Show();
            //dlg.SetDialogButtons();
            dlg.DismissEvent += delegate
            {
                dlg.Window.Attributes.WindowAnimations = Android.Resource.Animation.FadeOut;

                parent.apiClient.Disconnect();
            };
        }

        private static void CreateLocationDialog(LayoutInflater inflater, ScrollFormActivity parent, Bindings binding, EditText edit)
        {
            AlertDialog dlg = new AlertDialog.Builder(parent).Create();
            View LocationView = inflater.Inflate(Resource.Layout.LocationDialog, null);

            TextView txtLat = LocationView.FindViewById<TextView>(Resource.Id.lblLatitude);
            TextView txtLng = LocationView.FindViewById<TextView>(Resource.Id.lblLongitude);

            if (!string.IsNullOrWhiteSpace(edit.Text))
            {
                string[] editLocation = edit.Text.Split(',');

                txtLat.Text = editLocation[0];
                txtLng.Text = editLocation[1];

            }

            //gotta ask permission, 3 is location permission from base
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                parent.RequestPermissions(new string[] {
                    Android.Manifest.Permission.AccessCoarseLocation,
                    Android.Manifest.Permission.AccessFineLocation },
                    3);
            }
            else
            {
                parent.apiClient.Connect();
            }

            parent.LatLngChanged += delegate
            {
                txtLat.Text = $"Latitude: {parent.Lat_Lng.Latitude:F4}";
                txtLng.Text = $"Longitude: {parent.Lat_Lng.Longitude:F4}";
            };


            dlg.SetMessage("Geopoint - User Location");
            dlg.SetView(LocationView);
            dlg.SetButton("Save", delegate
            {
                edit.Text = $"{txtLat.Text.Trim(' ').Split(':')[1]}, {txtLng.Text.Trim(' ').Split(':')[1]}";
                XForm.GetElement(binding.nodeset).Value = edit.Text;

            });

            dlg.SetButton2("Cancel", delegate { return; });

            dlg.DismissEvent += delegate
            {
                dlg.Window.Attributes.WindowAnimations = Android.Resource.Animation.FadeOut;
                parent.apiClient.Disconnect();
            };

            dlg.Show();


        }

        #endregion

        #region Events
        /*
        private static void _formActivity_FormChanged(object sender, EventArgs e)
        {



            View v = ((XFormChangedEventArgs)e)._view;

            //obtain binding
            Bindings binding = DLL.GetBindingById((int)v.Tag);
            Console.WriteLine($"Form change event happening - {binding.nodeset}");

            CheckRelevence(v, binding);
            CheckRequired(v, binding);
            if (!string.IsNullOrEmpty(binding.constraint))
                CheckConstraint(v, binding);
            if (!string.IsNullOrEmpty(binding.calculate))
                CheckCalculate(v, _control, binding);
        }*/

        private static void UpdateControl(View v, Bindings binding, Controls _control)
        {
            CheckRelevence(v, binding);
            CheckRequired(v, binding);
            //CheckConstraint(v, binding);
            CheckCalculate(v, _control, binding);

        }

        private static void ApplyButtonEvents(View v, Paint paint)
        {
            //the event

            if(v is LinearLayout ll)
            {
                for(int i = 0; i < ll.ChildCount; i++)
                {
                    var button = ll.GetChildAt(i);
                    if(button is Button b)
                        b.Click += delegate
                        {
                            if (button.Background is ColorDrawable cd)
                            {
                                cd = (ColorDrawable)button.Background;
                                if (highlight)
                                    paint.Color = Color.Argb(128, cd.Color.R, cd.Color.G, cd.Color.B);
                                else
                                    paint.Color = cd.Color;
                            }
                            else if(button.Background is GradientDrawable )
                            {
                                if (ll.IndexOfChild(button) == 0)
                                    if (highlight)
                                        paint.Color = Color.Argb(128, 255, 0, 0);
                                    else
                                        paint.Color = Color.Red;
                                else
                                    if (highlight)
                                        paint.Color = Color.Argb(128, 255, 255, 255);
                                    else
                                        paint.Color = Color.Black;

                            }
                        };   
                }
            }
        }

        #endregion

        #region Show/Hide logic and validation
        private static void CheckRelevence(View v, Bindings _binding)
        {
            if (string.IsNullOrWhiteSpace(_binding.relevant))
                return;

            var relevant = XForm.Evaluate(_binding.relevant);
            if (relevant == null) return;
            if ((bool)relevant)
            {
                activity.RunOnUiThread(() => { v.Visibility = ViewStates.Visible; });
            }
            else
            {
                activity.RunOnUiThread(() => { v.Visibility = ViewStates.Gone; });
            }
        }
        private static void CheckRequired(View v, Bindings _binding)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_binding.required))
                    return;
                var required = XForm.Evaluate(_binding.required);
                if (required == null) return;
                TextView tv = (TextView)v.FindViewWithTag("title");
                if(tv != null)
                {
                    if ((bool)required && string.IsNullOrWhiteSpace(XForm.GetValue(_binding.nodeset)))
                        activity.RunOnUiThread(() => { tv.SetTextColor(Color.Red); });
                    else
                        activity.RunOnUiThread(() => { tv.SetTextColor(Color.Black); });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Required failed: {ex.ToString()}");
            }
        }

        private static void CheckCalculate(View v, Controls _control, Bindings _binding)
        {
            TextView _label = (TextView)v.FindViewWithTag("title");
         //   EditText _edit = v.FindViewById<EditText>(Resource.Id.textEdit);
            ImageButton hintButton = v.FindViewById<ImageButton>(Resource.Id.hintButton);


            if (_control.xpathhint != null)
                _control.hint = XForm.Evaluate(_control.xpathhint).ToString();
            
            else if (_control.xpathlabel != null)
                activity.RunOnUiThread(() => { _label.Text = XForm.Evaluate(_control.xpathlabel).ToString(); });
        }

        private static bool CheckConstraint(View v, Bindings binding)
        {
            LinearLayout ll = (LinearLayout)v;
            TextView constraintLabel = ll.FindViewById<TextView>(Resource.Id.constraintLabel);
            if (constraintLabel != null)
            {

                try
                {
                    if (binding.constraint != null)
                    {
                        constraintLabel.SetTextColor(Color.Red);
                        object eval = XForm.Evaluate(binding.constraint, binding.nodeset);
                        if (eval != null && !(bool)eval)
                        {
                            if (!string.IsNullOrWhiteSpace(XForm.GetValue(binding.nodeset)))
                            {
                                activity.RunOnUiThread(() =>
                                {
                                    constraintLabel.Text = binding.constraintMsg;
                                    constraintLabel.Visibility = ViewStates.Visible;

                                });
                                return false;
                            }
                        }
                        else
                        {
                            activity.RunOnUiThread(() =>
                            {
                                constraintLabel.Visibility = ViewStates.Gone;
                            });
                            return true;
                        }
                    }
                    else
                    {
                        activity.RunOnUiThread(() =>
                        {
                            constraintLabel.Visibility = ViewStates.Gone;
                        });
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"constraint says no.\n {ex.ToString()}");
                    
                }
            }
            return true;
        }
        #endregion



    }
}