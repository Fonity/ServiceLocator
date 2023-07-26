﻿using System;
#if UNITASK_ENABLED
using Cysharp.Threading.Tasks;
#endif

namespace BrunoMikoski.ServicesLocation
{
    public class ServiceReference<T> : IServiceObservable where T : class
    {
        private bool hasCachedInstance;
        private T instance;
        public T Reference
        {
            get
            {
                if (!hasCachedInstance)
                {
                    if (ServiceLocator.IsQuitting)
                        return null;

                    hasCachedInstance = ServiceLocator.Instance.TryGetInstance(out instance);
                    if (hasCachedInstance)
                    {
                        ServiceLocator.Instance.UnsubscribeToServiceChanges<T>(this);
                        ServiceLocator.Instance.SubscribeToServiceChanges<T>(this);
                    }
                }
                return instance;
            }
        }

        public bool Exists
        {
            get
            {
                if (ServiceLocator.IsQuitting)
                    return false;
                
                return ServiceLocator.Instance.HasService<T>();
            }
        }

        public static implicit operator T(ServiceReference<T> serviceReference)
        {
            return serviceReference.Reference;
        }

        public void ClearCache()
        {
            instance = null;
            hasCachedInstance = false;
        }
        
        void IServiceObservable.OnServiceRegistered(Type targetType)
        {
            if (!ServiceLocator.Instance.TryGetInstance(out T newInstance))
                return;
            
            if (Equals(newInstance, instance))
                return;
        
            instance = newInstance;
            hasCachedInstance = true;
        }
        
        void IServiceObservable.OnServiceUnregistered(Type targetType)
        {
            ClearCache();
        }

#if UNITASK_ENABLED
        public async UniTask WaitForServiceBeAvailableAsync()
        {
            await ServiceLocator.Instance.WaitForServiceAsync<T>();
        }
#endif
    }
}
