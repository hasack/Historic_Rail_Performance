using System.Windows;
using RailAppWPF.Classes;

namespace RailAppWPF
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {

        public Preferences()
        {
            InitializeComponent();

            mySettings_decrypted appSettings = mySecurity.Pull_Settings();

            tb_username.Text = appSettings.User_name;
            api_user_name.Text = appSettings.Stored_api_username;
            api_user_password.Password = appSettings.Stored_api_password;
            tb_log_path.Text = appSettings.log_path;
            tb_export_path.Text = appSettings.export_path;

            ResizeMode = ResizeMode.CanMinimize;

        }

        private void btn_pref_save_Click(object sender, RoutedEventArgs e)
        {

            mySettings_decrypted appSettings = new mySettings_decrypted
            {

                User_name = tb_username.Text,
                Stored_api_username = api_user_name.Text,
                Stored_api_password = api_user_password.Password,
                log_path = tb_log_path.Text,
                export_path = tb_export_path.Text

            };

            mySecurity.Save_Settings(appSettings);

            this.Close();
        }

        private void btn_pref_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }


}
