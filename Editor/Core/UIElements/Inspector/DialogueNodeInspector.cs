using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Editor.Graph;
using Chris.Serialization;
using Chris.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace NextGenDialogue.Graph.Editor
{
    /// <summary>
    /// Inspector for displaying and editing container node modules using IMGUI
    /// Only draws fields that have FieldResolver
    /// </summary>
    public class DialogueNodeInspector : IDisposable
    {
        private readonly ContainerNodeView _containerView;

        private readonly List<ModuleInfo> _moduleInfos;

        /// <summary>
        /// Information about a module for inspector display
        /// </summary>
        public class ModuleInfo
        {
            public ModuleNodeView ModuleView;

            public SerializedObjectWrapper Wrapper;

            public SoftObjectHandle Handle;

            public Type ModuleType;

            public HashSet<string> AllowedFieldNames;

            public Dictionary<string, FieldInfo> FieldInfoMap;

            /// <summary>
            /// Get PieceID fields from this module
            /// </summary>
            public IEnumerable<PieceIDFieldInfo> GetPieceIdFields()
            {
                foreach (var fieldName in AllowedFieldNames)
                {
                    if (!FieldInfoMap.TryGetValue(fieldName, out var fieldInfo)) continue;
                    if (fieldInfo.FieldType != typeof(PieceID)) continue;

                    // Get current value from module
                    var currentValue = Wrapper.Value != null 
                        ? fieldInfo.GetValue(Wrapper.Value) as PieceID 
                        : new PieceID();

                    yield return new PieceIDFieldInfo
                    {
                        FieldName = fieldName,
                        FieldInfo = fieldInfo,
                        CurrentValue = currentValue ?? new PieceID(),
                        ModuleInfo = this
                    };
                }
            }

            /// <summary>
            /// Check if module has non-PieceID fields
            /// </summary>
            public bool HasNonPieceIdFields()
            {
                return AllowedFieldNames.Any(fieldName =>
                {
                    if (!FieldInfoMap.TryGetValue(fieldName, out var fieldInfo)) return false;
                    return fieldInfo.FieldType != typeof(PieceID);
                });
            }
        }

        /// <summary>
        /// Information about a PieceID field
        /// </summary>
        public class PieceIDFieldInfo
        {
            public string FieldName;
            public FieldInfo FieldInfo;
            public PieceID CurrentValue;
            public ModuleInfo ModuleInfo;

            /// <summary>
            /// Update the PieceID value in the module
            /// </summary>
            public void SetValue(PieceID newValue)
            {
                if (ModuleInfo.Wrapper.Value == null) return;
                FieldInfo.SetValue(ModuleInfo.Wrapper.Value, newValue);
                SyncToFieldResolver();
            }

            /// <summary>
            /// Sync the value back to the FieldResolver
            /// </summary>
            private void SyncToFieldResolver()
            {
                var resolvers = ModuleInfo.ModuleView.GetAllFieldResolvers();
                foreach (var (resolver, fieldInfo) in resolvers)
                {
                    if (fieldInfo.Name != FieldName) continue;
                    var fieldValue = fieldInfo.GetValue(ModuleInfo.Wrapper.Value);
                    resolver.Value = fieldValue;
                    break;
                }
            }
        }

        /// <summary>
        /// Create inspector for the given container view
        /// </summary>
        /// <param name="containerView">Container view to inspect</param>
        public DialogueNodeInspector(ContainerNodeView containerView)
        {
            _containerView = containerView ?? throw new ArgumentNullException(nameof(containerView));
            _moduleInfos = new List<ModuleInfo>();
            InitializeModuleWrappers();
        }

        /// <summary>
        /// Get all module infos for external UIElement rendering
        /// </summary>
        public IReadOnlyList<ModuleInfo> GetModuleInfos()
        {
            return _moduleInfos;
        }

        /// <summary>
        /// Initialize wrappers for all module node views
        /// </summary>
        private void InitializeModuleWrappers()
        {
            var moduleViews = _containerView.GetAllModuleViews();

            foreach (var moduleView in moduleViews)
            {
                try
                {
                    // Build allowed field names and field info map from FieldResolvers
                    var allowedFieldNames = new HashSet<string>();
                    var fieldInfoMap = new Dictionary<string, FieldInfo>();
                    foreach (var (_, fieldInfo) in moduleView.GetAllFieldResolvers())
                    {
                        allowedFieldNames.Add(fieldInfo.Name);
                        fieldInfoMap[fieldInfo.Name] = fieldInfo;
                    }

                    // Compile module to get Module instance
                    var moduleInstance = CompileModule(moduleView);
                    if (moduleInstance == null) continue;

                    var moduleType = moduleInstance.GetType();

                    // Create wrapper for entire Module object
                    var handle = new SoftObjectHandle();
                    var wrapper = SerializedObjectWrapperManager.CreateWrapper(moduleType, ref handle);
                    wrapper.Value = moduleInstance;

                    var moduleInfo = new ModuleInfo
                    {
                        ModuleView = moduleView,
                        Wrapper = wrapper,
                        Handle = handle,
                        ModuleType = moduleType,
                        AllowedFieldNames = allowedFieldNames,
                        FieldInfoMap = fieldInfoMap
                    };

                    _moduleInfos.Add(moduleInfo);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create wrapper for module {moduleView.NodeType.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Compile module view to get Module instance
        /// Similar to ExecutableNodeInspector.CompilePort
        /// </summary>
        /// <param name="moduleView">Module view to compile</param>
        /// <returns>Module instance</returns>
        private static Module CompileModule(ModuleNodeView moduleView)
        {
            try
            {
                var moduleType = moduleView.NodeType;
                
                // Create module instance
                var moduleInstance = (Module)Activator.CreateInstance(moduleType);

                // Commit current values from FieldResolvers to module
                foreach (var (resolver, _) in moduleView.GetAllFieldResolvers())
                {
                    resolver.Commit(moduleInstance);
                }

                return moduleInstance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to compile module {moduleView.NodeType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Draw all modules using IMGUI
        /// Called from IMGUIContainer.onGUIHandler
        /// </summary>
        public void OnGUI()
        {
            if (_moduleInfos.Count == 0)
            {
                EditorGUILayout.HelpBox("No modules in this container", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical();

            foreach (var moduleInfo in _moduleInfos)
            {
                DrawModuleSection(moduleInfo);
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a single module section
        /// </summary>
        /// <param name="moduleInfo">Module info to draw</param>
        private static void DrawModuleSection(ModuleInfo moduleInfo)
        {
            if (!moduleInfo.Wrapper)
            {
                EditorGUILayout.HelpBox($"Module {moduleInfo.ModuleType.Name} wrapper is invalid", MessageType.Warning);
                return;
            }

            // Draw module header
            var moduleName = CeresLabel.GetLabel(moduleInfo.ModuleType);
            EditorGUILayout.LabelField(moduleName, EditorStyles.boldLabel);

            // Draw module fields filtered by FieldResolvers
            if (moduleInfo.AllowedFieldNames.Count > 0)
            {
                DrawFilteredModuleFields(moduleInfo);
            }
            else
            {
                EditorGUILayout.LabelField("No fields to display", EditorStyles.helpBox);
            }
        }

        /// <summary>
        /// Draw module fields filtered by allowed field names
        /// </summary>
        /// <param name="moduleInfo">Module info</param>
        /// <param name="skipPieceIdFields">Whether to skip PieceID fields (they are rendered with UIElements)</param>
        private static void DrawFilteredModuleFields(ModuleInfo moduleInfo, bool skipPieceIdFields = false)
        {
            using var serializedObject = new SerializedObject(moduleInfo.Wrapper);
            SerializedProperty prop = serializedObject.FindProperty("m_Value");

            EditorGUILayout.BeginVertical();

            if (prop != null && prop.NextVisible(true))
            {
                do
                {
                    if (!IsPropertyAllowed(prop, moduleInfo, skipPieceIdFields)) continue;

                    _ = InspectorPropertyDrawer.DrawPropertyField(prop, moduleInfo.FieldInfoMap, moduleInfo.AllowedFieldNames);
                }
                while (prop.NextVisible(false));
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                // Properties were modified, sync back to FieldResolvers
                SyncModuleToFieldResolvers(moduleInfo);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw module fields for a specific module, skipping PieceID fields
        /// Called from DialogueGraphInspectorPanel for hybrid UIElement/IMGUI rendering
        /// </summary>
        /// <param name="moduleInfo">Module info to draw</param>
        public void DrawModuleFieldsWithoutPieceId(ModuleInfo moduleInfo)
        {
            if (!moduleInfo.Wrapper)
            {
                EditorGUILayout.HelpBox($"Module {moduleInfo.ModuleType.Name} wrapper is invalid", MessageType.Warning);
                return;
            }

            DrawFilteredModuleFields(moduleInfo, skipPieceIdFields: true);
        }

        /// <summary>
        /// Check if a serialized property should be displayed
        /// </summary>
        /// <param name="prop">Serialized property</param>
        /// <param name="moduleInfo">Module info containing field information</param>
        /// <param name="skipPieceIdFields">Whether to skip PieceID fields (they are rendered with UIElements)</param>
        /// <returns>True if property should be displayed</returns>
        private static bool IsPropertyAllowed(SerializedProperty prop, ModuleInfo moduleInfo, bool skipPieceIdFields = false)
        {
            // Extract field name from property path (e.g., "m_Value.fieldName" -> "fieldName")
            var propertyPath = prop.propertyPath;
            var fieldName = propertyPath.Replace("m_Value.", "");

            // Check if it's a direct child of m_Value (not nested property)
            string rootFieldName;
            if (fieldName.Contains("."))
            {
                // This is a nested property, get root field name
                rootFieldName = fieldName.Split('.')[0];
            }
            else
            {
                rootFieldName = fieldName;
            }

            // Check if field is allowed
            if (!moduleInfo.AllowedFieldNames.Contains(rootFieldName))
            {
                return false;
            }

            // Skip PieceID fields if requested (they are rendered with UIElements)
            if (skipPieceIdFields && moduleInfo.FieldInfoMap.TryGetValue(rootFieldName, out var fieldInfo))
            {
                if (fieldInfo.FieldType == typeof(PieceID))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sync modified module value back to FieldResolvers
        /// </summary>
        /// <param name="moduleInfo">Module info to sync</param>
        private static void SyncModuleToFieldResolvers(ModuleInfo moduleInfo)
        {
            try
            {
                // Restore the module value back to FieldResolvers
                if (moduleInfo.Wrapper.Value is not Module moduleInstance) return;

                var resolvers = moduleInfo.ModuleView.GetAllFieldResolvers().ToArray();
                
                foreach (var (resolver, fieldInfo) in resolvers)
                {
                    // Read value from module instance
                    var fieldValue = fieldInfo.GetValue(moduleInstance);
                    // Write back to FieldResolver
                    resolver.Value = fieldValue;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to sync module {moduleInfo.ModuleType.Name} to resolvers: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            foreach (var moduleInfo in _moduleInfos)
            {
                if (moduleInfo.Wrapper)
                {
                    GlobalObjectManager.UnregisterObject(moduleInfo.Handle);
                }
            }
            _moduleInfos.Clear();
        }
    }
}

