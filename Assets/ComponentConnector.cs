// #define USE_CC_SEARCH

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor.Events;
using UnityEditor;
#endif

public interface IComponentConnector { }

[AttributeUsage(AttributeTargets.Field)]
public class ComponentConnectAttribute : Attribute {
    private string _name;
    private bool _inChildren;

    public ComponentConnectAttribute(string name, bool inChildren = false) {
        _name = name;
        _inChildren = inChildren;
    }

    public ComponentConnectAttribute() {
        _name = null;
        _inChildren = true;
    }

    public string name {
        set { _name = value; }
        get { return _name; }
    }

    public bool inChildren {
        get { return _inChildren; }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OnClickAttribute : Attribute {
    private string _name;
    private bool _inChildren;

    public OnClickAttribute(string name, bool inChildren = false) {
        _name = name;
        _inChildren = inChildren;
    }

    public string name
    {
        get { return _name; }
    }

    public bool inChildren
    {
        get { return _inChildren; }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class GetComponentAttribute : Attribute {
}

[ExecuteInEditMode]
[Serializable]
public class ComponentConnector : MonoBehaviour {

    public bool showNotExistLog = true;

    private const float _interval = 0.1f;
    private float _counter;

#if UNITY_EDITOR
    public void Update() {
        if (Application.isPlaying)
            return;

        _counter -= Time.deltaTime;
        if (_counter <= 0f) {
            _counter = _interval;
            Connect();
        }
    }

    public void Connect() {
        var lst = GetComponentsInChildren<Component>(true);
        foreach (var mb in lst) {
            if (mb == null)
                continue;

            try {
                var ms = mb.GetType().GetFields();
                foreach (var mi in ms) {
                        var attrs = mi.GetCustomAttributes(true);
                        foreach (object attr in attrs) {
                            if (attr is ComponentConnectAttribute) {
                                ComponentConnectImpl(attr, mb, mi);
                            } else if (attr is GetComponentAttribute) {
                                GetComponentImpl(attr, mb, mi);
                            }
                        }
                }
            } catch (Exception e) {
                Debug.Log("ComponentConnector-ComponentConnect: " + e.Message);
            }

            if (mb is IComponentConnector) {
                var methods = mb.GetType().GetMethods();
                foreach (var mi in methods) {
                    try {
                        var attrs = mi.GetCustomAttributes(true);
                        foreach (object attr in attrs) {
                            if (attr is OnClickAttribute) {
                                OnClickImpl(attr, mb, mi);
                            }
                        }
                    } catch (Exception e) {
                        Debug.Log("ComponentConnector-OnClick: " + e.Message);
                    }
                }
            }
        }
    }

    private void ComponentConnectImpl(object attr, Component mb, FieldInfo mi) {
        var value = mi.GetValue(mb);
        if (value != null && !value.ToString().Equals("null")) {
            if (!value.ToString().EndsWith("[]")) // Except Array
                return;
        }

        var cc = attr as ComponentConnectAttribute;

        if (cc.name == null)
            cc.name = mi.Name;

        var go = null as GameObject;

        if (cc.name.Contains("...")) {
            var d = cc.name.Split(new[] { "..." }, StringSplitOptions.RemoveEmptyEntries);
            if (d.Length == 1) {
                go = mb.gameObject.Search(d[0]);
                if (go == null && !cc.inChildren)
                    go = gameObject.Search(d[0]);
                if (go == null) {
                    var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                    foreach (var root in roots) {
                        go = root.Search(cc.name);
                        if (go != null)
                            break;
                    }
                }
            } else {
                go = mb.gameObject.SearchWithParentName(d[1], d[0]);
                if (go == null && !cc.inChildren)
                    go = gameObject.SearchWithParentName(d[1], d[0]);
                if (go == null) {
                    var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                    foreach (var root in roots) {
                        if (root == gameObject)
                            continue;
                        go = root.SearchWithParentName(d[1], d[0]);
                        if (go != null)
                            break;
                    }
                }
            }
        } else if (cc.name.EndsWith("~")) {
            var keyword = cc.name.Replace("~", "");
            var searched = mb.gameObject.SearchAllStartsWith(keyword);
            if (searched == null && !cc.inChildren)
                searched = gameObject.SearchAllStartsWith(keyword);

            if (searched == null) {
                var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in roots) {
                    searched = root.SearchAllStartsWith(keyword);
                    if (searched != null)
                        break;
                }
            }

            if (searched != null) {
                if (mi.FieldType == typeof(GameObject[])) {
                    mi.SetValue(mb, searched);
                } else {
                    var targetType = mi.FieldType.GetElementType();

                    var array = Array.CreateInstance(targetType, searched.Length);

                    for (var l = 0; l < searched.Length; l++)
                        array.SetValue(searched[l].GetComponent(targetType), l);

                    mi.SetValue(mb, array);
                }
            }
            return;
        } else {
            go = mb.gameObject.Search(cc.name);
            if (go == null && !cc.inChildren)
                go = gameObject.Search(cc.name);
            if (go == null) {
                var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in roots) {
                    go = root.Search(cc.name);
                    if (go != null)
                        break;
                }
            }
        }

        if (go == null) {
            if (showNotExistLog)
                Debug.LogWarning(string.Format("ComponentConnector: {0} GameObject does not exist. (from {1} in {2})", cc.name, mi.Name, mb.GetType()));
            return;
        }

        if (mi.FieldType == typeof(GameObject)) { // GameObject
            mi.SetValue(mb, go);
        } else { // Component
            var comp = go.GetComponent(mi.FieldType);
            if (comp != null)
                mi.SetValue(mb, comp);
        }
    }

    private void OnClickImpl(object attr, Component mb, MethodInfo mi) {
        var cc = attr as OnClickAttribute;

        var go = null as GameObject;

        go = mb.gameObject.Search(cc.name);
        if (go == null && !cc.inChildren)
            go = gameObject.Search(cc.name);

        if (go == null) {
            if (showNotExistLog)
                Debug.LogWarning(string.Format("ComponentConnector: {0} GameObject does not exist. (from {1} in {2})", cc.name, mi.Name, mb.GetType()));
            return;
        }

        var btn = go.GetComponent<Button>();
        if (btn == null)
            return;

        var pcnt = btn.onClick.GetPersistentEventCount();
        for (var i=0; i<pcnt; i++)
            UnityEventTools.RemovePersistentListener(btn.onClick, 0);

        var action = Delegate.CreateDelegate(typeof(UnityAction), mb, mi.Name) as UnityAction;
        UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    private void GetComponentImpl(object attr, Component mb, FieldInfo mi) {
        var cc = attr as GetComponentAttribute;
        var go = mb.gameObject;

        if (go == null) {
            return;
        }

        if (mi.FieldType == typeof(GameObject)) { // GameObject
            mi.SetValue(mb, go);
        } else { // Component
            var comp = go.GetComponent(mi.FieldType);
            if (comp != null)
                mi.SetValue(mb, comp);
        }
    }

#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ComponentConnector)), CanEditMultipleObjects]
public class ComponentConnectorEditor : Editor {
    private SerializedObject _object;

    public void OnEnable() {
        _object = new SerializedObject(targets);
    }

    public override void OnInspectorGUI() {
        _object.Update();
        DrawDefaultInspector();
        _object.ApplyModifiedProperties();
    }

    [MenuItem("CONTEXT/Component/Component Connect", false, Int32.MaxValue)]
    public static void ContextRun() {
        var ccs = FindObjectsOfType<ComponentConnector>();
        foreach (var cc in ccs) {
            cc.Connect();
        }
    }
}
#endif

#if !USE_CC_SEARCH
public static class ComponentConnectorSearchUtility {
    private static List<GameObject> _searchList = new List<GameObject>();

    public static GameObject[] SearchAllStartsWith(this GameObject target, string keyword) {
        _searchList.Clear();

        SearchAllStartsWithImpl(target, keyword);

        return _searchList.ToArray();
    }

    private static void SearchAllStartsWithImpl(GameObject target, string keyword) {
        if (target.name.StartsWith(keyword)) {
            _searchList.Add(target);
            return;
        }

        for (int i = 0; i < target.transform.childCount; ++i) {
            SearchAllStartsWithImpl(target.transform.GetChild(i).gameObject, keyword);
        }
    }

    public static GameObject Search(this GameObject target, string name) {
        if (target.name.ToLower().Equals(name.ToLower())) return target;

        for (int i = 0; i < target.transform.childCount; ++i) {
            var result = Search(target.transform.GetChild(i).gameObject, name);

            if (result != null) return result;
        }

        return null;
    }

    public static GameObject SearchWithParentName(this GameObject target, string name, string parentName) {
        if (target.name.ToLower().Equals(parentName.ToLower())) {
            for (var i = 0; i < target.transform.childCount; i++)
                if (target.transform.GetChild(i).name.Equals(name))
                    return target.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < target.transform.childCount; ++i) {
            var result = SearchWithParentName(target.transform.GetChild(i).gameObject, name, parentName);

            if (result != null)
                return result;
        }

        return null;
    }
}
#endif
