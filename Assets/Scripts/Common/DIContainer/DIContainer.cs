using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;
using System.Reflection;
using Common.UI;
using Cosmocompost.BRG.Plants;
using Cosmocompost.Cameras;
using Cosmocompost.Inputs;
using Cosmocompost.InstanceDrawing;
using Cosmocompost.Levels;

public class DIContainer : MonoBehaviour
{
    
    public ScriptableObject[] registeredScriptableObjects;
    
    private static Dictionary<Type, Object> services;
    private static Dictionary<Type, Func<Object>> dynamicServices;


    void Awake()
    {
        services = new Dictionary<Type, object>();
        dynamicServices = new Dictionary<Type, Func<object>>();
        
        Register(FindObjectOfType<LevelController>());
        Register(FindObjectOfType<InstanceDrawer>());
        Register(FindObjectOfType<RenderManager>());
        Register(FindObjectOfType<InputManager>());
        Register(FindObjectOfType<UIController>());
        Register(FindObjectOfType<BRGPlants>());
        Register(FindObjectOfType<CameraController>());
        Register(FindObjectOfType<MeteoriteEvent>());
        
        //Example
        /*
#if UNITY_IOS && !UNITY_EDITOR
        RegisterAs(new IOSDeviceVibrator(), typeof(AbstractDeviceVibrator));
#elif UNITY_EDITOR
        RegisterAs(new EditorDeviceVibrator(), typeof(AbstractDeviceVibrator));
#endif
         */

        /*
         * //Dynamic injections
        RegisterDynamicAs<Object>(() => Get<GameController>().CurrentLevel, typeof(BaseGameLevel));
      
      */
        
        for(int i = 0; i < registeredScriptableObjects.Length; i++) {
            RegisterAs(registeredScriptableObjects[i], registeredScriptableObjects[i].GetType());
        }
    }


    private void InjectInRegisteredObjects()
    {
        foreach (KeyValuePair<Type, object> service in services)
        {
            IInjectionCallbackReceiver icr = service.Value as IInjectionCallbackReceiver;
            icr?.OnInject();
        }
    }

    
     //////////////////////////////////
    //REG SERVICE
    
    private static void Register<T>(T service)
    {
        Type serviceType = typeof(T);
        if (services.ContainsKey(serviceType))
        {
            Debug.Log("service already reg " + serviceType);
        }
        else
        {
            services.Add(serviceType, service);
        }
    }

    //REG SERVICE WITH TYPE
    private static void RegisterAs<T>(T service, Type serviceType)
    {
        if (services.ContainsKey(serviceType))
        {
            Debug.Log("service already reg " + serviceType);
        }
        else
        {
            services.Add(serviceType, service);
        }
    }

    //REG DYNAMIC SERVICE
    private static void RegisteredDynamicAs<T>(Func<Object> serviceDelegate, Type serviceType)
    {
        if (dynamicServices.ContainsKey(serviceType))
        {
            Debug.Log("Dynamic service already registered " + serviceType);
        }
        else
        {
            dynamicServices.Add(serviceType, serviceDelegate);
        }
    }
    
    
//////////////////////////////////////


    //inject services into [Inject] fields
    public static void InjectProperties<T>(T obj)
    {
        Type t = typeof(T);
        InjectProperties(obj, t);
    }

    /*EXAMPLE
     private static void InjectPropertiesUsingActualObjectType(object obj) {
        Type t = obj.GetType();
        InjectProperties(obj, t);
    }
     */

    private static void InjectProperties(object obj, Type t)
    {
        FieldInfo[] fileds = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
                                         BindingFlags.FlattenHierarchy);
        foreach (FieldInfo f in fileds)
        {
            if (f.GetCustomAttribute<Inject>() != null)
            {
                Type fieldType = f.FieldType;
                f.SetValue(obj, Get(fieldType));
            }
        }
    }


///////////Get сервис с типом Т
    public static T Get<T>() where T : class
    {
        Object serviceObject;
        if (services.TryGetValue(typeof(T), out serviceObject))
        {
            return serviceObject as T;
        }

        if (dynamicServices.TryGetValue(typeof(T), out Func<Object> dynamicService))
        {
            return dynamicService.Invoke() as T;
        }

        return null;
    }

    public static Object Get(Type t)
    {
  
        if (services.TryGetValue(t, out Object serviceObject))
        {
            return serviceObject;
        }

        if (dynamicServices.TryGetValue(t, out Func<Object> dynamicService))
        {
            Object obj = dynamicService.Invoke();
            Debug.Log("Get dynamic service = " + obj);
            return obj;
        }

        return null;
    }

////////////////////////
}