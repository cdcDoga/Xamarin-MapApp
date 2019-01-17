using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace MapApp.ViewModals
{
    public class PickerViewModal : INotifyPropertyChanged
    {
        List<string> busses = new List<string>
        {
            "4-Kırmızı",
            "4-Mavi",
            "55-Siyah",
            "56-Siyah",
            "63-Kırmızı"
        };
        public List<string> Busses => busses;

        

        int bussesSelectedIndex;

        public int BussesSelectedIndex
        {
            get
            {
                return bussesSelectedIndex;
            }
            set
            {
                if (bussesSelectedIndex != value)
                {
                    bussesSelectedIndex = value;

                    // trigger some action to take such as updating other labels or fields
                    OnPropertyChanged(nameof(BussesSelectedIndex));
                    string selectedBus = Busses[bussesSelectedIndex];
                }
            }
        }

        protected virtual void OnPropertyChanged(string value = "null")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(value));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
