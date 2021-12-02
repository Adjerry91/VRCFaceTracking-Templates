using System.Collections;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Filters
{
    public abstract class AbstractPerformanceFilter : ScriptableObject
    {
        public abstract IEnumerator ApplyPerformanceFilter(
            GameObject avatarObject,
            AvatarPerformanceStats perfStats,
            PerformanceRating ratingLimit,
            AvatarPerformance.IgnoreDelegate shouldIgnoreComponent,
            AvatarPerformance.FilterBlockCallback onBlock
        );

        protected static IEnumerator RemoveComponentsOfTypeEnumerator<T>(GameObject target) where T : Component
        {
            if(target == null)
            {
                yield break;
            }

            foreach(T targetComponent in target.GetComponentsInChildren<T>(true))
            {
                if(targetComponent == null || targetComponent.gameObject == null)
                {
                    continue;
                }

                #if VERBOSE_COMPONENT_REMOVAL
                Debug.LogWarningFormat("Removing {0} comp from {1}", targetComponent.GetType().Name, targetComponent.gameObject.name);
                #endif

                yield return RemoveComponent(targetComponent);
            }
        }

        protected static IEnumerator RemoveComponent(Component targetComponent)
        {
            yield return RemoveDependencies(targetComponent);

            Destroy(targetComponent);
            yield return null;
        }

        protected static IEnumerator RemoveDependencies(Component targetComponent)
        {
            if(targetComponent == null)
            {
                yield break;
            }

            Component[] siblingComponents = targetComponent.GetComponents<Component>();
            if(siblingComponents == null || siblingComponents.Length == 0)
            {
                yield break;
            }

            System.Type componentType = targetComponent.GetType();
            foreach(Component siblingComponent in siblingComponents)
            {
                if(siblingComponent == null)
                {
                    continue;
                }

                bool deleteMe = false;
                object[] requireComponentAttributes = siblingComponent.GetType().GetCustomAttributes(typeof(RequireComponent), true);
                if(requireComponentAttributes.Length == 0)
                {
                    continue;
                }

                foreach(var requireComponentObject in requireComponentAttributes)
                {
                    RequireComponent requireComponentAttribute = requireComponentObject as RequireComponent;
                    if(requireComponentAttribute == null)
                    {
                        continue;
                    }

                    if(
                        requireComponentAttribute.m_Type0 != componentType &&
                        requireComponentAttribute.m_Type1 != componentType &&
                        requireComponentAttribute.m_Type2 != componentType
                    )
                    {
                        continue;
                    }

                    deleteMe = true;
                    break;
                }

                if(!deleteMe)
                {
                    continue;
                }

                #if VERBOSE_COMPONENT_REMOVAL
                Debug.LogWarningFormat("Deleting component dependency {0} found on {1}", siblingComponent.GetType().Name, targetComponent.gameObject.name);
                #endif

                yield return RemoveComponent(siblingComponent);
            }
        }
    }
}
