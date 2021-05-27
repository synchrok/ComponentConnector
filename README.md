# Component Connector ✨

Automatically connect "correct components" in Unity

![cc_img_1](/Images/cc_img_1.gif)

## Setup ⛏

1. Copy `ComponentConnector.cs` to your project.
1. Attach **ComponentConnector** component to root game object.

![cc_img_2](/Images/cc_img_2.png)

## How to use ⚡️

- Component Connect

Just add [ComponentConnector] attribute to your public field.
```cs
// Match with field name
[ComponentConnector] public GameObject someObject;

// Match with "AwesomeObj"
[ComponentConnector("AwesomeObj")] public GameObject awesomeObject;
```

- Get Component

Just add [GetComponent] attribute to your public field.
```cs
[GetComponent] public SpriteRenderer spriteRenderer;
```

![cc_img_3](/Images/cc_img_3.gif)

- Button OnClick Event Connect (Bonus but not recommended)

Add [OnClick] attribute to method, and extend IComponentConnector interface to class.
```cs
public class SomeClass : MonoBehaviour, IComponentConnector {

    [OnClick("SomeButton")]
    public void OnClickSomeButton() { ... }
    
}
```

![cc_img_4](/Images/cc_img_4.gif)

## Performance issue 😅

**Component Connector** basically performs a **full scan** operation after the change of the editor is detected or compiled.

Therefore, if there are many objects in the scene, performance degradation may occur within the editor, and in this case I recommend the following methods.

1. Disable the **ComponentConnector** script that exists in the scene
1. Perform a scan job directly using the **Context Menu** of the desired component

I have plan to change to a better algorithm in the future.

![cc_img_5](/Images/cc_img_5.png)

## Warning ⚠

>I do not responsibility for the issue that you encountered by using this code. But I saved my time for the projects below.


## Projects using Component Connector 👀
- **Help Me! Endz** ([Google Play](https://play.google.com/store/apps/details?id=com.awesomepiece.endz) / [App Store](https://apps.apple.com/kr/app/%EB%8F%84%EC%99%80%EC%A4%98-%EC%97%94%EC%A6%88/id1498183231))
- **King God Castle** ([Google Play](https://play.google.com/store/apps/details?id=com.awesomepiece.castle) / [App Store](https://apps.apple.com/kr/app/%ED%82%B9%EA%B0%93%EC%BA%90%EC%8A%AC/id1526791430))

## License

**Component Connector** is under MIT license. See the [LICENSE](LICENSE) file for more info.

