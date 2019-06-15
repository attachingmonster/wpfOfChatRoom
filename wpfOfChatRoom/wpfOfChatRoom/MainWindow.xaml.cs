using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpfOfChatRoom.DAL;
using wpfOfChatRoom.Methods;
using wpfOfChatRoom.Model;
using wpfOfChatRoom.ViewModels;

namespace wpfOfChatRoom
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var data = unitOfWork.DataRepository.Get();                     
        }
        
     
        #region 成员定义
        AccountContext db = new AccountContext();//数据库上下文实例
        UnitOfWork unitOfWork = new UnitOfWork();//单元工厂实例      
        HttpClient client = new HttpClient();
        String User;//登录的用户
        #endregion

        #region 登录界面事件

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var user = unitOfWork.DataRepository.Get();         //combobox的更新
            cbxUserAccountLogin.ItemsSource = user.ToList();       //combobox数据源连接数据库
            cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
            cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
            cbxUserAccountLogin.SelectedIndex = 0;               //登陆界面 combobox初始显示第一项
            var u = user.Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();
            if (u != null)
            {
                if (u.RememberPassword == "1")              //判断该对象的 记住密码 是否为 已选
                {
                    pbxUserPasswordLogin.Password = CreateMD5.EncryptWithMD5(u.UserPassword);//给passwordbox一串固定密码
                    cheRememberPwdLogin.IsChecked = true;     //让记住密码选择框显示选中
                }
            }
        }

        private void comboBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)//登录界面上combobox的值改变事件
        {
            pbxUserPasswordLogin.Password = "";
        }

        private void loginChangePwd_Click(object sender, RoutedEventArgs e)//登录界面的修改密码按钮事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            ChangePasswordWindow.Visibility = Visibility.Visible;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)//登录按钮事件
        {
            try
            {
                if (cbxUserAccountLogin.Text != "")//判断账号是否为空
                {
                    if (pbxUserPasswordLogin.Password != "")//判断密码是否为空
                    {
                        ViewModelLogin viewModelLogin = new ViewModelLogin();
                        viewModelLogin.Account = cbxUserAccountLogin.Text;
                        viewModelLogin.Password = CreateMD5.EncryptWithMD5(pbxUserPasswordLogin.Password);
                        if (cheRememberPwdLogin.IsChecked == true)
                        {
                            viewModelLogin.RememberPassword = "1";
                        }
                        else
                        {
                            viewModelLogin.RememberPassword = "0";
                        }
                        //传输登录信息到webapi
                        ViewModelInformation viewModelInformation = new ViewModelInformation();
                        viewModelInformation = await LoginView(viewModelLogin);
                        MessageBox.Show(viewModelInformation.Message);
                        if (viewModelInformation.Message == "登录成功")
                        {
                            User = cbxUserAccountLogin.Text;

                            //判断本地数据库是否存在账号
                            var sysUser = unitOfWork.DataRepository.Get().Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();
                            if (sysUser == null)
                            {
                                var CurrentData = new Data();
                                CurrentData.UserAccount = cbxUserAccountLogin.Text;
                                CurrentData.UserPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordLogin.Password);
                                CurrentData.RememberPassword = viewModelLogin.RememberPassword;
                                unitOfWork.DataRepository.Insert(CurrentData);    //增加新SysUser
                                unitOfWork.Save();
                            }
                            else
                            {
                                var users = unitOfWork.DataRepository.Get().Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();
                                if (users != null)
                                {
                                    users.RememberPassword = viewModelLogin.RememberPassword;
                                    unitOfWork.Save();
                                }
                            }

                            var ur = unitOfWork.DataRepository.Get();
                            var u = ur.Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();                          
                            if (pbxUserPasswordLogin.Password == "123" || pbxUserPasswordLogin.Password == CreateMD5.EncryptWithMD5("123")|| pbxUserPasswordLogin.Password == CreateMD5.EncryptWithMD5(CreateMD5.EncryptWithMD5("123")))
                            {
                                MessageBox.Show("按“确认”进入修改密码界面");
                                LoginWindow.Visibility = Visibility.Collapsed;
                                ChangePasswordWindow.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                LoginWindow.Visibility = Visibility.Collapsed;
                                ChatWindow.Visibility = Visibility.Visible;
                                UserMessage.Content = User;
                                Height = 583.38;
                                Width = 629.545;
                                //初始化聊天界面
                                ViewModel viewModel = new ViewModel();
                                viewModel.Message = "1";//给个固定可以传到webapi的参数
                                var viewModelDataReturn = await DataModelReturn(viewModel);
                                ReceiveText.Text = "";
                                foreach (var v in viewModelDataReturn)
                                {
                                    ReceiveText.AppendText(v.UserAccount + ":" + "\n" + v.Message + "\n" + "\n");
                                }
                                ReceiveText.ScrollToEnd();
                            }
                        }
                       
                       
                       
                        var user = unitOfWork.DataRepository.Get();         //combobox的更新
                        cbxUserAccountLogin.ItemsSource = user.ToList();       //combobox数据源连接数据库
                        cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
                        cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
                        cbxUserAccountLogin.SelectedIndex = 0;
                    }
                    else
                    {
                        throw new Exception("密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("账号不能为空！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录失败！错误信息：\n" + ex.Message+"网络错误");
            }
        }


        /// <summary>
        /// 登录信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> LoginView(ViewModelLogin viewModelLogin)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                /*
                var response = await client.PostAsJsonAsync("https://localhost:44311/api/Login/PostLogin", viewModelLogin);
                response.EnsureSuccessStatusCode();
                

                var response = await client.PostAsJsonAsync("http://212.64.121.213/api/Login/PostLogin", viewModelLogin);
                response.EnsureSuccessStatusCode();
                */
                var response = await client.PostAsJsonAsync("http://www.kakazz.cn/api/Login/PostLogin", viewModelLogin);
                response.EnsureSuccessStatusCode();
                
                viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation == null)
                {
                    viewModelInformation.Message = "网络错误";
                    return viewModelInformation;
                }
                else
                {
                    return viewModelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                //后续保存到数据库里，另外再续返回到webapi的数据库里备查
                viewModelInformation.Message = ex.Message;
                return viewModelInformation;
            }
            catch (System.FormatException)
            {
                return viewModelInformation;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)//退出系统
        {
            SystemCommands.CloseWindow(this);
        }

        private void Min_Click(object sender, RoutedEventArgs e)//缩小窗口
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void Move_window(object sender, MouseButtonEventArgs e)//移动窗口
        {
            this.DragMove();
        }
        #endregion

        #region 修改密码界面事件
        private async void ChangePsw_Click(object sender, RoutedEventArgs e)//修改密码事件
        {
            if (tbxOldPasswordChangePwd.Visibility != Visibility.Collapsed)    //如果显示密码按钮事件开始，则让显示密码值赋值给隐藏密码的值
            {
                pbxOldPasswordChangePwd.Password = tbxOldPasswordChangePwd.Text;
                pbxUserPasswordChangePwd.Password = tbxUserPasswordChangePwd.Text;
                pbxSurePasswordChangePwd.Password = tbxSurePasswordChangePwd.Text;
            }
            try
            {
                if (tbxUserAccountChangePwd.Text != "")
                {
                    if (pbxOldPasswordChangePwd.Password != "")
                    {
                        if (pbxUserPasswordChangePwd.Password != "")
                        {
                            int number = 0, character = 0;
                            foreach (char c in pbxUserPasswordChangePwd.Password)   //规范密码必须由ASCII码33~126之间的字符构成
                            {
                                if (!(33 <= c && c <= 126))
                                {
                                    throw new Exception("符号错误，请重新输入！");
                                }
                                if ('0' <= c && c <= '9') //number记录数字个数
                                {
                                    number++;
                                }
                                else                      //character记录字符个数
                                {
                                    character++;
                                }
                            }
                            if (number < 5 || character < 2)  //密码的安全系数
                            {
                                throw new Exception("新密码安全系数太低！");
                            }
                            if (pbxOldPasswordChangePwd.Password != pbxUserPasswordChangePwd.Password)
                            {
                                if (pbxSurePasswordChangePwd.Password != "")
                                {
                                    if (pbxUserPasswordChangePwd.Password == pbxSurePasswordChangePwd.Password)
                                    {
                                        ViewModelChangePsw viewModelChangePsw = new ViewModelChangePsw();
                                        viewModelChangePsw.Account = tbxUserAccountChangePwd.Text;
                                        viewModelChangePsw.OldPassword = CreateMD5.EncryptWithMD5(pbxOldPasswordChangePwd.Password);
                                        viewModelChangePsw.NewPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordChangePwd.Password);
                                        viewModelChangePsw.SurePassword = CreateMD5.EncryptWithMD5(pbxSurePasswordChangePwd.Password);
                                        ViewModelInformation viewModelInformation = new ViewModelInformation();
                                        viewModelInformation = await ChangePswView(viewModelChangePsw);
                                        MessageBox.Show(viewModelInformation.Message);
                                        if(viewModelInformation.Message== "修改成功！")
                                        {
                                            //返回登录界面
                                            ChangePasswordWindow.Visibility = Visibility.Collapsed;
                                            LoginWindow.Visibility = Visibility.Visible;                                         

                                            //更新本地数据库
                                            var user = unitOfWork.DataRepository.Get();
                                            var u = user.Where(s => s.UserAccount.Equals(tbxUserAccountChangePwd.Text)).FirstOrDefault();
                                            if(u!=null)
                                            {
                                                u.UserPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordChangePwd.Password);
                                                unitOfWork.Save();
                                            }

                                            tbxUserAccountChangePwd.Text = "";
                                            pbxOldPasswordChangePwd.Password = "";
                                            pbxUserPasswordChangePwd.Password = "";
                                            pbxSurePasswordChangePwd.Password = "";
                                            tbxOldPasswordChangePwd.Text = "";
                                            tbxUserPasswordChangePwd.Text = "";
                                            tbxSurePasswordChangePwd.Text = "";
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("两次输入的密码不一致！");
                                    }
                                }
                                else
                                {
                                    throw new Exception("确认密码不能为空！");
                                }
                            }
                            else
                            {
                                throw new Exception("新密码与原密码不能相同！");
                            }

                        }
                        else
                        {
                            throw new Exception("新密码不能为空");
                        }

                    }
                    else
                    {
                        throw new Exception("原密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("账号不能为空！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改密码失败！错误信息：\n" + ex.Message+"网络错误");
            }

        }

        private void ShowChangePassword_Check(object sender, RoutedEventArgs e)//修改密码界面中显示密码事件
        {
            tbxOldPasswordChangePwd.Visibility = Visibility.Visible;
            pbxOldPasswordChangePwd.Visibility = Visibility.Collapsed;
            tbxOldPasswordChangePwd.Text = pbxOldPasswordChangePwd.Password;

            tbxUserPasswordChangePwd.Visibility = Visibility.Visible;
            pbxUserPasswordChangePwd.Visibility = Visibility.Collapsed;
            tbxUserPasswordChangePwd.Text = pbxUserPasswordChangePwd.Password;

            tbxSurePasswordChangePwd.Visibility = Visibility.Visible;
            pbxSurePasswordChangePwd.Visibility = Visibility.Collapsed;
            tbxSurePasswordChangePwd.Text = pbxSurePasswordChangePwd.Password;
        }

        private void HiddenChangePassword_Check(object sender, RoutedEventArgs e)//修改密码界面中隐藏密码事件
        {
            tbxOldPasswordChangePwd.Visibility = Visibility.Collapsed;
            pbxOldPasswordChangePwd.Visibility = Visibility.Visible;
            pbxOldPasswordChangePwd.Password = tbxOldPasswordChangePwd.Text;

            tbxUserPasswordChangePwd.Visibility = Visibility.Collapsed;
            pbxUserPasswordChangePwd.Visibility = Visibility.Visible;
            pbxUserPasswordChangePwd.Password = tbxUserPasswordChangePwd.Text;

            tbxSurePasswordChangePwd.Visibility = Visibility.Collapsed;
            pbxSurePasswordChangePwd.Visibility = Visibility.Visible;
            pbxSurePasswordChangePwd.Password = tbxSurePasswordChangePwd.Text;
        }


        private void changePwdBack_click(object sender, RoutedEventArgs e)//修改密码界面的返回事件
        {
            ChangePasswordWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// 修改密码信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> ChangePswView(ViewModelChangePsw viewModelChangePsw)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                /*
                var response = await client.PostAsJsonAsync("https://localhost:44311/api/ChangePsw/PostChangePsw", viewModelChangePsw);
                response.EnsureSuccessStatusCode();
                */
                var response = await client.PostAsJsonAsync("http://www.kakazz.cn/api/ChangePsw/PostChangePsw", viewModelChangePsw);
                response.EnsureSuccessStatusCode();
                             
                viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation == null)
                {
                    viewModelInformation.Message = "网络错误";
                    return viewModelInformation;
                }
                else
                {
                    return viewModelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                //后续保存到数据库里，另外再续返回到webapi的数据库里备查
                viewModelInformation.Message = ex.Message;
                return viewModelInformation;
            }
            catch (System.FormatException)
            {
                return viewModelInformation;
            }
        }




        #endregion

        private void CbxUserAccountLogin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var users = unitOfWork.DataRepository.Get();
            if (cbxUserAccountLogin.SelectedValue != null)//获取combobox选择的值
            {
                var sysUser = users.Where(s => s.UserAccount.Equals(cbxUserAccountLogin.SelectedValue.ToString())).FirstOrDefault();
                if (sysUser != null)
                {
                    //combobox选中的账号是否记忆密码
                    if (sysUser.RememberPassword == "1")
                    {
                        pbxUserPasswordLogin.Password = CreateMD5.EncryptWithMD5(sysUser.UserPassword);//给passwordbox一串固定密码
                        //pbxUserPasswordLogin.Password = CreateMD5.EncryptWithMD5("12345");//给passwordbox一串固定密码
                        cheRememberPwdLogin.IsChecked = true;
                    }
                    else
                    {
                        pbxUserPasswordLogin.Password = "";
                        cheRememberPwdLogin.IsChecked = false;
                    }
                }
            }
        }

        #region 聊天界面事件
        private void ChatWindowMin_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void ChatWindowClose_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private async void Sent_Click(object sender, RoutedEventArgs e)//聊天界面上的发送按钮事件
        {           
            //ReceiveText.AppendText(cbxUserAccountLogin.Text + ":" + "\n" + SentText.Text + "\n" + "\n");
            //ReceiveText.ScrollToEnd();
            ViewModelData viewModelData = new ViewModelData();
            viewModelData.UserAccount = User;
            viewModelData.Message = SentText.Text;
            var viewModelDataReturn = await DataModel(viewModelData);
            ReceiveText.Text = "";
            foreach (var v in viewModelDataReturn)
            {            
                ReceiveText.AppendText(v.UserAccount+":"+"\n"+v.Message + "\n" + "\n");
            }       
            ReceiveText.ScrollToEnd();
        }

        private async void UpdateMessage_Click(object sender, RoutedEventArgs e)//聊天界面上的刷新消息按钮事件
        {
            ViewModel viewModel = new ViewModel();
            viewModel.Message = "1";//给个固定可以传到webapi的参数
            var viewModelDataReturn = await DataModelReturn(viewModel);
            ReceiveText.Text = "";
            foreach (var v in viewModelDataReturn)
            {
                ReceiveText.AppendText(v.UserAccount + ":" + "\n" + v.Message + "\n" + "\n");
            }
            ReceiveText.ScrollToEnd();          
        }

        //用post向webapi传递消息的同时从webapi返回消息到消息框
        private async Task<List<ViewModelDataReturn>> DataModel(ViewModelData viewModelData)
        {
            //Post异步提交信息，格式为Json
            /*
            var response = await client.PostAsJsonAsync("https://localhost:44311/api/DataModel/PostDataModel", viewModelData);
            response.EnsureSuccessStatusCode();
            */
            
            var response = await client.PostAsJsonAsync("http://www.kakazz.cn/api/DataModel/PostDataModel", viewModelData);
            response.EnsureSuccessStatusCode();
           
            var viewModelDataReturn = await response.Content.ReadAsAsync<List<ViewModelDataReturn>>();
            return viewModelDataReturn;
        }
        
        //用get从webapi得到消息
        /*
        public async Task<List<ViewModelDataReturn>> ViewModelData()
        {
            var response = await client.GetAsync("https://localhost:44311/api/DataModelReturn/GetDataModel");
            response.EnsureSuccessStatusCode();
            
            var response = await client.GetAsync("http://www.jajazz.club/api/DataModelReturn/GetDataModel");
            response.EnsureSuccessStatusCode();
            
            var viewModelDataReturn = await response.Content.ReadAsAsync<List<ViewModelDataReturn>>();
            return viewModelDataReturn;
        }
       */
        //用post从webapi得到用户聊天的数据
        private async Task<List<ViewModelDataReturn>> DataModelReturn(ViewModel viewModel)
        {
            //Post异步提交信息，格式为Json                
            var response = await client.PostAsJsonAsync("http://www.kakazz.cn/api/DataModelReturn/PostDataModelReturn", viewModel);
            response.EnsureSuccessStatusCode();
          
            var viewModelDataReturn = await response.Content.ReadAsAsync<List<ViewModelDataReturn>>();
            return viewModelDataReturn;
        }

        #endregion

        
        

    }
}
