using MessageBox.Avalonia;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Localizations;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SpockWallet.ViewModels.SecurityModels
{
    public class SecurityWindowViewModel : ViewModelBase
    {
        public bool IsSuceess { get; set; }

        public delegate void PasswordEntered();

        public event PasswordEntered OnPasswordEntered;

        private string errorMsg;

        public string ErrorMsg
        {
            get { return errorMsg; }
            set
            {
                this.RaiseAndSetIfChanged(ref errorMsg, value);
            }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set
            {
                this.RaiseAndSetIfChanged(ref password, value);
            }
        }

        public void Enter()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {

                ErrorMsg = AppResources.PleaseInputYourPassword;

                return;
            }

            DataContext.DbPassword = Password;

            IsSuceess = TestPassword() || TestV1_0_5Password();

            if (!IsSuceess)
            {
                ErrorMsg = AppResources.InputPasswordIncorrect;

                return;
            }

            OnPasswordEntered?.Invoke();
        }

        private bool TestPassword()
        {
            try
            {
                var conn = new SQLiteConnection(@"Data Source=wallet.db;");
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = $"PRAGMA key = {Password};";
                command.ExecuteNonQuery();
                command.Dispose();

                var testQueryCommand = conn.CreateCommand();
                testQueryCommand.CommandText = "select COUNT() from sqlite_master where type='table'";
                testQueryCommand.ExecuteScalar();
                testQueryCommand.Dispose();

                conn.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TestV1_0_5Password()
        {
            try
            {
                var conn = new SQLiteConnection(@"Data Source=wallet.db;");
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = $"PRAGMA key = '{Password}';";
                command.ExecuteNonQuery();
                command.Dispose();

                var testQueryCommand = conn.CreateCommand();
                testQueryCommand.CommandText = "select COUNT() from sqlite_master where type='table'";
                testQueryCommand.ExecuteScalar();
                testQueryCommand.Dispose();

                conn.Close();

                DataContext.IsClientV_1_0_5 = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
