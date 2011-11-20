using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Xml.Linq;
using System.IO;
using System.Text;

using System.Runtime.Serialization.Json;
using System.ComponentModel;

namespace TwitterWP7
{
    public partial class MainPage : PhoneApplicationPage
    {

        #region Fields and Constants

        private const string TwitterSearchTweetURL = "http://twitter.com/statuses/user_timeline/{0}.{1}";
        private const string TwitterSearchHashtagURL = "http://search.twitter.com/search.json?q={0}";
        private const string XmlFormat = "xml";
        private WebClient twitterClient;
        IEnumerable<TwitterItem> tweets = null;

        #endregion

        public MainPage()
        {
            InitializeComponent();
            twitterClient = new WebClient();
            BtnFindTwitt.IsEnabled = !String.IsNullOrWhiteSpace(TxtTwitter.Text);
            BtnFindHashtag.IsEnabled = !String.IsNullOrWhiteSpace(TxtHashtag.Text);
        }

        protected void loadTweets(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TxtTwitter.Text))
            {
                MessageBox.Show("Do you need type twitter alias or hashtag before!");
                return;
            }

            Uri twitterAccountAddress = new Uri(BuildURLBasedOnTwitterAccount());
            twitterClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadTweetsFromTwitterAccountUsingXmlFormat);
            twitterClient.DownloadStringAsync(twitterAccountAddress);
        }

        protected void loadHashtag(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TxtHashtag.Text))
            {
                MessageBox.Show("Do you need type twitter alias or hastag before!");
                return;
            }

            Uri twitterHashtagSearchAddress = new Uri(BuildAddressForSerchingTwitterHashtag());
            twitterClient.OpenReadCompleted += new OpenReadCompletedEventHandler(DownloadTweetsBasedOnHashtagUsingJsonFormat);
            twitterClient.OpenReadAsync(twitterHashtagSearchAddress);

        }

        protected string BuildURLBasedOnTwitterAccount()
        {
            return String.Format(TwitterSearchTweetURL, TxtTwitter.Text.Trim(), XmlFormat);
        }

        protected string BuildAddressForSerchingTwitterHashtag()
        {
            return String.Format(TwitterSearchHashtagURL, TxtHashtag.Text);
        }

        protected void DownloadTweetsFromTwitterAccountUsingXmlFormat(object sender, DownloadStringCompletedEventArgs download)
        {
            try
            {
                if (HasErrors(download))
                    return;

                XElement xmlTweets = XElement.Parse(download.Result);
                tweets = from tweet in xmlTweets.Descendants("status")
                             select new TwitterItem
                             {
                                 ImageSource = tweet.Element("user").Element("profile_image_url").Value,
                                 Message = tweet.Element("text").Value,
                                 UserName = tweet.Element("user").Element("screen_name").Value
                             };

            }
            catch(Exception xmlRequestException)
            {
                WriteLog();
            }
            finally
            {
                lstTweets.ItemsSource = LoadNoItemsMessage(tweets);
            }
        }

        protected void DownloadTweetsBasedOnHashtagUsingJsonFormat(object sender, OpenReadCompletedEventArgs download)
        {
            TwitterResults twitterResults = null;
            try
            {
                if (HasErrors(download))
                    return;

                Stream stream = download.Result;
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(TwitterResults));
                twitterResults = (TwitterResults)dataContractJsonSerializer.ReadObject(stream);
                tweets = ConvertTwitterResultFromJsonSearchingToTwitterItems(twitterResults);

            }
            catch (Exception jsonRequestException)
            {
                WriteLog();
            }
            finally
            {
                lstTweets.ItemsSource = LoadNoItemsMessage(tweets);
            }
        }

        protected IEnumerable<TwitterItem> LoadNoItemsMessage(IEnumerable<TwitterItem> tweets)
        {
            if (tweets == null || tweets.Count() == 0)
                tweets = new TwitterItem[] { new TwitterItem() { Message = "No Items were found!" } };

            return tweets;
        }

        protected bool HasErrors(AsyncCompletedEventArgs e)
        {
            bool hasErrors = e.Error != null;
            if (hasErrors)
                MessageBox.Show("Falhou!!");

            return hasErrors;
        }

        protected static IEnumerable<TwitterItem> ConvertTwitterResultFromJsonSearchingToTwitterItems(TwitterResults twitterResults)
        {
            var twitterItems = from tweet in twitterResults.results
                               select new TwitterItem
                               {
                                   ImageSource = tweet.profile_image_url,
                                   Message = tweet.text,
                                   UserName = tweet.from_user
                               };
            return twitterItems;
        }

        #region Control Buttons

        private void EnableFindTweetButton(object sender, KeyEventArgs e)
        {
            TxtHashtag.Text = String.Empty;
            BtnFindTwitt.IsEnabled = TxtTwitter.Text.Length > 0;
            BtnFindHashtag.IsEnabled = false;
        }

        private void EnableFindHashtagButton(object sender, KeyEventArgs e)
        {
            TxtTwitter.Text = String.Empty;
            BtnFindHashtag.IsEnabled = TxtHashtag.Text.Length > 0;
            BtnFindTwitt.IsEnabled = false;
        }

        #endregion

        #region Logging

        protected void WriteLog()
        {
            MessageBox.Show("Opss! Looks like something is wrong, contact support.");
            // TODO - Check how to perform Logging using WP7
        }

        #endregion
    }
}
