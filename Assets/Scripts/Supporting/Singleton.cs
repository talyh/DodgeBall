using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// defines a generic Singleton template that can be used as a parent for other 
// classes that need to be singleton (such as GameController, CameraContoller, et)
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // this will be the actual instance, but will define a public getter of name "instance"
    // to represent its interaction with the world
    private static T _instance;

    // creates a lock for the getter so multiple gets are queued if needed
    private static object _lock = new object();

    // to help determine whether we should return null in case a get is attempted
    // after the singleton instance is destroyed (which would only happen on ApplicationQuit)
    private static bool applicationIsQuitting = false;

    // public getter for "_instance"
    public static T instance
    {
        get
        {
            // if the instance has been destroyed, return "null"
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] instance '" + typeof(T) +
                "' already destroyed on application quit." +
                " Won't create again - returning null.");
                return null;
            }

            // lock each get so multiple requests are queued
            lock (_lock)
            {
                // if a value hasn't been set for _instance yet
                if (_instance == null)
                {
                    // set the instance as a  find of an object in the scene that is 
                    // of the type we're trying to get
                    _instance = (T)FindObjectOfType(typeof(T));

                    // if we find one
                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopening the scene might fix it.");
                        // return instance (if we didn't find any, instance would be null and we wouldn't
                        // want to return that)
                        return _instance;
                    }

                    // if we didn't find any object in the scene that is of the right type
                    if (_instance == null)
                    {
                        // create a new object
                        GameObject singleton = new GameObject();
                        // add a component of the type we're referring to tha new object
                        _instance = singleton.AddComponent<T>();
                        // set the new object name
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        // add the newly created object to the DontDestroyOnLoad pile
                        // so it continues lives when changing from scene to scene
                        DontDestroyOnLoad(singleton);
                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                        " is needed in the scene, so '" + singleton +
                        "' was created with DontDestroyOnLoad.");
                    }
                    else // do nothing
                    {
                        DontDestroyOnLoad(_instance);
                        Debug.Log("[Singleton] Using instance already created: " +
                        _instance.gameObject.name);
                    }
                }

                // return the instance
                return _instance;
            }
        }
    }

    public void Initialize()
    { }

    // turn on the applicationQuitting when the instance is destroyed
    public void OnDestroy()
    {
        applicationIsQuitting = true;

        AdditionalDestroyTasks();
    }

    protected virtual void AdditionalDestroyTasks()
    { }

    public void Awake()
    {
        applicationIsQuitting = false;

        AdditionalAwakeTasks();
    }

    protected virtual void AdditionalAwakeTasks()
    { }
}