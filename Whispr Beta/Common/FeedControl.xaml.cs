using System;
using System.Collections.Generic;
using System.Windows;
using WhisprBeta.Common;

namespace WhisprBeta
{
    public partial class FeedControl
    {
        /*
         * TODO: Perhaps get rid of immediate messages somehow. Can they be merged with just normal Messages and handled from there?
         */
        private const int NumOfImmediatePosts = 20;
        private const int NumOfNormalPosts = 50;

        public List<Message> Messages { get; private set; } 

        public FeedControl()
        {
            InitializeComponent();
            Messages = new List<Message>();
        }

        public void Initialize()
        {
            for (var i = 0; i < NumOfImmediatePosts + NumOfNormalPosts; i++)
            {
                stackPanelScroll.Children.Add(new MessageControl());
            }
            ImmediateListInit();
        }
        public void UpdateFeed(List<Message> whisprs)
        {
            /* There is a constant amount of stackPanel children (defined somewhere at the top of the file).
             * Let's say it's 50. If the amount of Whisprs loaded is less than that, then the extra children
             * are simply collapsed. If the amount is bigger than 50, well, then the latest 50 are displayed and
             * all the children are visible.
             * 
             * In the loop below, go trough stackPanel children and place Latest Whisprs in them. If there is
             * not enough Whisprs to fill the entire stackPanel, collapse the rest of the children.
             */
            Messages = whisprs;
            for (var i = NumOfImmediatePosts; i < stackPanelScroll.Children.Count; i++)
            {
                var postUi = (MessageControl)stackPanelScroll.Children[i];
                var whispIdx = i - NumOfImmediatePosts;
                if (whispIdx < whisprs.Count)
                {
                    // There are still whisprs to display. Display the next one:
                    Message message = whisprs[whispIdx];
                    if (postUi.Id != message.id)
                    {
                        postUi.Id = message.id;
                        postUi.Text = message.text;
                        postUi.UnixTimeStamp = message.pubTime;
                    }
                    postUi.UpdatePublishTimeText();
                    postUi.IsImmediate = false;
                    postUi.Show();
                }
                else
                {
                    // There are no more Whirps to display. Hide this and all further stackPanel children
                    postUi.Hide();
                }
            }
            if (whisprs.Count > 0)
            {
                ImmediateListCheck(whisprs);
            }
        }

        public void OutdateFeed()
        {
            foreach (UIElement whisprUiElement in stackPanelScroll.Children)
            {
                ((MessageControl)whisprUiElement).IsOutdated = true;
            }
        }

        #region Immediate list
        /* Immediate list is a list of messages that have just been posted from this phone, but were not yet synchronized with
         * the server. When message is placed on the immediate list, it is instantly display in the feed of the user who posted
         * it, while being sent to the server. Once the server is synchronized and the server returns the same message back to
         * the phone, the message is removed from the immediate list.
         */
        private List<Message> immediateList;

        private void ImmediateListInit()
        {
            immediateList = new List<Message>();
        }

        public void ImmediateListAdd(Message message)
        {
            immediateList.Add(message);
            ImmediateListUpdate();
        }

        private void ImmediateListUpdate()
        {
            for (var i = 0; i < NumOfImmediatePosts; i++)
            {
                MessageControl postUi = (MessageControl)stackPanelScroll.Children[i];
                if (i < immediateList.Count)
                {
                    Message message = immediateList[i];
                    postUi.Text = message.text;
                    postUi.IsImmediate = true;
                    postUi.Show();
                }
                else
                {
                    postUi.Hide();
                }
            }
        }

        private void ImmediateListCheck(List<Message> whisprs)
        {
            lock (immediateList)
            {
                if (immediateList.Count > 0)
                {
                    List<Message> postsToRemove = new List<Message>();
                    foreach (Message immediatePost in immediateList)
                    {
                        foreach (Message receivedPost in whisprs)
                        {
                            if (receivedPost.text == immediatePost.text)
                            {
                                // This message has been received from server, so remove it from the immediate list
                                postsToRemove.Add(immediatePost);
                                break;
                            }
                        }
                    }
                    foreach (Message postToRemove in postsToRemove)
                    {
                        immediateList.Remove(postToRemove);
                    }
                    // Update status if anything has changed
                    if (postsToRemove.Count > 0)
                    {
                        ImmediateListUpdate();
                    }
                }
            }
        }
        #endregion

    }
}
