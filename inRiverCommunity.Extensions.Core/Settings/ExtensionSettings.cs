using inRiver.Remoting.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace inRiverCommunity.Extensions.Core.Settings
{
    // TODO: Document
    public static class ExtensionSettings
    {


        // TODO: Fallback to server setting for specified extension settings
        

        // TODO: Document
        public static T GetSettings<T>(this inRiverContext context, Dictionary<string, string> settingsDictionary = null)
        {
            // Validate input
            if (context == null)
                throw new Exception("The context cannot be null!");


            var settings = (T)Activator.CreateInstance(typeof(T));


            if (settingsDictionary != null && settingsDictionary.Count > 0)
            {
                foreach (var property in typeof(T).GetProperties())
                {
                    // Get setting name
                    var settingName = property.Name;
                    var attribute = property.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(ExtensionSettingAttribute)) as ExtensionSettingAttribute;

                    if (!string.IsNullOrEmpty(attribute?.Name))
                        settingName = attribute.Name;


                    if (!settingsDictionary.ContainsKey(settingName))
                    {
                        if (attribute != null && attribute.Mandatory)
                            throw new Exception($"The setting '{settingName}' is mandatory!");
                    }
                    else
                    {
                        string value = null;

                        // Get value from server setting
                        if (attribute != null && !string.IsNullOrEmpty(attribute.GetValueFromServerSetting))
                        {
                            value = context.ExtensionManager.UtilityService.GetServerSetting(attribute.GetValueFromServerSetting); // TODO: Test

                            if (attribute.Mandatory && string.IsNullOrEmpty(value))
                                throw new Exception($"The '{settingName}' property is mandatory and fetches its value from the '{attribute.GetValueFromServerSetting}' server setting, but the server setting value is null or empty!");
                        }
                        // Get value from string dictionary
                        else
                        {
                            // Get value from dictionary
                            value = settingsDictionary[settingName];

                            if (attribute != null && string.IsNullOrEmpty(value))
                            {
                                // Fall back to server setting because the value is empty
                                if (!string.IsNullOrEmpty(attribute.FallBackToServerSettingIfValueIsEmpty))
                                {
                                    value = context.ExtensionManager.UtilityService.GetServerSetting(attribute.GetValueFromServerSetting); // TODO: Test

                                    if (attribute.Mandatory && string.IsNullOrEmpty(value))
                                        throw new Exception($"The '{settingName}' property is mandatory and requires a value, the provided value and the fallback to the server setting '{attribute.GetValueFromServerSetting}' are both be null or empty!");
                                }
                                // Throw error if a value is mandatory
                                else if (attribute.Mandatory)
                                    throw new Exception($"The value for the setting '{settingName}' is mandatory cannot be null or empty!");
                            }
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            if (property.PropertyType.IsEnum)
                            {
                                property.SetValue(settings, Enum.Parse(property.PropertyType, value), null);
                            }
                            else if (typeof(List<string>).IsAssignableFrom(property.PropertyType))
                            {
                                if (string.IsNullOrEmpty(attribute?.CollectionDelimiter))
                                    throw new Exception($"You need to specify a 'CollectionDelimiter' value for the 'ExtensionSetting' attribute for the property '{settingName}'!");


                                var list = new List<string>();

                                foreach (var splitValue in value.Split(new string[] { attribute.CollectionDelimiter }, StringSplitOptions.None))
                                {
                                    var listValue = splitValue;


                                    // Trim value
                                    if (attribute.CollectionTrimValues && !string.IsNullOrEmpty(listValue))
                                        listValue = listValue.Trim();


                                    // Don't add empty values
                                    if (attribute.CollectionRemoveEmptyValues && string.IsNullOrEmpty(listValue))
                                        continue;


                                    list.Add(listValue);
                                }

                                property.SetValue(settings, list, null);
                            }
                            // TODO: Implement
                            //else if (property.PropertyType.IsArray)
                            //{
                            //    if (string.IsNullOrEmpty(attribute?.CollectionDelimiter))
                            //        throw new Exception($"You need to specify a 'CollectionDelimiter' value for the 'ExtensionSetting' attribute for the property '{settingName}'!");

                            //    property.SetValue(settings, value.Split(new string[] { attribute.CollectionDelimiter }, StringSplitOptions.None), null);
                            //}
                            else
                            {
                                property.SetValue(settings, Convert.ChangeType(value, property.PropertyType), null);
                            }
                        }
                    }
                }
            }


            return settings;
        }

        
        // TODO: Document
        public static Dictionary<string, string> GetSettingsAsDictionary<T>(this inRiverContext context, T settingsObject = default)
        {
            // Validate input
            if (context == null)
                throw new Exception("The context cannot be null!");


            var dictionary = new Dictionary<string, string>();


            // Create a new setting object if one wasn't passed
            if (EqualityComparer<T>.Default.Equals(settingsObject, default))
                settingsObject = (T)Activator.CreateInstance(typeof(T));


            // Loop all setting properties
            foreach (var property in typeof(T).GetProperties())
            {
                // Get setting name
                var settingName = property.Name;
                var attribute = property.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(ExtensionSettingAttribute)) as ExtensionSettingAttribute;

                if (!string.IsNullOrEmpty(attribute?.Name))
                    settingName = attribute.Name;


                // Get value
                string stringValue = null;
                var value = property.GetValue(settingsObject);

                if (value != null)
                {
                    if (typeof(List<string>).IsAssignableFrom(property.PropertyType))
                    {
                        if (string.IsNullOrEmpty(attribute?.CollectionDelimiter))
                            throw new Exception($"You need to specify a 'CollectionDelimiter' value for the 'ExtensionSetting' attribute for the property '{settingName}'!");


                        stringValue = string.Join(attribute.CollectionDelimiter, (List<string>)value);
                    }
                    // TODO: How do I implement this?
                    //else if (property.PropertyType.IsArray)
                    //{

                    //}
                    // Treat everything else as strings
                    else
                    {
                        stringValue = value.ToString();
                    }
                }


                // Throw an exception if the setting already exists
                if (dictionary.ContainsKey(settingName))
                    throw new Exception($"There already is a setting named '{settingName}'!");


                // Add value to dictionary
                dictionary.Add(settingName, stringValue);
            }


            return dictionary;
        }


    }
}
