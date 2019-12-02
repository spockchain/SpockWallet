using MessageBox.Avalonia;
using ReactiveUI;
using SpockWallet.Data;
using SpockWallet.Localizations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpockWallet.ViewModels.SecurityModels
{
    public class SecuritySetWindowViewModel : ViewModelBase
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

        private string passwordAgain;

        public string PasswordAgain
        {
            get { return passwordAgain; }
            set
            {
                this.RaiseAndSetIfChanged(ref passwordAgain, value);
            }
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(PasswordAgain))
            {
                ErrorMsg = AppResources.PleaseSetYourPassword;

                return;
            }

            if (Password.Length < 8 || PasswordAgain.Length < 8)
            {
                ErrorMsg = AppResources.PasswordNotStrong;

                return;
            }

            if (Password != PasswordAgain)
            {

                ErrorMsg = AppResources.PasswordsDiffer;

                return;
            }

            DataContext.DbPassword = Password;
            DataContext.IsClientV_1_0_5 = true;

            IsSuceess = true;

            OnPasswordEntered?.Invoke();
        }
    }
}
