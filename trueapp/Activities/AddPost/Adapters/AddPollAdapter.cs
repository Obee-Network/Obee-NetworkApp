using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;

namespace ObeeNetwork.Activities.AddPost.Adapters
{
    public class PollAnswers
    {
        public int Id { get; set; }
        public string Answer { get; set; }
    }

    public class AddPollAdapter : RecyclerView.Adapter
    {
        private int Position;
        private readonly Activity ActivityContext;
        public readonly ObservableCollection<PollAnswers> AnswersList = new ObservableCollection<PollAnswers>();

        public AddPollAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public override int ItemCount
        {
            get
            {
                if (AnswersList != null)
                    return AnswersList.Count;
                return 0;
            }
        }

        public event EventHandler<AddPollAdapterClickEventArgs> ItemClick;
        public event EventHandler<AddPollAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Gif_View
                var itemView = LayoutInflater.From(parent.Context) .Inflate(Resource.Layout.Style_AddPoll, parent, false);
                var vh = new AddPollAdapterViewHolder(itemView, Click, CloseClickListener);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                Position = position;
                if (viewHolder is AddPollAdapterViewHolder holder)
                {
                    var itemcount = Position + 1;
                    holder.Number.Text = itemcount.ToString(); 
                    holder.Input.Hint = ActivityContext.GetText(Resource.String.Lbl2_Answer) + " " + itemcount;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

 
        public PollAnswers GetItem(int position)
        {
            return AnswersList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        private void Click(AddPollAdapterClickEventArgs args)
        {
            try
            {
                var item = AnswersList[args.Position];
                item.Answer = args.Text;
                args.Input.RequestFocus();
                ItemClick?.Invoke(this, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private void CloseClickListener(AddPollAdapterClickEventArgs args)
        {
            if (AnswersList.Count > 2)
            {
                var item = AnswersList[args.Position];
                AnswersList.Remove(item);
                NotifyDataSetChanged();
                ItemLongClick?.Invoke(this, args);
            }
            else
            {
                
                Snackbar mySnackbar = Snackbar.Make(args.View,
                    ActivityContext.GetText(Resource.String.Lbl2_PollsLimitTwo), Snackbar.LengthShort);
                mySnackbar.Show();
            }
        }
    }

    public class AddPollAdapterViewHolder : RecyclerView.ViewHolder
    {
       
        public AddPollAdapterViewHolder(View itemView, Action<AddPollAdapterClickEventArgs> clickListener,  Action<AddPollAdapterClickEventArgs> closeClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                var circel = (TextView)MainView.FindViewById(Resource.Id.bgcolor);
                Number = (TextView)MainView.FindViewById(Resource.Id.number);
                Input = (EditText)MainView.FindViewById(Resource.Id.text_input);
                CloseButton = (Button)MainView.FindViewById(Resource.Id.Close);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, circel, IonIconsFonts.Record);

                Methods.SetColorEditText(Input, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");
                CloseButton.SetTypeface(font, TypefaceStyle.Normal);
                CloseButton.Text = IonIconsFonts.CloseRound;

                //Create an Event
                Input.AfterTextChanged += (sender, e) => clickListener(new AddPollAdapterClickEventArgs { View = itemView, Position = AdapterPosition, Text= Input.Text , Input = Input });
                CloseButton.Click += (sender, e) => closeClickListener(new AddPollAdapterClickEventArgs { View = itemView, Position = AdapterPosition, Text = Input.Text });
                //itemView.Click += (sender, e) => clickListener(new AddPollAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        #region Variables Basic

        public View MainView { get; private set; }
        public TextView Number { get; private set; }
        public EditText Input { get; private set; }
        public Button CloseButton { get; private set; }

        #endregion
    }

    public class AddPollAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public string Text { get; set; }
        public EditText Input { get; set; }
    }
}