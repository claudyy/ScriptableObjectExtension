# ScriptableObjectExtension
With scriptable object Extension it is easier to work with unity's scriptable object. Unity has a functionality where you can assign scriptable object to other scriptable object. But these is not easy to do, you have to implement this your self. With this extension it is super easy to do this. When having a lot of scriptable objects it can be hard to balance all the values. So the export features can make this better. You can export your data and put them into spread sheet, where you can have all your at one glance.

# SubAsset
  SubAsset makes it easier to create child scriptable objects. It gives you an easy way to create childs. And some additionaly functionality. You can sort, delet or copy them.
  You have to inherant from ScriptalbeObjectParent. Now you can add childs in the editor. But you can overidde some function to custom it more to your need.
 
  Folder()
    With this you can define where the pop up window that adds the child start looking at.
  GetIndent(ScriptableObject subAsset)
    If you want you sub Assets to have an indent in the editor you can return here an indent number. by default it is zero
  GetWarning(ScriptableObject subAsset)
    you can color the sub Asset red if something is wrong with it. Return true to color it red
  GetWarningMessage(ScriptableObject subAsset)
    If GetWarning returns true you can also implement a message that should be shown
  
  GetAssets<T>()
    This function return a list of all sub assets of the type T.
  
# ConditionStack
  

# Exporter

# Reflaction Exporter
