using System.Collections;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Filters;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
    #if VRC_CLIENT
    [CreateAssetMenu(
        fileName =  "New PerformanceFilterSet",
        menuName = "VRC Scriptable Objects/Performance/PerformanceFilterSet"
    )]
    #endif
    public class PerformanceFilterSet : ScriptableObject
    {
        public AbstractPerformanceFilter[] performanceFilters;

        public IEnumerator ApplyPerformanceFilters(
            GameObject avatarObject,
            AvatarPerformanceStats perfStats,
            PerformanceRating ratingLimit,
            AvatarPerformance.IgnoreDelegate shouldIgnoreComponent,
            AvatarPerformance.FilterBlockCallback onBlock
        )
        {
            foreach(AbstractPerformanceFilter performanceFilter in performanceFilters)
            {
                if(performanceFilter == null)
                {
                    continue;
                }
                
                bool avatarBlocked = false;
                yield return performanceFilter.ApplyPerformanceFilter(avatarObject, perfStats, ratingLimit, shouldIgnoreComponent, () => { avatarBlocked = true; });

                if(!avatarBlocked)
                {
                    continue;
                }

                onBlock();
                break;
            }
        }
    }
}
