﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TorboFile.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class FolderCleanSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static FolderCleanSettings defaultInstance = ((FolderCleanSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new FolderCleanSettings())));
        
        public static FolderCleanSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool recursive {
            get {
                return ((bool)(this["recursive"]));
            }
            set {
                this["recursive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool moveToTrash {
            get {
                return ((bool)(this["moveToTrash"]));
            }
            set {
                this["moveToTrash"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string lastDirectory {
            get {
                return ((string)(this["lastDirectory"]));
            }
            set {
                this["lastDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool confirmDelete {
            get {
                return ((bool)(this["confirmDelete"]));
            }
            set {
                this["confirmDelete"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool saveLastDirectory {
            get {
                return ((bool)(this["saveLastDirectory"]));
            }
            set {
                this["saveLastDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool hasDeleteRange {
            get {
                return ((bool)(this["hasDeleteRange"]));
            }
            set {
                this["hasDeleteRange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool hasPreserveRange {
            get {
                return ((bool)(this["hasPreserveRange"]));
            }
            set {
                this["hasPreserveRange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Lemur.Types.DataRange deleteRange {
            get {
                return ((global::Lemur.Types.DataRange)(this["deleteRange"]));
            }
            set {
                this["deleteRange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Lemur.Types.DataRange preserveRange {
            get {
                return ((global::Lemur.Types.DataRange)(this["preserveRange"]));
            }
            set {
                this["preserveRange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool deleteEmptyFiles {
            get {
                return ((bool)(this["deleteEmptyFiles"]));
            }
            set {
                this["deleteEmptyFiles"] = value;
            }
        }
    }
}
