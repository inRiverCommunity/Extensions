using inRiver.Remoting.Extension;
using inRiver.Remoting.Extension.Interface;
using inRiverCommunity.Extensions.Core.Settings;
using System;
using System.Collections.Generic;

namespace inRiverCommunity.Extensions.ExtensionSettingsSample
{
    public class MySampleExtension : IScheduledExtension
    {
        

        // This class represents the settings for your extension, the class must be public.
        public class MySettingsClass
        {
            // Each public property represents a setting.
            public string SampleString { get; set; }


            // You can set a default value for any supported data type.
            public string SampleStringWithDefaultValue { get; set; } = "My default value";


            // Enums are also supported...
            public MySampleEnum SampleEnum { get; set; }


            // ... along with many of the basic .NET data types work, if you find one that doesn't work feel free to submit an issue or create a pull request with a fix.
            public int SampleInteger { get; set; }


            // You can validate the input and throw exceptions when something is wrong.
            public int SampleIntegerBetween10And20
            {
                get
                {
                    return _sampleIntegerBetween10And20;
                }
                set
                {
                    if (value < 10 || value > 2)
                        throw new Exception("The 'SampleIntegerBetween10And20' setting needs to be between 10 and 20!");

                    _sampleIntegerBetween10And20 = value;
                }
            }

            private int _sampleIntegerBetween10And20; // You could also set a default value here if you want.


            // You can specify a different settings name if you don't want to or can use the property name.
            [ExtensionSetting(Name = "AlternativeSettingsNameForProperty")]
            public string SampleStringWithDifferentSettingsName { get; set; }

            // String lists are supported.
            [ExtensionSetting(
                CollectionDelimiter = ",", // It's mandatory to specify a delimiter for string lists.
                CollectionTrimValues = true, // Set this if you want to 'string.Trim()' entered values.
                CollectionRemoveEmptyValues = true // Set this if you want to remove empty values.
            )]
            public List<string> SampleEntityTypeIdList { get; set; } = new List<string> { "Product", "Item", "Resource" };


            // You can select to always get the value from a server setting.
            [ExtensionSetting(
                GetValueFromServerSetting = "SomeDatabaseConnectionString"
            )]
            public string SampleDatabaseConnectionString { get; set; }


            // You can also fall back to a server setting if the settings value is empty.
            [ExtensionSetting(
                FallBackToServerSettingIfValueIsEmpty = "SomeApiEndpoint"
            )]
            public string SampleApiEndpoint { get; set; }
        }
        

        public inRiverContext Context { get; set; }


        // The settings class and the default values can be transformed to a dictionary.
        public Dictionary<string, string> DefaultSettings => Context.GetSettingsAsDictionary<MySettingsClass>(); // This is a shorthand for 'ExtensionSettings.GetSettingsAsDictionary<SettingsClass>(Context);'.


        public void Execute(bool force)
        {
            // Parse your settings in a try-catch statement as parsing and validations may throw exceptions.
            MySettingsClass settings;
            
            try
            {
                settings = Context.GetSettings<MySettingsClass>(); // This is a shorthand for 'ExtensionSettings.GetSettings<SettingsClass>(Context);'.
            }
            catch (Exception ex)
            {
                // TODO: Log failure to load extension settings
                return;
            }


            // TODO: Implement your extension code and use the 'settings' variable to get your loaded settings.
        }


        public string Test()
        {
            // It's strongly recommended to load your settings in the test metod as a way of verifying that the settings can be loaded correctly.
            MySettingsClass settings;

            try
            {
                settings = Context.GetSettings<MySettingsClass>();
            }
            catch (Exception ex)
            {
                return $"Failed to parse extension settings: {ex.Message}";
            }


            return $"Settings loaded successfully.";
        }


    }
}
