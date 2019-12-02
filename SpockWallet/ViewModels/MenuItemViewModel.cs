﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SpockWallet.ViewModels
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public IList<MenuItemViewModel> Items { get; set; }
    }
}
