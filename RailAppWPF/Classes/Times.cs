using System.ComponentModel;

namespace RailAppWPF.Classes
{
    public class Times
    {

        public static BindingList<string> genTimes()
        {

            BindingList<string> time_elements = new();

            time_elements.Add("0000");
            time_elements.Add("0100");
            time_elements.Add("0200");
            time_elements.Add("0300");
            time_elements.Add("0400");
            time_elements.Add("0500");
            time_elements.Add("0600");
            time_elements.Add("0700");
            time_elements.Add("0800");
            time_elements.Add("0900");
            time_elements.Add("1000");
            time_elements.Add("1100");
            time_elements.Add("1200");
            time_elements.Add("1300");
            time_elements.Add("1400");
            time_elements.Add("1500");
            time_elements.Add("1600");
            time_elements.Add("1700");
            time_elements.Add("1800");
            time_elements.Add("1900");
            time_elements.Add("2000");
            time_elements.Add("2100");
            time_elements.Add("2200");
            time_elements.Add("2300");

            return time_elements;


        }


    }

}
