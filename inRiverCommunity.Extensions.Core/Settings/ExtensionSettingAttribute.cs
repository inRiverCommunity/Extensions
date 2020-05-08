using System;

namespace inRiverCommunity.Extensions.Core.Settings
{
    /// <summary>
    /// This class is used to set attributes on your extension settings class properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExtensionSettingAttribute : Attribute
    {
        
        /// <summary>
        /// Set this attribute if the name in Context.Settings is different from the property.
        /// </summary>
        public string Name;

        /// <summary>
        /// If the property value is mandatory, string.IsNullOrEmpty() is used. 
        /// </summary>
        public bool Mandatory;

        /// <summary>
        /// For collections this will be the delimiter that will be used.
        /// </summary>
        public string CollectionDelimiter;

        /// <summary>
        /// If the values should be trimmed, string.Trim() is used.
        /// </summary>
        public bool CollectionTrimValues;

        /// <summary>
        /// If empty collection values should be removed, string.IsNullOrEmpty() is used. 
        /// </summary>
        public bool CollectionRemoveEmptyValues;

        /// <summary>
        /// If the data is json serialized.
        /// </summary>
        public bool JsonSerialized;

        /// <summary>
        /// The value will always be fetched from this server setting, any value set in Context.Settings are ignored.
        /// </summary>
        public string GetValueFromServerSetting;

        /// <summary>
        /// If the value set in Context.Settings is empty or null, the value from this server setting (if any) will be used.
        /// </summary>
        public string FallBackToServerSettingIfValueIsEmpty;
        
    }
}
