using inRiver.Remoting.Extension;
using inRiverCommunity.Extensions.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inRiverCommunity.Extensions.TestConsoleApp
{
    class Program
    {


        public class One
        {
            public string MyString { get; set; } = "String value for One";

            public int MyInteger { get; set; } = 1;

            public string NoDefaultValue { get; set; }
        }

        public class Two
        {
            public string MyString { get; set; } = "String value for Two";

            public string Username { get; set; } = "Je suis Roy.";

            public bool DefaultBool { get; set; }
        }


        static void Main(string[] args)
        {
            var s1 = ExtensionSettings.GetSettingsAsDictionary<One>();

            var s2 = ExtensionSettings.GetSettingsAsDictionary(typeof(One));
            var s3 = ExtensionSettings.GetSettingsAsDictionary(typeof(One), typeof(Two));
            var s4 = ExtensionSettings.GetSettingsAsDictionary(typeof(Two), typeof(One));
            var s4a = ExtensionSettings.GetSettingsAsDictionary(new List<Type> { typeof(One), typeof(Two) });

            var one = new One();
            var s5 = ExtensionSettings.GetSettingsAsDictionary(one);

            one.MyString = "Hello World!";
            var s6 = ExtensionSettings.GetSettingsAsDictionary(one);


            var a1 = ExtensionSettings.GetSettings<One>(s3);
            var a2 = ExtensionSettings.GetSettings<Two>(s3);

            var a3 = ExtensionSettings.GetSettings<One>(s4);
            var a4 = ExtensionSettings.GetSettings<Two>(s4);

            var a5 = ExtensionSettings.GetSettings<One>();
        }


    }
}
