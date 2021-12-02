using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace VRC.SDKBase.Validation
{
    public static class ValidationUtils
    {
        public static void RemoveIllegalComponents(GameObject target, HashSet<Type> whitelist, bool retry = true, bool onlySceneObjects = false, bool logStripping = true)
        {
            List<Component> foundComponents = FindIllegalComponents(target, whitelist);
            foreach(Component component in foundComponents)
            {
                if(component == null)
                {
                    continue;
                }
                
                if(onlySceneObjects && component.GetInstanceID() < 0)
                {
                    continue;
                }

                if(logStripping)
                {
                    Core.Logger.LogWarning($"Removing {component.GetType().Name} comp from {component.gameObject.name}");
                }

                RemoveComponent(component);
            }
        }

        public static List<Component> FindIllegalComponents(GameObject target, HashSet<Type> whitelist)
        {
            List<Component> foundComponents = new List<Component>();
            Component[] allComponents = target.GetComponentsInChildren<Component>(true);
            foreach(Component component in allComponents)
            {
                if(component == null)
                {
                    continue;
                }

                Type componentType = component.GetType();
                if(whitelist.Contains(componentType))
                {
                    continue;
                }

                foundComponents.Add(component);
            }

            return foundComponents;
        }

        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        private static readonly Dictionary<string, HashSet<Type>> _whitelistCache = new Dictionary<string, HashSet<Type>>();
        public static HashSet<Type> WhitelistedTypes(string whitelistName, IEnumerable<string> componentTypeWhitelist)
        {
            if (_whitelistCache.ContainsKey(whitelistName))
            {
                return _whitelistCache[whitelistName];
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<Type> whitelist = new HashSet<Type>();
            foreach(string whitelistedTypeName in componentTypeWhitelist)
            {
                Type whitelistedType = GetTypeFromName(whitelistedTypeName, assemblies);
                if(whitelistedType == null)
                {
                    continue;
                }

                if(whitelist.Contains(whitelistedType))
                {
                    continue;
                }

                whitelist.Add(whitelistedType);
            }

            AddDerivedClasses(whitelist);

            _whitelistCache[whitelistName] = whitelist;

            return _whitelistCache[whitelistName];
        }

        public static HashSet<Type> WhitelistedTypes(string whitelistName, IEnumerable<Type> componentTypeWhitelist)
        {
            if (_whitelistCache.ContainsKey(whitelistName))
            {
                return _whitelistCache[whitelistName];
            }

            HashSet<Type> whitelist = new HashSet<Type>();
            whitelist.UnionWith(componentTypeWhitelist);

            AddDerivedClasses(whitelist);

            _whitelistCache[whitelistName] = whitelist;

            return _whitelistCache[whitelistName];
        }

        private static void AddDerivedClasses(HashSet<Type> whitelist)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies)
            {
                foreach(Type type in assembly.GetTypes())
                {
                    if(whitelist.Contains(type))
                    {
                        continue;
                    }

                    if(!typeof(Component).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    Type currentType = type;
                    while(currentType != typeof(object) && currentType != null)
                    {
                        if(whitelist.Contains(currentType))
                        {
                            whitelist.Add(type);
                            break;
                        }

                        currentType = currentType.BaseType;
                    }
                }
            }
        }

        public static Type GetTypeFromName(string name, Assembly[] assemblies = null)
        {
            if (_typeCache.ContainsKey(name))
            {
                
                return _typeCache[name];
            }

            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (Assembly assembly in assemblies)
            {
                Type found = assembly.GetType(name);
                if(found == null)
                {
                    continue;
                }

                _typeCache[name] = found;
                return found;
            }

            //This is really verbose for some SDK scenes, eg.
            //If they don't have FinalIK installed
#if VRC_CLIENT && UNITY_EDITOR
            Debug.LogWarningFormat("Could not find type {0}", name);
#endif

            _typeCache[name] = null;
            return null;
        }

        private static readonly Dictionary<Type, ImmutableArray<RequireComponent>> _requireComponentsCache = new Dictionary<Type, ImmutableArray<RequireComponent>>();
        private static void RemoveDependencies(Component rootComponent)
        {
            if (rootComponent == null)
            {
                return;
            }

            Component[] components = rootComponent.GetComponents<Component>();
            if (components == null || components.Length == 0)
            {
                return;
            }

            Type compType = rootComponent.GetType();
            foreach (var siblingComponent in components)
            {
                if (siblingComponent == null)
                {
                    continue;
                }

                Type siblingComponentType = siblingComponent.GetType();
                if(!_requireComponentsCache.TryGetValue(siblingComponentType, out ImmutableArray<RequireComponent> requiredComponentAttributes))
                {
                    requiredComponentAttributes = siblingComponentType.GetCustomAttributes(typeof(RequireComponent), true).Cast<RequireComponent>().ToImmutableArray();
                    _requireComponentsCache.Add(siblingComponentType, requiredComponentAttributes);
                }

                bool deleteMe = false;
                foreach (RequireComponent requireComponent in requiredComponentAttributes)
                {
                    if (requireComponent == null)
                    {
                        continue;
                    }

                    if(requireComponent.m_Type0 != compType && requireComponent.m_Type1 != compType && requireComponent.m_Type2 != compType)
                    {
                        continue;
                    }

                    deleteMe = true;
                    break;
                }

                if (deleteMe && siblingComponent != rootComponent)
                {
                    RemoveComponent(siblingComponent);
                }
            }
        }

        public static void RemoveComponent(Component comp)
        {
            if (comp == null)
            {
                return;
            }

            RemoveDependencies(comp);

#if VRC_CLIENT
            UnityEngine.Object.DestroyImmediate(comp, true);
#else
            UnityEngine.Object.DestroyImmediate(comp, false);
#endif
        }

        public static void RemoveComponentsOfType<T>(GameObject target) where T : Component
        {
            if (target == null)
                return;

            foreach (T comp in target.GetComponentsInChildren<T>(true))
            {
                if (comp == null || comp.gameObject == null)
                    continue;

                RemoveComponent(comp);
            }
        }
    }
}
