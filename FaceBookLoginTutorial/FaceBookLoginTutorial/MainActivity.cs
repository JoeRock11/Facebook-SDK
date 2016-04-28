using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Java.Security;
using Xamarin.Facebook;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Facebook.Login;
using System.Collections.Generic;
using Android.Graphics;
using Java.Net;
using Android.Provider;
using Xamarin.Facebook.Share.Model;
using System.IO;
using Xamarin.Facebook.Share.Widget;
using Xamarin.Facebook.Share;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;

namespace FaceBookLoginTutorial
{
    [Activity(Label = "FaceBookLoginTutorial", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback
    {    
        private ICallbackManager mCallBackManager;
        private MyProfileTracker mProfileTracker;

        private TextView mTxtFirstName;
        private TextView mTxtLastName;
        private TextView mTxtName;
        private ProfilePictureView mProfilePic;
        private ShareButton mBtnShared;
        private Button mBtnGetEmail;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FacebookSdk.SdkInitialize(this.ApplicationContext);

            mProfileTracker = new MyProfileTracker();
            mProfileTracker.mOnProfileChanged += mProfileTracker_mOnProfileChanged;
            mProfileTracker.StartTracking();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button faceBookButton = FindViewById<Button>(Resource.Id.button);
            mTxtFirstName = FindViewById<TextView>(Resource.Id.txtFirstName);
            mTxtLastName = FindViewById<TextView>(Resource.Id.txtLastName);
            mTxtName = FindViewById<TextView>(Resource.Id.txtName);
            mProfilePic = FindViewById<ProfilePictureView>(Resource.Id.profilePic);
            mBtnShared = FindViewById<ShareButton>(Resource.Id.btnShare);
            mBtnGetEmail = FindViewById<Button>(Resource.Id.btnGetEmail);
                                  
            //if (AccessToken.CurrentAccessToken != null)
            //{
            //    //The user is logged in through Facebook
            //    faceBookButton.Text = "Logged Out";                
            //}
                               
            LoginButton button = FindViewById<LoginButton>(Resource.Id.login_button);

            button.SetReadPermissions(new List<string> { "public_profile", "user_friends", "email" });

            mCallBackManager = CallbackManagerFactory.Create();

            button.RegisterCallback(mCallBackManager, this);


            mBtnGetEmail.Click += (o, e) =>
            {
                GraphRequest request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);

                Bundle parameters = new Bundle();
                parameters.PutString("fields", "id,name,age_range,email");
                request.Parameters = parameters;
                request.ExecuteAsync();
            };

            //LoginManager.Instance.RegisterCallback(mCallBackManager, this);

            //faceBookButton.Click += (o, e) =>
            //{
            //    if (AccessToken.CurrentAccessToken != null)
            //    {
            //        //The user is logged in through Facebook
            //        LoginManager.Instance.LogOut();
            //        faceBookButton.Text = "My Facebook login button";
            //    }

            //    else
            //    {
            //        //The user is not logged in
            //        LoginManager.Instance.LogInWithReadPermissions(this, new List<string> { "public_profile", "user_friends" });
            //        faceBookButton.Text = "Logged Out";
            //    }

            //};

            ShareLinkContent content = new ShareLinkContent.Builder().Build();
            mBtnShared.ShareContent = content;
        }

        public void OnCompleted(Org.Json.JSONObject json, GraphResponse response)
        {
            string data = json.ToString();
            FacebookResult result = JsonConvert.DeserializeObject<FacebookResult>(data);            
        }

        void client_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void mProfileTracker_mOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            if (e.mProfile != null)
            {                               
                try
                {
                    mTxtFirstName.Text = e.mProfile.FirstName;
                    mTxtLastName.Text = e.mProfile.LastName;
                    mTxtName.Text = e.mProfile.Name;
                    mProfilePic.ProfileId = e.mProfile.Id;
                }

                catch (Exception ex)
                {
                    //Handle error
                }
            }
            
            else
            {
                //the user must have logged out
                mTxtFirstName.Text = "First Name";
                mTxtLastName.Text = "Last Name";
                mTxtName.Text = "Name";
                mProfilePic.ProfileId = null;
            }
        }

        public void OnCancel()
        {
            //throw new NotImplementedException();
        }

        public void OnError(FacebookException error)
        {
            //throw new NotImplementedException();
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            LoginResult loginResult = result as LoginResult;
            Console.WriteLine(AccessToken.CurrentAccessToken.UserId);       
        }

        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);          
            mCallBackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        protected override void OnDestroy()
        {
            mProfileTracker.StopTracking();
            base.OnDestroy();
        }       
    }

    public class MyProfileTracker : ProfileTracker
    {
        public event EventHandler<OnProfileChangedEventArgs> mOnProfileChanged;

        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
        {
            if (mOnProfileChanged != null)
            {
                mOnProfileChanged.Invoke(this, new OnProfileChangedEventArgs(newProfile));
            }
        }
    }

    public class OnProfileChangedEventArgs : EventArgs
    {
        public Profile mProfile;

        public OnProfileChangedEventArgs(Profile profile) { mProfile = profile; }
    }

}

