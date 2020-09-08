using inRiver.Remoting.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace inRiverCommunity.Extensions.Core.Settings
{
    /// <summary>
    /// This class contains methods when working with extension settings.
    /// </summary>
    public static class ExtensionSettings
    {


        #region GetSettings Methods

        /// <summary>
        /// Initializes or parses a settings dictionary to the settings class of your choosing.
        /// </summary>
        /// <typeparam name="T">Your settings class type</typeparam>
        /// <param name="settingsDictionary">A settings dictionary</param>
        /// <returns>An initialized settings class</returns>
        public static T GetSettings<T>(Dictionary<string, string> settingsDictionary = null)
        {
            return GetSettings<T>(null, settingsDictionary);
        }

        /// <summary>
        /// Initializes or parses a settings dictionary to the settings class of your choosing.
        /// </summary>
        /// <typeparam name="T">Your settings class type</typeparam>
        /// <param name="context">An inRiver context</param>
        /// <param name="settingsDictionary">A settings dictionary</param>
        /// <returns>An initialized settings class</returns>
        public static T GetSettings<T>(this inRiverContext context, Dictionary<string, string> settingsDictionary = null)
        {
            var settings = (T)Activator.CreateInstance(typeof(T));


            // TODO: Add try catch validation for each setting and gather the collective result for easier validation to the user


            // Get the settings dictionary from the context if the context is specified and the passed settings dictionary is null
            if (settingsDictionary == null && context != null)
                settingsDictionary = context.Settings;


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
                            if (context == null)
                                throw new Exception($"Cannot get the '{settingName}' value from the '{attribute.GetValueFromServerSetting}' server setting because the passed 'context' (inRiverContext) is null!");

                            try
                            {
                                value = context.ExtensionManager.UtilityService.GetServerSetting(attribute.GetValueFromServerSetting);
                            }
                            catch { }

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
                                    if (context == null)
                                        throw new Exception($"Cannot get the '{settingName}' value by falling back to the '{attribute.FallBackToServerSettingIfValueIsEmpty}' server setting because the passed 'context' (inRiverContext) is null!");

                                    try
                                    {
                                        value = context.ExtensionManager.UtilityService.GetServerSetting(attribute.GetValueFromServerSetting);
                                    }
                                    catch { }

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
                            if (attribute != null && attribute.JsonSerialized)
                            {
                                property.SetValue(settings, JsonConvert.DeserializeObject(value, property.PropertyType));
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                property.SetValue(settings, Enum.Parse(property.PropertyType, value));
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

                                property.SetValue(settings, list);
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
                                property.SetValue(settings, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                    }
                }
            }


            return settings;
        }


        #endregion

        #region GetSettingsAsDictionary Methods


        /// <summary>
        /// Converts your settings class/object to a settings dictionary.
        /// </summary>
        /// <typeparam name="T">Your settings class type</typeparam>
        /// <param name="context">An inRiver context</param>
        /// <param name="settingsObject">An initialized settings class</param>
        /// <returns>An initialized settings dictionary</returns>
        public static Dictionary<string, string> GetSettingsAsDictionary<T>(this inRiverContext context, T settingsObject = default)
        {
            // Create a new setting object if one wasn't passed
            if (EqualityComparer<T>.Default.Equals(settingsObject, default))
                settingsObject = (T)Activator.CreateInstance(typeof(T));


            return GetSettingsAsDictionary_Internal(settingsObject);
        }

        /// <summary>
        /// Converts your settings class/object to a settings dictionary.
        /// </summary>
        /// <typeparam name="T">Your settings class type</typeparam>
        /// <param name="settingsObject">An initialized settings class</param>
        /// <returns>An initialized settings dictionary</returns>
        public static Dictionary<string, string> GetSettingsAsDictionary<T>(T settingsObject = default)
        {
            // Create a new setting object if one wasn't passed
            if (EqualityComparer<T>.Default.Equals(settingsObject, default))
                settingsObject = (T)Activator.CreateInstance(typeof(T));


            return GetSettingsAsDictionary_Internal(settingsObject);
        }


        /// <summary>
        /// TODO: Document (If multiple types are passed with duplicate identical keys, the first default value is taken)
        /// </summary>
        /// <param name="context">An inRiver context</param>
        /// <param name="typeList"></param>
        /// <returns>An initialized settings dictionary of default values</returns>
        public static Dictionary<string, string> GetSettingsAsDictionary(this inRiverContext context, params Type[] typeList)
        {
            return GetSettingsAsDictionary(typeList);
        }


        /// <summary>
        /// TODO: Document (If multiple types are passed with duplicate identical keys, the first default value is taken)
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns>An initialized settings dictionary of default values</returns>
        public static Dictionary<string, string> GetSettingsAsDictionary(params Type[] typeList)
        {
            // Validate input
            if (typeList == null || typeList.Length == 0)
                return new Dictionary<string, string>();


            // Get settings dictionary for single passed type
            if (typeList.Length == 1)
                return GetSettingsAsDictionary_Internal(Activator.CreateInstance(typeList[0]));


            // Get settings dictionary for multiple passed types
            var dictionary = new Dictionary<string, string>();


            foreach (var type in typeList)
            {
                if (type == null)
                    continue;


                // Get type dictionary
                var typeDictionary = GetSettingsAsDictionary(type);

                if (typeDictionary == null || typeDictionary.Count == 0)
                    continue;


                // Add type dictionary item to root dictionary
                foreach (var key in typeDictionary.Keys)
                {
                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, typeDictionary[key]);
                }
            }


            return dictionary;
        }


        /// <summary>
        /// Converts your settings object to a settings dictionary.
        /// </summary>
        /// <param name="settingsObject">An initialized settings class object</param>
        /// <returns>An initialized settings dictionary</returns>
        private static Dictionary<string, string> GetSettingsAsDictionary_Internal(object settingsObject)
        {
            var dictionary = new Dictionary<string, string>();

            if (settingsObject == null)
                return dictionary;


            // Loop all setting properties
            foreach (var property in settingsObject.GetType().GetProperties())
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
                    if (attribute != null && attribute.JsonSerialized)
                    {
                        stringValue = JsonConvert.SerializeObject(value);
                    }
                    else if (typeof(List<string>).IsAssignableFrom(property.PropertyType))
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


        #endregion


    }
}
