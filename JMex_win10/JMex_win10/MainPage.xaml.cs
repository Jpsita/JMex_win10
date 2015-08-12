using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using Windows.UI.Text;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JMex_win10
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool isInConv { get; set; } = false;
        private bool logged = false;

        public List<conversation> convList = new List<conversation>();
        int lastMsgNumb = 0;
        private String currentfileName = "", lastFileName = "", user = "", currentConvName = "";

        public MainPage()
        {
            this.InitializeComponent();
            //showDialog();
            try
            {
                getMessages();
            }
            catch (AggregateException ae)
            {

            }
        }

        private void updateList()
        {
            lstConvs.Items.Clear();
            foreach(conversation i in convList)
            {
                lstConvs.Items.Add("#" + i.name);
            }
        }

        private async void showDialog()
        {
            ContentDialogResult res = await loginDialog.ShowAsync();
            if(res == ContentDialogResult.Secondary || res == ContentDialogResult.None)
            {
                Application.Current.Exit();
            }else if(res == ContentDialogResult.Primary)
            {
                doLogin();
            }
        }

        private async void doLogin()
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://jmex.altervista.org/login.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Credentials = CredentialCache.DefaultCredentials;
            String user = loginBox.Text.Trim();
            String psw = pswBox.Password.Trim();
            String msg = "login=Login&username=";
            String msg2 = user;
            String msg3 = "&password=";
            String msg4 = psw;
            String msg5 = "&client=Client";
            Stream s = await req.GetRequestStreamAsync();
            Byte[] data1 = Encoding.UTF8.GetBytes(msg);
            Byte[] data2 = Encoding.UTF8.GetBytes(msg2);
            Byte[] data3 = Encoding.UTF8.GetBytes(msg3);
            Byte[] data4 = Encoding.UTF8.GetBytes(msg4);
            Byte[] data5 = Encoding.UTF8.GetBytes(msg5);
            s.Write(data1, 0, data1.Length);
            s.Write(data2, 0, data2.Length);
            s.Write(data3, 0, data3.Length);
            s.Write(data4, 0, data4.Length);
            s.Write(data5, 0, data5.Length);
            s.Flush();
            HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync();
            String response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            if (response == "Succesfully logged.")
            {
                getConversations(user);
            }
        }

        private async void getConversations(String user)
        {
            logged = true;
            this.user = user;
            HttpWebRequest req = WebRequest.CreateHttp("http://jmex.altervista.org/getConversations.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Headers["X-Requested-With"] = "XMLHttpRequest";
            String usr = "user=" + user;
            Byte[] usrData = Encoding.UTF8.GetBytes(usr);
            Stream s = await req.GetRequestStreamAsync();
            s.Write(usrData, 0, usrData.Length);
            s.Flush();
            HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync();
            String response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            JsonArray array = JsonArray.Parse(response);
            //convText.Text += Environment.NewLine + response;
            int count = array.Count;
            convList.Clear();
            conversation general = new conversation();
            general.fileName = "demo_post.json";
            general.name = "general";
            convList.Add(general);
            for (uint i = 0; i < count; i++)
            {
                if (array.GetObjectAt(i).ValueType == JsonValueType.Object)
                {
                    JsonObject obj = array.GetObjectAt(i);
                    //convText.Text = Environment.NewLine + obj["filename"] + " / " + obj["name"];
                    conversation conv = new conversation();
                    conv.name = obj["name"].GetString();
                    conv.fileName = obj["filename"].GetString();
                    convList.Add(conv);
                }
            }
            updateList();
        }

        private void loginDialog_opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void  AddConvButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult res = await convDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                String x = convBox.Text.Trim();
                HttpWebRequest req = WebRequest.CreateHttp("http://jmex.altervista.org/addConversation.php");
                req.Method = "POST";
                req.UseDefaultCredentials = true;
                req.Headers["X-Requested-With"] = "XMLHttpRequest";
                req.ContentType = "application/x-www-form-urlencoded";
                Stream s = await req.GetRequestStreamAsync();
                String msg = "user=" + this.user + "&convname=" + x;
                Byte[] data = Encoding.UTF8.GetBytes(msg);
                s.Write(data, 0, data.Length);
                s.Flush();
#if DEBUG
                HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync();
                String y = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.WriteLine(y);
#endif
                getConversations(this.user);
            }

        }

        private void lstConvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(vsGroup.CurrentState == RelLayoutNarrowView)
            {
                VisualStateManager.GoToState(this, relLayoutNarrowViewMsg.Name, false);
            }
            String convName, filename;
            int selectedIndex = lstConvs.SelectedIndex;
            conversation selectedConv;
            try {
                selectedConv = convList[selectedIndex];
                convName = selectedConv.name;
                filename = selectedConv.fileName;
                currentfileName = filename;
                currentConvName = convName;
                if(vsGroup.CurrentState == relLayoutNarrowViewMsg)
                {
                    this.convName.Text = "#" + convName;
                }
            }
            catch(Exception ex)
            {
                messagesPanel.Children.Clear();
                currentConvName = "";
                currentfileName = "";
            }

        }

        private async Task getMessages()
        {
            while (true)
            {
                if (currentfileName != "") {
                    
                    String lastname = "";
                    HttpWebRequest req = WebRequest.CreateHttp("http://jmex.altervista.org/" + currentfileName);
                    req.Method = "GET";
                    HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync();
                    String x = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    JsonObject array = JsonObject.Parse(x);
                    JsonArray msgArray = array["messages"].GetArray();
                    if (msgArray.Count != lastMsgNumb || currentfileName != lastFileName)
                    {
                        messagesPanel.Children.Clear();
                        lastMsgNumb = msgArray.Count;
                        lastFileName = currentfileName;
                        for (uint i = 0; i < msgArray.Count; i++)
                        {
                            TextBlock txtB = new TextBlock();
                            txtB.HorizontalAlignment = HorizontalAlignment.Stretch;
                            JsonObject obj = msgArray.GetObjectAt(i);
                            String user = obj["user"].GetString();
                            String msg = obj["text"].GetString();
                            Span msgContainer = new Span();
                            txtB.Margin = new Thickness(0);
                            if (user != lastname)
                            {
                                
                                Run name = new Run();
                                
                                name.Text =user + Environment.NewLine;
                                name.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
                                if(user == this.user)
                                {
                                    FontWeight wh = new FontWeight();
                                    wh.Weight = 800;
                                    name.FontWeight = wh;
                                    
                                }
                                txtB.Margin = new Thickness(0, 8, 0, 0);
                                txtB.Inlines.Add(name);
                                lastname = user;
                            }
                            Run text = new Run();
                            text.Text = msg;
                            txtB.Inlines.Add(text );
                           
                            if(user == this.user)
                            {
                                txtB.TextAlignment = TextAlignment.Right;
                                
                            }
                            messagesPanel.Children.Add(txtB);
                        }
                    }
                }
                await Task.Delay(100);
            }
            
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (logged && txtMessage.Text != String.Empty)
            {
                HttpWebRequest req = WebRequest.CreateHttp("http://jmex.altervista.org/postmessage.php");
                req.Headers["X-Requested-With"] = "XMLHttpRequest";
                req.Method = "POST";
                req.UseDefaultCredentials = true;
                req.ContentType = "application/x-www-form-urlencoded";
                Stream s = await req.GetRequestStreamAsync();
                String msg = "action=postmessage&conv=" + this.currentConvName + "&user=" + this.user + "&text=" + txtMessage.Text;
                Byte[] data = Encoding.UTF8.GetBytes(msg);
                s.Write(data, 0, data.Length);
                s.Flush();
                txtMessage.Text = "";
                HttpWebResponse resp = (HttpWebResponse) await req.GetResponseAsync();
                String sh = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.WriteLine(sh);
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            loginDialog.Visibility = Visibility.Visible;
            showDialog();
        }

        private void goBack(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, RelLayoutNarrowView.Name, false);
        }

        public struct conversation
        {
            public String name { get; set; }
            public String fileName { get; set; }
        }
    }

}
