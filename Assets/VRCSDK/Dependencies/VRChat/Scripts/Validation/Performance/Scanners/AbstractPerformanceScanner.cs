using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
    public abstract class AbstractPerformanceScanner : ScriptableObject
    {
        private const int MAXIMUM_COMPONENT_SCANS_PER_FRAME = 10;
        private static int _componentScansThisFrame = 0;
        private static int _componentScansFrameNumber = 0;

        private readonly Stack<IEnumerator> _coroutines = new Stack<IEnumerator>();

        private bool _limitComponentScansPerFrame = true;

        public abstract IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent);

        public void RunPerformanceScan(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
        {
            _limitComponentScansPerFrame = false;

            try
            {
                _coroutines.Push(RunPerformanceScanEnumerator(avatarObject, perfStats, shouldIgnoreComponent));
                while(_coroutines.Count > 0)
                {
                    IEnumerator currentCoroutine = _coroutines.Peek();
                    if(currentCoroutine.MoveNext())
                    {
                        IEnumerator nestedCoroutine = currentCoroutine.Current as IEnumerator;
                        if(nestedCoroutine != null)
                        {
                            _coroutines.Push(nestedCoroutine);
                        }
                    }
                    else
                    {
                        _coroutines.Pop();
                    }
                }

                _coroutines.Clear();
            }
            finally
            {
                _limitComponentScansPerFrame = true;
            }
        }

        protected IEnumerator ScanAvatarForComponentsOfType(Type componentType, GameObject avatarObject, List<Component> destinationBuffer)
        {
            yield return HandleComponentScansPerFrameLimit();

            Profiler.BeginSample("Component Scan");
            destinationBuffer.Clear();
            destinationBuffer.AddRange(avatarObject.GetComponentsInChildren(componentType, true));
            Profiler.EndSample();
        }

        protected IEnumerator ScanAvatarForComponentsOfType<T>(GameObject avatarObject, List<T> destinationBuffer)
        {
            yield return HandleComponentScansPerFrameLimit();

            Profiler.BeginSample("Component Scan");
            destinationBuffer.Clear();
            avatarObject.GetComponentsInChildren(true, destinationBuffer);
            Profiler.EndSample();
            yield return null;
        }

        private IEnumerator HandleComponentScansPerFrameLimit()
        {
            if(!_limitComponentScansPerFrame)
            {
                yield break;
            }

            while(_componentScansThisFrame >= MAXIMUM_COMPONENT_SCANS_PER_FRAME)
            {
                if(Time.frameCount > _componentScansFrameNumber)
                {
                    _componentScansFrameNumber = Time.frameCount;
                    _componentScansThisFrame = 0;
                    break;
                }

                yield return null;
            }

            _componentScansThisFrame++;
        }
    }
}
